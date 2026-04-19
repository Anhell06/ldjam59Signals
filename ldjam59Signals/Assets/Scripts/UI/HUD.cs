using UnityEngine;

public class HUD : MonoBehaviour
{
    public GameObject hint;
    void Update()
    {
        hint.gameObject.SetActive((Vector3.Distance(Game.Instance.FirstPersonController.transform.position, new Vector3(85,0,-20))) < 20);
    }
}
