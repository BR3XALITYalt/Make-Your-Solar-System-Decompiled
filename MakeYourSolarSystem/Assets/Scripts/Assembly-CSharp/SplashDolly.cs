using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SplashDolly : MonoBehaviour
{
    [Header("References")]
    public RectTransform imageRect;   // your splash image RectTransform
    public CanvasGroup canvasGroup;   // for fade in/out

    [Header("Settings")]
    public float duration = 3f;           // total splash duration
    public float zoomAmount = 0.04f;      // how much to zoom (0.08 = 8%)
    public float fadeInDuration = 0.4f;
    public float fadeOutDuration = 0.4f;

    private Vector3 startScale;
    private Vector3 endScale;

    void Start()
    {
        startScale = Vector3.one;
        endScale = Vector3.one * (1f + zoomAmount);
        imageRect.localScale = startScale;
        canvasGroup.alpha = 0f;
        StartCoroutine(PlaySplash());
    }

    IEnumerator PlaySplash()
    {
        float elapsed = 0f;

        // Fade in
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeInDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
        elapsed = 0f;

        // Dolly zoom during full display
        float holdDuration = duration - fadeInDuration - fadeOutDuration;
        while (elapsed < holdDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / holdDuration;
            imageRect.localScale = Vector3.Lerp(startScale, endScale, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        elapsed = 0f;

        // Fade out
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / fadeOutDuration);
            imageRect.localScale = Vector3.Lerp(startScale, endScale, 1f);
            yield return null;
        }

        SceneManager.LoadScene("Main");
    }
}