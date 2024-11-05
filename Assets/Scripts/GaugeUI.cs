using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GaugeUI : MonoBehaviour
{
    [SerializeField] float filled = 1;
    [SerializeField] float reducedSpeed = 3;
    [SerializeField] Image gauge;
    [SerializeField] Image reduced;

    // Update is called once per frame
    void Update()
    {
        float reducedFill = Mathf.Lerp(reduced.fillAmount, filled, Time.deltaTime * reducedSpeed);
        gauge.fillAmount = filled;
        reduced.fillAmount = filled < reducedFill ? reducedFill : filled;
    }

    public void SetFilled(float filled)
    {
        this.filled = Mathf.Clamp(filled, 0, 1);
    }
}
