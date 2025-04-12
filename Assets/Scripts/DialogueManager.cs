// DialogueManager.cs - Sistema para mostrar diálogos y opciones
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    [Header("Componentes UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Button nextButton;
    
    [Header("Prompt de Colección")]
    [SerializeField] private GameObject collectionPanel;
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;
    
    [Header("Configuración")]
    [SerializeField] private float typingSpeed = 0.03f;
    [SerializeField] private bool useTypewriterEffect = true;
    [SerializeField] private AudioClip typingSound;
    [SerializeField] private float typingSoundInterval = 0.1f;
    [SerializeField] private AudioClip dialogueOpenSound;
    [SerializeField] private AudioClip dialogueCloseSound;
    
    private CameraController cameraController;
    private Coroutine typingCoroutine;
    private Action onDialogueComplete;
    private bool isDialogueActive = false;
    private float lastTypingSoundTime;
    
    private void Awake()
    {
        // Asegurarse de que los paneles estén desactivados
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
            
        if (collectionPanel != null)
            collectionPanel.SetActive(false);
            
        cameraController = FindFirstObjectByType<CameraController>();
    }
    
    private void Start()
    {
        // Configurar botones
        if (nextButton != null)
            nextButton.onClick.AddListener(OnNextButtonClicked);
            
        if (yesButton != null)
            yesButton.onClick.AddListener(() => OnCollectionResponseClicked(true));
            
        if (noButton != null)
            noButton.onClick.AddListener(() => OnCollectionResponseClicked(false));
    }
    
    public void ShowDialogue(string name, string text, Vector3 worldPosition, Action onComplete = null)
    {
        if (isDialogueActive)
        {
            // Si hay un diálogo en curso, finalizarlo primero
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);
                
            if (onDialogueComplete != null)
            {
                Action temp = onDialogueComplete;
                onDialogueComplete = null;
                temp.Invoke();
            }
        }
        
        // Establecer como diálogo activo
        isDialogueActive = true;
        
        // Guardar callback
        onDialogueComplete = onComplete;
        
        // Configurar los textos
        nameText.text = name;
        dialogueText.text = "";
        
        // Mostrar el panel
        dialoguePanel.SetActive(true);
        
        // Reproducir sonido de apertura
        if (dialogueOpenSound != null)
            AudioSource.PlayClipAtPoint(dialogueOpenSound, Camera.main.transform.position, 0.7f);
            
        // Enfocar la cámara en la posición del NPC
        if (cameraController != null)
            cameraController.FocusOnPosition(worldPosition);
            
        // Iniciar efecto de escritura
        if (useTypewriterEffect)
            typingCoroutine = StartCoroutine(TypeText(text));
        else
            dialogueText.text = text;
    }
    
    private IEnumerator TypeText(string text)
    {
        dialogueText.text = "";
        lastTypingSoundTime = -typingSoundInterval; // Para asegurar que suene al inicio
        
        foreach (char c in text)
        {
            dialogueText.text += c;
            
            // Sonido de escritura
            if (typingSound != null && Time.time - lastTypingSoundTime >= typingSoundInterval)
            {
                AudioSource.PlayClipAtPoint(typingSound, Camera.main.transform.position, 0.5f);
                lastTypingSoundTime = Time.time;
            }
            
            yield return new WaitForSeconds(typingSpeed);
        }
        
        typingCoroutine = null;
    }
    
    private void OnNextButtonClicked()
    {
        if (typingCoroutine != null)
        {
            // Si está escribiendo, completar inmediatamente
            StopCoroutine(typingCoroutine);
            dialogueText.text = dialogueText.text; // Mostrar texto completo
            typingCoroutine = null;
        }
        else
        {
            // Si ya se completó la escritura, cerrar el diálogo
            CloseDialogue();
        }
    }
    
    private void CloseDialogue()
    {
        // Reproducir sonido de cierre
        if (dialogueCloseSound != null)
            AudioSource.PlayClipAtPoint(dialogueCloseSound, Camera.main.transform.position, 0.7f);
            
        // Ocultar panel
        dialoguePanel.SetActive(false);
        
        // Restaurar la cámara
        if (cameraController != null)
            cameraController.ReturnToDefaultView();
            
        // Marcar como inactivo
        isDialogueActive = false;
        
        // Llamar al callback
        if (onDialogueComplete != null)
        {
            Action temp = onDialogueComplete;
            onDialogueComplete = null;
            temp.Invoke();
        }
    }
    
    public void ShowCollectionPrompt(string promptMessage, Action<bool> onResponse, Vector3 worldPosition)
    {
        if (collectionPanel == null) return;
        
        // Configurar el texto
        promptText.text = promptMessage;
        
        // Mostrar el panel
        collectionPanel.SetActive(true);
        
        // Guardar el callback
        collectionCallback = onResponse;
        
        // Enfocar la cámara si es necesario
        if (cameraController != null)
            cameraController.FocusOnPosition(worldPosition);
    }
    
    private Action<bool> collectionCallback;
    
    private void OnCollectionResponseClicked(bool accepted)
    {
        // Ocultar el panel
        collectionPanel.SetActive(false);
        
        // Restaurar la cámara
        if (cameraController != null)
            cameraController.ReturnToDefaultView();
            
        // Llamar al callback
        if (collectionCallback != null)
        {
            Action<bool> temp = collectionCallback;
            collectionCallback = null;
            temp.Invoke(accepted);
        }
    }
    
    // Para poder cerrar el diálogo con teclas
    private void Update()
    {
        if (isDialogueActive && Input.GetKeyDown(KeyCode.Space))
        {
            OnNextButtonClicked();
        }
    }
}