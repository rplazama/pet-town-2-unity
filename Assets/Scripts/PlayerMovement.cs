using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 3.0f;
    [SerializeField] private float turnSpeed = 8.0f;
    [SerializeField] private float gravity = -9.81f;
    
    [Header("Navegación")]
    [SerializeField] private bool allowMouseNavigation = true;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float stoppingDistance = 0.2f;
    
    // Componentes
    private CharacterController controller;
    private Animator animator;
    private Camera mainCamera;
    
    // Variables de estado
    private Vector3 moveDirection = Vector3.zero;
    private Vector3 targetPoint;
    private bool isMovingToPoint = false;
    private float verticalVelocity;
    private bool isMoving = false;
    
    // Constantes
    private const string MOVING_PARAM = "IsMoving";
    
    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        mainCamera = Camera.main;
        targetPoint = transform.position;
    }
    
    void Update()
    {
        HandleInput();
        ApplyGravity();
        MoveCharacter();
        UpdateAnimations();
    }
    
    void HandleInput()
    {
        // Controles por teclado (WASD o flechas)
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");
        
        // AQUÍ ESTÁ EL CAMBIO PRINCIPAL: Movimiento relativo a la cámara
        if (horizontalInput != 0 || verticalInput != 0)
        {
            // Obtener dirección forward y right de la cámara, pero ignorando componente Y
            Vector3 forward = mainCamera.transform.forward;
            Vector3 right = mainCamera.transform.right;
            
            forward.y = 0;
            right.y = 0;
            forward.Normalize();
            right.Normalize();
            
            // Calcular dirección relativa a la cámara
            Vector3 cameraRelativeMovement = 
                forward * verticalInput + 
                right * horizontalInput;
            
            moveDirection = cameraRelativeMovement.normalized;
            isMoving = true;
            isMovingToPoint = false;
        }
        // Si no hay entrada por teclado y está habilitada la navegación por ratón
        else if (allowMouseNavigation && Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
            {
                targetPoint = hit.point;
                isMovingToPoint = true;
                isMoving = true;
            }
        }
        
        // Navegación por clic de ratón
        if (isMovingToPoint)
        {
            Vector3 directionToTarget = targetPoint - transform.position;
            directionToTarget.y = 0; // Ignorar altura
            
            float distanceToTarget = directionToTarget.magnitude;
            
            if (distanceToTarget <= stoppingDistance)
            {
                // Llegamos al destino
                isMovingToPoint = false;
                isMoving = false;
                moveDirection = Vector3.zero;
            }
            else
            {
                // Moverse hacia el punto
                moveDirection = directionToTarget.normalized;
            }
        }
        else if (horizontalInput == 0 && verticalInput == 0 && !isMovingToPoint)
        {
            // No hay entrada, detener movimiento
            isMoving = false;
            moveDirection = Vector3.zero;
        }
    }
    
    void ApplyGravity()
    {
        // Aplica gravedad solo si no está en el suelo
        if (controller.isGrounded)
        {
            verticalVelocity = -0.5f; // Una pequeña fuerza hacia abajo para mantener grounded
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }
    }
    
    void MoveCharacter()
    {
        if (isMoving)
        {
            // Rotar suavemente hacia la dirección del movimiento
            if (moveDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
            }
            
            // Calcular el vector de movimiento
            Vector3 motion = moveDirection * moveSpeed;
            
            // Aplicar velocidad vertical (gravedad)
            motion.y = verticalVelocity;
            
            // Aplicar movimiento
            controller.Move(motion * Time.deltaTime);
        }
        else
        {
            // Aplicar solo gravedad cuando no hay movimiento horizontal
            controller.Move(new Vector3(0, verticalVelocity, 0) * Time.deltaTime);
        }
    }
    
    void UpdateAnimations()
    {
        // Actualizar la animación según el estado de movimiento
        animator.SetBool(MOVING_PARAM, isMoving);
    }
    
    // Visual debugging (para ayudar en el editor)
    void OnDrawGizmos()
    {
        if (isMovingToPoint)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(targetPoint, 0.2f);
            Gizmos.DrawLine(transform.position, targetPoint);
        }
    }
}