using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using System.Linq;
using System.Collections;

[RequireComponent(typeof(XRGrabInteractable))]
public class RightTriggerTeleportToPreview : MonoBehaviour
{
    [Header("VFX")]
    public GameObject teleportVFX;

    [Header("Sound")]
    public AudioSource audioSource;

    private XRGrabInteractable grabInteractable;
    private InputAction triggerAction;
    private XRDragPreviewSnap previewSnap;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();

        triggerAction = new InputAction(
            binding: "<XRController>{RightHand}/trigger"
        );
    }

    void OnEnable()
    {
        triggerAction.Enable();
        triggerAction.performed += OnTriggerPressed;
    }

    void OnDisable()
    {
        triggerAction.performed -= OnTriggerPressed;
        triggerAction.Disable();
    }

    void OnTriggerPressed(InputAction.CallbackContext ctx)
    {
        if (!grabInteractable.isSelected)
            return;

        previewSnap = GetComponent<XRDragPreviewSnap>();

        if (previewSnap == null)
            return;

        if (!previewSnap.CanTeleport())
            return;

        StartCoroutine(TeleportRoutine());
    }

    IEnumerator TeleportRoutine()
    {
        // Отпускаем объект
        var interactor = FindObjectsByType<XRBaseInteractor>(FindObjectsSortMode.None)
            .FirstOrDefault(i =>
                i.hasSelection &&
                i.firstInteractableSelected == grabInteractable);

        if (interactor != null && interactor.interactionManager != null)
        {
            interactor.interactionManager.SelectExit(
                (IXRSelectInteractor)interactor,
                (IXRSelectInteractable)grabInteractable
            );
        }

        yield return null;


        transform.SetPositionAndRotation(
            previewSnap.PreviewPositionFromSnap(),
            previewSnap.PreviewRotationFromSnap()
        );

        Physics.SyncTransforms();


        if (teleportVFX != null)
        {
            GameObject vfx = Instantiate(
                teleportVFX,
                transform.position,
                Quaternion.identity
            );

            Destroy(vfx, 3f);
        }

        if (audioSource != null)
        {
            audioSource.Play();
        }
    }
}