using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance { get { return instance; } private set { } }
    public static bool isGameOver;
    public static bool isRespawning;



    private Niddle[] niddles;
    public int toSolveNiddleID;
    [SerializeField] private float puzzleCount = 3;
    public bool allPuzzleSolved;


    [Header("Narratives")]
    public bool firstDizzy;
    public int dizzyTutorialPlotID;
    private bool hasDizzyGuided;
    public bool firstWater;
    public int waterTutorialPlotID;
    private bool haswaterGuided;
    public int congratulationPlotID;
    public int finalPlotID;

    [Header("Camera")]
    public CinemachineVirtualCamera finalCamera;
    public CinemachineDollyCart[] cameraClips;
    public CinemachineDollyCart[] puzzleClips;
    public float[] showDurations;
    private int currentCameraClipIndex;
    private CinemachineDollyCart nextCameraClip;


    private void Awake()
    {
        if (instance == null) instance = this;

        InitializeCameraClip();

        InitializeNiddleData();
    }
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        StartCoroutine(SwitchToCameraClipShowState(cameraClips[currentCameraClipIndex], showDurations[currentCameraClipIndex], true,0f));
    }

    // Update is called once per frame
    void Update()
    {
        if (firstDizzy&& !hasDizzyGuided)
        {
            if (LinesManager.isPlayingLines) return;
            LinesManager.Instance.DisplayLine(dizzyTutorialPlotID, 0);
            hasDizzyGuided = true;
        }

        if (firstWater && !haswaterGuided)
        {
            if (LinesManager.isPlayingLines) return;
            LinesManager.Instance.DisplayLine(waterTutorialPlotID, 0);
            haswaterGuided = true;
        }
    }

    void InitializeNiddleData()
    {
        niddles = GameObject.FindObjectsOfType<Niddle>(true);
        puzzleCount = niddles.Length;

        for (int i = 0; i < niddles.Length; i++)
        {
            niddles[i].OnDeactive.AddListener(SolveOnePuzzle);
        }
    }

    public void SolveOnePuzzle()
    {
        puzzleCount -= 1;

        if (puzzleCount == 0)
        {
            allPuzzleSolved = true;
        }

        StartCoroutine(SwitchToCameraClipShowState(puzzleClips[toSolveNiddleID], 3f, false, 1));

        if (LinesManager.isPlayingLines) return;
        LinesManager.Instance.DisplayLine(congratulationPlotID, 0);
    }

    public IEnumerator SwitchToCameraClipShowState(CinemachineDollyCart camera, float duration,bool isCount,float delay)
    {
        yield return new WaitForSeconds(delay);
        camera.gameObject.SetActive(true);
        PlayerInput.Instance.m_ExternalInputBlocked = true;

        StartCoroutine(WaitCameraClipEnd(camera,duration, isCount));
    }

    IEnumerator WaitCameraClipEnd(CinemachineDollyCart camera, float duration, bool isCount)
    {
        yield return new WaitForSeconds(duration);
        camera.gameObject.SetActive(false);
        PlayerInput.Instance.m_ExternalInputBlocked = false;

        if (isCount)
        {
            nextCameraClip = cameraClips[currentCameraClipIndex >= cameraClips.Length - 1 ? 0 : currentCameraClipIndex + 1];
        }
        else
        {
            if (allPuzzleSolved)
            {
                StartCoroutine(DealWithFinalShow());
            }
        }

    }

    private void InitializeCameraClip()
    {
        if (cameraClips.Length > 0)
        {
            currentCameraClipIndex = 0;
            nextCameraClip = cameraClips[currentCameraClipIndex];

        }
    }

    private IEnumerator DealWithFinalShow()
    {
        if (LinesManager.isPlayingLines) 
            yield return null;
        LinesManager.Instance.DisplayLine(finalPlotID, 0);
        finalCamera.gameObject.SetActive(true);
        PlayerInput.Instance.m_ExternalInputBlocked = true;
        yield return new WaitUntil(() => !LinesManager.isPlayingLines);
        PlayerInput.Instance.m_ExternalInputBlocked = false;
        finalCamera.gameObject.SetActive(false);
    }
}
