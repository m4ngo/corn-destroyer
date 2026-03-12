using EzySlice;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

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

    private void Start()
    {
        scoreText = GUIManager.Instance.elements[2].GetComponent<TMP_Text>();
    }

    public void OnAttack(InputValue value)
    {
        Physics.SphereCast(cam.position, interactRadius, cam.forward, out RaycastHit hit, interactRange);
        if (hit.collider == null || !hit.collider.CompareTag(breakableTag))
        {
            return;
        }
        Shatter(hit.transform.gameObject, Random.Range(minSliceCount, maxSliceCount + 1));
        score += hit.collider.GetComponent<Breakable>().score;
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
        currentPieces.Add(original);

        for (int i = 0; i < count; i++)
        {
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
        rb.angularDamping = 2f;
        rb.linearDamping = 0.3f;

        Bounds b = slice.GetComponent<MeshRenderer>().bounds;
        BoxCollider collider = slice.AddComponent<BoxCollider>();
        collider.center = b.center - rb.position;
        collider.size = b.size / 3f;
    }

    private void PruneShards()
    {
        while (shards.Count > maxShards)
        {
            GameObject g = shards[0];
            shards.RemoveAt(0);
            Destroy(g);
        }
    }
}