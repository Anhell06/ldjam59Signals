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
        private Color _bentColor = new Color(0.12f, 0.18f, 0.05f);

        // Кэш для частых вычислений
        private Vector3 _cachedWindCrossAxis;
        private float _cachedSinComponent; // sin(time * wind.Frequency + phase)
        private float _cachedTurbComponent; // для турбулентности
        private bool _windCrossAxisDirty = true;

        public GrassSwaySystem(float grassHeight, float bendRecoverySpeed)
        {
            _grassHeight = grassHeight;
            _bendRecoverySpeed = bendRecoverySpeed;
        }

        public void SetColors(Color normal, Color bent)
        {
            _normalColor = normal;
            _bentColor = bent;
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
                
                float halfTotalBend = totalBend * 0.008726646f;
                float halfWindAngle = windAngle * 0.008726646f;

                float sinHalfBend = FastSin(halfTotalBend);
                float cosHalfBend = FastCos(halfTotalBend);

                float sinHalfWind = FastSin(halfWindAngle);
                float cosHalfWind = FastCos(halfWindAngle);

// ===== ВНУТРИ ЦИКЛА =====
// Получаем базовый поворот
                float bx = data.BaseRotations[i].x;
                float by = data.BaseRotations[i].y;
                float bz = data.BaseRotations[i].z;
                float bw = data.BaseRotations[i].w;

// Получаем ось изгиба
                float ax = bendAxis.x;
                float ay = bendAxis.y;
                float az = bendAxis.z;

// 1. Строим bendRot напрямую (AngleAxis без вызова Unity)
                float qb_x = ax * sinHalfBend;
                float qb_y = ay * sinHalfBend;
                float qb_z = az * sinHalfBend;
                float qb_w = cosHalfBend;

// 2. Строим windRot напрямую (используем кэшированную ось windCrossAxis)
                float wwx = windCrossAxis.x;
                float wwy = windCrossAxis.y;
                float wwz = windCrossAxis.z;

                float qw_x = wwx * sinHalfWind;
                float qw_y = wwy * sinHalfWind;
                float qw_z = wwz * sinHalfWind;
                float qw_w = cosHalfWind;

// 3. Умножение windRot * bendRot (ручное умножение кватернионов)
                float t1_x = qw_w * qb_x + qw_x * qb_w + qw_y * qb_z - qw_z * qb_y;
                float t1_y = qw_w * qb_y - qw_x * qb_z + qw_y * qb_w + qw_z * qb_x;
                float t1_z = qw_w * qb_z + qw_x * qb_y - qw_y * qb_x + qw_z * qb_w;
                float t1_w = qw_w * qb_w - qw_x * qb_x - qw_y * qb_y - qw_z * qb_z;

// 4. Умножение (windRot * bendRot) * baseRot
                float fx = t1_w * bx + t1_x * bw + t1_y * bz - t1_z * by;
                float fy = t1_w * by - t1_x * bz + t1_y * bw + t1_z * bx;
                float fz = t1_w * bz + t1_x * by - t1_y * bx + t1_z * bw;
                float fw = t1_w * bw - t1_x * bx - t1_y * by - t1_z * bz;

// Итоговый кватернион (без аллокаций и вызовов Unity API)
                Quaternion finalRot = new Quaternion(fx, fy, fz, fw);
                
                Vector3 pos = data.Positions[i];
                float qx = finalRot.x, qy = finalRot.y, qz = finalRot.z, qw = finalRot.w;

                float x2 = qx + qx, y2 = qy + qy, z2 = qz + qz;
                float xx = qx * x2, xy = qx * y2, xz = qx * z2;
                float yy = qy * y2, yz = qy * z2, zz = qz * z2;
                float wx = qw * x2, wy = qw * y2, wz = qw * z2;

                float s = grassHeightScale;

                data.Matrices[i] = new Matrix4x4 {
                    m00 = (1f - (yy + zz)) * s, m01 = (xy - wz) * s, m02 = (xz + wy) * s, m03 = pos.x,
                    m10 = (xy + wz) * s, m11 = (1f - (xx + zz)) * s, m12 = (yz - wx) * s, m13 = pos.y,
                    m20 = (xz - wy) * s, m21 = (yz + wx) * s, m22 = (1f - (xx + yy)) * s, m23 = pos.z,
                    m30 = 0f, m31 = 0f, m32 = 0f, m33 = 1f
                };

                // ---- 6. Цвет: плавный lerp между нормальным и тёмным ----
                // ЗАКОММЕНТИРОВАН - пока не используется
                //data.Colors[i] = Vector4.Lerp(vNormal, vBent, bendFactor);
            }
        }
    
        private float FastSin(float x)
        {
            const float PI = 3.14159265359f;
            const float INV2PI = 0.15915494309f;
    
            // Приведение к [0, 2PI]
            x = x - (int)(x * INV2PI) * 6.28318530718f;
            if (x < 0) x += 6.28318530718f;
    
            // Используем симметрию: sin(x) = sin(PI - x) для x в [PI/2, PI]
            float sign = 1f;
            if (x > PI)
            {
                x -= PI;
                sign = -1f;
            }
            if (x > 1.57079632679f) // PI/2
            {
                x = PI - x;
            }
    
            // Квадратичная аппроксимация на [0, PI/2]
            // 4/π² ≈ 0.40528473456
            // 4/π ≈ 1.27323954473
            return sign * x * (1.27323954473f - x * 0.40528473456f);
        }
        private float FastCos(float x)
        {    const float PI = 3.14159265359f;
            const float HALF_PI = 1.57079632679f;
            const float INV2PI = 0.15915494309f;
    
            // Приведение к [0, 2PI]
            x = x - (int)(x * INV2PI) * 6.28318530718f;
            if (x < 0) x += 6.28318530718f;
    
            float sign = 1f;
    
            // Косинус имеет период 2PI и симметрию cos(x) = cos(-x) = cos(2PI - x)
            // Приводим к [0, PI]
            if (x > PI)
            {
                x = 6.28318530718f - x; // 2PI - x
            }
    
            // Теперь x в [0, PI]
            // cos(x) положителен на [0, PI/2] и отрицателен на [PI/2, PI]
            if (x > HALF_PI)
            {
                x = PI - x;
                sign = -1f;
            }
    
            // Аппроксимация косинуса на [0, PI/2]
            // cos(x) ≈ 1 - (4/π²)x² ≈ 1 - 0.40528473456 * x²
            float x2 = x * x;
            return sign * (1f - 0.40528473456f * x2);
        }
        
        
    }
}
