using UnityEngine;

public class LifesController : MonoBehaviour
{
    [SerializeField] private int _maxHealsCount = 3;
    [SerializeField] private RespawnController _respawnController;
    [SerializeField] private FirstPersonController _playerTemplate;

    public static LifesController Instance;

    private int _currentLifesCount;
    private Transform _playerTransform;
    private FirstPersonController _playerController;

    private void Awake()
    {
        Instance = this;
    }

    public void SpawnPlayerOnGameStart()
    {
        _currentLifesCount = _maxHealsCount;
        _playerController = _respawnController.FirstSpawn<FirstPersonController>(_playerTemplate);
        _playerTransform = _playerController.transform;
    }

    public void Die()
    {
        _currentLifesCount--;

        if (_currentLifesCount > 0)
        {
            _playerController.MovementBlocked = true;
            _respawnController.Respawn(_playerTransform, _currentLifesCount);
            _playerController.MovementBlocked = false;
            return;
        }

        //Note: Loose here

        _currentLifesCount = _maxHealsCount;
    }
}