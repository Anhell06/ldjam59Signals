using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class TextureToQuad : MonoBehaviour
{
    [Header("Texture Settings")]
    public Texture2D sourceTexture;
    public bool autoFitSize = true;
    public float baseWidth = 1f;
    
    [Header("Material Settings")]
    public Material customMaterial;
    
    private MeshRenderer meshRenderer;
    
    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        
        if (sourceTexture != null)
        {
            // Создаём Quad с правильными пропорциями
            CreateQuadWithTexture();
        }
    }
    
    void CreateQuadWithTexture()
    {
        // Создаём Mesh для Quad
        Mesh mesh = new Mesh();
        
        float textureAspect = (float)sourceTexture.width / (float)sourceTexture.height;
        float width = baseWidth;
        float height = autoFitSize ? width / textureAspect : 1f;
        
        // Вершины Quad'а (прямоугольник, ориентированный на камеру)
        Vector3[] vertices = new Vector3[]
        {
            new Vector3(-width/2, -height/2, 0),
            new Vector3( width/2, -height/2, 0),
            new Vector3(-width/2,  height/2, 0),
            new Vector3( width/2,  height/2, 0)
        };
        
        // Треугольники
        int[] triangles = new int[]
        {
            2, 0, 1,
            3, 2, 1
        };
        
        // UV координаты
        Vector2[] uv = new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
        };
        
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.RecalculateNormals();
        
        GetComponent<MeshFilter>().mesh = mesh;
        
        if (customMaterial != null)
        {
            meshRenderer.material = customMaterial;
            meshRenderer.material.mainTexture = sourceTexture;
        }
        else
        {
            Material material = new Material(Shader.Find("Standard"));
            material.mainTexture = sourceTexture;
            meshRenderer.material = material;
        }
    }
}