using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Touch-driven web swinging via raycast, SpringJoint, and LineRenderer.
/// Attach to the player alongside PlayerController.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class WebShooter : MonoBehaviour
{
    [Header("Raycast")]
    [SerializeField] private float maxWebDistance = 40f;
    [SerializeField] private float coneHalfAngle = 22.5f;
    [SerializeField] private LayerMask webTargetLayers = ~0;
    [SerializeField] private string buildingTag = "Building";

    [Header("Spring Joint")]
    [SerializeField] private float spring = 120f;
    [SerializeField] private float damper = 8f;
    [SerializeField] private float minDistance = 2f;
    [SerializeField] private float maxDistance = 30f;

    [Header("Visual")]
    [SerializeField] private Material webLineMaterial;
    [SerializeField] private float webLineWidth = 0.06f;
    [SerializeField] private Transform webOrigin;

    private Rigidbody rb;
    private PlayerController playerController;
    private SpringJoint activeJoint;
    private LineRenderer activeLine;
    private Vector3 anchorPoint;
    private bool isSwinging;
    private bool inputHeld;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerController = GetComponent<PlayerController>();

        if (webOrigin == null)
        {
            webOrigin = transform;
        }
    }

    private void Update()
    {
        if (playerController != null && !playerController.IsAlive)
        {
            ReleaseWeb();
            return;
        }

        bool pressedThisFrame = false;
        bool releasedThisFrame = false;

        if (Touchscreen.current != null)
        {
            var touch = Touchscreen.current.primaryTouch;
            pressedThisFrame = touch.press.wasPressedThisFrame;
            releasedThisFrame = touch.press.wasReleasedThisFrame;
            inputHeld = touch.press.isPressed;
        }
        else if (Mouse.current != null)
        {
            pressedThisFrame = Mouse.current.leftButton.wasPressedThisFrame;
            releasedThisFrame = Mouse.current.leftButton.wasReleasedThisFrame;
            inputHeld = Mouse.current.leftButton.isPressed;
        }

        if (pressedThisFrame)
        {
            TryAttachWeb();
        }

        if (releasedThisFrame || (!inputHeld && isSwinging))
        {
            ReleaseWeb();
        }

        UpdateWebVisual();
    }

    private void TryAttachWeb()
    {
        if (isSwinging)
        {
            return;
        }

        Vector3 origin = webOrigin.position;
        Vector3 direction = GetRandomDirectionInCone(transform.forward, coneHalfAngle);

        if (!Physics.Raycast(origin, direction, out RaycastHit hit, maxWebDistance, webTargetLayers, QueryTriggerInteraction.Ignore))
        {
            return;
        }

        if (!hit.collider.CompareTag(buildingTag))
        {
            return;
        }

        anchorPoint = hit.point;
        activeJoint = gameObject.AddComponent<SpringJoint>();
        activeJoint.autoConfigureConnectedAnchor = false;
        activeJoint.anchor = transform.InverseTransformPoint(webOrigin.position);
        activeJoint.connectedAnchor = anchorPoint;
        activeJoint.spring = spring;
        activeJoint.damper = damper;
        activeJoint.minDistance = minDistance;
        activeJoint.maxDistance = maxDistance;

        activeLine = CreateLineRenderer();
        isSwinging = true;
    }

    private void ReleaseWeb()
    {
        if (activeJoint != null)
        {
            Destroy(activeJoint);
            activeJoint = null;
        }

        if (activeLine != null)
        {
            Destroy(activeLine.gameObject);
            activeLine = null;
        }

        isSwinging = false;
    }

    private void UpdateWebVisual()
    {
        if (activeLine == null)
        {
            return;
        }

        activeLine.SetPosition(0, webOrigin.position);
        activeLine.SetPosition(1, anchorPoint);
    }

    private LineRenderer CreateLineRenderer()
    {
        var lineObject = new GameObject("WebLine");
        lineObject.transform.SetParent(transform, false);

        var line = lineObject.AddComponent<LineRenderer>();
        line.positionCount = 2;
        line.startWidth = webLineWidth;
        line.endWidth = webLineWidth;
        line.useWorldSpace = true;
        line.numCapVertices = 4;

        if (webLineMaterial != null)
        {
            line.material = webLineMaterial;
        }

        return line;
    }

    private static Vector3 GetRandomDirectionInCone(Vector3 forward, float halfAngleDegrees)
    {
        float halfAngleRad = halfAngleDegrees * Mathf.Deg2Rad;
        float z = Random.Range(Mathf.Cos(halfAngleRad), 1f);
        float t = Random.Range(0f, Mathf.PI * 2f);
        float r = Mathf.Sqrt(1f - z * z);
        float x = r * Mathf.Cos(t);
        float y = r * Mathf.Sin(t);

        Vector3 localDir = new Vector3(x, y, z);
        return Quaternion.LookRotation(forward.normalized, Vector3.up) * localDir;
    }

    private void OnDisable()
    {
        ReleaseWeb();
    }
}
