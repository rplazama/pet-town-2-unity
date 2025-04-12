// EndScreenController.cs - Control de la pantalla de fin de juego
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class EndScreenController : MonoBehaviour
{
    [SerializeField] private GameObject endScreenPanel;
    [SerializeField] private TextMeshProUGUI endScreenText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button continueButton;
    
    [Header("Configuración")]
    [SerializeField] private float showDelay = 1f;
    [SerializeField] private AudioClip endScreenMusic;
    
    private AudioSource audioSource;
    
    private void Awake()
    {
        if (endScreenPanel != null)
            endScreenPanel.SetActive(false);
            
        // Configurar botones si existen
        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartClicked);
            
        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueClicked);
            
        // Crear AudioSource si es necesario
        if (endScreenMusic != null && GetComponent<AudioSource>() == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = endScreenMusic;
            audioSource.loop = true;
            audioSource.playOnAwake = false;
        }
        else
        {
            audioSource = GetComponent<AudioSource>();
        }
    }
    
    public void ShowEndScreen()
    {
        StartCoroutine(ShowEndScreenDelayed());
    }
    
    private IEnumerator ShowEndScreenDelayed()
    {
        yield return new WaitForSeconds(showDelay);
        
        if (endScreenPanel != null)
            endScreenPanel.SetActive(true);
            
        // Reproducir música de fin
        if (audioSource != null && endScreenMusic != null)
        {
            audioSource.Play();
        }
        
        // También se puede activar con teclado
        StartCoroutine(WaitForKeyInput());
    }
    
    private IEnumerator WaitForKeyInput()
    {
        bool waitingForInput = true;
        
        while (waitingForInput)
        {
            // Reiniciar con Espacio
            if (Input.GetKeyDown(KeyCode.Space))
            {
                OnRestartClicked();
                waitingForInput = false;
            }
            // Continuar con P
            else if (Input.GetKeyDown(KeyCode.P))
            {
                OnContinueClicked();
                waitingForInput = false;
            }
            
            yield return null;
        }
    }
    
    private void OnRestartClicked()
    {
        // Reiniciar juego
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            gameManager.ResetGame();
        }
        
        // Ocultar pantalla
        HideEndScreen();
    }
    
    private void OnContinueClicked()
    {
        // Solo ocultar la pantalla
        HideEndScreen();
    }
    
    private void HideEndScreen()
    {
        if (endScreenPanel != null)
            endScreenPanel.SetActive(false);
            
        // Detener música
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}
