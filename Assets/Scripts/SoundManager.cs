using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private GameObject prefab;

    [Header("사운드 바인딩")]
    [SerializeField] public List<AudioClip> hurtSounds = new List<AudioClip>();
    [SerializeField] public AudioClip parrySound;

    private static SoundManager instance = null;
    private Queue<GameObject> queue;
    
    public static SoundManager Instance
    {
        get { return instance; }
    }

    // Start is called before the first frame update
    void Awake()
    {
        if (instance)
        {
            Destroy(this.gameObject);
            return;
        }

        instance = this;
        queue = new Queue<GameObject>();
        DontDestroyOnLoad(this.gameObject);
    }

    public void PutSoundPart(GameObject soundPart)
    {
        soundPart.GetComponent<SoundPart>().ResetStatus();
        soundPart.SetActive(false);
        soundPart.transform.SetParent(transform);
        queue.Enqueue(soundPart);
    }

    public GameObject CreateSoundPart(Vector3 pos, AudioClip clip, float soundRange, float lifeTime = 1)
    {
        if (queue.Count == 0) queue.Enqueue(Instantiate(prefab));

        GameObject soundPart = queue.Dequeue();
        soundPart.GetComponent<SoundPart>().ResetStatus();
        soundPart.transform.SetParent(null);
        soundPart.transform.position = pos;
        soundPart.SetActive(true);

        SoundPart soundScript = soundPart.GetComponent<SoundPart>();
        soundScript.Init(clip, soundRange, lifeTime);
        return soundPart;
    }
}
