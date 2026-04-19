using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class MapCheckView : MonoBehaviour
{
    [SerializeField] private Vector2 _itemOffsets;
    [SerializeField] private Vector2Int _fieldSize;
    [SerializeField] private MapCheckElementView _mapCheckElementViewTemplate;
    [SerializeField] private Transform _elementContainer;

    [Header("Animation")]
    [SerializeField] private int _sameTimeCheckablePoints;
    [SerializeField] private Vector2 _minAndMaxCheckDuration;

    [Header("Editor")]
    [SerializeField] private bool _showGizmos;

    private int _viewAnimationsInProgress = 0;
    private List<List<EMapCheckElementState>> _currentStates;
    private List<List<bool>> _animationSequenceSemaphore2dList = new ();
    private List<List<MapCheckElementView>> _mapCheckElementView2dList = new();
    private Queue<MapCheckElementView> _elementViewsPool = new();

    private CancellationTokenSource _cancellationTokenSource = new();
    private List<int> _preAllocatedIndexToCheck = new();

    public Vector2Int FieldSize => _fieldSize;

    public void InitFromBool(bool[,] boolStates2dArray)
    {
        List<List<EMapCheckElementState>> states2dList = new();
        for (int x = 0; x < boolStates2dArray.GetLength(0); x++)
        {
            states2dList.Add(new List<EMapCheckElementState>());

            for (int y = 0; y < boolStates2dArray.GetLength(1); y++)
                states2dList[x].Add(boolStates2dArray[x,y] ? EMapCheckElementState.Positive : EMapCheckElementState.Negative);
        }

        Init(states2dList);
    }

    public void Init(List<List<EMapCheckElementState>> states2dList)
    {
        StopAnimation();
        ResetAllStates();

        _currentStates = states2dList;

        var viewPosition = Vector3.zero;

        for (var x = 0; x < states2dList.Count; x++)
        {
            var statesList = states2dList[x];

            if (_mapCheckElementView2dList.Count <= x)
                _mapCheckElementView2dList.Add(new List<MapCheckElementView>());

            if (_animationSequenceSemaphore2dList.Count <= x)
                _animationSequenceSemaphore2dList.Add(new List<bool>());

            for (var y = 0; y < statesList.Count; y++)
            {
                if (_mapCheckElementView2dList[x].Count <= y)
                    _mapCheckElementView2dList[x].Add(GetOrCreateView());

                if (_animationSequenceSemaphore2dList[x].Count <= y)
                    _animationSequenceSemaphore2dList[x].Add(false);

                var elementToSet = _mapCheckElementView2dList[x][y];
                elementToSet.SetActive(false);

                viewPosition.x = x * _itemOffsets.x / 2f;
                viewPosition.z = y * _itemOffsets.y / 2f;
                elementToSet.transform.position = _elementContainer.TransformPoint(viewPosition);
            }
        }
    }

    public void StopAnimation()
    {
        if(_cancellationTokenSource != null)
            _cancellationTokenSource.Cancel();

        _viewAnimationsInProgress = 0;
        ResetAllStates();
        ResetAnimationSemaphores();
    }

    private void ViewAnimationCallback()
    {
        // Счётчик никогда не должен уходить в минус:
        // StopAnimation() сбрасывает его в 0 раньше, чем некоторые коллбэки успевают сработать.
        if (_viewAnimationsInProgress > 0)
            _viewAnimationsInProgress--;
        StartNewAnimationIsNeeded();
    }

    public void StartNewAnimationIsNeeded()
    {
        if (_cancellationTokenSource.IsCancellationRequested)
        {
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        for (var x = 0; x < _animationSequenceSemaphore2dList.Count; x++)
        {
            var semaphoreRow = _animationSequenceSemaphore2dList[x];
            _preAllocatedIndexToCheck.Clear();

            for (var y = 0; y < semaphoreRow.Count; y++)
            {
                if (!semaphoreRow[y])
                    _preAllocatedIndexToCheck.Add(y);
            }

            if(_preAllocatedIndexToCheck.Count == 0)
                continue;

            while (_preAllocatedIndexToCheck.Count != 0)
            {
                var randomIndex = Random.Range(0, _preAllocatedIndexToCheck.Count);
                var yToPlay = _preAllocatedIndexToCheck[randomIndex];

                // Мутируем состояние ДО вызова SetStateWithDelay:
                // при задержке = 0 Task.Delay(0) завершается синхронно, коллбэк
                // вызывается прямо внутри SetStateWithDelay и рекурсивно обращается
                // к _preAllocatedIndexToCheck — если RemoveAt был бы после, список
                // уже оказался бы изменён и индекс вышел бы за границы.
                semaphoreRow[yToPlay] = true;
                _viewAnimationsInProgress++;
                _preAllocatedIndexToCheck.RemoveAt(randomIndex);

                _mapCheckElementView2dList[x][yToPlay].SetState(EMapCheckElementState.InProgress);
                _mapCheckElementView2dList[x][yToPlay].SetStateWithDelay(_currentStates[x][yToPlay],
                    Mathf.RoundToInt(Random.Range(_minAndMaxCheckDuration.x, _minAndMaxCheckDuration.y) * 1000),
                    _cancellationTokenSource.Token, ViewAnimationCallback);

                if (_viewAnimationsInProgress >= _sameTimeCheckablePoints)
                    return;
            }
        }
    }

    public void SetState(int x, int y, EMapCheckElementState state)
    {
        try
        {
            _mapCheckElementView2dList[x][y].SetState(state);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    public void ResetAllStates()
    {
        foreach (var mapElementViewList in _mapCheckElementView2dList)
        {
            foreach (var mapCheckElementView in mapElementViewList)
            {
                mapCheckElementView.SetActive(false);
            }
        }
    }

    private void ResetAnimationSemaphores()
    {
        for (var x = 0; x < _animationSequenceSemaphore2dList.Count; x++)
        {
            var semaphoreRow = _animationSequenceSemaphore2dList[x];

            for (var y = 0; y < semaphoreRow.Count; y++)
            {
                semaphoreRow[y] = false;
            }
        }
    }

    private void ReturnAllToPool()
    {
        foreach (var mapElementViewList in _mapCheckElementView2dList)
        {
            foreach (var mapCheckElementView in mapElementViewList)
            {
                ReturnToPool(mapCheckElementView);
            }
        }

        _mapCheckElementView2dList.Clear();
    }

    private void ReturnToPool(MapCheckElementView elementToReturn)
    {
        elementToReturn.SetActive(false);
        _elementViewsPool.Enqueue(elementToReturn);
    }

    private MapCheckElementView GetOrCreateView()
    {
        if (_elementViewsPool.TryDequeue(out var mapCheckElementView))
            return mapCheckElementView;

        return Instantiate(_mapCheckElementViewTemplate, _elementContainer);
    }

    private void OnDrawGizmos()
    {
        if (!_showGizmos)
            return;


        if (_mapCheckElementViewTemplate == null || _elementContainer == null)
            return;

        Vector3 cubePosition = Vector3.zero;

        Gizmos.color = new Color(0, 1, 0, 0.1f);
        for (var x = 0; x < _fieldSize.x; x++)
        {
            for (var y = 0; y < _fieldSize.y; y++)
            {
                cubePosition.x = x * _itemOffsets.x / 2f;
                cubePosition.z = y * _itemOffsets.y / 2f;
                Gizmos.DrawCube(_elementContainer.TransformPoint(cubePosition),
                    _mapCheckElementViewTemplate.transform.localScale);
            }
        }
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            var result = new bool[_fieldSize.x, _fieldSize.x];
            TextureComparerResized.Compare(Game.Instance.currentTexture, Game.Instance.targetTexture, result);

            List<List<EMapCheckElementState>> testStates = new();
            for (int x = 0; x < _fieldSize.x; x++)
            {
                testStates.Add(new List<EMapCheckElementState>());

                for (int y = 0; y < _fieldSize.y; y++)
                    testStates[x].Add(result[x, y] ? EMapCheckElementState.Positive : EMapCheckElementState.Negative);
            }

            Init(testStates);
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            StopAnimation();
            ResetAllStates();
            StartNewAnimationIsNeeded();
        }
    }
#endif
}