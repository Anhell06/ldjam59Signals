using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Intro : MonoBehaviour
{
    [SerializeField]
    private List<Image> sprites;

    public float delay = 1f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (var s in sprites)
        {
           s.gameObject.SetActive(false); 
        }
        StartCoroutine(ShowRoutine());
    }
    
    private
    IEnumerator ShowRoutine()
    {
        for (int i = 0; i < sprites.Count; i++)
        {
            sprites[i].gameObject.SetActive(true);
            yield return new WaitForSeconds(delay);
            sprites[i].gameObject.SetActive(false); 
        }
    }
}
