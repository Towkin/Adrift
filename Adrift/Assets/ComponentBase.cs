using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;


public class ComponentBase : MonoBehaviour
{
    public Rigidbody _Body;
    public Collider _collider;
    public Vector3 _colliderExt;
    public bool _isLocked = false;
    public bool _isWorking = true;
    public AbstractConnection _Connection;

    public bool IsConnected()
    {
        return _Connection != null;
    }

    public bool CanDisconnect()
    {
        if(!_Connection)
        {
            return true;
        }
        return !_isLocked && _Connection.CanDisconnect();
    }

    public void Disconnect()
    {
        if (_Connection)
        {
            _Body.isKinematic = false;
            _Connection.SetConnection(null);
            Assert.IsTrue(_Connection == null);
        }
    }

    public void ConnectTo(AbstractConnection con)
    {
        if(!IsConnected())
        {
            if(con && con.CanConnect(this))
            {
                con.SetConnection(this);
                Assert.IsTrue(_Connection == con);

                _Body.isKinematic = true;
                transform.SetPositionAndRotation(_Connection._TranformProxy.transform.position, _Connection._TranformProxy.transform.rotation);
            }
        }
    }

    void Awake()
    {
        _Body = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
        if(_Connection)
        {
            var c = _Connection;
            Disconnect();
            ConnectTo(c);
        }
        if(_collider)
        {
            _colliderExt = _collider.bounds.extents;
        }
        else
        {
            _colliderExt.Set(1, 1, 1);
        }
    }
}
