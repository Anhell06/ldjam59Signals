using System;
using System.Collections;
using System.Threading;
using UnityEngine;

public enum EMapCheckElementState
{
    None,
    InProgress,
    Negative,
    Positive,
}

public class MapCheckElementView : MonoBehaviour
{
    private Coroutine _setStateDelayRoutine;

    [SerializeField] private GameObject _neutralStateGameObject;
    [SerializeField] private GameObject _positiveStateGameObject;
    [SerializeField] private GameObject _negativeStateGameObject;

    public void SetActive(bool isActive)
    {
        _negativeStateGameObject.SetActive(isActive);
        _positiveStateGameObject.SetActive(isActive);
        _neutralStateGameObject.SetActive(isActive);
    }

    public void SetState(EMapCheckElementState state)
    {
        SetActive(false);

        switch (state)
        {
            case EMapCheckElementState.Positive:
                _positiveStateGameObject.SetActive(true);
                break;
            case EMapCheckElementState.Negative:
                _negativeStateGameObject.SetActive(true);
                break;
            case EMapCheckElementState.InProgress:
                _neutralStateGameObject.SetActive(true);
                break;
        }
    }

    public void SetStateWithDelay(EMapCheckElementState state, int delayMs, CancellationToken cancellationToken, Action onComplete = null)
    {
        if (_setStateDelayRoutine != null)
        {
            StopCoroutine(_setStateDelayRoutine);
            _setStateDelayRoutine = null;
        }

        if (delayMs <= 0)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            SetState(state);
            onComplete?.Invoke();
            return;
        }

        _setStateDelayRoutine = StartCoroutine(SetStateWithDelayRoutine(state, delayMs / 1000f, cancellationToken, onComplete));
    }

    private IEnumerator SetStateWithDelayRoutine(EMapCheckElementState state, float delaySec, CancellationToken cancellationToken, Action onComplete)
    {
        float end = Time.realtimeSinceStartup + delaySec;
        while (Time.realtimeSinceStartup < end)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _setStateDelayRoutine = null;
                yield break;
            }

            yield return null;
        }

        if (cancellationToken.IsCancellationRequested)
        {
            _setStateDelayRoutine = null;
            yield break;
        }

        SetState(state);
        onComplete?.Invoke();
        _setStateDelayRoutine = null;
    }
}
