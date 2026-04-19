using System;
using UnityEngine;
using UnityEngine.AI;

public class CowEnemy : MonoBehaviour
{
    [SerializeField]
    private Billboard billboard;

    [SerializeField] 
    private NavMeshAgent agent;
    public NavMeshAgent Agent => agent;

    private TexturePainter _texturPainter;

    private Vector3 prevPosition;

    private void Start()
    {
        _texturPainter = Game.Instance.TexturePainter;
        
    }

    private void Update()
    {
        var ray = new Ray(transform.position, Vector3.down);
;
        if (Physics.Raycast(ray, out var hit, 100, LayerMask.GetMask("Cornfield")))
        {
            Vector2 uv = hit.textureCoord;
            _texturPainter.Paint(UnityEngine.Color.white, uv);
        }
        var position = transform.position;
        billboard.SetMovingDirection((position - prevPosition));
        prevPosition = position;
    }
}
