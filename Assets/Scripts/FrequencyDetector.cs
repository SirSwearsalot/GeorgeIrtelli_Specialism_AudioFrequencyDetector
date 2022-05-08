using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(AudioSource))]
public class FrequencyDetector : MonoBehaviour
{

    private AudioSource AS;

    private float[] Samples;
    private GameObject[] EQSquares;

    [Header("Visual Elements")]
    [SerializeField] bool ActivateEQ = true;
    [SerializeField] bool ActivateIndicator = true;
    [SerializeField] bool ActivateThreshold = true;


    [Space]
    public bool[] ActiveBand = new bool[4];

    private GameObject[] IndicatorSquares = new GameObject[4];
    private GameObject[] ThresholdBars = new GameObject[4];



    GameObject EQFolder;
    GameObject IndicatorFolder;
    GameObject ThresholdFolder;

    public float FrequencyMulti = 1;

    [Range(0, 0.5f)] 
    public float[] FrequencyThreshold = new float[4];
    private bool[] BandActive;
    
    public float[] BarAmp;

    private int[] FrequencySpectrumDivision = {20,30,50,70,100,200,300,500,800,2000,5000,7000};


    private void Awake()
    {
        createChildren();
    }

    void createChildren()
    {
        GameObject go; 
        if(transform.childCount == 0)
        {
            go = new GameObject();
            go.transform.parent = this.transform;
            go.transform.localPosition = Vector3.zero;
            go.transform.name = "EQMaster";
            EQFolder = go;
        }

        if (transform.childCount == 1)
        {
            go = new GameObject();
            go.transform.parent = this.transform;
            go.transform.localPosition = Vector3.zero;
            go.transform.name = "Indicator";
            IndicatorFolder = go;
        }

        if (transform.childCount == 2)
        {
            go = new GameObject();
            go.transform.parent = this.transform;
            go.transform.localPosition = Vector3.zero;
            go.transform.name = "ThresholdBars";
            ThresholdFolder = go;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        AS = GetComponent<AudioSource>();

        Samples = new float[8192];
        EQSquares = new GameObject[12];

        BarAmp = new float[12];

        generateEQ();
        //generateBands();
        generateIndicators();
        generateThresholdBars();
    }

    void generateEQ()
    {
        for (int i = 0; i < EQSquares.Length; i++)
        {
            EQSquares[i] = GameObject.CreatePrimitive(PrimitiveType.Cube);
            EQSquares[i].transform.parent = transform.GetChild(0);
            EQSquares[i].transform.localPosition = transform.GetChild(0).localPosition + new Vector3(i * 0.06f, 0, 0);
        }
    } 

    void generateIndicators()
    {
        for (int i = 0; i < IndicatorSquares.Length; i++)
        {
            IndicatorSquares[i] = GameObject.CreatePrimitive(PrimitiveType.Cube);
            IndicatorSquares[i].transform.parent = transform.GetChild(1);
            IndicatorSquares[i].transform.localScale = Vector3.one / 50;
            IndicatorSquares[i].transform.position = transform.GetChild(1).position + new Vector3(i * 0.06f * 3, 0.02f, -0.05f);
            IndicatorSquares[i].GetComponent<MeshRenderer>().material.color = Color.red;

        }
    }

    void generateThresholdBars()
    {
        for (int i = 0; i < IndicatorSquares.Length; i++)
        {
            ThresholdBars[i] = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ThresholdBars[i].transform.parent = transform.GetChild(2);
            ThresholdBars[i].transform.localScale = new Vector3(0.16f,0.005f, 0.02f);
            ThresholdBars[i].transform.position = transform.GetChild(2).position + new Vector3(i * 0.06f * 3 + 0.06f, 0.02f, 0);
            ThresholdBars[i].GetComponent<MeshRenderer>().material.color = Color.red;

        }
    }

    // Update is called once per frame
    void Update()
    {
        AnalyseAudio();
        UpdateEQ();
        ThresholdCheck();

        ShowUI();
    }

    void ShowUI()
    {
        if (ActivateEQ)
            EQFolder.SetActive(true);
        else
            EQFolder.SetActive(false);

        if (ActivateIndicator)
            IndicatorFolder.SetActive(true);
        else
            IndicatorFolder.SetActive(false);

        if (ActivateThreshold)
            ThresholdFolder.SetActive(true);
        else
            ThresholdFolder.SetActive(false);


    }

    void AnalyseAudio()
    {
        AS.GetSpectrumData(Samples, 0, FFTWindow.BlackmanHarris);
    }

    void UpdateEQ()
    {
        for (int i = 0; i < EQSquares.Length; i++)
        {

            float AvgFreq = 1;

            for(int j = 0; j < 12; j++)
            {
                AvgFreq = Samples[(i * 682) + j];
            }

            AvgFreq = (AvgFreq / 12) * FrequencyMulti;

            EQSquares[i].transform.localScale = Vector3.one / 20 + Vector3.Slerp(new Vector3(0, EQSquares[i].transform.localScale.y,0),
                                                                                Vector3.up * AvgFreq * FrequencySpectrumDivision[i],
                                                                                800f * Time.deltaTime);

            EQSquares[i].transform.localPosition = new Vector3(EQSquares[i].transform.localPosition.x,
                                                            EQSquares[i].transform.localScale.y / 2,
                                                            EQSquares[i].transform.localPosition.z);

            BarAmp[i] = AvgFreq * FrequencySpectrumDivision[i];
        }

        for (int i = 0; i < ThresholdBars.Length; i++)
        {
            ThresholdBars[i].transform.position = transform.GetChild(2).position + new Vector3(ThresholdBars[i].transform.localPosition.x, FrequencyThreshold[i], 0);
        }
    }

    void ThresholdCheck()
    {

        bool ThresholdPassed = false;
        int BarIndex = 0;

        for (int j = 0; j < 4; j++)
        {
            ThresholdPassed = false;
            for (int i = 0; i < 3; i++)
            {
                if (BarAmp[BarIndex] > FrequencyThreshold[j])
                {
                    IndicatorSquares[j].GetComponent<MeshRenderer>().material.color = Color.green;
                    ThresholdPassed = true;
                    ActiveBand[j] = true;
                }
                else if (!ThresholdPassed)
                {
                    IndicatorSquares[j].GetComponent<MeshRenderer>().material.color = Color.red;
                    ActiveBand[j] = false;
                }
                BarIndex++;
            }
        }
    }
}
