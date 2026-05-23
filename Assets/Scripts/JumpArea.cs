using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpArea : MonoBehaviour
{
    public bool Jump = true;

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            //    if (Jump)
            //    {
            //         = GetComponent<Rigidbody>();

            //        Rigidbody.
            //    }
        }
    }
}
