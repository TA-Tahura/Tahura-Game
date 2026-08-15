using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshPro))]
public class FadeOutTMP : MonoBehaviour
{
    [SerializeField] private float waitTime = 10f;
    [SerializeField] private float fadeDuration = 1.5f;

    private TextMeshPro _tmp;

    private void Awake()
    {
        _tmp = GetComponent<TextMeshPro>();
    }

    private void Start()
    {
        StartCoroutine(WaitAndFadeRoutine());
    }

    private IEnumerator WaitAndFadeRoutine()
    {
        yield return new WaitForSeconds(waitTime);

        float startAlpha = _tmp.alpha;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            _tmp.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeDuration);
            yield return null;
        }

        _tmp.alpha = 0f;
        Destroy(gameObject);
    }
}