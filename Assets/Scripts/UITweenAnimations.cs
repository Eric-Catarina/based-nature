
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; 

public static class UITweenAnimations
{
    
    public static void PanelAppear(RectTransform panelRect, float duration = 0.3f, Ease fadeEase = Ease.OutSine, Ease scaleEase = Ease.OutBack)
    {
        if (panelRect == null) return;

        
        panelRect.DOKill(); 

        panelRect.gameObject.SetActive(true);

        Vector3 originalScale = panelRect.localScale;
        panelRect.localScale = Vector3.zero;
        CanvasGroup cg = panelRect.GetComponent<CanvasGroup>();

        if (cg == null)
        {
            cg = panelRect.gameObject.AddComponent<CanvasGroup>();
        }
        cg.alpha = 0;

        DOTween.Sequence()
               .Append(panelRect.DOScale(originalScale, duration).SetEase(scaleEase))
               .Join(cg.DOFade(1, duration).SetEase(fadeEase))
               .SetUpdate(true);
    }

    public static void PanelDisappear(RectTransform panelRect, float duration = 0.2f, Ease easeType = Ease.InBack, System.Action onComplete = null)
    {
        if (panelRect == null) return;

        panelRect.DOKill(); 
        Vector3 originalScale = panelRect.localScale;

        panelRect.DOScale(Vector3.zero, duration).SetEase(easeType).SetUpdate(true).OnComplete(() =>
        {
            
            panelRect.localScale = originalScale; 
            onComplete?.Invoke();
        });
    }


    
    public static void HoverScale(RectTransform targetRect, float targetScale = 1.1f, float duration = 0.15f, Ease easeType = Ease.OutSine)
    {
        if (targetRect == null) return;
        targetRect.DOScale(targetScale, duration).SetEase(easeType).SetUpdate(true);
    }

    
    public static void UnhoverScale(RectTransform targetRect, float originalScale = 1f, float duration = 0.1f, Ease easeType = Ease.OutSine)
    {
        if (targetRect == null) return;
        targetRect.DOScale(originalScale, duration).SetEase(easeType).SetUpdate(true);
    }

    
    public static void HoverColor(Image targetImage, Color targetColor, float duration = 0.15f, Ease easeType = Ease.OutSine)
    {
        if (targetImage == null) return;
        targetImage.DOColor(targetColor, duration).SetEase(easeType).SetUpdate(true);
    }

    
    public static void UnhoverColor(Image targetImage, Color originalColor, float duration = 0.1f, Ease easeType = Ease.OutSine)
    {
        if (targetImage == null) return;
        targetImage.DOColor(originalColor, duration).SetEase(easeType).SetUpdate(true);
    }

    
    public static void Jiggle(RectTransform targetRect, float strength = 5f, float duration = 0.3f, int vibrato = 10, Ease easeType = Ease.OutElastic)
    {
        if (targetRect == null) return;
        
        targetRect.DOShakeRotation(duration, new Vector3(0, 0, strength), vibrato, 90, false).SetEase(easeType).SetUpdate(true);
        
        
    }

    
    
    public static void AnimateWave(Transform container, float delayBetweenElements = 0.05f, float elementAnimDuration = 0.3f, Ease easeType = Ease.OutBack)
    {
        if (container == null) return;

        for (int i = 0; i < container.childCount; i++)
        {
            RectTransform childRect = container.GetChild(i) as RectTransform;
            if (childRect != null)
            {
                childRect.localScale = Vector3.zero; 
                childRect.DOScale(Vector3.one, elementAnimDuration)
                         .SetEase(easeType)
                         .SetDelay(i * delayBetweenElements) 
                         .SetUpdate(true);
            }
        }
    }

    
    public static void PunchScale(RectTransform targetRect, float punchScaleFactor = 1.2f, float duration = 0.2f, int vibrato = 2, float elasticity = 0.5f)
    {
        if (targetRect == null) return;
        Vector3 originalScale = targetRect.localScale;
        targetRect.DOPunchScale(Vector3.one * (punchScaleFactor -1f) , duration, vibrato, elasticity)
                  .SetUpdate(true)
                  .OnComplete(() => targetRect.localScale = originalScale); 
    }
}