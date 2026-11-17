using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelSliede : MonoBehaviour
{
    public float duration = .4f;
    private bool isPlaying = false;

    // 슬라이드를 시작했으면 true (진행 중이면 무시하고 false)
    public bool Slide(RectTransform current, RectTransform target)
    {
        if (isPlaying) return false;
        StartCoroutine(SlideRoutine(current, target));
        return true;
    }

    IEnumerator SlideRoutine(RectTransform current, RectTransform target)
    {
        isPlaying = true;

        float width = current.rect.width;
        float timer = 0f;

        target.anchoredPosition = new Vector2(width, 0f);

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float lerp = Mathf.Clamp01(timer / duration);

            float currentX = Mathf.Lerp(0, -width, lerp);

            float targetX = Mathf.Lerp(width, 0, lerp);

            current.anchoredPosition = new Vector2(currentX, 0);
            target.anchoredPosition = new Vector2(targetX, 0);

            yield return null;
        }

        current.anchoredPosition = new Vector2(-width, 0);
        target.anchoredPosition = Vector2.zero;

        isPlaying = false;
    }
}
