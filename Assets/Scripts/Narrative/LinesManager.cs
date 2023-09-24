using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class LinesManager : MonoBehaviour
{
    private static LinesManager instance;
    public static LinesManager Instance { get { return instance; } private set { } }

    public GameObject lineUI;

    public Color LerpColorA;
    public Color LerpColorB;

    [SerializeField] public static bool isPlayingLines;
    [SerializeField] public static bool isReadingStartPlot;

    protected Text text;
    public Text nameText;
    protected PlotManager plotManager;
    protected Lines[] allLines;
    protected Lines currentLine;

    

    private void Awake()
    {
        if (instance == null)
            instance = this;


    }
    // Start is called before the first frame update
    void Start()
    {
        isPlayingLines = false;
        isReadingStartPlot = false;

        text = lineUI.GetComponentInChildren<Text>();

        plotManager = Resources.Load<PlotManager>("DataAssets/Lines");
        allLines = plotManager.lines;
    }
    
    public void DisplayLine(int plotID,int index)
    {
        if (isPlayingLines && index ==0) return;

        if(index == 0)
        {
            StopAllCoroutines();
            StartCoroutine(SetLineUI(true, 0f));
            isPlayingLines = true;

            if(plotID == 0)
            {
                isReadingStartPlot = true;
            }
            else
            {
                isReadingStartPlot = false;
            }

        }

        for(int i = 0; i < allLines.Length; i++)
        {
            if(allLines[i].plotID==plotID && allLines[i].index == index)
            {
                currentLine = allLines[i];
                text.text = currentLine.text;
                nameText.text = currentLine.name;
                StartCoroutine(WaitSoundEndToNextLine(currentLine));
            }
        }
    }
    IEnumerator WaitSoundEndToNextLine(Lines line)
    {
        yield return new WaitForSeconds(currentLine.duration);
        
        if(line.nextIndex != -1)
        {
            DisplayLine(line.plotID, line.nextIndex);
        }
        else
        {
            StartCoroutine(SetLineUI(false,1f));
        }
    }

    IEnumerator SetLineUI(bool active, float delay)
    {
        yield return new WaitForSeconds(delay);
        if(lineUI!=null)
        lineUI.SetActive(active);

        if (!active)
        {
            isPlayingLines = false;
        }
    }
}
