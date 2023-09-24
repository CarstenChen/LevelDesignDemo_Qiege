using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public int plotID;
    public bool waitGameEnd = false;
    private bool finished;
    public GameObject toActivate;
    private void OnTriggerStay(Collider other)
    {
        if (waitGameEnd && !GameManager.Instance.allPuzzleSolved) return;

        if (LinesManager.isPlayingLines) return;

        if (other.tag == "Player")
        {
            if (!finished)
            {
                LinesManager.Instance.DisplayLine(plotID, 0);
                finished = true;

                if (toActivate != null) toActivate.SetActive(!toActivate.activeSelf);
            }

        }
    }
}
