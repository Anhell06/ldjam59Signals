using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class CowController : MonoBehaviour
{
    [Serializable]
    private class CowPath
    {
        public Transform pointA;
        public Transform pointB;
    }

    [SerializeField] private CowEnemy _cow;
    [SerializeField] private List<CowPath> _paths;

    [SerializeField] private float _minTimeSec;
    [SerializeField] private float _maxTimeSec;

    private CowPath _path;

    private IEnumerator Start()
    {
        _cow.gameObject.SetActive(false);
        yield return new WaitForSeconds(Random.Range(_minTimeSec, _maxTimeSec));
        _cow.gameObject.SetActive(true);
        var path = _paths[Random.Range(0, _paths.Count)];
        yield return null;
        _cow.transform.position = path.pointA.position;
        yield return null;
        _cow.Agent.enabled = true;
        _cow.Agent.isStopped = false;
        yield return null;
        _cow.Agent.SetDestination(path.pointB.position);
        yield return null;
        _cow.Agent.SetDestination(path.pointB.position);
        yield return null;
        _cow.Agent.SetDestination(path.pointB.position);
        yield return null;
        _cow.Agent.SetDestination(path.pointB.position);
        while (true)
        {
            yield return null;
            if (Vector3.Distance(_cow.transform.position, path.pointB.position) < 0.1f)
            {
                _cow.gameObject.SetActive(false);
                yield break;
            }
        }
    }
}
