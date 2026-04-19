using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

public class LifesController : MonoBehaviour
{
    [SerializeField] private int _maxHealsCount = 3;
    [SerializeField] private RespawnController _respawnController;
    [SerializeField] private FirstPersonController _playerTemplate;
    [SerializeField] private GameObject _showWhileRespawn;
    [SerializeField] private float _respawnTime = 3f;
    public static LifesController Instance;

    private int _currentLifesCount;
    private Transform _playerTransform;
    public FirstPersonController PlayerController { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void SpawnPlayerOnGameStart()
    {
        _currentLifesCount = _maxHealsCount;
        PlayerController = _respawnController.FirstSpawn<FirstPersonController>(_playerTemplate);
        Game.Instance.HandItemManager ??= FindAnyObjectByType<HandItemManager>();
        _playerTransform = PlayerController.transform;
    }

    public async void Die()
    {
        _currentLifesCount--;

        if (_currentLifesCount >= 0)
        {
            PlayerController.SetControllerEnabled(false);
            _showWhileRespawn.gameObject.SetActive(true);
            await Task.Delay(Mathf.RoundToInt(_respawnTime * 1000));
            _respawnController.Respawn(_playerTransform, _currentLifesCount);
            PlayerController.SetControllerEnabled(true);
            _showWhileRespawn.gameObject.SetActive(false);
            return;
        }

        //Note: Loose here

        _currentLifesCount = _maxHealsCount;
    }
}