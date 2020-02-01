using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbstractConnection : MonoBehaviour
{
    ComponentBase _Component;
    
    public event System.Action<AbstractConnection> OnStateChanged;
}

public class ComponentBase : MonoBehaviour
{
    public Rigidbody _Body;
    public Collider _collider;
    AbstractConnection _Connection;
    
    void Awake()
    {
        _Body = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>(); 
    }
}
