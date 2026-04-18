using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MapCheckView : MonoBehaviour
{
    [SerializeField] private Vector2 _itemOffsets;
    [SerializeField] private Vector2Int _fieldSize;
    [SerializeField] private MapCheckElementView _mapCheckElementViewTemplate;
    [SerializeField] private Transform _elementContainer;
    [SerializeField] private bool _showGizmos;

    private List<List<MapCheckElementView>> _mapCheckElementView2dList = new ();
    private Queue<MapCheckElementView> _elementViewsPool = new ();

    public void Init(List<List<EMapCheckElementState>> states2dList)
    {
        ResetAllStates();

        var viewPosition = Vector3.zero;

        for (var x = 0; x < states2dList.Count; x++)
        {
            var statesList = states2dList[x];

            if(_mapCheckElementView2dList.Count <= x)
                _mapCheckElementView2dList.Add(new List<MapCheckElementView>());

            for (var y = 0; y < statesList.Count; y++)
            {
                if(_mapCheckElementView2dList[x].Count <= y)
                    _mapCheckElementView2dList[x].Add(GetOrCreateView());

                var elementToSet = _mapCheckElementView2dList[x][y];
                elementToSet.SetState(states2dList[x][y]);

                viewPosition.x = x * _itemOffsets.x/2f;
                viewPosition.z = y * _itemOffsets.y/2f;
                elementToSet.transform.position = _elementContainer.TransformPoint(viewPosition);
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

    private void ResetAllStates()
    {
        foreach (var mapElementViewList in _mapCheckElementView2dList)
        {
            foreach (var mapCheckElementView in mapElementViewList)
            {
                mapCheckElementView.SetActive(false);
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
        if(_elementViewsPool.TryDequeue(out var mapCheckElementView))
            return mapCheckElementView;

        return Instantiate(_mapCheckElementViewTemplate, _elementContainer);
    }

    private void OnDrawGizmos()
    {
        if (!_showGizmos)
            return;


        if(_mapCheckElementViewTemplate == null || _elementContainer == null)
            return;

        Vector3 cubePosition = Vector3.zero;

        Gizmos.color = new Color(0, 1, 0, 0.1f);
        for (var x = 0; x < _fieldSize.x; x++)
        {
            for (var y = 0; y < _fieldSize.y; y++)
            {
                cubePosition.x = x * _itemOffsets.x/2f;
                cubePosition.z = y * _itemOffsets.y/2f;
                Gizmos.DrawCube(_elementContainer.TransformPoint(cubePosition), _mapCheckElementViewTemplate.transform.localScale);
            }
        }
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            List<List<EMapCheckElementState>> testStates = new();
            for (int x = 0; x < _fieldSize.x; x++)
            {
                testStates.Add(new List<EMapCheckElementState>());

                for (int y = 0; y < _fieldSize.y; y++)
                    testStates[x].Add((EMapCheckElementState)Random.Range(1,4));
            }

            Init(testStates);
        }
    }
#endif

}
