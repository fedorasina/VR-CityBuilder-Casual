using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class XRDragPreviewSnap : MonoBehaviour
{
    [Header("Preview Prefabs")]
    public GameObject validPreviewPrefab;
    public GameObject invalidPreviewPrefab;

    [Header("Grid")]
    public float gridSize = 0.1f;
    public Transform gridOrigin;

    [Header("Raycast")]
    public float raycastDistance = 20f;
    public float hideDelay = 0.1f;

    [Header("Smooth")]
    public float positionSmoothSpeed = 15f;
    public float rotationSmoothSpeed = 15f;

    [Header("Rotation")]
    public float rotationStep = 15f;
    public float holdRepeatDelay = 0.2f;

    private XRGrabInteractable grabInteractable;
    private GameObject previewInstance;
    private Collider[] previewColliders;

    private InputAction rotateAction;

    private Vector3 targetPos;
    private Quaternion targetRot;

    private float lastValidHitTime;
    private bool hasValidHit;
    private bool isValidPlacement = true;

    private float currentYRotation;
    private float holdTimer;

    private float pivotToBottomOffset;

    private bool lastPlacementState = true;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();

        previewInstance = Instantiate(validPreviewPrefab);
        previewInstance.SetActive(false);

        previewColliders = previewInstance.GetComponentsInChildren<Collider>();

        rotateAction = new InputAction(binding: "<XRController>{RightHand}/primary2DAxis");

        CalculatePivotOffset();
    }

    void OnEnable()
    {
        rotateAction.Enable();
    }

    void OnDisable()
    {
        rotateAction.Disable();
    }

    void Update()
    {
        if (!grabInteractable.isSelected)
        {
            previewInstance.SetActive(false);
            return;
        }

        HandleRotationInput();

        Transform reference = grabInteractable.attachTransform != null
            ? grabInteractable.attachTransform
            : transform;

        Vector3 origin = reference.position + Vector3.up * 0.05f;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, raycastDistance))
        {
            if (hit.collider.CompareTag("Ground"))
            {
                lastValidHitTime = Time.time;
                hasValidHit = true;

                targetPos = GetSnappedPosition(hit);
                targetRot = Quaternion.Euler(0, currentYRotation, 0);
            }
        }

        HandleVisibility();

        if (previewInstance.activeSelf)
        {
            CheckCollision();

            if (isValidPlacement != lastPlacementState)
            {
                SwapPrefab(isValidPlacement);
                lastPlacementState = isValidPlacement;
            }

            HandleMovement();
        }
    }

    // =========================
    // ROTATION
    // =========================

    void HandleRotationInput()
    {
        Vector2 stick = rotateAction.ReadValue<Vector2>();

        if (Mathf.Abs(stick.x) > 0.6f)
        {
            holdTimer -= Time.deltaTime;

            if (holdTimer <= 0f)
            {
                currentYRotation += stick.x > 0 ? rotationStep : -rotationStep;
                holdTimer = holdRepeatDelay;
            }
        }
        else
        {
            holdTimer = 0f;
        }
    }

    // =========================
    // VISIBILITY
    // =========================

    void HandleVisibility()
    {
        if (!hasValidHit)
        {
            previewInstance.SetActive(false);
            return;
        }

        if (!previewInstance.activeSelf)
            previewInstance.SetActive(true);

        if (Time.time - lastValidHitTime > hideDelay)
        {
            previewInstance.SetActive(false);
            hasValidHit = false;
        }
    }

    // =========================
    // PREFAB SWAP (фикс)
    // =========================

    void SwapPrefab(bool isValid)
    {
        Vector3 pos = previewInstance.transform.position;
        Quaternion rot = previewInstance.transform.rotation;

        Destroy(previewInstance);

        previewInstance = Instantiate(isValid ? validPreviewPrefab : invalidPreviewPrefab);
        previewInstance.transform.SetPositionAndRotation(pos, rot);

        previewColliders = previewInstance.GetComponentsInChildren<Collider>();

        previewInstance.SetActive(true);

        CalculatePivotOffset();
    }

    // =========================
    // MOVEMENT
    // =========================

    void HandleMovement()
    {
        previewInstance.transform.position = Vector3.Lerp(
            previewInstance.transform.position,
            targetPos,
            Time.deltaTime * positionSmoothSpeed
        );

        previewInstance.transform.rotation = Quaternion.Slerp(
            previewInstance.transform.rotation,
            targetRot,
            Time.deltaTime * rotationSmoothSpeed
        );
    }

    // =========================
    // COLLISION
    // =========================

    void CheckCollision()
    {
        isValidPlacement = true;

        GameObject[] gamepoints = GameObject.FindGameObjectsWithTag("Gamepoint");

        foreach (var gp in gamepoints)
        {
            Collider gpCol = gp.GetComponent<Collider>();
            if (gpCol == null) continue;

            foreach (var col in previewColliders)
            {
                if (col.bounds.Intersects(gpCol.bounds))
                {
                    isValidPlacement = false;
                    return;
                }
            }
        }
    }

    // =========================
    // SNAP
    // =========================

    Vector3 GetSnappedPosition(RaycastHit hit)
    {
        Vector3 pos = hit.point;

        Vector3 origin = gridOrigin != null ? gridOrigin.position : Vector3.zero;

        pos.x = Mathf.Round((pos.x - origin.x) / gridSize) * gridSize + origin.x;
        pos.z = Mathf.Round((pos.z - origin.z) / gridSize) * gridSize + origin.z;

        pos.y += pivotToBottomOffset;

        return pos;
    }

    void CalculatePivotOffset()
    {
        Renderer[] renderers = previewInstance.GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0) return;

        Bounds b = renderers[0].bounds;

        foreach (var r in renderers)
            b.Encapsulate(r.bounds);

        pivotToBottomOffset = previewInstance.transform.position.y - b.min.y;
    }

    // =========================
    // TELEPORT API
    // =========================

    public bool CanTeleport()
    {
        return previewInstance != null &&
               previewInstance.activeSelf &&
               isValidPlacement;
    }

    public Vector3 PreviewPositionFromSnap()
    {
        return targetPos;
    }

    public Quaternion PreviewRotationFromSnap()
    {
        return targetRot;
    }
}