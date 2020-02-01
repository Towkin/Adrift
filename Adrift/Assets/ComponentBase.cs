using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbstractConnection : MonoBehaviour
{
    ComponentBase _Component;
}

public class ComponentBase : MonoBehaviour
{
    public Rigidbody _Body;
    public Collider _collider;
    AbstractConnection _Connection;
    // Start is called before the first frame update
    void Start()
    {
        _Body = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>(); 
    }

    // Update is called once per frame
    void Update()
    {

    }
}
