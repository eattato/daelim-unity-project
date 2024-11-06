using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleData
{
    Color color;
    string text;
    int fontSize;
    int spacing;

    public TitleData(Color color, string text, int fontSize, int spacing)
    {
        this.color = color;
        this.text = text;
        this.fontSize = fontSize;
        this.spacing = spacing;
    }
}
