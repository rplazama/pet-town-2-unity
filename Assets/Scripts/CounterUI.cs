// CounterUI.cs - Muestra el contador de animales
using UnityEngine;
using TMPro;

public class CounterUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI counterText;
    [SerializeField] private string counterFormat = "Animales encontrados: {0}/{1}";
    
    [Header("Efectos")]
    [SerializeField] private bool animateOnUpdate = true;
    [SerializeField] private float animationDuration = 0.5f;
    [SerializeField] private AudioClip updateSound;
    
    private int currentCount = 0;
    
    public void UpdateCounter(int found, int total)
    {
        bool isIncrement = found > currentCount;
        currentCount = found;
        
        if (counterText != null)
        {
            counterText.text = string.Format(counterFormat, found, total);
            
            if (isIncrement && animateOnUpdate)
            {
                AnimateCounter();
            }
            
            if (isIncrement && updateSound != null)
            {
                AudioSource.PlayClipAtPoint(updateSound, Camera.main.transform.position);
            }
        }
    }
    
    private void AnimateCounter()
    {
        // Esta es una animación simple, puedes hacerla más compleja
        LeanTween.cancel(gameObject);
        transform.localScale = Vector3.one;
        
        LeanTween.scale(gameObject, Vector3.one * 1.2f, animationDuration * 0.5f)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnComplete(() => {
                LeanTween.scale(gameObject, Vector3.one, animationDuration * 0.5f)
                    .setEase(LeanTweenType.easeInQuad);
            });
    }
}