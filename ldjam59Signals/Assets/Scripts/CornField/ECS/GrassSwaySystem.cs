using UnityEngine;

namespace GrassField.CustomECS
{
    public sealed class GrassSwaySystem
    {
        private readonly float _grassHeight;
        private readonly float _bendRecoverySpeed;

        // Цвета (задаются снаружи через SetColors)
        private Color _normalColor = new Color(0.30f, 0.50f, 0.15f);
        private Color _bentColor   = new Color(0.12f, 0.18f, 0.05f);

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

        public void Execute(GrassComponents data, WindData wind, float time, float dt)
        {
            int count = data.Count;

            Vector3 windCrossAxis = Vector3.Cross(wind.Direction, Vector3.up).normalized;
            if (windCrossAxis == Vector3.zero) windCrossAxis = Vector3.right;

            // Предвычисляем Color как Vector4 один раз
            Vector4 vNormal = (Vector4)_normalColor;
            Vector4 vBent   = (Vector4)_bentColor;

            for (int i = 0; i < count; i++)
            {
                // ---- 1. Суммарный изгиб ----------------------------------
                // MaskBendAngle статичен (не меняется между кадрами).
                // BendAngle динамичен (от интерактора, восстанавливается).
                // Берём максимум — маска не мешает интерактору и наоборот.
                float totalBend = Mathf.Max(data.MaskBendAngle[i], data.BendAngle[i]);

                // Какая ось «победила»
                Vector3 totalAxis = data.MaskBendAngle[i] >= data.BendAngle[i]
                    ? data.MaskBendAxis[i]
                    : data.BendAxis[i];

                // ---- 2. Восстановление ТОЛЬКО динамического изгиба ------
                // MaskBendAngle НЕ трогаем — он статичен.
                data.BendAngle[i] = Mathf.Lerp(data.BendAngle[i], 0f,
                                               _bendRecoverySpeed * dt);

                // ---- 3. Степень приминания [0..1] для качания и цвета ---
                float bendFactor = Mathf.Clamp01(totalBend / 80f);
                float swayWeight = 1f - bendFactor; // примятые не качаются

                // ---- 4. Ветровое качание (подавляется у примятых) --------
                float sway = Mathf.Sin(time * wind.Frequency + data.SwayPhase[i])
                           * data.SwayAmplitude[i]
                           * wind.Strength
                           * data.WindInfluence[i]
                           * swayWeight;

                float turb = Mathf.Sin(time * wind.Frequency * 2.39f + data.SwayPhase[i] * 1.7f)
                           * wind.Turbulence
                           * data.WindInfluence[i]
                           * swayWeight;

                float windAngle = (sway + turb) * 30f;

                // ---- 5. Итоговый поворот ---------------------------------
                Quaternion baseRot = Quaternion.Euler(0f, data.RotationsY[i], 0f);
                Quaternion windRot = Quaternion.AngleAxis(windAngle, windCrossAxis);
                Quaternion bendRot = totalBend > 0.01f
                    ? Quaternion.AngleAxis(totalBend, totalAxis)
                    : Quaternion.identity;

                data.Matrices[i] = Matrix4x4.TRS(
                    data.Positions[i],
                    windRot * bendRot * baseRot,
                    new Vector3(1f, _grassHeight, 1f));

                // ---- 6. Цвет: плавный lerp между нормальным и тёмным ----
                //data.Colors[i] = Vector4.Lerp(vNormal, vBent, bendFactor);
            }
        }
    }
}
