using UnityEngine;
using UnityEngine.Serialization;

public class LifesController : MonoBehaviour
{
    [SerializeField] private int _maxHealsCount = 3;
    [SerializeField] private RespawnController _respawnController;
    [SerializeField] private FirstPersonController _playerTemplate;

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

    public void Die()
    {
        _currentLifesCount--;

        if (_currentLifesCount > 0)
        {
            PlayerController.MovementBlocked = true;
            _respawnController.Respawn(_playerTransform, _currentLifesCount);
            PlayerController.MovementBlocked = false;
            return;
        }

        //Note: Loose here

        _currentLifesCount = _maxHealsCount;
    }
}