using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundMachine : AbstractMachine
{
    public Vector3 movespeed = new Vector3(0,0, -80);
    private void FixedUpdate()
    {
        if(isWorking)
        {
            transform.position += movespeed * Time.deltaTime;
        }
    }
}
