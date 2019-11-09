using UnityEngine;
using TensorFlow;
using System.Linq;
using UnityEngine.UI;
using System.Threading;
using System.Collections;
using System;
using System.Collections.Generic;
using static HelloRequester;

public class Classification : MonoBehaviour {

    [Header("Constants")]
	private const int INPUT_SIZE = 224;
	private const int IMAGE_MEAN = 117;
	private const float IMAGE_STD = 1;
	private const string INPUT_TENSOR = "input";
	private const string OUTPUT_TENSOR = "output";

    [Header("Inspector Stuff")]
	public CameraFeedBehavior camFeed;
    public TextAsset labelMap;
    public TextAsset dictMap;
    public TextAsset model;
    public AudioSource audioSource;
	public Text console;

    private Thread modelThread;
    private string label = "";
    private string label3 = "x";
    private TFTensor tensor;
    private TFTensor tensor3;
    private Thread modelThread3;
    private TFGraph graph;
    private TFSession session;
    private string [] labels;
    private string obj3;
    private bool search = false;
    private bool tempsearch = false;
    char[] spearator = { ':' };
    private Dictionary<string, string> d;
    private string dictVal = "";


    //to remove
    //private bool fond = false;
    //private string[] labels2;
    //private TFSession session2;
    //private TFGraph graph2;
    //public static Color32[] img32;
    //public TextAsset labelMap2;
    //public TextAsset model2;
    //private Thread modelThread2;

    // Use this for initialization
    void Start() {
#if UNITY_ANDROID
        TensorFlowSharp.Android.NativeBinding.Init();
#endif
		//load labels into string array
		labels = labelMap.ToString ().Split ('\n');

        //load graph
        graph = new TFGraph();
        graph.Import(model.bytes);
        session = new TFSession(graph);

        string[] lines = dictMap.text.Split("\n"[0]);
        d = new Dictionary<string, string>();
        foreach (string line in lines)
        {
            String[] strlist = line.Split(spearator);
            d.Add(strlist[0].ToLower().Trim(), strlist[1].Trim());
        }


    }

    private void Update () {
		//process image on click or touch
		if (Input.GetMouseButtonDown(0) && !CheckInternetConnection()){
			ProcessImage2();
		}

        if (label.Length > 1)
            StartCoroutine(HandleIt());

        if (dictVal.Length > 0)
            StartCoroutine(HandleIt2());

    }


    private IEnumerator GetCamPics()
    {
        if (search)
        {
            while (tempsearch)
            {
                tensor3 = TransformInput(camFeed.GetImage(), INPUT_SIZE, INPUT_SIZE);
                search = false;
                yield return new WaitForSeconds(2.0f);
            }
        }
    }

    private IEnumerator HandleIt()
    {
        if (label.Length > 1)
        {
            audioSource.clip = Resources.Load("imageNetSounds/" + label) as AudioClip;
            audioSource.Play();
            

            label = "";
            yield return new WaitForSeconds(1.0f);
            label = "";
        }
    }


    private IEnumerator HandleIt2()
    {
        if (dictVal.Length > 0)
        {
            if (dictVal == "0")
            {
                audioSource.clip = Resources.Load("wasteNetSounds/Organic") as AudioClip;
}
            else
            {
                audioSource.clip = Resources.Load("wasteNetSounds/Inorganic") as AudioClip;
}
            audioSource.Play();

            dictVal = "";
            yield return new WaitForSeconds(1.0f);
            dictVal = "";
        }
    }



    void onActivityResult(string recognizedText)
    {
        char[] delimiterChars = { '~' };
        string[] result = recognizedText.Split(delimiterChars);


        GameObject.Find("Text").GetComponent<Text>().text = result[0].ToLower();
        if (result[0].ToLower() == "scan object")
            ProcessImage();
        else if (result[0].ToLower() == "classify object")
            ProcessImage2();
    }

    //---------------------------------------------------------------------------------------

    public void ProcessImage(){
        //pass in input tensor
        tensor = TransformInput(camFeed.GetImage(), INPUT_SIZE, INPUT_SIZE);

        modelThread = new Thread(RunModel);
        modelThread.Start();
    }

    void RunModel(){
        var runner = session.GetRunner();
        runner.AddInput(graph[INPUT_TENSOR][0], tensor).Fetch(graph[OUTPUT_TENSOR][0]);
        var output = runner.Run();
        //put results into one dimensional array
        float[] probs = ((float[][])output[0].GetValue(jagged: true))[0];
        //get max value of probabilities and find its associated label index
        float maxValue = probs.Max();
        int maxIndex = probs.ToList().IndexOf(maxValue);
        //print label with highest probability
        label = labels[maxIndex];
        print(label);
    }

    //---------------------------------------------------------------------------------------

    public void ProcessImage2()
    {
        //pass in input tensor
        tensor = TransformInput(camFeed.GetImage(), INPUT_SIZE, INPUT_SIZE);

        modelThread = new Thread(RunModel2);
        modelThread.Start();
    }

    void RunModel2()
    {
        var runner = session.GetRunner();
        runner.AddInput(graph[INPUT_TENSOR][0], tensor).Fetch(graph[OUTPUT_TENSOR][0]);
        var output = runner.Run();
        //put results into one dimensional array
        float[] probs = ((float[][])output[0].GetValue(jagged: true))[0];
        //get max value of probabilities and find its associated label index
        float maxValue = probs.Max();
        int maxIndex = probs.ToList().IndexOf(maxValue);
        //print label with highest probability
        var label2 = labels[maxIndex];

        dictVal = d[label2];
    }

    

    //---------------------------------------------------------------------------------------

    public static TFTensor TransformInput (Color32 [] pic, int width, int height) {
		float [ ] floatValues = new float [width * height * 3];

		for (int i = 0; i < pic.Length; ++i) {
			var color = pic [i];

			floatValues [i * 3 + 0] = (color.r - IMAGE_MEAN) / IMAGE_STD;
			floatValues [i * 3 + 1] = (color.g - IMAGE_MEAN) / IMAGE_STD;
			floatValues [i * 3 + 2] = (color.b - IMAGE_MEAN) / IMAGE_STD;
		}

		TFShape shape = new TFShape (1, width, height, 3);

		return TFTensor.FromBuffer (shape, floatValues, 0, floatValues.Length);
	}

    public bool CheckInternetConnection()
    {
        return !(Application.internetReachability == NetworkReachability.NotReachable);
    }
}

