using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class TextureToPlane : MonoBehaviour
{
    [Header("Texture Settings")]
    [Tooltip("Импортированная текстура")]
    public Texture2D sourceTexture;
    
    [Tooltip("Автоматически подгонять размер Plane под пропорции текстуры")]
    public bool autoFitSize = true;
    
    [Tooltip("Базовая ширина Plane (если autoFitSize = true, высота рассчитается автоматически)")]
    public float _baseWidth = 1f;
    public float widthMultiplier = 1f;

    public float baseWidth
    {
        get { return _baseWidth * sourceTexture.width * widthMultiplier; }
    }

    [Tooltip("Фиксированная ширина (если autoFitSize = false)")]
    public float fixedWidth = 1f;
    
    [Tooltip("Фиксированная высота (если autoFitSize = false)")]
    public float fixedHeight = 1f;
    
    [Header("Material Settings")]
    [Tooltip("Создать новый материал автоматически")]
    public bool createNewMaterial = true;
    
    [Tooltip("Основной цвет материала (если текстура не задана)")]
    public Color baseColor = Color.white;
    
    [Tooltip("Режим тайлинга текстуры")]
    public Vector2 tiling = Vector2.one;
    
    [Tooltip("Смещение текстуры")]
    public Vector2 offset = Vector2.zero;
    
    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private Material material;
    
    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();
        
        if (sourceTexture != null)
        {
            ApplyTextureToPlane();
            
            if (autoFitSize)
            {
                FitPlaneSizeToTexture();
            }
        }
        else
        {
            Debug.LogWarning("Texture not assigned to " + gameObject.name + ". Creating plane with default material.");
            CreateDefaultMaterial();
        }
    }
    
    void ApplyTextureToPlane()
    {
        if (createNewMaterial)
        {
            // Создаём новый материал с текстурой
            Shader shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
            
            material = new Material(shader);
            material.mainTexture = sourceTexture;
            material.mainTextureScale = tiling;
            material.mainTextureOffset = offset;
            material.color = baseColor;
            
            meshRenderer.material = material;
        }
        else
        {
            // Используем существующий материал
            if (meshRenderer.material != null)
            {
                meshRenderer.material.mainTexture = sourceTexture;
                meshRenderer.material.mainTextureScale = tiling;
                meshRenderer.material.mainTextureOffset = offset;
            }
            else
            {
                Debug.LogError("No material assigned to MeshRenderer!");
            }
        }
    }
    
    void FitPlaneSizeToTexture()
    {
        if (sourceTexture == null) return;
        
        // Получаем пропорции текстуры
        float textureAspect = (float)sourceTexture.width / (float)sourceTexture.height;
        
        // Plane по умолчанию имеет размер 10x10 единиц (в локальных координатах)
        // Изменяем transform.localScale для нужных пропорций
        
        Vector3 newScale = transform.localScale;
        
        // Устанавливаем ширину как baseWidth, высоту рассчитываем по пропорциям
        newScale.x = baseWidth;
        newScale.z = baseWidth / textureAspect; // Для Plane ось Z - это высота в 3D пространстве
        
        // Если Plane повёрнут, можно использовать другой подход
        // Для Plane, который смотрит на камеру (Billboard), используйте ось Y
        if (transform.rotation.eulerAngles.x != 0 || transform.rotation.eulerAngles.z != 0)
        {
            newScale.y = baseWidth / textureAspect;
            newScale.x = baseWidth;
        }
        
        transform.localScale = newScale;
        
        //Debug.Log($"Plane resized. Texture: {sourceTexture.width}x{sourceTexture.height}, " +
                  //$"Aspect: {textureAspect:F2}, New Size: {newScale.x} x {newScale.z}");
    }
    
    void CreateDefaultMaterial()
    {
        if (createNewMaterial)
        {
            Shader shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
            
            material = new Material(shader);
            material.color = baseColor;
            meshRenderer.material = material;
        }
    }
    
    // Публичный метод для обновления текстуры в рантайме
    public void UpdateTexture(Texture2D newTexture)
    {
        sourceTexture = newTexture;
        
        if (sourceTexture != null)
        {
            ApplyTextureToPlane();
            
            if (autoFitSize)
            {
                FitPlaneSizeToTexture();
            }
        }
    }
    
    // Публичный метод для ручной установки размера
    public void SetPlaneSize(float width, float height)
    {
        autoFitSize = false;
        Vector3 newScale = transform.localScale;
        newScale.x = width;
        
        // Определяем, какую ось используем для высоты в зависимости от поворота
        if (transform.rotation.eulerAngles.x != 0 || transform.rotation.eulerAngles.z != 0)
        {
            newScale.y = height;
        }
        else
        {
            newScale.z = height;
        }
        
        transform.localScale = newScale;
    }
}