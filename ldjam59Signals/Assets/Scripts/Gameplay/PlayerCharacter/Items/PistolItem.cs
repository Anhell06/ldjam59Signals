using GrassField.CustomECS;
using System.Drawing;
using UnityEngine;
using UnityEngine.Rendering;

public class PistolItem : AbstractItem
{
    private bool primaryActionActive;
    private bool secondaryActionActive;

    public Camera _cumZalupa4;
    private TexturePainter _texturPainter;
    [SerializeField]
    private Animator leaserBeamAnimator;

    public void Start()
    {
        _cumZalupa4 = Camera.main;
        _texturPainter = Game.Instance.TexturePainter;
    }

    public override void PrimaryAction()
    {
        primaryActionActive = true;
        leaserBeamAnimator.SetBool("Bend", true);

        var ray = _cumZalupa4.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out var hit))
        {
            Vector2 uv = hit.textureCoord;
            _texturPainter.Paint(UnityEngine.Color.black, uv);
        }
    }
    private void UpdatePrimaryActionView()
    {
        if (!primaryActionActive)
            return;
    }
    public override void PrimaryActionStop()
    {
        primaryActionActive = false;
        leaserBeamAnimator.SetBool("Bend", false);
    }

    public override void SecondaryAction()
    {
        secondaryActionActive = true;
        leaserBeamAnimator.SetBool("Straighten", true);

        var ray = _cumZalupa4.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out var hit))
        {
            Vector2 uv = hit.textureCoord;
            _texturPainter.Paint(UnityEngine.Color.white, uv);
        }
    }
   
    
    private void UpdateSecondaryActionView()
    {
        if (!secondaryActionActive)
            return;
    }
    
    public override void SecondaryActionStop()
    {
        secondaryActionActive = false;
        leaserBeamAnimator.SetBool("Straighten", false);
    }

    public override void OnUpdate()
    {
        UpdatePrimaryActionView();
        UpdateSecondaryActionView();
    }
}
