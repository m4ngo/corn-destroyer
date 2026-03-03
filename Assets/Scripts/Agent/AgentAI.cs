using UnityEngine;

public class AgentAI : MonoBehaviour
{
    [Header("Stats")]
    public float moveSpeed = 4.0f;
    public float viewDistance = 100.0f;
    public LayerMask wallMask;
    public float followTime = 1.0f;
    private float isFollowing = 0.0f;
    public float waypointDist = 1.0f;

    [Header("Setup")]
    public Transform graphic;
    public Transform[] waypoints;
    public PlayerInteraction target;
    private int currentWaypoint;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        // find and update target
        Physics.Raycast(transform.position,
                        target.transform.position - transform.position,
                        out RaycastHit hit, viewDistance, wallMask);

        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            isFollowing = followTime;
        }
        else
        {
            isFollowing -= Time.deltaTime;
        }

        // check waypoints
        if (isFollowing <= 0)
        {
            if (Vector3.Distance(transform.position, waypoints[currentWaypoint].position) <= waypointDist)
            {
                currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
            }
        }

        // move toward target
        Vector3 vel = GetTarget() - transform.position;
        vel.y = 0f;
        vel = vel.normalized;
        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, vel * moveSpeed, Time.deltaTime * 5f);

        // look at target
        graphic.rotation = Quaternion.Lerp(graphic.rotation, Quaternion.LookRotation(GetTarget() - transform.position), Time.deltaTime * 5f);
    }

    private Vector3 GetTarget()
    {
        return isFollowing > 0 ? target.transform.position : waypoints[currentWaypoint].position;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Break") || collision.collider.CompareTag("Wall"))
        {
            target.Shatter(collision.gameObject, 4);
        }
    }
}
