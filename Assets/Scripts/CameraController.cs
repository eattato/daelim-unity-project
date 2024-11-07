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
    Vector3 camForward = Vector3.zero;
    Vector3 rotationOffset = Vector3.zero;
    List<Dictionary<string, float>> camShakeList;

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
        camForward = transform.forward;
        camShakeList = new List<Dictionary<string, float>>();
    }

    // Update is called once per frame
    void Update()
    {
        trackPos = camTarget.position + camOffset;
        CheckLockon();

        if (lockon)
        {
            Vector3 lookVector = Vector3Utils.LookVector(trackPos, lockon.position);
            camForward = Vector3.SmoothDamp(camForward, lookVector, ref lookVelocity, lookSmoothTime);
            transform.forward = camForward;
            transform.position = trackPos;
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
            transform.position = trackPos + camForward * -camDistance;
            transform.LookAt(trackPos);
            camForward = transform.forward;
        }

        // 이펙트 적용
        rotationOffset = Vector3.zero;
        List<Dictionary<string, float>> nextCamShakeList = new List<Dictionary<string, float>>();

        foreach (Dictionary<string, float> camShakeInfo in camShakeList)
        {
            float percent = 1 - (Time.time - camShakeInfo["start"]) / camShakeInfo["duration"];
            if (percent < 0) return;

            float amount = camShakeInfo["amount"] * percent;
            rotationOffset += Random.insideUnitSphere * amount;
            nextCamShakeList.Add(camShakeInfo);
        }

        transform.rotation *= Quaternion.Euler(rotationOffset);
        camShakeList = nextCamShakeList;
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

    public void AddCamShake(float amount, float duration)
    {
        Dictionary<string, float> camShakeInfo = new Dictionary<string, float>();
        camShakeInfo.Add("start", Time.time);
        camShakeInfo.Add("duration", duration);
        camShakeInfo.Add("amount", amount);
        camShakeList.Add(camShakeInfo);
    }
}
