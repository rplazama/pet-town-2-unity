using UnityEngine;
using System.Collections.Generic;

public class DialogueNPC : BaseNPC
{
    [Header("Información del NPC")]
    [SerializeField] private string npcType; // Tipo/categoría del NPC
    
    [Header("Diálogos")]
    [SerializeField, TextArea(3, 5)] private List<string> dialogueLines = new List<string>();
    [SerializeField] private bool randomDialogue = false;
    [SerializeField] private bool cycleDialogues = true;
    
    [Header("Diálogos Alternativos")]
    [SerializeField] private bool hasAlternativeDialogues = false;
    [SerializeField, TextArea(3, 5)] private List<string> alternativeDialogueLines = new List<string>();
    [SerializeField] private bool useAlternativeDialogue = false;
    
    [Header("Comportamiento")]
    [SerializeField] private bool turnsToFacePlayer = true;
    [SerializeField] private float turnSpeed = 5f;
    [SerializeField] private bool showInteractionPrompt = true;
    
    private int currentDialogueIndex = 0;
    private Transform playerTransform;
    private GameObject interactionPromptObject;
    
    protected override void Start()
    {
        base.Start();
        
        // Buscar al jugador si es necesario
        if (turnsToFacePlayer && playerTransform == null)
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
            
        // Crear indicador de interacción si está habilitado
        if (showInteractionPrompt)
            SetupInteractionPrompt();
    }
    
    protected override void Update()
    {
        base.Update();
        
        // Actualizar visibilidad del indicador de interacción
        if (interactionPromptObject != null)
            interactionPromptObject.SetActive(playerInRange);
    }
    
    private void SetupInteractionPrompt()
    {
        // Crear un objeto simple que sirva como indicador visual
        interactionPromptObject = new GameObject("InteractionPrompt");
        interactionPromptObject.transform.SetParent(transform);
        interactionPromptObject.transform.localPosition = new Vector3(0, 2f, 0); // Encima del NPC
        
        // Añadir un sprite o texto como prefieran
        // Ejemplo simple:
        SpriteRenderer sprite = interactionPromptObject.AddComponent<SpriteRenderer>();
        // Asigna aquí un sprite de "!" o similar
        
        // Escalar y orientar el sprite
        interactionPromptObject.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        
        // Inicialmente desactivado
        interactionPromptObject.SetActive(false);
    }
    
    public override void OnInteract()
    {
        base.OnInteract();
        
        // Hacer que el NPC mire al jugador
        if (turnsToFacePlayer && playerTransform != null)
            StartCoroutine(TurnToFacePlayer());
            
        // Seleccionar conjunto de diálogos adecuado
        List<string> currentDialogues = useAlternativeDialogue && hasAlternativeDialogues ? 
            alternativeDialogueLines : dialogueLines;
            
        if (currentDialogues.Count == 0) return;
        
        // Obtener el diálogo apropiado
        string dialogue;
        if (randomDialogue)
        {
            int randomIndex = Random.Range(0, currentDialogues.Count);
            dialogue = currentDialogues[randomIndex];
        }
        else
        {
            dialogue = currentDialogues[currentDialogueIndex];
            
            // Avanzar al siguiente diálogo para la próxima vez
            if (cycleDialogues)
            {
                currentDialogueIndex = (currentDialogueIndex + 1) % currentDialogues.Count;
            }
        }
        
        // Mostrar diálogo
        if (dialogueManager != null)
        {
            dialogueManager.ShowDialogue(npcName, dialogue, transform.position + Vector3.up * 1.5f);
        }
        else
        {
            Debug.LogWarning($"No se encontró DialogueManager para el NPC {npcName}");
        }
    }
    
    private System.Collections.IEnumerator TurnToFacePlayer()
    {
        if (playerTransform == null) yield break;
        
        // Calcular dirección hacia el jugador (ignorando eje Y)
        Vector3 directionToPlayer = playerTransform.position - transform.position;
        directionToPlayer.y = 0;
        
        if (directionToPlayer != Vector3.zero)
        {
            // Calcular rotación objetivo
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            Quaternion startRotation = transform.rotation;
            
            // Rotar suavemente
            float elapsedTime = 0;
            float duration = 0.5f; // Duración de la rotación
            
            while (elapsedTime < duration)
            {
                transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsedTime / duration);
                elapsedTime += Time.deltaTime * turnSpeed;
                yield return null;
            }
            
            // Asegurar rotación final exacta
            transform.rotation = targetRotation;
        }
    }
    
    public void SetAlternativeDialogueMode(bool useAlternative)
    {
        useAlternativeDialogue = useAlternative;
    }
    
    public void SetDialogue(List<string> newDialogues)
    {
        dialogueLines = new List<string>(newDialogues);
        currentDialogueIndex = 0;
    }
    
    public void AddDialogue(string newDialogue)
    {
        dialogueLines.Add(newDialogue);
    }
    
    public void ResetDialogueIndex()
    {
        currentDialogueIndex = 0;
    }
    
    // Para visualizar el rango de interacción en el editor
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        // Dibujar un cubo pequeño donde aparecerá el indicador de interacción
        Gizmos.color = Color.cyan;
        Vector3 promptPosition = transform.position + Vector3.up * 2f;
        Gizmos.DrawCube(promptPosition, Vector3.one * 0.2f);
    }
}