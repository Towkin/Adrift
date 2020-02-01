using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;


public class AbstractConnection : MonoBehaviour
{
    public Material _highlightMaterial;
    Material _RestoreMaterial;
    public MeshRenderer _MainMesh;
    public ComponentBase _Component;
    public AbstractConnection[] _dependentConnections;
    public event System.Action<AbstractConnection> OnStateChanged;
    public GameObject _TranformProxy;
    public string _TypeName = "Any";
    public bool CanDisconnect()
    {
        return true;
    }
    private void Awake()
    {
        _TranformProxy.SetActive(false);
        _TranformProxy.transform.hasChanged = false;
    }

    public bool CanConnect(ComponentBase comp)
    {
        if (_TypeName == "Any" || _TypeName == comp._TypeName)
        {
            return true;
        }
        return false;
    }

    public bool IsOccupied() //occupied 
    {
        return _Component != null;
    }

    public bool SetConnection(ComponentBase component)
    {
        if(_Component)
        {
            Assert.IsTrue(_Component._Connection == this);
            _Component._Connection = null;
        }
        _Component = component;
        if (_Component)
        {
            _Component._Connection = this;
        }
        if (OnStateChanged != null)
        {
            OnStateChanged.Invoke(this);
        }
        Assert.IsTrue(!_TranformProxy.transform.hasChanged);
        return true;
    }

    bool _lastHighlighted;

    public void SetHighlighted(bool isHighlighted)
    {
        if(isHighlighted != _lastHighlighted)
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
}
