using System.Collections;
using System.Linq;
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

    private void Start()
    {
        StartCoroutine(CallAEvery10Seconds());
    }

    private IEnumerator CallAEvery10Seconds()
    {
        while (true)
        {
            var fieldSize = Game.Instance.MapCheckView.FieldSize;
            var result = new bool[fieldSize.x, fieldSize.x];
            TextureComparerResized.Compare(Game.Instance.currentTexture, Game.Instance.targetTexture, result);

            Game.Instance.MapCheckView.InitFromBool(result);
            Game.Instance.MapCheckView.StartNewAnimationIsNeeded();

            yield return new WaitForSeconds(3f);

            Game.Instance.MapCheckView.StopAnimation();
            Game.Instance.MapCheckView.ResetAllStates();

            yield return new WaitForSeconds(5f);

            var allresult = true;

            for (int i = 0; i < result.GetLength(0); i++)
            {
                for (int y = 0; y < result.GetLength(1); y++)
                {
                    if(!result[i,y])
                        allresult = false;  
                }
            }

            if (allresult) { LifesController.Instance.ShowWinScreen(); };
        }
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
