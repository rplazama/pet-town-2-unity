using UnityEngine;
using System.Collections;

public abstract class BaseNPC : MonoBehaviour
{
    [Header("Información Básica")]
    [SerializeField] protected string npcName;
    [SerializeField] protected Transform dialogueAnchor;
    
    [Header("Interacción")]
    [SerializeField] protected float interactionDistance = 2f;
    [SerializeField] protected bool highlightOnHover = true;
    [SerializeField] protected AudioClip interactionSound;
    
    [Header("Configuración Visual")]
    [SerializeField] protected Material defaultMaterial;
    [SerializeField] protected Material highlightMaterial;
    
    protected bool playerInRange = false;
    protected DialogueManager dialogueManager;
    protected GameManager gameManager;
    protected bool isInteracting = false;
    protected Renderer[] renderers;
    
    protected virtual void Awake()
    {
        // Obtener todos los renderers para el efecto de highlight
        renderers = GetComponentsInChildren<Renderer>();
        
        // Crear un anchor para diálogos si no existe
        if (dialogueAnchor == null)
        {
            dialogueAnchor = new GameObject("DialogueAnchor").transform;
            dialogueAnchor.SetParent(transform);
            dialogueAnchor.localPosition = new Vector3(0, 1.5f, 0); // Encima de la cabeza
        }
    }
    
    protected virtual void Start()
    {
        // Buscar dependencias en la escena
        dialogueManager = FindFirstObjectByType<DialogueManager>();
        gameManager = FindFirstObjectByType<GameManager>();
        
        if (dialogueManager == null)
            Debug.LogWarning($"NPC {npcName}: DialogueManager no encontrado en la escena");
    }
    
    protected virtual void Update()
    {
        // Comprobar si el jugador está en rango
        CheckPlayerDistance();
    }
    
    protected void CheckPlayerDistance()
    {
        // Buscar al jugador (asumiendo que tiene el tag "Player")
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        
        // Calcular distancia
        float distance = Vector3.Distance(transform.position, player.transform.position);
        bool wasInRange = playerInRange;
        playerInRange = distance <= interactionDistance;
        
        // Aplicar highlight si es necesario
        if (highlightOnHover && !isInteracting)
        {
            if (playerInRange && !wasInRange)
                SetHighlight(true);
            else if (!playerInRange && wasInRange)
                SetHighlight(false);
        }
        
        // Notificar cambios de estado
        if (playerInRange && !wasInRange)
            OnPlayerEnterRange();
        else if (!playerInRange && wasInRange)
            OnPlayerExitRange();
    }
    
    protected virtual void OnPlayerEnterRange()
    {
        // Este método puede ser sobrescrito por las clases derivadas
    }
    
    protected virtual void OnPlayerExitRange()
    {
        // Este método puede ser sobrescrito por las clases derivadas
    }
    
    public virtual void OnInteract()
    {
        // Solo interactuar si el jugador está en rango
        if (!playerInRange) return;
        
        isInteracting = true;
        
        // Reproducir sonido si existe
        if (interactionSound != null)
            AudioSource.PlayClipAtPoint(interactionSound, transform.position, 0.7f);
    }
    
    public virtual void EndInteraction()
    {
        isInteracting = false;
        
        // Restaurar materiales normales si estaba usando highlight
        if (highlightOnHover && playerInRange)
            SetHighlight(true);
        else
            SetHighlight(false);
    }
    
    protected void SetHighlight(bool enabled)
    {
        if (highlightMaterial == null || defaultMaterial == null) return;
        
        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.materials;
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = enabled ? highlightMaterial : defaultMaterial;
            }
            renderer.materials = materials;
        }
    }
    
    // Método de ayuda para hacer que el NPC mire al jugador
    protected IEnumerator LookAtPlayer(float duration = 1.0f)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) yield break;
        
        Vector3 targetDirection = player.transform.position - transform.position;
        targetDirection.y = 0; // Ignorar altura
        
        if (targetDirection.magnitude < 0.1f) yield break;
        
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = Quaternion.LookRotation(targetDirection);
        
        float elapsed = 0;
        while (elapsed < duration)
        {
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        transform.rotation = endRotation;
    }
    
    // Devuelve la distancia de interacción para scripts externos
    public float GetInteractionDistance()
    {
        return interactionDistance;
    }
    
    // Mostrar radio de interacción en el editor
    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
        
        if (dialogueAnchor != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(dialogueAnchor.position, 0.1f);
            Gizmos.DrawLine(transform.position, dialogueAnchor.position);
        }
    }
}