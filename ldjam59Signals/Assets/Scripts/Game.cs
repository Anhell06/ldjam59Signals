
using UnityEngine;

public class Game : MonoBehaviour
{
    public static Game Instance;
    public FirstPersonController FirstPersonController;
    public HandItemManager HandItemManager;
    public TexturePainter TexturePainter;

    private void Awake()
    {
        Instance = this;
        FirstPersonController = FindAnyObjectByType<FirstPersonController>();
        HandItemManager = FindAnyObjectByType<HandItemManager>();
        TexturePainter = FindAnyObjectByType<TexturePainter>();
    }
}
