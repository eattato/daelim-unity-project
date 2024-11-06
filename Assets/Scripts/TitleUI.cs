using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleUI : MonoBehaviour
{
    [SerializeField] Image titleBG;
    [SerializeField] Color deadColor;
    [SerializeField] Color victoryColor;
    [SerializeField] float titleTime = 1;

    float titleDuration = 0;
    Queue<TitleData> queue;
    TitleData current;

    // Start is called before the first frame update
    void Start()
    {
        queue = new Queue<TitleData>();
    }

    // Update is called once per frame
    void Update()
    {
        if (titleDuration < Time.time && queue.Count > 0)
        {
            current = queue.Dequeue();
        }
    }

    public void AddTitle(Color color, string text, int fontSize, int spacing)
    {
        TitleData titleData = new TitleData(color, text, fontSize, spacing);
        queue.Enqueue(titleData);
    }
}
