using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TransitionScreen : MonoBehaviour
{
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private Color fadeColor = Color.black;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // Start invisible and don't block raycasts
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
    }

    public IEnumerator FadeInAndOut(float holdDuration = 0.5f)
    {
        // Fade in
        yield return StartCoroutine(FadeTo(1f, fadeDuration));

        // Hold
        yield return new WaitForSecondsRealtime(holdDuration);

        // Fade out
        yield return StartCoroutine(FadeTo(0f, fadeDuration));
    }

    private IEnumerator FadeTo(float targetAlpha, float duration)
    {
        // Enable raycast blocking when fading in
        if (targetAlpha > 0.5f)
            canvasGroup.blocksRaycasts = true;

        float elapsedTime = 0f;
        float startAlpha = canvasGroup.alpha;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / duration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;

        // Disable raycast blocking when fully transparent
        if (targetAlpha < 0.5f)
            canvasGroup.blocksRaycasts = false;
    }

    public void ShowImmediately()
    {
        canvasGroup.alpha = 1f;
    }

    public void HideImmediately()
    {
        canvasGroup.alpha = 0f;
    }
}
