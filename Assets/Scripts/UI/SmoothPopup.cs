using System.Collections;
using UnityEngine;

/// <summary>
/// Attach this to a GameObject with a CanvasGroup to smoothly fade it in and out.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class SmoothPopup : MonoBehaviour
{
    // How long the fade should take
    [SerializeField] private float fadeDuration = 0.2f;

    private CanvasGroup canvasGroup;
    private Coroutine currentFadeCoroutine;

    void Awake()
    {
        // Get the CanvasGroup component
        canvasGroup = GetComponent<CanvasGroup>();

        // Start hidden
        canvasGroup.alpha = 0f;
    }

    /// <summary>
    /// Fades the UI in (makes it visible).
    /// </summary>
    public void Show()
    {
        // If we're already fading, stop that coroutine
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
        }
        // Start a new coroutine to fade to 1 (visible)
        currentFadeCoroutine = StartCoroutine(Fade(1f));
    }

    /// <summary>
    /// Fades the UI out (makes it invisible).
    /// </summary>
    public void Hide()
    {
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
        }
        // Start a new coroutine to fade to 0 (invisible)
        currentFadeCoroutine = StartCoroutine(Fade(0f));
    }

    private IEnumerator Fade(float targetAlpha)
    {
        float startAlpha = canvasGroup.alpha;
        float time = 0;

        while (time < fadeDuration)
        {
            // Calculate the new alpha value
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            
            // Wait for the next frame
            time += Time.deltaTime;
            yield return null;
        }

        // Ensure the alpha is set to the final target value
        canvasGroup.alpha = targetAlpha;
        currentFadeCoroutine = null;
    }
}
