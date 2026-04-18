using UnityEngine;

namespace GrassField.CustomECS
{
    // =====================================================================
    //  GrassRenderSystem
    //
    //  Передаёт матрицы и цвета в GPU за один поток батчей.
    //  Цвет каждого стебля задаётся через SetVectorArray("_BaseColor")
    //  на общем MaterialPropertyBlock — один DrawMeshInstanced на батч,
    //  никаких «кусков» разного цвета.
    // =====================================================================
    public sealed class GrassRenderSystem
    {
        private const int BatchSize = 1023;

        private readonly Mesh                _mesh;
        private readonly Material            _material;
        private readonly int                 _layer;
        private readonly Matrix4x4[]         _matrixBatch = new Matrix4x4[BatchSize];
        private readonly Vector4[]           _colorBatch  = new Vector4[BatchSize];
        private readonly MaterialPropertyBlock _mpb        = new MaterialPropertyBlock();

        private static readonly int ColorArrayID = Shader.PropertyToID("_BaseColor");

        public GrassRenderSystem(Mesh mesh, Material material, int layer = 0)
        {
            _mesh     = mesh;
            _material = material;
            _layer    = layer;
        }

        public void Execute(GrassComponents data)
        {
            int total     = data.Count;
            int processed = 0;

            while (processed < total)
            {
                int batchCount = Mathf.Min(BatchSize, total - processed);

                System.Array.Copy(data.Matrices, processed, _matrixBatch, 0, batchCount);
                System.Array.Copy(data.Colors,   processed, _colorBatch,  0, batchCount);

                // SetVectorArray передаёт цвет каждого экземпляра отдельно —
                // GPU получает ровно те цвета, которые посчитал SwaySystem.
                _mpb.SetVectorArray(ColorArrayID, _colorBatch);

                Graphics.DrawMeshInstanced(
                    _mesh, 0, _material,
                    _matrixBatch, batchCount,
                    _mpb,
                    UnityEngine.Rendering.ShadowCastingMode.On,
                    receiveShadows: true,
                    _layer);

                processed += batchCount;
            }
        }
    }
}
