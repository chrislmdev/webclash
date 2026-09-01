using UnityEngine;

public enum BoosterType
{
    SpeedBoost,
    DoubleJump
}

/// <summary>
/// Collectible booster that applies a temporary effect to the player.
/// </summary>
[RequireComponent(typeof(Collider))]
public class BoosterPack : MonoBehaviour
{
    [SerializeField] private BoosterType boosterType = BoosterType.SpeedBoost;
    [SerializeField] private float spinSpeed = 90f;
    [SerializeField] private float bobAmplitude = 0.25f;
    [SerializeField] private float bobFrequency = 2f;

    private Vector3 startLocalPosition;
    private ObjectPooler ownerPool;
    private string poolTag;

    public string PoolTag => poolTag;

    private void Awake()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
        startLocalPosition = transform.localPosition;
    }

    public void Initialize(ObjectPooler pool, string tag)
    {
        ownerPool = pool;
        poolTag = tag;
    }

    private void Update()
    {
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);
        float bob = Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
        transform.localPosition = startLocalPosition + Vector3.up * bob;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        var player = other.GetComponent<PlayerController>();
        if (player == null || !player.IsAlive)
        {
            return;
        }

        switch (boosterType)
        {
            case BoosterType.SpeedBoost:
                player.ApplySpeedBoost();
                break;
            case BoosterType.DoubleJump:
                player.GrantDoubleJump();
                player.TryDoubleJump();
                break;
        }

        ReturnToPool();
    }

    private void ReturnToPool()
    {
        if (ownerPool != null && !string.IsNullOrEmpty(poolTag))
        {
            ownerPool.ReturnToPool(poolTag, gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
