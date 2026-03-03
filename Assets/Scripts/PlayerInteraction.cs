using EzySlice;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public GameObject objectToSlice; // non-null

    private void Start()
    {
        Shatter(objectToSlice, 5);
    }

    public void Shatter(GameObject o, int count)
    {
        for (int i = 0; i < count; ++i)
        {
            Vector3 rand = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
            GameObject[] slices = Slice(o, o.transform.position, rand);
            foreach (GameObject slice in slices)
            {
                Rigidbody rb = slice.AddComponent<Rigidbody>();
                rb.mass = 0.01f;
                rb.angularDamping = 5.0f;
                rb.linearDamping = 1.0f;
                slice.AddComponent<SphereCollider>().radius = 0.2f;
            }
        }
        Destroy(o);
    }

    public GameObject[] Slice(GameObject o, Vector3 planeWorldPosition, Vector3 planeWorldDirection)
    {
        return o.SliceInstantiate(planeWorldPosition, planeWorldDirection);
    }
}
