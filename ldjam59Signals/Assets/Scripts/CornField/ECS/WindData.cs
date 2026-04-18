using UnityEngine;

namespace GrassField.CustomECS
{
    // Простая value-структура, передаётся в систему по значению — никаких аллокаций.
    public struct WindData
    {
        public Vector3 Direction;   // Нормализованное направление
        public float   Strength;    // Сила [0..∞]
        public float   Turbulence;  // Шум поверх основного качания [0..1]
        public float   Frequency;   // Частота волны (чем больше — тем быстрее)
    }
}
