using UnityEngine;

public class ActivateIfGamecontrolActive : MonoBehaviour
{
    [Header("Object to check")]
    public string targetTag = "Preview";

    [Header("Object to toggle")]
    public GameObject targetObject;

    void Update()
    {
        GameObject control = GameObject.FindGameObjectWithTag(targetTag);

        bool active = control != null && control.activeInHierarchy;

        if (targetObject != null && targetObject.activeSelf != active)
        {
            targetObject.SetActive(active);
        }
    }
}