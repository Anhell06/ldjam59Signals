using System;
using System.Collections;
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

    public void Die()
    {
        if (_dieInProgress)
            return;

        _dieInProgress = true;
        _currentLifesCount--;

        PlayerController.SetControllerEnabled(false);
        _deathSound.Play();
        StartCoroutine(DieRoutine());
    }

    private IEnumerator DieRoutine()
    {
        try
        {
            if (_currentLifesCount >= 0)
            {
                _showWhileRespawn.gameObject.SetActive(true);
                yield return new WaitForSecondsRealtime(_respawnTime);
                _respawnController.Respawn(_playerTransform, _currentLifesCount);
                _showWhileRespawn.gameObject.SetActive(false);
            }
            else
            {
                _looseScreen.gameObject.SetActive(true);
                yield return new WaitForSecondsRealtime(_respawnTime);
                SceneManager.LoadScene("MainMenu");
            }
        }
        finally
        {
            PlayerController.SetControllerEnabled(true);
            _dieInProgress = false;
        }
    }

    internal void ShowWinScreen()
    {
        StartCoroutine(ShowWinScreenRoutine());
    }

    private IEnumerator ShowWinScreenRoutine()
    {
        PlayerController.SetControllerEnabled(false);
        _winScreen.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(_respawnTime);
        SceneManager.LoadScene("MainMenu");
    }
}