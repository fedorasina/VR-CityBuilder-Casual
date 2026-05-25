using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpawnFromList : MonoBehaviour
{
    [Header("Prefabs to spawn")]
    public List<GameObject> prefabs;

    [Header("Spawn points")]
    public List<Transform> spawnPoints;

    [Header("Parent (optional)")]
    public Transform parent;

    [Header("Check settings")]
    public float checkRadius = 0.5f;
    public string requiredTag = "Gamepoint";

    [Header("Spawn effect (scale)")]
    public float spawnAnimDuration = 0.3f;

    [Header("Physics settings")]
    public float kinematicDelay = 1f;

    [Header("Sound")]
    public AudioSource audioSource;
    public AudioClip spawnSound;

    [Header("VFX")]
    public GameObject vfxPrefab; // Particle System или VFX Graph
    public float vfxDestroyDelay = 2f;

    public void Spawn()
    {
        if (prefabs.Count == 0 || spawnPoints.Count == 0)
        {
            Debug.LogWarning("Prefabs or spawn points not assigned!");
            return;
        }

        foreach (Transform point in spawnPoints)
        {
            if (IsOccupied(point.position))
                continue;

            PlayVFX(point.position);

            GameObject prefab = prefabs[Random.Range(0, prefabs.Count)];
            GameObject obj = Instantiate(prefab, point.position, point.rotation, parent);

            StartCoroutine(SpawnEffect(obj));
            StartCoroutine(HandleKinematic(obj));

            if (audioSource != null && spawnSound != null)
                audioSource.PlayOneShot(spawnSound);
        }
    }

    void PlayVFX(Vector3 position)
    {
        if (vfxPrefab == null)
            return;

        GameObject vfx = Instantiate(vfxPrefab, position, Quaternion.identity);

        // автоудаление эффекта
        Destroy(vfx, vfxDestroyDelay);
    }

    bool IsOccupied(Vector3 position)
    {
        Collider[] colliders = Physics.OverlapSphere(position, checkRadius);

        foreach (Collider col in colliders)
        {
            if (col.CompareTag(requiredTag))
                return true;
        }

        return false;
    }

    IEnumerator SpawnEffect(GameObject obj)
    {
        float time = 0f;

        Vector3 targetScale = obj.transform.localScale;
        obj.transform.localScale = Vector3.zero;

        while (time < spawnAnimDuration)
        {
            float t = time / spawnAnimDuration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            obj.transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, smoothT);

            time += Time.deltaTime;
            yield return null;
        }

        obj.transform.localScale = targetScale;
    }

    IEnumerator HandleKinematic(GameObject obj)
    {
        Rigidbody rb = obj.GetComponent<Rigidbody>();

        if (rb == null)
            yield break;

        rb.isKinematic = false;

        yield return new WaitForSeconds(kinematicDelay);

        rb.isKinematic = true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        if (spawnPoints == null) return;

        foreach (Transform point in spawnPoints)
        {
            if (point != null)
                Gizmos.DrawWireSphere(point.position, checkRadius);
        }
    }
}