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
    public bool _hasHiddenWorkingStatus = false;
    public bool _lastHighlighted = false;
    public AbstractConnection _Connection;
    public Material _RestoreMaterial;
    public Material _highlightMaterial;
    public MeshRenderer _MainMesh;
    public string _TypeName = "Any";
    float mSnapAnimTime = 0;

    [System.Serializable]
    public struct AudioData
    {
        public FMODUnity.StudioEventEmitter
            Connect,
            Disconnect;
    };
    public AudioData Audio;

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

            if (Audio.Disconnect.EventInstance.hasHandle())
            {
                Audio.Disconnect.Play();
            }
           
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
                mSnapAnimTime = 1.0f;
                if (Audio.Connect.EventInstance.hasHandle())
                {
                    Audio.Connect.Play();
                }
            }
        }
    }

    void Awake()
    {
        _Body = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
        
        if(_collider)
        {
            _colliderExt = _collider.bounds.extents;
        }
        else
        {
            _colliderExt.Set(1, 1, 1);
        }

    }

    private void Start()
    {

        if (_Connection)
        {
            var c = _Connection;
            _Connection = null;
            //Disconnect();
            ConnectTo(c);
        }
    }


    public void SetHighlighted(bool isHighlighted)
    {
        if (_MainMesh && isHighlighted != _lastHighlighted)
        {
            if (isHighlighted)
            {
                Assert.IsTrue(_MainMesh.material != _highlightMaterial);
                _RestoreMaterial = _MainMesh.material;
                _MainMesh.material = _highlightMaterial;
            }
            else
            {
                Assert.IsTrue(_RestoreMaterial);
                _MainMesh.material = _RestoreMaterial;
            }
        }
        _lastHighlighted = isHighlighted;
    }

    void FixedUpdate()
    {
        if (_Connection)
        {
            if (mSnapAnimTime > 0)
            {
                mSnapAnimTime -= Time.fixedDeltaTime*8.0f;
                transform.position += ((_Connection._TranformProxy.transform.position + _Connection._TranformProxy.transform.up * 0.9f) - transform.position) * 17.0f * Time.fixedDeltaTime;
            }
            else
            {
                transform.position += ((_Connection._TranformProxy.transform.position ) - transform.position) * 29.0f * Time.fixedDeltaTime;
            }
            transform.rotation = Quaternion.Slerp(transform.rotation, _Connection._TranformProxy.transform.rotation, 12.0f * Time.fixedDeltaTime);
        }
    }
}
