using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TitleData
{
    string text;
    Color color;
    int fontSize;
    int spacing;

    public TitleData(string text, Color color, int fontSize, int spacing)
    {
        this.text = text;
        this.color = color;
        this.fontSize = fontSize;
        this.spacing = spacing;
    }

    public void Apply(TMP_Text label)
    {
        label.text = text;
        label.color = color;
        label.fontSize = fontSize;
        label.characterSpacing = spacing;
    }
}
