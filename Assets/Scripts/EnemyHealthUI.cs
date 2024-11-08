using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyHealthUI : MonoBehaviour
{
    [SerializeField] Entity target;
    [SerializeField] float damageStackTime = 2;
    [SerializeField] float disappearTime = 7;
    [SerializeField] bool isBoss;

    CanvasGroup canvasGroup;
    GaugeUI gauge;
    TMP_Text damageLabel;
    TMP_Text nameLabel;

    float health = 0;
    float maxHealth = 0;
    float damageStack = 0;
    float lastUpdateTime = -999;

    // Start is called before the first frame update
    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        gauge = transform.Find("hp").GetComponent<GaugeUI>();
        damageLabel = transform.Find("damage").GetComponent<TMP_Text>();
        nameLabel = transform.Find("damage").GetComponent<TMP_Text>();
        health = target.Health;
        maxHealth = target.MaxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isBoss)
        {
            Transform targetTransform = target.transform.Find("lockon") ? target.transform.Find("lockon") : target.transform;
            transform.position = Camera.main.WorldToScreenPoint(targetTransform.position);
        }

        if (health != target.Health) // health changed
        {
            gauge.SetFilled(target.Health / target.MaxHealth);
            if (health > target.Health)
            {
                damageStack += health - target.Health;
                damageLabel.text = damageStack.ToString();
            }

            lastUpdateTime = Time.time;
            health = target.Health;
            canvasGroup.alpha = 1;
        }

        if (Time.time - lastUpdateTime > damageStackTime)
        {
            damageStack = 0;
            damageLabel.text = "";
        }

        if (!isBoss)
        {
            if (Time.time - lastUpdateTime > disappearTime) canvasGroup.alpha = 0;
        }
    }

    public void SetTarget(Entity target)
    {
        this.target = target;
    }
}
