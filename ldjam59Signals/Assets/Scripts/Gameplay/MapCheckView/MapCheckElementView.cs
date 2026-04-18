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
}
