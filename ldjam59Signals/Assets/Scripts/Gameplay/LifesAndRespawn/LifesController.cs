using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class LifesController : MonoBehaviour
{
    [SerializeField] private int _maxHealsCount = 3;
    [SerializeField] private RespawnController _respawnController;
    [SerializeField] private FirstPersonController _playerTemplate;
    [SerializeField] private GameObject _showWhileRespawn;
    [SerializeField] private GameObject _looseScreen;
    [SerializeField] private GameObject _winScreen;
    [SerializeField] private AudioSource _deathSound;
    [SerializeField] private float _respawnTime = 3f;
    public static LifesController Instance;

    private bool _dieInProgress;

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
        if (_dieInProgress)
            return;

        _dieInProgress = true;
        _currentLifesCount--;

        PlayerController.SetControllerEnabled(false);
        _deathSound.Play();
        try
        {
            if (_currentLifesCount >= 0)
            {
                _showWhileRespawn.gameObject.SetActive(true);
                await Task.Delay(Mathf.RoundToInt(_respawnTime * 1000));
                _respawnController.Respawn(_playerTransform, _currentLifesCount);
                _showWhileRespawn.gameObject.SetActive(false);
                return;
            }
            else
            {
                _looseScreen.gameObject.SetActive(true);
                await Task.Delay(Mathf.RoundToInt(_respawnTime * 1000));
                SceneManager.LoadScene("MainMenu");
                return;
            }
        }
        finally
        {
            PlayerController.SetControllerEnabled(true);
            _dieInProgress = false;
        }
    }

    internal async void ShowWinScreen()
    {
        PlayerController.SetControllerEnabled(false);
        _winScreen.gameObject.SetActive(true);
        await Task.Delay(Mathf.RoundToInt(_respawnTime * 1000));
        SceneManager.LoadScene("MainMenu");
    }
}