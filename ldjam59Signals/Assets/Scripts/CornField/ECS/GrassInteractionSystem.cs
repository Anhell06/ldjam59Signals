using UnityEngine;

namespace GrassField.CustomECS
{
    // =====================================================================
    //  GrassInteractionSystem
    //
    //  Читает позиции «интеракторов» (игрок, снаряды и т.д.)
    //  и записывает BendAngle + BendAxis для стеблей в радиусе.
    //
    //  Запускается ДО GrassSwaySystem, чтобы та уже видела BendAngle.
    // =====================================================================
    public sealed class GrassInteractionSystem
    {
        // Статический список всех активных интеракторов в сцене.
        // GrassInteractor.OnEnable/OnDisable управляют списком.
        public static readonly System.Collections.Generic.List<GrassInteractor> 
            ActiveInteractors = new();

        public void Execute(GrassComponents data)
        {
            if (ActiveInteractors.Count == 0) return;

            int count = data.Count;

            for (int i = 0; i < count; i++)
            {
                Vector3 grassPos = data.Positions[i];

                foreach (var interactor in ActiveInteractors)
                {
                    Vector3 delta = grassPos - interactor.Position;
                    // Используем только XZ-плоскость: высота стебля не влияет на расстояние
                    delta.y = 0f;
                    float sqrDist = delta.sqrMagnitude;
                    float sqrRad  = interactor.Radius * interactor.Radius;

                    if (sqrDist < sqrRad && sqrDist > 0.0001f)
                    {
                        float dist    = Mathf.Sqrt(sqrDist);
                        float falloff = 1f - (dist / interactor.Radius); // 1 в центре, 0 на краю
                        float angle   = interactor.Force * falloff * 60f; // до 60°

                        // Берём максимальный изгиб если интеракторов несколько
                        if (angle > data.BendAngle[i])
                        {
                            data.BendAngle[i] = angle;
                            data.BendAxis[i]  = Vector3.Cross(
                                delta.normalized, Vector3.up).normalized;
                        }
                    }
                }
            }
        }
    }
}
