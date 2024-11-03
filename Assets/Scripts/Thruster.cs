using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Thruster : MonoBehaviour
{
    Rigidbody rigid;
    List<Thrust> thrustList;

    // Start is called before the first frame update
    void Start()
    {
        rigid = GetComponent<Rigidbody>();
        thrustList = new List<Thrust>();
    }

    // Update is called once per frame
    void Update()
    {
        List<Thrust> nextThrustList = new List<Thrust>();

        foreach (Thrust thrust in thrustList)
        {
            if (thrust.EndTime < Time.time) continue;
            nextThrustList.Add(thrust);
            rigid.AddForce(thrust.Force - rigid.velocity);
        }

        thrustList = nextThrustList;
    }

    public void AddThrust(Vector3 force, float duration)
    {
        Thrust thrust = new Thrust(force, duration);
        thrustList.Add(thrust);
    }
}
