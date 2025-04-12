// NPCInteractionSystem.cs - Detecta clics en NPCs y gestiona interacciones
using UnityEngine;

public class NPCInteractionSystem : MonoBehaviour
{
    [SerializeField] private LayerMask npcLayer;
    [SerializeField] private float maxInteractionDistance = 10f;
    [SerializeField] private KeyCode interactionKey = KeyCode.E;
    
    private Camera mainCamera;
    private Transform playerTransform;
    
    private void Start()
    {
        mainCamera = Camera.main;
        playerTransform = transform; // Asume que este script está adjunto al jugador
    }
    
    private void Update()
    {
        // Comprobar interacción con clic o tecla
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(interactionKey))
        {
            TryInteract();
        }
    }
    
    private void TryInteract()
    {
        Ray ray;
        
        // Si se usa clic, lanzar rayo desde la cámara al puntero del mouse
        if (Input.GetMouseButtonDown(0))
        {
            ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        }
        // Si se usa tecla, lanzar rayo desde el jugador hacia delante
        else
        {
            ray = new Ray(playerTransform.position, playerTransform.forward);
        }
        
        // Comprobar si el rayo golpea a algún NPC
        if (Physics.Raycast(ray, out RaycastHit hit, maxInteractionDistance, npcLayer))
        {
            // Buscar componentes NPC en el objeto golpeado o sus padres
            BaseNPC npc = hit.collider.GetComponentInParent<BaseNPC>();
            
            if (npc != null)
            {
                // Comprobar distancia si es una tecla (el clic ya usa el rayo)
                if (!Input.GetMouseButtonDown(0))
                {
                    float distanceToNPC = Vector3.Distance(playerTransform.position, npc.transform.position);
                    if (distanceToNPC > npc.GetInteractionDistance())
                        return;
                }
                
                // Invocar interacción
                npc.OnInteract();
            }
        }
    }
}