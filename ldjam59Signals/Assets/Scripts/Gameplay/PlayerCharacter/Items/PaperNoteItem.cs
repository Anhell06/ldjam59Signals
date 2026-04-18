using UnityEngine;

public class PaperNoteItem : AbstractItem
{ 
    private static readonly int MainTex = Shader.PropertyToID("_MainTex");

    public override void PrimaryAction()
    {
        // Бумажка не имеет активного действия.
    }

    public override void SecondaryAction()
    {
        // Дополнительное действие также отсутствует.
    }
}
