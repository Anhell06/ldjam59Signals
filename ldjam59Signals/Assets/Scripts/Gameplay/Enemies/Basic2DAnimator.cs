using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Basic2DAnimator : MonoBehaviour
{
    [SerializeField]
    private List<Texture2D> textures;

    [SerializeField] 
    private TextureToPlane tp;
    
    void OnEnable()
    {
        StartCoroutine(UpdateRoutine());
    }

    private IEnumerator UpdateRoutine()
    {
        var wait = new WaitForSeconds(0.2f);
        var i = 0;
        yield return null;
        while (true)
        {
            i = (i + 1) % textures.Count;
            tp.UpdateTexture(textures[i]);
            yield return wait;
        }
    }
}
