using System.Collections.Generic;
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

        private Dictionary<int, int> _hashSet = new();

        public GrassInteractionSystem()
        {
        }

        public void Execute(GrassComponents data, int start, int end)
        {
            if (ActiveInteractors.Count == 0) return;

            int count = data.Count;

            foreach (var interactor in ActiveInteractors)
            {
                var interactorPosition = interactor.Position;
                for (int i = start; i < end; i++)
                {
                    Vector3 grassPos = data.Positions[i];
                    Vector3 delta = new Vector3(grassPos.x - interactorPosition.x, 0, grassPos.z - interactorPosition.z);
                    // Используем только XZ-плоскость: высота стебля не влияет на расстояние
                    float sqrDist = delta.sqrMagnitude;
                    float sqrRad = interactor.SqrRadius;

                    if (sqrDist < sqrRad)
                    {
                        float dist = Mathf.Sqrt(sqrDist);
                        float falloff = 1f - (dist / interactor.Radius); // 1 в центре, 0 на краю
                        float angle = interactor.Force * falloff * 60f; // до 60°

                        // Берём максимальный изгиб если интеракторов несколько
                        if (angle > data.BendAngle[i])
                        {
                            data.BendAngle[i] = angle;
                            data.BendAxis[i] = -Vector3.Cross(
                                delta.normalized, Vector3.up).normalized;
                        }
                    }
                }
            }
        }
    }
}
