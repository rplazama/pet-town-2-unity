// MainMenuController.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class MainMenuController : MonoBehaviour
{
    [Header("Elementos UI")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image logoImage;
    [SerializeField] private TextMeshProUGUI introText;
    [SerializeField] private TextMeshProUGUI controlsText;
    [SerializeField] private TextMeshProUGUI startPrompt;
    
    [Header("Configuración")]
    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private float logoFloatSpeed = 3f;
    [SerializeField] private float logoFloatAmount = 10f;
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private AudioClip startSound;
    
    [Header("Animación")]
    [SerializeField] private float fadeInDuration = 1f;
    [SerializeField] private float textAnimationDelay = 0.5f;
    
    private AudioSource audioSource;
    private Vector3 logoStartPosition;
    
    private void Awake()
    {
        // Configurar AudioSource para música de fondo
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = backgroundMusic;
        audioSource.loop = true;
        audioSource.volume = 0.5f;
        
        // Guardar posición inicial del logo para animación
        if (logoImage != null)
            logoStartPosition = logoImage.rectTransform.anchoredPosition;
            
        // Inicialmente ocultar todos los elementos para la animación de entrada
        SetElementsAlpha(0);
    }
    
    private void Start()
    {
        // Iniciar música
        if (audioSource != null && backgroundMusic != null)
            audioSource.Play();
            
        // Iniciar animación de entrada
        StartCoroutine(AnimateIntro());
    }
    
    private IEnumerator AnimateIntro()
    {
        // Esperar un momento antes de comenzar
        yield return new WaitForSeconds(0.5f);
        
        // Animar fondo
        float elapsed = 0;
        while (elapsed < fadeInDuration)
        {
            float alpha = Mathf.Lerp(0, 1, elapsed / fadeInDuration);
            SetBackgroundAlpha(alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }
        SetBackgroundAlpha(1);
        
        // Animar logo
        elapsed = 0;
        while (elapsed < fadeInDuration)
        {
            float alpha = Mathf.Lerp(0, 1, elapsed / fadeInDuration);
            SetLogoAlpha(alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }
        SetLogoAlpha(1);
        
        // Mostrar textos con delay
        yield return new WaitForSeconds(textAnimationDelay);
        FadeInText(introText, fadeInDuration);
        
        yield return new WaitForSeconds(textAnimationDelay);
        FadeInText(controlsText, fadeInDuration);
        
        yield return new WaitForSeconds(textAnimationDelay);
        FadeInText(startPrompt, fadeInDuration);
        
        // Comenzar animación continua del logo
        StartCoroutine(AnimateLogoFloat());
        
        // Iniciar parpadeo del prompt de inicio
        StartCoroutine(PulseStartPrompt());
    }
    
    private IEnumerator AnimateLogoFloat()
    {
        while (true)
        {
            float yOffset = Mathf.Sin(Time.time * logoFloatSpeed) * logoFloatAmount;
            if (logoImage != null)
            {
                logoImage.rectTransform.anchoredPosition = logoStartPosition + new Vector3(0, yOffset, 0);
            }
            yield return null;
        }
    }
    
    private IEnumerator PulseStartPrompt()
    {
        while (true)
        {
            // Pulsar entre 50% y 100% de opacidad
            float elapsed = 0;
            float duration = 1.5f;
            
            // Fade out
            while (elapsed < duration/2)
            {
                float alpha = Mathf.Lerp(1, 0.5f, elapsed / (duration/2));
                if (startPrompt != null)
                    startPrompt.alpha = alpha;
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            // Fade in
            while (elapsed < duration)
            {
                float alpha = Mathf.Lerp(0.5f, 1, (elapsed - duration/2) / (duration/2));
                if (startPrompt != null)
                    startPrompt.alpha = alpha;
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            yield return null;
        }
    }
    
    private void Update()
    {
        // Iniciar juego al presionar espacio
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartGame();
        }
    }
    
    public void StartGame()
    {
        // Reproducir sonido
        if (startSound != null)
            AudioSource.PlayClipAtPoint(startSound, Camera.main.transform.position);
            
        // Iniciar corrutina de transición
        StartCoroutine(TransitionToGame());
    }
    
    private IEnumerator TransitionToGame()
    {
        // Ocultar elementos con animación
        float elapsed = 0;
        float fadeDuration = 1f;
        
        while (elapsed < fadeDuration)
        {
            float alpha = Mathf.Lerp(1, 0, elapsed / fadeDuration);
            SetElementsAlpha(alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Cargar escena de juego
        SceneManager.LoadScene(gameSceneName);
    }
    
    private void SetElementsAlpha(float alpha)
    {
        SetBackgroundAlpha(alpha);
        SetLogoAlpha(alpha);
        if (introText != null) introText.alpha = alpha;
        if (controlsText != null) controlsText.alpha = alpha;
        if (startPrompt != null) startPrompt.alpha = alpha;
    }
    
    private void SetBackgroundAlpha(float alpha)
    {
        if (backgroundImage != null)
        {
            Color color = backgroundImage.color;
            color.a = alpha;
            backgroundImage.color = color;
        }
        
        if (mainPanel != null)
        {
            CanvasGroup canvasGroup = mainPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = mainPanel.AddComponent<CanvasGroup>();
            canvasGroup.alpha = alpha;
        }
    }
    
    private void SetLogoAlpha(float alpha)
    {
        if (logoImage != null)
        {
            Color color = logoImage.color;
            color.a = alpha;
            logoImage.color = color;
        }
    }
    
    private void FadeInText(TextMeshProUGUI text, float duration)
    {
        if (text == null) return;
        
        StartCoroutine(FadeInTextCoroutine(text, duration));
    }
    
    private IEnumerator FadeInTextCoroutine(TextMeshProUGUI text, float duration)
    {
        text.alpha = 0;
        
        float elapsed = 0;
        while (elapsed < duration)
        {
            float alpha = Mathf.Lerp(0, 1, elapsed / duration);
            text.alpha = alpha;
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        text.alpha = 1;
    }
}