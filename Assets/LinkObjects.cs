using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinkObjects : MonoBehaviour
{
    public GameObject startObject;
    public GameObject targetObject;
    public GameObject linkEffect;
    public bool linkOnStart = false;

    private LineRenderer lr;
    private bool onWork;

    // Start is called before the first frame update
    void Start()
    {
        lr = GetComponent<LineRenderer>();
        if (linkOnStart) LinkObjectsWithLine();
    }


    // Update is called once per frame
    void Update()
    {
        if (!onWork) return;

        LinkObjectsWithLine();
        if (linkEffect) linkEffect.transform.position = CalculateMidPoint();

        if (Vector3.Distance(startObject.transform.position, targetObject.transform.position) < 0.5f)
            GetComponentInParent<AIMonsterController>().EndDizzyState();
    }

    void LinkObjectsWithLine()
    {
        lr.positionCount = 2;
        lr.SetPosition(0, startObject.transform.position);
        lr.SetPosition(1, targetObject.transform.position);
    }

    Vector3 CalculateMidPoint()
    {
        Vector3 midPos = startObject.transform.position + 0.5f * (targetObject.transform.position - startObject.transform.position);

        return midPos;
    }

    public void StartSoul()
    {
        lr.enabled = true;
        targetObject.SetActive(true);
        if (linkEffect) linkEffect.SetActive(true);
        onWork = true;
    }
    public void EndSoul()
    {
        lr.enabled = false;
        targetObject.SetActive(false);
        if (linkEffect) linkEffect.SetActive(false);
        onWork = false;
    }

    public void StartMonsterLink()
    {
        lr.enabled = true;
        onWork = true;
    }
}
