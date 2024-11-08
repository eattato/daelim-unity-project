using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthUIManager : MonoBehaviour
{
    [SerializeField] GameObject healthUIprefab;

    Dictionary<Entity, EnemyHealthUI> data;

    // Start is called before the first frame update
    void Start()
    {
        data = new Dictionary<Entity, EnemyHealthUI>();
    }

    // Update is called once per frame
    void Update()
    {
        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            Entity entity = enemy.GetComponent<Entity>();
            if (!data.ContainsKey(entity))
            {
                GameObject uiObject = Instantiate(healthUIprefab);
                uiObject.transform.SetParent(transform);

                EnemyHealthUI ui = uiObject.GetComponent<EnemyHealthUI>();
                ui.SetTarget(entity);
                data[entity] = ui;
            }
        }
    }
}
