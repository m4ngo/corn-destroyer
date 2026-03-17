using EzySlice;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class PlayerInteraction : MonoBehaviour
{
    public string breakableTag;
    public int minSliceCount = 2;
    public int maxSliceCount = 6;
    public Transform cam;
    public float interactRange = 4.0f;
    public float interactRadius = 0.25f;
    public Material sliceMaterial;

    public int maxShards = 50;
    private List<GameObject> shards = new List<GameObject>();
    private bool isPaused = false;

    private TMP_Text scoreText;
    public int score = 0;
    private float attackTimer = 0.0f;

    public Animator anim;

    [Header("Audio")]
    public int minImpactIndex;
    public int maxImpactIndex;

    private void Start()
    {
        scoreText = GUIManager.Instance.elements[2].GetComponent<TMP_Text>();
    }

    private void Update()
    {
        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
        }
    }

    public void OnAttack(InputValue value)
    {
        if (attackTimer > 0)
        {
            return;
        }

        anim.SetTrigger("swing" + Random.Range(1, 3));
        attackTimer = 0.5f;
        RaycastHit[] hits = Physics.SphereCastAll(cam.position, interactRadius, cam.forward, interactRange);
        foreach (RaycastHit h in hits)
        {
            if (!h.collider.CompareTag(breakableTag))
            {
                continue;
            }
            Shatter(h.transform.gameObject, Random.Range(minSliceCount, maxSliceCount + 1));
            score += Random.Range(1, 5);
            EffectManager.Instance.SpawnEffect(0, h.point);
        }
        scoreText.text = "Score: $" + score + "00";
    }

    public void OnPause(InputValue value)
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0.0f : 1.0f;
        GUIManager.Instance.SetPage(isPaused ? "game_settings" : "game");
        GUIManager.Instance.HideCursor(!isPaused);
    }

    public void Shatter(GameObject original, int count)
    {
        List<GameObject> currentPieces = new List<GameObject>();
        Transform parent = original.transform.parent != null ? original.transform.parent : null;
        Vector3 soundPos = original.transform.position;
        currentPieces.Add(original);

        for (int i = 0; i < count; i++)
        {
            SoundManager.Instance.Play(Random.Range(minImpactIndex, maxImpactIndex), 1.0f, soundPos);
            List<GameObject> newPieces = new List<GameObject>();

            foreach (GameObject piece in currentPieces)
            {
                if (piece == null) continue;

                Vector3 direction = Random.onUnitSphere;

                SlicedHull hull = piece.Slice(piece.transform.position, direction, sliceMaterial);

                if (hull != null)
                {
                    GameObject upper = hull.CreateUpperHull(piece, sliceMaterial);
                    GameObject lower = hull.CreateLowerHull(piece, sliceMaterial);

                    if (parent != null)
                    {
                        upper.transform.SetParent(parent, false);
                        lower.transform.SetParent(parent, false);
                    }

                    SetupSlice(upper);
                    SetupSlice(lower);

                    newPieces.Add(upper);
                    newPieces.Add(lower);

                    Destroy(piece);
                }
                else
                {
                    newPieces.Add(piece);
                }
            }   

            currentPieces = newPieces;
        }
        shards = shards.Concat(currentPieces).ToList();
        PruneShards();
    }

    private void SetupSlice(GameObject slice)
    {
        Rigidbody rb = slice.AddComponent<Rigidbody>();
        rb.mass = 0.01f;
        rb.angularDamping = 25f;
        rb.linearDamping = 0.3f;
        rb.linearVelocity = Random.insideUnitSphere.normalized * Random.Range(2.0f, 3.5f);

        //Bounds b = slice.GetComponent<MeshRenderer>().bounds;
        SphereCollider collider = slice.AddComponent<SphereCollider>();
        //collider.center = b.center - rb.position;
        collider.radius = 0.1f;
    }

    private void PruneShards()
    {
        while (shards.Count > maxShards)
        {
            int rand = Random.Range(0, shards.Count);
            GameObject g = shards[rand];
            shards.RemoveAt(rand);
            Destroy(g);
        }
    }
}