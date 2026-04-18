using GrassField.CustomECS;
using UnityEngine;

public class TexturePainter : MonoBehaviour
{
    public Camera cam;
    public Color drawColor = Color.black;
    public int brushSize = 8;

    public Texture2D Texture;
    private Renderer rend;

    [SerializeField]
    private GrassWorld grassWorld;

    void Start()
    {
        cam = Camera.main;
        rend = GetComponent<Renderer>();

        // Клонируем текстуру, чтобы можно было менять
        Texture = Instantiate(rend.material.mainTexture) as Texture2D;
        Texture.Apply();

        rend.material.mainTexture = Texture;
    }

    public void Paint(Color color, Vector2 uv)
    {
        int x = (int)(uv.x * Texture.width);
        int y = (int)(uv.y * Texture.height);

        DrawCircle(x, y, brushSize, color);
        Texture.Apply();
        grassWorld.SetMask(Texture);
    }

    void DrawCircle(int cx, int cy, int r, Color color)
    {
        for (int x = -r; x <= r; x++)
        {
            for (int y = -r; y <= r; y++)
            {
                if (x * x + y * y <= r * r)
                {
                    int px = cx + x;
                    int py = cy + y;

                    if (px >= 0 && px < Texture.width && py >= 0 && py < Texture.height)
                    {
                        Texture.SetPixel(px, py, color);
                    }
                }
            }
        }
    }
}