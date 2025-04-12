using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float dialogueSmoothTime = 0.5f;
    
    [Header("Configuración de Vista de Diálogo")]
    [SerializeField] private float dialogueDistance = 4f;
    [SerializeField] private float dialogueHeight = 1.5f;
    [SerializeField] private bool rotateToFaceTarget = true;
    
    private Vector3 defaultPosition;
    private Quaternion defaultRotation;
    private Transform target;
    private Vector3 focusPosition;
    private bool isInDialogueMode = false;
    
    private void Awake()
    {
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;
            
        // Guardar posición y rotación inicial
        defaultPosition = cameraTransform.position;
        defaultRotation = cameraTransform.rotation;
    }
    
    public void FocusOnPosition(Vector3 position)
    {
        isInDialogueMode = true;
        focusPosition = position;
        
        // Encontrar el target (personaje del jugador)
        if (target == null)
            target = GameObject.FindGameObjectWithTag("Player")?.transform;
    }
    
    public void ReturnToDefaultView()
    {
        isInDialogueMode = false;
    }
    
    private void LateUpdate()
    {
        if (isInDialogueMode && cameraTransform != null)
        {
            UpdateDialogueCamera();
        }
        else if (cameraTransform.position != defaultPosition || cameraTransform.rotation != defaultRotation)
        {
            // Volver suavemente a la posición predeterminada
            cameraTransform.position = Vector3.Lerp(cameraTransform.position, defaultPosition, Time.deltaTime * (1f/dialogueSmoothTime));
            cameraTransform.rotation = Quaternion.Slerp(cameraTransform.rotation, defaultRotation, Time.deltaTime * (1f/dialogueSmoothTime));
        }
    }
    
    private void UpdateDialogueCamera()
    {
        if (target == null) return;
        
        // Calcular punto medio entre el jugador y el NPC
        Vector3 midPoint = (target.position + focusPosition) * 0.5f;
        
        // Calcular dirección desde el punto medio hacia la cámara
        Vector3 lookDirection = (focusPosition - target.position).normalized;
        
        // Ajustar para que la cámara esté ligeramente elevada y a un lado
        Vector3 cameraOffset = lookDirection * dialogueDistance;
        cameraOffset.y = dialogueHeight;
        
        // Calcular posición final de la cámara
        Vector3 targetPosition = midPoint + cameraOffset;
        
        // Mover cámara suavemente
        cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetPosition, Time.deltaTime * (1f/dialogueSmoothTime));
        
        // Rotar cámara para mirar al punto medio
        if (rotateToFaceTarget)
        {
            Quaternion targetRotation = Quaternion.LookRotation(midPoint - targetPosition);
            cameraTransform.rotation = Quaternion.Slerp(cameraTransform.rotation, targetRotation, Time.deltaTime * (1f/dialogueSmoothTime));
        }
    }
}