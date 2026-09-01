using UnityEngine;

/// <summary>
/// Kill zone trigger for the lava floor plane at Y = 0.
/// </summary>
[RequireComponent(typeof(Collider))]
public class FloorIsLava : MonoBehaviour
{
    private void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        var player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.OnLavaContact();
            return;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.TriggerGameOver();
        }
    }
}
