using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundPart : MonoBehaviour
{
    [SerializeField] float soundRange = 5f;
    [SerializeField] float duration = 1f;
    [SerializeField] AudioClip audioClip;

    AudioSource audioSource;
    float lifeTime = 0;
    bool init = false;

    public float SoundRange
    {
        get { return soundRange; }
    }

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (init && Time.time > lifeTime)
        {
            SoundManager.Instance.PutSoundPart(gameObject);
        }
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, soundRange);
    }

    public void Init(AudioClip clip, float soundRange, float duration)
    {
        this.audioClip = clip;
        this.soundRange = soundRange;
        this.lifeTime = Time.time + duration;
        this.init = true;
        audioSource.PlayOneShot(clip);
    }

    public void ResetStatus()
    {
        this.init = false;
    }
}
