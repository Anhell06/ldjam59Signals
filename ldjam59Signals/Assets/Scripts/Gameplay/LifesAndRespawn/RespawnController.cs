using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;


[Serializable]
public class LifesCountToRespawnPoint
{
    public int LifesCount;
    public Transform RespawnPoint;
    public GameObject[] DisableOnRespawn;
}
public class RespawnController : MonoBehaviour
{
    [SerializeField] private List<LifesCountToRespawnPoint> _lifesCountToRespawnPoint;
    [SerializeField] private Transform _defaultRespawnPoint;
    public void Reset()
    {
        foreach (var item in _lifesCountToRespawnPoint)
        {
            SetActiveDisableOnRespawn(item.DisableOnRespawn, true);
        }
    }

    public T FirstSpawn<T>(T template) where T : MonoBehaviour
    {
        return Instantiate(template, _defaultRespawnPoint.position, _defaultRespawnPoint.rotation);
    }

    public void Respawn(Transform transformRespawn, int currentLifesCount = -1)
    {
        var respawnPointData = _lifesCountToRespawnPoint.FirstOrDefault(x=>x.LifesCount == currentLifesCount);

        if (respawnPointData == null)
        {
            transformRespawn.position = _defaultRespawnPoint.position;
            transformRespawn.rotation = _defaultRespawnPoint.rotation;
            return;
        }

        SetActiveDisableOnRespawn(respawnPointData.DisableOnRespawn, false);

        transformRespawn.position = respawnPointData.RespawnPoint.position;
        transformRespawn.rotation = respawnPointData.RespawnPoint.rotation;
    }

    private void SetActiveDisableOnRespawn(GameObject[] disableOnRespawnArray, bool isActive)
    {
        foreach(var disableOnRespawn in disableOnRespawnArray)
            disableOnRespawn.SetActive(isActive);
    }
}
