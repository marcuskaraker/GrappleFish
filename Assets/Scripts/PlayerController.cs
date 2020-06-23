using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EZCameraShake;

public class PlayerController : MonoBehaviour
{
    public float speed = 1;
    public float sprintSpeed = 2;
    public float hookSprintSpeed = 3;
    public float jumpForce = 20f;
    public float groundJumpDis = 0.2f;
    public float rotationSpeed = 1f;
    public float swimRotMagnitude = 10;

    [Space]
    [SerializeField] GrapplingGun grapplingGun;

    [Header("Model")]
    [SerializeField] GameObject model;
    [SerializeField] float modelRotationSpeed = 20f;

    [SerializeField] GameObject cursorVisuals;
    [SerializeField] Transform projectileParent;
    public Transform ProjectileParent { get { return projectileParent; } }

    public LayerMask groundCheckMask;
    public LayerMask terrainLayer;

    bool onGround;
    bool isSprinting;
    public bool AboveWater { get; private set; }

    float currentSpeed;

    Rigidbody rb;

    private Quaternion lookRotation;
    private Vector3 lookDirection;

    private Quaternion calculatedRotation;
    bool hasSetRotation;

    Camera mainCamera;
    Vector3 cursorPos;

    Vector3 objectOnScreenPos;

    public bool IsFrozen { get; set; }

    Hook stuckHookTarget;
    Eye[] eyes;

    ConfigurableJoint hookJoint;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;

        eyes = GetComponentsInChildren<Eye>();
        hookJoint = GetComponent<ConfigurableJoint>();
    }

    private void FixedUpdate()
    {
        AboveWater = rb.transform.position.y > 0;

        rb.angularVelocity = Vector3.zero;

        // Ground Check
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, groundJumpDis, groundCheckMask))
        {
            onGround = true;
        }
        else
        {
            onGround = false;
        }

        // Moement
        isSprinting = Input.GetKey(KeyCode.LeftShift);

        Vector3 moveDir = AboveWater ? Vector3.zero : lookDirection;
        currentSpeed = speed / 5;
        if (Input.GetMouseButton(1))
        {
            grapplingGun.ReturnHook();

            if (stuckHookTarget)
            {
                // Move towards the hook stuck in terrain.
                moveDir = (stuckHookTarget.transform.position - transform.position).normalized;
                currentSpeed = hookSprintSpeed;
            }
            else if(!AboveWater)
            {
                // Sprint.
                currentSpeed = isSprinting ? sprintSpeed : speed;
            }            
        }

        if (!AboveWater || (stuckHookTarget && Input.GetMouseButton(1)))
        {
            Vector3 moveVector = moveDir * currentSpeed;
            rb.velocity = Vector3.Lerp(rb.velocity, moveVector, Time.deltaTime * 7);
        }

        if (stuckHookTarget != null && Vector3.Distance(transform.position, stuckHookTarget.transform.position) < 0.5f)
        {
            Destroy(stuckHookTarget.gameObject);
        }

        // Rotation
        if (hasSetRotation)
        {
            rb.rotation = calculatedRotation;
        }
        
    }

    private void Update()
    {
        // Air/Water mechanics.
        if (AboveWater && rb.useGravity != true)
        {
            rb.AddForce(Vector3.up * rb.velocity.magnitude, ForceMode.Impulse);
            rb.useGravity = true;
            CameraShaker.Instance.ShakeOnce(4, 3, 0, 0.15f);
        }
        else if(!AboveWater && rb.useGravity == true)
        {
            rb.useGravity = false;
            CameraShaker.Instance.ShakeOnce(4, 3, 0, 0.15f);
        }

        // Aiming
        RaycastHit mouseHit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out mouseHit, 100, terrainLayer))
        {
            cursorPos = mouseHit.point;
            cursorVisuals.transform.position = cursorPos;
        }

        Vector3 aimPos = stuckHookTarget != null ? stuckHookTarget.transform.position : new Vector3(cursorPos.x, cursorPos.y, 0);
        aimPos.z = 0;

        lookDirection = (aimPos - transform.position).normalized;

        float AngleRad = Mathf.Atan2(aimPos.y - transform.position.y, aimPos.x - transform.position.x);
        // Get Angle in Degrees
        float AngleDeg = (180 / Mathf.PI) * AngleRad;

        // Influence noise rotation (to look like it is swimming).
        float influenseAngle = Mathf.Lerp(-swimRotMagnitude, swimRotMagnitude, Mathf.PerlinNoise(transform.position.x * 0.34234f, transform.position.y * 0.23423f));

        // Calculate rotation. (Rotation is actually set in FixedUpdate).
        float currentRotationSpeed = (rotationSpeed * (currentSpeed / 2));
        calculatedRotation = Quaternion.Lerp(rb.rotation, Quaternion.Euler(0, 0, AngleDeg + influenseAngle), Time.fixedDeltaTime * currentRotationSpeed);
        hasSetRotation = true;

        // Model rotation sc    
        float zRot = 0f;
        if (Vector3.Dot((aimPos - transform.position).normalized, Vector3.right) < 0)
        {
            zRot = 180f;
        }

        model.transform.localRotation = Quaternion.Lerp(model.transform.localRotation, Quaternion.Euler(0, 90, zRot), Time.deltaTime * modelRotationSpeed * currentRotationSpeed);

        // Fire
        bool didShoot = false;
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.F))
        {
            if (grapplingGun.HookOut)
            {
                grapplingGun.UnstuckHook();
            }

            // Shoot
            didShoot = grapplingGun.Fire(aimPos);
        }

        if (didShoot)
        {
            CameraShaker.Instance.ShakeOnce(4, 3, 0, 0.15f);
        }

        // Eyes
        foreach (Eye eye in eyes)
        {
            eye.SetTargetPosition(aimPos);
        }
    }

    public void MoveToHook(Hook target)
    {
        stuckHookTarget = target;
    }

    public void SetJoint(Rigidbody rb)
    {
        if (hookJoint == null)
        {
            return;
        }

        hookJoint.connectedBody = rb;
    }
}
