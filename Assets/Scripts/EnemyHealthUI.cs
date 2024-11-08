using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyHealthUI : MonoBehaviour
{
    [SerializeField] GameObject healthUIprefab;

    Dictionary<Entity, (Transform, float, float, float)> data; // UI, 마지막 히트 시간, 마지막 기록된 체력, 마지막 업데이트된 체력

    // Start is called before the first frame update
    void Start()
    {
        data = new Dictionary<Entity, (Transform, float, float, float)>();
    }

    // Update is called once per frame
    void Update()
    {
        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            Entity entity = enemy.GetComponent<Entity>();
            if (!data.ContainsKey(entity))
            {
                GameObject ui = Instantiate(healthUIprefab);
                ui.transform.SetParent(transform);
                ui.SetActive(false);
                data[entity] = (ui.transform, -999, entity.Health, entity.Health);
            }

            (Transform ui, float lastUpdate, float health, float savedHealth) uiData = data[entity];
            Transform entityTransform = entity.transform.Find("lockon") ? entity.transform.Find("lockon") : entity.transform;
            uiData.ui.position = Camera.main.WorldToScreenPoint(entityTransform.position);

            if (entity.Health != uiData.health)
            {
                GaugeUI gauge = uiData.ui.Find("hp").GetComponent<GaugeUI>();
                gauge.SetFilled(entity.Health / entity.MaxHealth);

                TMP_Text damageLabel = uiData.ui.Find("damage").GetComponent<TMP_Text>();
                damageLabel.text = (uiData.savedHealth - entity.Health).ToString();

                float updatedHealth = Time.time - uiData.lastUpdate > 2 ? entity.Health : uiData.savedHealth;
                data[entity] = (uiData.ui, Time.time, entity.Health, updatedHealth);
            }

            uiData.ui.gameObject.SetActive(Time.time - uiData.lastUpdate < 5);
        }
    }
}
