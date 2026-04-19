using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour
{
    public static Game Instance;

    public FirstPersonController PlayerPrefab;
    public string CornFieldScene;
    public string GameplayScene;
    public string ArtScene;

    public FirstPersonController FirstPersonController =>  LifesController.Instance.PlayerController;
    public HandItemManager HandItemManager;
    public TexturePainter TexturePainter;
    public MapCheckView MapCheckView;

    public Texture2D targetTexture;
    public Texture2D currentTexture => TexturePainter.Texture;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
#if UNITY_EDITOR
        Application.targetFrameRate = -1;
#endif
        Instance = this;

        SceneManager.sceneLoaded += OnSceneLoaded;

        SceneManager.LoadScene(ArtScene, LoadSceneMode.Additive);
        SceneManager.LoadScene(CornFieldScene, LoadSceneMode.Additive);
        SceneManager.LoadScene(GameplayScene, LoadSceneMode.Additive);

        //FirstPersonController = Instantiate(PlayerPrefab);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        HandItemManager ??= FindAnyObjectByType<HandItemManager>();
        TexturePainter ??= FindAnyObjectByType<TexturePainter>();
        MapCheckView ??= FindAnyObjectByType<MapCheckView>();

        if(LifesController.Instance != null)
            LifesController.Instance.SpawnPlayerOnGameStart();
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            LifesController.Instance.Die();
    }
#endif
}
