using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GateOpenType
{
    combat,puzzle
}
public class GateController : MonoBehaviour
{
    public GameObject relative;
    public GateOpenType type;

    // Start is called before the first frame update
    void Start()
    {
        switch (type)
        {
            case GateOpenType.combat:
                relative.GetComponent<CombatController>().OnCombatOver.AddListener(OpenTheGate);
                break;
            case GateOpenType.puzzle:
                    relative.GetComponent<SpatialCrack>().OnDeactive.AddListener(OpenTheGate);
                break;
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OpenTheGate()
    {
        this.gameObject.SetActive(false);
    }
}
