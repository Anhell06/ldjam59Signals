using UnityEngine;

namespace GrassField.CustomECS
{
    // =====================================================================
    //  GrassInteractor — добавьте на игрока или любой объект,
    //  который должен гнуть траву.
    //
    //  OnEnable/OnDisable автоматически регистрируют в GrassInteractionSystem.
    // =====================================================================
    public sealed class GrassInteractor : MonoBehaviour
    {
        [Range(0.1f, 8f)]  public float Radius = 1.5f;
        [Range(0f,   1f)]  public float Force  = 0.9f;

        // GrassInteractionSystem читает это свойство напрямую
        public Vector3 Position => transform.position;

        void OnEnable()  => GrassInteractionSystem.ActiveInteractors.Add(this);
        void OnDisable() => GrassInteractionSystem.ActiveInteractors.Remove(this);

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.2f, 1f, 0.3f, 0.3f);
            Gizmos.DrawSphere(transform.position, Radius);
            Gizmos.color = new Color(0.2f, 1f, 0.3f, 1f);
            Gizmos.DrawWireSphere(transform.position, Radius);
        }
#endif
    }
}
