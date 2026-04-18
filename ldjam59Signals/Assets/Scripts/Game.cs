
using UnityEngine;

public class Game : MonoBehaviour
{
    public static Game Instance;
    public FirstPersonController FirstPersonController;
    public HandItemManager HandItemManager;
    public TexturePainter TexturePainter;

    public Texture2D targetTexture;
    public Texture2D currentTexture => TexturePainter.Texture;

    private void Awake()
    {
        Instance = this;
        FirstPersonController = FindAnyObjectByType<FirstPersonController>();
        HandItemManager = FindAnyObjectByType<HandItemManager>();
        TexturePainter = FindAnyObjectByType<TexturePainter>();
    }
}
