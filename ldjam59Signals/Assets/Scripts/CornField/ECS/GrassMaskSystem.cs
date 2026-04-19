using UnityEngine;

namespace GrassField.CustomECS
{
    // =====================================================================
    //  GrassMaskSystem
    //
    //  Читает текстуру-маску ОДИН РАЗ при SetMask() и сразу записывает
    //  MaskBendAngle/MaskBendAxis в компоненты — больше не трогает их.
    //
    //  Execute() в Update() не нужен: маска статична.
    //  GrassSwaySystem сама суммирует MaskBend + динамический BendAngle.
    // =====================================================================
    public sealed class GrassMaskSystem
    {
        private float _maxBendAngle;
        private Vector3 _bendDirection = Vector3.forward;

        public GrassMaskSystem(float maxBendAngle = 80f)
        {
            _maxBendAngle = maxBendAngle;
        }

        public void SetBendDirection(Vector3 direction)
        {
            _bendDirection = direction.normalized;
            if (_bendDirection == Vector3.zero) _bendDirection = Vector3.forward;
        }

        // ------------------------------------------------------------------
        //  SetMask — вызывается ОДИН РАЗ (или при смене текстуры/bounds).
        //  Сразу «запекает» результат в MaskBendAngle[]/MaskBendAxis[].
        //  После этого GrassSwaySystem просто читает эти значения — никакого
        //  конфликта с Lerp восстановления.
        // ------------------------------------------------------------------
        public void SetMask(GrassComponents data, Texture2D mask, Rect worldBounds)
        {
            // Сброс старой маски
            for (int i = 0; i < data.Count; i++)
            {
                data.MaskBendAngle[i] = 0f;
                data.MaskBendAxis[i]  = Vector3.right;
            }

            var worldBoundsX = worldBounds.x;
            var worldBoundsY = worldBounds.y;

            if (mask == null) return;

            var pixels  = mask.GetPixels32();  // единственная аллокация
            int     texW    = mask.width;
            int     texH    = mask.height;
            float   invW    = 1f / worldBounds.width;
            float   invH    = 1f / worldBounds.height;

            Vector3 axis = Vector3.Cross(_bendDirection, Vector3.up).normalized;
            if (axis == Vector3.zero) axis = Vector3.right;

            for (int i = 0; i < data.Count; i++)
            {
                Vector3 pos = data.Positions[i];
                float u = (pos.x - worldBoundsX) * invW;
                float v = (pos.z - worldBoundsY) * invH;

                if (u < 0f || u > 1f || v < 0f || v > 1f) continue;

                int px = (int)(u * texW);
                int py = (int)(v * texH);

                if (py * texW + px >= pixels.Length) continue;
                float brightness = pixels[py * texW + px].r;
                if (brightness < 0.01f) continue;

                data.MaskBendAngle[i] = brightness * _maxBendAngle;
                data.MaskBendAxis[i]  = axis;
            }
        }
    }
}
