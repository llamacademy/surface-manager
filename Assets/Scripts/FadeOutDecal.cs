using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class FadeOutDecal : PoolableObject
{
    [SerializeField]
    private float VisibileDuration = 3;
    [SerializeField]
    private float FadeDuration = 2;

    private Vector3 InitialScale;

    private void Awake()
    {
        InitialScale = transform.localScale;
    }

    private void OnEnable()
    {
        transform.localScale = InitialScale;
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        yield return new WaitForSeconds(VisibileDuration);

        float time = 0;
        while(time < 1)
        {
            transform.localScale = Vector3.Lerp(InitialScale, Vector3.zero, time);
            time += Time.deltaTime / FadeDuration;
            yield return null;
        }

        gameObject.SetActive(false);
    }
}
