using UnityEngine;

public class DroneControlPanelItem : AbstractItem
{

    public override void PrimaryAction()
    {
        var fieldSize = Game.Instance.MapCheckView.FieldSize;
        var result = new bool[fieldSize.x, fieldSize.x];
        TextureComparerResized.Compare(Game.Instance.currentTexture, Game.Instance.targetTexture, result);

        Game.Instance.MapCheckView.InitFromBool(result);
        Game.Instance.MapCheckView.StartNewAnimationIsNeeded();
    }
    public override void PrimaryActionStop()
    {
    }

    public override void SecondaryAction()
    {
    }
    public override void SecondaryActionStop()
    {
    }

    protected override void OnItemHolstered()
    {
        Game.Instance.MapCheckView.StopAnimation();
    }
}
