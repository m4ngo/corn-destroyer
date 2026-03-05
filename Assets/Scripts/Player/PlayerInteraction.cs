using EzySlice;
using System.Collections.Generic;
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

    private bool isPaused = false;

    public void OnAttack(InputValue value)
    {
        RaycastHit[] hits = Physics.SphereCastAll(cam.position, interactRadius, cam.forward, interactRange);
        foreach (RaycastHit h in hits)
        {
            if (!h.collider.CompareTag(breakableTag))
            {
                continue;
            }
            Shatter(h.transform.gameObject, Random.Range(minSliceCount, maxSliceCount + 1));
        }
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
}