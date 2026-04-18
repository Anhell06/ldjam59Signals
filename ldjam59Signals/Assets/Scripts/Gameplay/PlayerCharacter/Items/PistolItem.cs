using UnityEngine;

public class PistolItem : AbstractItem
{
    private bool primaryActionActive;
    private bool secondaryActionActive;
    
    [SerializeField]
    private Animator leaserBeamAnimator;

    public override void PrimaryAction()
    {
        primaryActionActive = true;
        leaserBeamAnimator.SetBool("Bend", true);
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
