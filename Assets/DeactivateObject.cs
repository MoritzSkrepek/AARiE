using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeactivateObject : MonoBehaviour
{
    public bool isActive;
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Renderer>().enabled = isActive;
    }
}
