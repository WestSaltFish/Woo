using System.Collections.Generic;
using UnityEngine;

public class Server : MonoBehaviour
{
    public static Server instance = null;

    private Dictionary<uint, Client> _clients = new();
    public void Awake()
    {
        if (instance != null)
            Destroy(gameObject);
        else
            instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
