using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Niddle : MonoBehaviour
{
    public GameObject[] disableObjects;
    public UnityEvent OnDeactive;
    public int niddleID;

    public SpatialCrack[] spatialCracks;
    private int relativeCount;

    // Start is called before the first frame update
    void Awake()
    {
        InitializeRelatives();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DeactiveSelf()
    {
        GameManager.Instance.toSolveNiddleID = niddleID;
        OnDeactive.Invoke();

        StartCoroutine(DisableObjects());

    }

    IEnumerator DisableObjects()
    {
        yield return new WaitForSeconds(1.5f);
        GetComponent<LineRenderer>().enabled = false;
        for (int i = 0; i < disableObjects.Length; i++)
        {
            disableObjects[i].SetActive(false);
        }
    }
    void InitializeRelatives()
    {
        relativeCount = spatialCracks.Length;

        for (int i = 0; i < spatialCracks.Length; i++)
        {
            spatialCracks[i].OnDeactive.AddListener(SolveOnePuzzleElement);
                }
    }

    public void SolveOnePuzzleElement()
    {
        relativeCount -= 1;
        if(relativeCount == 0)
        {
            DeactiveSelf();
        }
    }
}
