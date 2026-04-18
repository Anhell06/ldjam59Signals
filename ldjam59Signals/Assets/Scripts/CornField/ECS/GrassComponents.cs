using UnityEngine;

namespace GrassField.CustomECS
{
    public sealed class GrassComponents
    {
        public readonly int Count;

        // --- Transform ---------------------------------------------------
        public readonly Vector3[] Positions;
        public readonly float[]   RotationsY;

        // --- Sway --------------------------------------------------------
        public readonly float[] SwayPhase;
        public readonly float[] SwayAmplitude;
        public readonly float[] WindInfluence;

        // --- Изгиб от интерактора (восстанавливается со временем) --------
        public readonly float[]   BendAngle;
        public readonly Vector3[] BendAxis;

        // --- Изгиб от маски (статичный, НЕ восстанавливается) ------------
        // Устанавливается один раз при SetMask, не трогается в Update.
        public readonly float[]   MaskBendAngle;
        public readonly Vector3[] MaskBendAxis;

        // --- Rendering ---------------------------------------------------
        public readonly Matrix4x4[] Matrices;

        // Цвет каждого стебля — пишет GrassSwaySystem, читает GrassRenderSystem.
        // Хранится как Vector4 (r,g,b,a) — именно такой тип принимает
        // MaterialPropertyBlock.SetVectorArray("_BaseColor", ...)
        public readonly Vector4[] Colors;

        public GrassComponents(int count)
        {
            Count = count;

            Positions      = new Vector3[count];
            RotationsY     = new float[count];
            SwayPhase      = new float[count];
            SwayAmplitude  = new float[count];
            WindInfluence  = new float[count];
            BendAngle      = new float[count];
            BendAxis       = new Vector3[count];
            MaskBendAngle  = new float[count];
            MaskBendAxis   = new Vector3[count];
            Matrices       = new Matrix4x4[count];
            Colors         = new Vector4[count];
        }
    }
}
