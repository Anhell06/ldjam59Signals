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

    [SerializeField] private Light greenLight;
    [SerializeField] private Light yellowLight;

    [SerializeField] private float pistolDist = 5f;
    public void Start()
    {
        _cumZalupa4 = Camera.main;
        _texturPainter = Game.Instance.TexturePainter;
        greenLight.gameObject.SetActive(false);
        yellowLight.gameObject.SetActive(false);
    }

    public override void PrimaryAction()
    {
        primaryActionActive = true;
        leaserBeamAnimator.SetBool("Bend", true);
        greenLight.gameObject.SetActive(true);
    }
    
    private void UpdatePrimaryActionView()
    {
        if (!primaryActionActive)
            return;
        
        var ray = _cumZalupa4.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out var hit,  pistolDist, LayerMask.GetMask("Cornfield")))
        {
            Vector2 uv = hit.textureCoord;
            _texturPainter.Paint(UnityEngine.Color.white, uv);
        }
    }
    public override void PrimaryActionStop()
    {
        primaryActionActive = false;
        greenLight.gameObject.SetActive(false);
        leaserBeamAnimator.SetBool("Bend", false);
    }

    public override void SecondaryAction()
    {
        secondaryActionActive = true;
        leaserBeamAnimator.SetBool("Straighten", true);
        yellowLight.gameObject.SetActive(true);
    }
   
    
    private void UpdateSecondaryActionView()
    {
        if (!secondaryActionActive)
            return;

        var ray = _cumZalupa4.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out var hit, pistolDist, LayerMask.GetMask("Cornfield")))
        {
            Vector2 uv = hit.textureCoord;
            _texturPainter.Paint(UnityEngine.Color.black, uv);
        }
    }
    
    public override void SecondaryActionStop()
    {
        secondaryActionActive = false;
        yellowLight.gameObject.SetActive(false);
        leaserBeamAnimator.SetBool("Straighten", false);
    }

    public override void OnUpdate()
    {
        UpdatePrimaryActionView();
        UpdateSecondaryActionView();
    }
}
