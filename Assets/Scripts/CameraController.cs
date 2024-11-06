using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{
    
    [Header("기본 설정")]
    [SerializeField] Transform camTarget;
    [SerializeField] Vector3 camOffset = Vector3.zero;
    [SerializeField] float camDistance = 3;
    [SerializeField] float camSensitivity = 1f;

    [Header("락온 설정")]
    [SerializeField] RectTransform lockonUi;
    [SerializeField] float lookSmoothTime = 0.01f;
    [SerializeField] float lockonMaxDistance = 30f;
    [SerializeField] float lockonMinSize = 0.1f;
    [SerializeField] float lockonMaxSize = 1f;

    Transform lockon = null;
    Vector3 lookVelocity = Vector3.zero;
    Vector3 trackPos = Vector3.zero;

    public Vector3 TrackPos
    {
        get { return trackPos; }
    }

    public Transform Lockon
    {
        get { return lockon; }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        trackPos = camTarget.position + camOffset;
        CheckLockon();

        if (lockon)
        {
            Vector3 lookVector = Vector3Utils.LookVector(trackPos, lockon.position);
            transform.position = trackPos;
            transform.forward = Vector3.SmoothDamp(transform.forward, lookVector, ref lookVelocity, lookSmoothTime);
            //transform.LookAt(lockon);
            transform.position += transform.forward * -camDistance;

            float distance = Vector3.Distance(Camera.main.transform.position, lockon.position);
            float sizeFactor = Mathf.Clamp01(distance / lockonMaxDistance);
            float scale = Mathf.Lerp(lockonMaxSize, lockonMinSize, sizeFactor);
            lockonUi.position = Camera.main.WorldToScreenPoint(lockon.position);
            lockonUi.localScale = new Vector3(scale, scale, 1);
        }
        else
        {
            Quaternion rot = Quaternion.Euler(new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0) * camSensitivity);
            transform.rotation *= rot;
            transform.position = trackPos + transform.forward * -camDistance;
            transform.LookAt(trackPos);
        }
    }

    void CheckLockon()
    {
        if (!Input.GetKeyDown(KeyCode.Q)) return;

        if (lockon)
        {
            lockon = null;
            lockonUi.gameObject.SetActive(false);
            return;
        }

        // 락온 대상 지정
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies.Length == 0) return;

        lockon = enemies[0].transform.Find("lockon");
        lockon = lockon ? lockon : enemies[0].transform;

        foreach (GameObject enemy in enemies)
        {
            Transform lockTransform = enemy.transform.Find("lockon");
            lockTransform = lockTransform ? lockTransform : enemy.transform;

            Vector3 oldLookVector = (lockon.position - transform.position).normalized;
            Vector3 currentLookVector = (lockTransform.position - transform.position).normalized;

            float oldUnit = Vector3.Dot(transform.forward, oldLookVector);
            float currentUnit = Vector3.Dot(transform.forward, currentLookVector);

            // 일치할수록 1, 반대일수록 -1
            if (currentUnit > oldUnit)
            {
                lockon = lockTransform;
            }
        }

        if (lockon)
        {
            lockonUi.gameObject.SetActive(true);
        }
    }
}
