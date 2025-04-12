using UnityEngine;

public class EnhancedCameraController : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothSpeed = 5f;
    
    [Header("Niveles de Zoom")]
    [SerializeField] private float zoomCloseDistance = 3f;
    [SerializeField] private float zoomMediumDistance = 7f;
    [SerializeField] private float zoomFarDistance = 12f;
    
    [Header("Posiciones Predefinidas")]
    [SerializeField] private Vector3 offsetClose = new Vector3(0, 2, -3);
    [SerializeField] private Vector3 offsetMedium = new Vector3(0, 5, -7);
    [SerializeField] private Vector3 offsetFar = new Vector3(0, 10, -12);
    
    [Header("Control Manual")]
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private float minVerticalAngle = 10f;
    [SerializeField] private float maxVerticalAngle = 80f;
    [SerializeField] private KeyCode rotationModeKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode resetCameraKey = KeyCode.R;
    
    private Vector3 currentOffset;
    private int currentZoomLevel = 1; // 0: Cercano, 1: Medio, 2: Lejano
    private float currentZoomDistance;
    
    // Variables para la rotación manual
    private float horizontalAngle = 0f;
    private float verticalAngle = 40f;
    private bool inRotationMode = false;
    private bool isCustomPosition = false;
    
    void Start()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.transform;
        }
        
        // Iniciar con zoom medio
        SetZoomLevel(1);
        currentZoomDistance = zoomMediumDistance;
    }
    
    void LateUpdate()
    {
        if (target == null)
            return;
            
        // Cambiar entre modos predefinidos y rotación personalizada
        if (Input.GetKeyDown(rotationModeKey))
        {
            inRotationMode = !inRotationMode;
            if (inRotationMode)
                isCustomPosition = true;
        }
        
        // Reset de la cámara a posición predefinida
        if (Input.GetKeyDown(resetCameraKey))
        {
            isCustomPosition = false;
            inRotationMode = false;
            SetZoomLevel(currentZoomLevel);
        }
        
        // Procesamiento de teclas para zoom
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetZoomLevel(0);
            if (isCustomPosition)
                AdjustCustomZoomDistance(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SetZoomLevel(1);
            if (isCustomPosition)
                AdjustCustomZoomDistance(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SetZoomLevel(2);
            if (isCustomPosition)
                AdjustCustomZoomDistance(2);
        }
        
        if (inRotationMode || isCustomPosition)
        {
            if (inRotationMode)
                HandleRotationInput();
            PositionCameraWithOrbit();
        }
        else
        {
            PositionCameraWithOffset();
        }
    }
    
    void HandleRotationInput()
    {
        // Rotación horizontal con el mouse
        if (Input.GetMouseButton(1)) // Botón derecho del mouse
        {
            horizontalAngle += Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            verticalAngle -= Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;
            
            // Limitar el ángulo vertical
            verticalAngle = Mathf.Clamp(verticalAngle, minVerticalAngle, maxVerticalAngle);
        }
        
        // Zoom con rueda del mouse
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0)
        {
            currentZoomDistance -= scrollInput * 5f;
            currentZoomDistance = Mathf.Clamp(currentZoomDistance, 2f, 20f);
        }
    }
    
    void PositionCameraWithOrbit()
    {
        // Calcular posición basada en ángulos y la distancia actual de zoom
        float radHorizontal = horizontalAngle * Mathf.Deg2Rad;
        float radVertical = verticalAngle * Mathf.Deg2Rad;
        
        float x = currentZoomDistance * Mathf.Sin(radHorizontal) * Mathf.Cos(radVertical);
        float y = currentZoomDistance * Mathf.Sin(radVertical);
        float z = currentZoomDistance * Mathf.Cos(radHorizontal) * Mathf.Cos(radVertical);
        
        Vector3 desiredPosition = target.position + new Vector3(x, y, z);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        
        // Mirar hacia el jugador
        transform.LookAt(target.position + Vector3.up * 1f);
    }
    
    void PositionCameraWithOffset()
    {
        // Usar el sistema de posiciones predefinidas
        Vector3 desiredPosition = target.position + currentOffset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        
        // Mirar hacia el jugador
        transform.LookAt(target.position + Vector3.up * 1f);
    }
    
    public void SetZoomLevel(int level)
    {
        currentZoomLevel = Mathf.Clamp(level, 0, 2);
        
        switch (currentZoomLevel)
        {
            case 0: // Cercano
                currentOffset = offsetClose;
                currentZoomDistance = zoomCloseDistance;
                break;
            case 1: // Medio
                currentOffset = offsetMedium;
                currentZoomDistance = zoomMediumDistance;
                break;
            case 2: // Lejano
                currentOffset = offsetFar;
                currentZoomDistance = zoomFarDistance;
                break;
        }
    }
    
    // Ajusta la distancia de zoom manteniendo la rotación personalizada
    private void AdjustCustomZoomDistance(int zoomLevel)
    {
        switch (zoomLevel)
        {
            case 0: // Cercano
                currentZoomDistance = zoomCloseDistance;
                break;
            case 1: // Medio
                currentZoomDistance = zoomMediumDistance;
                break;
            case 2: // Lejano
                currentZoomDistance = zoomFarDistance;
                break;
        }
    }
    
    // Método público para permitir cambios desde otros scripts
    public void SetCustomRotation(float horizontal, float vertical, float distance)
    {
        horizontalAngle = horizontal;
        verticalAngle = Mathf.Clamp(vertical, minVerticalAngle, maxVerticalAngle);
        currentZoomDistance = Mathf.Clamp(distance, 2f, 20f);
        isCustomPosition = true;
    }
}