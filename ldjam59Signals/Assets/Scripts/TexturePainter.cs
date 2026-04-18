using GrassField.CustomECS;
using UnityEngine;

public class TexturePainter : MonoBehaviour
{
    public Camera cam;
    public Color drawColor = Color.black;
    public int brushSize = 8;

    private Texture2D texture;
    private Renderer rend;

    [SerializeField]
    private GrassWorld grassWorld;

    void Start()
    {
        cam = Camera.main;
        rend = GetComponent<Renderer>();

        // Клонируем текстуру, чтобы можно было менять
        texture = Instantiate(rend.material.mainTexture) as Texture2D;
        texture.Apply();

        rend.material.mainTexture = texture;
    }

    void Update()
    {
        if (Input.GetMouseButton(0)) // ЛКМ
        {
            Paint(drawColor);
        }

        if (Input.GetMouseButton(1)) // ПКМ
        {
            Paint(Color.white);
        }
    }

    void Paint(Color color)
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.gameObject != gameObject) return;

            Vector2 uv = hit.textureCoord;

            int x = (int)(uv.x * texture.width);
            int y = (int)(uv.y * texture.height);

            DrawCircle(x, y, brushSize, color);
            texture.Apply();
            grassWorld.SetMask(texture);
        }
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

                    if (px >= 0 && px < texture.width && py >= 0 && py < texture.height)
                    {
                        texture.SetPixel(px, py, color);
                    }
                }
            }
        }
    }
}