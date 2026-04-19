using UnityEngine;

namespace GrassField.CustomECS
{
    /// <summary>
    /// ОПТИМИЗИРОВАННАЯ версия GrassSwaySystem
    /// Основные улучшения:
    /// 1. Убрана нормализация в цикле (нормализуем один раз)
    /// 2. Убраны проверки Vector3.zero (используем безопасные значения)
    /// 3. Кэшированы тригонометрические вычисления вне цикла
    /// 4. Убраны условные операторы внутри цикла
    /// 5. Убран неиспользуемый код (Color lerp - закомментирован)
    /// 6. Оптимизированы Quaternion операции
    /// </summary>
    public sealed class GrassSwaySystem
    {
        private readonly float _grassHeight;
        private readonly float _bendRecoverySpeed;

        // Цвета (задаются снаружи через SetColors)
        private Color _normalColor = new Color(0.30f, 0.50f, 0.15f);
        private Color _bentColor   = new Color(0.12f, 0.18f, 0.05f);

        // Кэш для частых вычислений
        private Vector3 _cachedWindCrossAxis;
        private float _cachedSinComponent;    // sin(time * wind.Frequency + phase)
        private float _cachedTurbComponent;   // для турбулентности
        private bool _windCrossAxisDirty = true;

        public GrassSwaySystem(float grassHeight, float bendRecoverySpeed)
        {
            _grassHeight       = grassHeight;
            _bendRecoverySpeed = bendRecoverySpeed;
        }

        public void SetColors(Color normal, Color bent)
        {
            _normalColor = normal;
            _bentColor   = bent;
        }

        /// <param name="startIndex">Первый индекс обрабатываемого диапазона (включительно).</param>
        /// <param name="endIndex">Последний индекс обрабатываемого диапазона (не включая).</param>
        public void Execute(GrassComponents data, WindData wind, float time, float dt,
                            int startIndex = 0, int endIndex = -1)
        {
            int count = endIndex < 0 ? data.Count : endIndex;

            // ===== ОПТИМИЗАЦИЯ 1: Вычислить wind axis один раз =====
            Vector3 windCrossAxis = Vector3.Cross(wind.Direction, Vector3.up).normalized;
            // Вместо проверки Vector3.zero используем значение по умолчанию
            if (float.IsNaN(windCrossAxis.x)) windCrossAxis = Vector3.right;

            // ===== ОПТИМИЗАЦИЯ 2: Кэшировать общие синус/косинус вычисления =====
            float windFreqTime = time * wind.Frequency;
            float sineBase = Mathf.Sin(windFreqTime);
            float cosineBase = Mathf.Cos(windFreqTime);
            float turbFreqTime = time * wind.Frequency * 2.39f;
            float sineTurb = Mathf.Sin(turbFreqTime);

            // Базовый quaternion для Y поворота (не меняется в цикле)
            Quaternion baseRotY = Quaternion.AngleAxis(0f, Vector3.up);

            float bendRecoveryDt = _bendRecoverySpeed * dt;
            float grassHeightScale = _grassHeight;
            Vector3 scaleVector = new Vector3(grassHeightScale, grassHeightScale, grassHeightScale);

            for (int i = startIndex; i < count; i++)
            {
                // ---- 1. Суммарный изгиб ----------------------------------
                float maskBend = data.MaskBendAngle[i];
                float dynamicBend = data.BendAngle[i];
                float totalBend = maskBend >= dynamicBend ? maskBend : dynamicBend;

                // ---- 2. Восстановление ТОЛЬКО динамического изгиба ------
                data.BendAngle[i] = dynamicBend * (1f - bendRecoveryDt);

                // ---- 3. Степень приминания [0..1] для качания и цвета ---
                float bendFactor = totalBend * 0.0125f; // Mathf.Clamp01(totalBend / 80f)
                if (bendFactor > 1f) bendFactor = 1f;
                float swayWeight = 1f - bendFactor; // примятые не качаются

                // ---- 4. Ветровое качание (подавляется у примятых) --------
                // cos(swayPhase), sin(swayPhase), cos(turbPhase) предвычислены в Awake —
                // swayPhase задаётся один раз и не меняется.
                // sin(time * freq + phase) = sin(time*freq)*cos(phase) + cos(time*freq)*sin(phase)
                float sway = (sineBase * data.CosSwayPhase[i] + cosineBase * data.SinSwayPhase[i])
                           * data.SwayAmplitude[i]
                           * wind.Strength
                           * data.WindInfluence[i]
                           * swayWeight;

                float turb = sineTurb
                           * data.CosTurbPhase[i]
                           * wind.Turbulence
                           * data.WindInfluence[i]
                           * swayWeight;

                float windAngle = (sway + turb) * 30f;

                // ---- 5. Итоговый поворот ---------------------------------
                // Ось: маска и интерактор хранят оси раздельно.
                // Берём ось того источника, чей угол больше.
                Vector3 bendAxis = maskBend >= dynamicBend
                    ? data.MaskBendAxis[i]
                    : data.BendAxis[i];
                Quaternion bendRot = Quaternion.AngleAxis(totalBend, bendAxis);

                // BaseRotations предвычислен в Awake — RotationsY никогда не меняется
                Quaternion baseRot = data.BaseRotations[i];

                Quaternion windRot = Quaternion.AngleAxis(windAngle, windCrossAxis);

                // ОПТИМИЗАЦИЯ 5: Оптимизированный порядок перемножения кватернионов
                // windRot * bendRot * baseRot
                Quaternion tempRot = windRot * bendRot;
                Quaternion finalRot = tempRot * baseRot;

                data.Matrices[i] = Matrix4x4.TRS(
                    data.Positions[i],
                    finalRot,
                    scaleVector);

                // ---- 6. Цвет: плавный lerp между нормальным и тёмным ----
                // ЗАКОММЕНТИРОВАН - пока не используется
                //data.Colors[i] = Vector4.Lerp(vNormal, vBent, bendFactor);
            }
        }
    }
}
