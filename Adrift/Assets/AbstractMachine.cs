using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class AbstractMachine : MonoBehaviour
{
    public GameObject[] _IfActiveObject; //Object activated when the machine is active and hidden when inactive
    public GameObject[] _IfInactiveObject; //Object activated when the machine is active and hidden when inactive
    public AbstractConnection[] _connectionRoots;
    public event System.Action<AbstractMachine> OnStateChanged;
    public AbstractMachine[] _dependentOnMachines; //a list of machines that need to work for this machine to work


    private void Awake()
    {
        foreach (AbstractConnection c in _connectionRoots)
        {
            SetupConectionsRecursive(c);
        }
        foreach (var m in _dependentOnMachines)
        {
            SetupDependecyMachinesRecursive(m);
        }
        UpdateIsWorkingState();

        ApplyWorkingState();
    }


    void SetupDependecyMachinesRecursive(AbstractMachine machine)
    {
        machine.OnStateChanged += Con_OnStateChanged;
        foreach (AbstractMachine m in machine._dependentOnMachines)
        {
            SetupDependecyMachinesRecursive(m);
        }
    }
    void SetupConectionsRecursive(AbstractConnection con)
    {
        con.OnStateChanged += Con_OnStateChanged;
        foreach(AbstractConnection c in con._dependentConnections)
        {
            SetupConectionsRecursive(c);
        }
    }

    protected void Con_OnStateChanged(AbstractMachine obj)
    {
        UpdateIsWorkingState();
    }
    protected void Con_OnStateChanged(AbstractConnection obj)
    {
        UpdateIsWorkingState();
    }

    public bool isWorking = false;
    bool CheckIsWorkingState(AbstractConnection con)
    {
        if (con._Component)
        {
            if(!con._Component._isWorking)
            {
                return false;
            }
        }
        else
        {
            return false;
        } 
        foreach (AbstractConnection c in con._dependentConnections)
        {
            if(!CheckIsWorkingState(c))
            {
                return false;
            }
        }
        return true;
    }


    private void UpdateIsWorkingState()
    {
        bool lastIsWorking = isWorking;
        isWorking = true;
        foreach (AbstractMachine m in _dependentOnMachines)
        {
            if (!m.isWorking)
            {
                isWorking = false;
                break;
            }
        }
        if (isWorking)
        {
            foreach (AbstractConnection c in _connectionRoots)
            {
                if (!CheckIsWorkingState(c))
                {
                    isWorking = false;
                    break;
                }
            }
        }
        if (lastIsWorking != isWorking)
        {
            ApplyWorkingState();
        }
    }

    void ApplyWorkingState()
    {
        if (OnStateChanged != null)
        {
            OnStateChanged.Invoke(this);
        }
        foreach (var obj in _IfActiveObject)
        {
            obj.SetActive(isWorking);
        }
        foreach (var obj in _IfInactiveObject)
        {
            obj.SetActive(!isWorking);
        }
    }

}
