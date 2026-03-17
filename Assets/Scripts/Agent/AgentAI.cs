using UnityEngine;
using TMPro;

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
    public int currentWaypoint;
    private Rigidbody rb;
    private GameObject spottedText;
    private TMP_Text scoreText;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        spottedText = GUIManager.Instance.elements[3];
        scoreText = GUIManager.Instance.elements[4].GetComponent<TMP_Text>();
    }

    private void Update()
    {
        spottedText.SetActive(isFollowing > 0);

        // find and update target
        Physics.Raycast(transform.position,
                        target.transform.position - transform.position,
                        out RaycastHit hit, viewDistance, wallMask);

        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            isFollowing = followTime;
        }
        else if (isFollowing > 0)
        {
            isFollowing -= Time.deltaTime;
            if (isFollowing <= 0)
            {
                SetClosestWaypoint();
            }
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

    void SetClosestWaypoint()
    {
        int index = -1;
        float dist = 99999f;
        for (int i = 0; i < waypoints.Length; i++)
        {
            float d = Vector3.Distance(waypoints[i].position, transform.position);
            if (d <= dist)
            {
                dist = d;
                index = i;
            }
        }
        currentWaypoint = index;
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
        else if (collision.collider.CompareTag("Player"))
        {
            scoreText.text = $"You caused ${FindObjectsByType<PlayerInteraction>(FindObjectsSortMode.None)[0].score}00 of property damage!";
            LoadSceneManager.Instance.GameOver();
        }
    }
}
