using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TitleUI : MonoBehaviour
{
    [SerializeField] Image titleBG;
    [SerializeField] TMP_Text titleText;
    [SerializeField] float titleTime = 1;
    [SerializeField] float transPercent = 0.1f;

    [Header("")]
    [SerializeField] Color deadColor;
    [SerializeField] List<string> deadLines = new List<string>();

    [Header("")]
    [SerializeField] Color victoryColor;
    [SerializeField] List<string> victoryLines = new List<string>();

    float bgTransparency = 0;
    float titleDuration = 0;
    Queue<TitleData> queue;
    TitleData current;

    public Color DeadColor
    {
        get { return deadColor; }
    }

    public Color VictoryColor
    {
        get { return victoryColor; }
    }

    public string DeadLine
    {
        get { return deadLines[Random.Range(0, deadLines.Count)]; }
    }

    public string VictoryLine
    {
        get { return victoryLines[Random.Range(0, victoryLines.Count)]; }
    }

    // Start is called before the first frame update
    void Start()
    {
        queue = new Queue<TitleData>();
        bgTransparency = titleBG.color.a;
        //AddTitle("test1", Color.red);
        //AddTitle("test2", Color.green);
        //AddTitle("test3", Color.blue);
    }

    // Update is called once per frame
    void Update()
    {
        if (titleDuration < Time.time) current = null;

        if (current == null && queue.Count > 0)
        {
            titleDuration = Time.time + titleTime;
            current = queue.Dequeue();
            current.Apply(titleText);

            titleBG.gameObject.SetActive(true);
            titleText.gameObject.SetActive(true);
        }

        if (current == null)
        {
            titleBG.gameObject.SetActive(false);
            titleText.gameObject.SetActive(false);
            return;
        }

        float percent = 1 - (titleDuration - Time.time) / titleTime;
        if (percent < transPercent)
        {
            float alpha = percent / transPercent;
            titleBG.color = new Color(1, 1, 1, alpha * bgTransparency);
            titleText.color = new Color(titleText.color.r, titleText.color.g, titleText.color.b, alpha);
            //Debug.Log(alpha);
        } else if (percent > 1 - transPercent)
        {
            float alpha = 1 - (percent - (1 - transPercent)) / transPercent;
            titleBG.color = new Color(1, 1, 1, alpha * bgTransparency);
            titleText.color = new Color(titleText.color.r, titleText.color.g, titleText.color.b, alpha);
            //Debug.Log(alpha);
        } else
        {
            titleBG.color = new Color(1, 1, 1, bgTransparency);
            titleText.color = new Color(titleText.color.r, titleText.color.g, titleText.color.b, 1);
        }

        titleText.fontSize = current.FontSize * 0.8f + current.FontSize * 0.2f * percent;
    }

    public void AddTitle(string text, Color color, int fontSize = 72, int spacing = 0)
    {
        TitleData titleData = new TitleData(text, color, fontSize, spacing);
        queue.Enqueue(titleData);
    }
}
