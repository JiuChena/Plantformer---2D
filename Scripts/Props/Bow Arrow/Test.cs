using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour, IArrowAttacked
{
    private void Start()
    {
        
    }

    private void Update()
    {
        
    }

    public void Attacked()
    {
        Destroy(this.gameObject);
    }
}
