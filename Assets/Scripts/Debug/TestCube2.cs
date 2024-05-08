using UnityEngine;

public class TestCube2 : MonoBehaviour
{
    private Rigidbody _rb;

    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.y <= -5)
        {
            transform.position = new(0, 2, 0);
            _rb.velocity = Vector3.zero;
        }
    }
}
