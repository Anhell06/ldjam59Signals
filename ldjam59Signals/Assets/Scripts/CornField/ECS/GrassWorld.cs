using UnityEngine;

namespace GrassField.CustomECS
{
    public sealed class GrassWorld : MonoBehaviour
    {
        [Header("Поле")]
        [SerializeField] private int   fieldWidth   = 60;
        [SerializeField] private int   fieldHeight  = 60;
        [SerializeField] private float spacing      = 0.4f;
        [SerializeField] private float jitter       = 0.1f;

        [Header("Стебли")]
        [SerializeField] private float minHeight         = 0.4f;
        [SerializeField] private float maxHeight         = 1.0f;
        [SerializeField] private float bendRecoverySpeed = 3f;

        [Header("Рендер")]
        [SerializeField] private Mesh     grassMesh;
        [SerializeField] private Material grassMaterial;
        [SerializeField] private int      renderLayer = 0;

        [Header("Цвета")]
        [SerializeField] private Color normalColor = new Color(0.30f, 0.50f, 0.15f);
        [SerializeField] private Color bentColor   = new Color(0.12f, 0.18f, 0.05f);

        [Header("Ветер")]
        [SerializeField] private Vector3 windDirection  = new Vector3(1f, 0f, 0.3f);
        [SerializeField] [Range(0f, 3f)]   private float windStrength   = 0.7f;
        [SerializeField] [Range(0f, 1f)]   private float windTurbulence = 0.25f;
        [SerializeField] [Range(0.1f, 4f)] private float windFrequency  = 1.2f;

        [Header("Маска приминания")]
        [Tooltip("Текстура: белый = примят, чёрный = стоит. Read/Write Enabled обязателен.")]
        [SerializeField] private Texture2D maskTexture;
        [SerializeField] private Vector2   maskCenter        = Vector2.zero;
        [SerializeField] private Vector2   maskSize          = new Vector2(20f, 20f);
        [SerializeField] [Range(0f, 90f)] private float maskMaxBendAngle = 80f;
        [SerializeField] private Vector3   maskBendDirection = Vector3.forward;

        [Header("Оптимизация")]
        [Tooltip("На сколько кадров размазать обновление качания. 1 = обновлять всё каждый кадр.")]
        [SerializeField] [Range(1, 8)] private int swayChunks = 4;

        // ---- Приватные поля -----------------------------------------
        private GrassComponents        _components;
        private GrassInteractionSystem _interactionSystem;
        private GrassMaskSystem        _maskSystem;
        private GrassSwaySystem        _swaySystem;
        private GrassRenderSystem      _renderSystem;
        private WindData               _wind;

        private int _currentSwayChunk;

        void Awake()
        {
            int total = fieldWidth * fieldHeight;
            _components = new GrassComponents(total);

            var   rng = new System.Random(42);
            float ox  = transform.position.x - fieldWidth  * spacing * 0.5f;
            float oy  = transform.position.y;
            float oz  = transform.position.z - fieldHeight * spacing * 0.5f;

            for (int x = 0; x < fieldWidth; x++)
            for (int z = 0; z < fieldHeight; z++)
            {
                int i = x * fieldHeight + z;
                _components.Positions[i]     = new Vector3(
                    ox + x * spacing + (float)(rng.NextDouble()*2-1)*jitter, oy,
                    oz + z * spacing + (float)(rng.NextDouble()*2-1)*jitter);
                _components.RotationsY[i]    = (float)(rng.NextDouble() * 360.0);
                _components.SwayPhase[i]     = (float)(rng.NextDouble() * Mathf.PI * 2f);
                _components.SwayAmplitude[i] = Lerp(0.3f, 1f, (float)rng.NextDouble());
                _components.WindInfluence[i] = Lerp(0.5f, 1f, (float)rng.NextDouble());
                _components.Colors[i]        = (Vector4)normalColor;

                float h = Lerp(minHeight, maxHeight, (float)rng.NextDouble());
                _components.Matrices[i] = Matrix4x4.TRS(
                    _components.Positions[i],
                    Quaternion.Euler(0, _components.RotationsY[i], 0),
                    new Vector3(h, h, h));
            }

            // Предвычисляем константы, которые не меняются за всё время жизни поля
            for (int i = 0; i < total; i++)
            {
                float phase = _components.SwayPhase[i];
                _components.BaseRotations[i] = Quaternion.Euler(0f, _components.RotationsY[i], 0f);
                _components.CosSwayPhase[i]  = Mathf.Cos(phase);
                _components.SinSwayPhase[i]  = Mathf.Sin(phase);
                _components.CosTurbPhase[i]  = Mathf.Cos(phase * 1.7f);
            }

            float avg          = (minHeight + maxHeight) * 0.5f;
            _interactionSystem = new GrassInteractionSystem();
            _maskSystem        = new GrassMaskSystem(maskMaxBendAngle);
            _swaySystem        = new GrassSwaySystem(avg, bendRecoverySpeed);
            _renderSystem      = new GrassRenderSystem(grassMesh, grassMaterial, renderLayer);

            _swaySystem.SetColors(normalColor, bentColor);
            UpdateWindData();
            ApplyMask();

            Debug.Log($"[GrassWorld] {total} стеблей.");
        }

        void Update()
        {
            // Маска запечена — в Update не вызываем.
            // Только интерактор → качание → рендер.
            _interactionSystem.Execute(_components);

            // Time-sliced sway: обновляем только 1/swayChunks стеблей за кадр.
            // dt масштабируется на количество чанков, чтобы восстановление
            // изгиба происходило с той же визуальной скоростью.
            int total      = _components.Count;
            int chunkSize  = (total + swayChunks - 1) / swayChunks; // ceiling division
            int start      = _currentSwayChunk * chunkSize;
            int end        = Mathf.Min(start + chunkSize, total);
            float scaledDt = Time.deltaTime * swayChunks;

            _swaySystem.Execute(_components, _wind, Time.time, scaledDt, start, end);

            _currentSwayChunk = (_currentSwayChunk + 1) % swayChunks;

            _renderSystem.Execute(_components);
        }

        // ---- Публичное API -------------------------------------------

        public void SetMask(Texture2D texture)
        {
            maskTexture = texture;
            ApplyMask();
        }

        public void SetMaskBounds(Vector2 center, Vector2 size)
        {
            maskCenter = center;
            maskSize   = size;
            ApplyMask();
        }

        public void SetMaskBendDirection(Vector3 direction)
        {
            maskBendDirection = direction;
            _maskSystem.SetBendDirection(direction);
            ApplyMask();
        }

        public void SetWind(Vector3 direction, float strength)
        {
            windDirection = direction;
            windStrength  = strength;
            UpdateWindData();
        }

        // ---- Приватные методы ----------------------------------------

        private void ApplyMask()
        {
            if (_maskSystem == null || _components == null) return;
            var rect = new Rect(
                maskCenter.x - maskSize.x * 0.5f,
                maskCenter.y - maskSize.y * 0.5f,
                maskSize.x, maskSize.y);
            _maskSystem.SetBendDirection(maskBendDirection);
            _maskSystem.SetMask(_components, maskTexture, rect);
        }

        private void UpdateWindData()
        {
            _wind = new WindData
            {
                Direction   = windDirection.normalized,
                Strength    = windStrength,
                Turbulence  = windTurbulence,
                Frequency   = windFrequency,
            };
        }

        private void OnValidate()
        {
            ApplyMask();
            UpdateWindData();
            //if (_swaySystem != null) _swaySystem.SetColors(normalColor, bentColor);
        }

        private static float Lerp(float a, float b, float t) => a + (b - a) * t;

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.85f, 0.1f, 0.3f);
            Vector3 c = new Vector3(maskCenter.x, transform.position.y, maskCenter.y);
            Gizmos.DrawCube(c, new Vector3(maskSize.x, 0.05f, maskSize.y));
            Gizmos.color = new Color(1f, 0.85f, 0.1f, 1f);
            Gizmos.DrawWireCube(c, new Vector3(maskSize.x, 0.05f, maskSize.y));
        }
#endif
    }
}
