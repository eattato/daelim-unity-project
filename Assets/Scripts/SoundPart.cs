using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundPart : MonoBehaviour
{
    [SerializeField] float soundRange = 5f;

    public float SoundRange
    {
        get { return soundRange; }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, soundRange);
    }
}
