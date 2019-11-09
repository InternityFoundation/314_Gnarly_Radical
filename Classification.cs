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
    public MessageBehavior messageBehavior;
    public Text console;
    public HelloClient hc;

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

        hc = new HelloClient();
    }

    private void Update () {
        //process image on click or touch
        if (Input.GetMouseButtonDown(0) && !CheckInternetConnection()){
            ProcessImage2();
        }

        if (label.Length > 1)
            StartCoroutine(HandleIt());

       /* if (hc._helloRequester.message != null)
        {
            if (hc._helloRequester.message.Length > 1)
                StartCoroutine(serverClassi());
        }*/

        if (msg != null)
        {
            if (msg.Length > 1)
                StartCoroutine(serverClassi());
        }

        if (dictVal.Length > 0)
            StartCoroutine(HandleIt2());

        if (label3 == obj3)
        {
            obj3 = "";
            obj3 = "";
            obj3 = "";
            StartCoroutine(HandleIt3());
        }

        // if (search)
        //StartCoroutine(GetCamPics());

        if (search)
        {
            StartCoroutine(test());
        }
    }

    private IEnumerator test()
    {
        if (search)
        {

                    search = false;
                    int n = 0;
                    console.text = search.ToString();
                    while (tempsearch)
                    {
                        console.text = "1";
                        Debug.Log(n);
                        tensor3 = TransformInput(camFeed.GetImage(), INPUT_SIZE, INPUT_SIZE);
                        console.text = "2";
                        var runner = session.GetRunner();
                        runner.AddInput(graph[INPUT_TENSOR][0], tensor3).Fetch(graph[OUTPUT_TENSOR][0]);
                        console.text = "3";
                        var output = runner.Run();
                        //put results into one dimensional array
                        float[] probs = ((float[][])output[0].GetValue(jagged: true))[0];
                        console.text = "4";
                        //get max value of probabilities and find its associated label index
                        float maxValue = probs.Max();
                        int maxIndex = probs.ToList().IndexOf(maxValue);
                        console.text = "5";
                        //print label with highest probability
                        Debug.Log(n++);
                        label3 = labels[maxIndex];
                        console.text = "6";
                        console.text = label3;
                        console.text = "7";
                        n = n + 1;
                        console.text = n.ToString();
                        //Thread.Sleep(2000);
                        yield return new WaitForSeconds(2.0f);

                    }

        }
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
            messageBehavior.ShowMessage(label);
            label = "";
            yield return new WaitForSeconds(1.0f);
            label = "";
        }
    }


    private IEnumerator serverClassi()
    {
        if (msg.Length > 1)
        {
            Debug.Log("reeee");
            if (msg == "Recyclable")
            {
                audioSource.clip = Resources.Load("wasteNetSounds/Recyclable") as AudioClip;
                messageBehavior.ShowMessage("Recyclable");
            }
            else
            {
                audioSource.clip = Resources.Load("wasteNetSounds/Nonrecyclable") as AudioClip;
                messageBehavior.ShowMessage("Nonrecyclable");
            }
            audioSource.Play();

            msg = "";
            yield return new WaitForSeconds(1.0f);
            msg = "";
        }
    }

    private IEnumerator HandleIt2()
    {
        if (dictVal.Length > 0)
        {
            if (dictVal == "0")
            {
                audioSource.clip = Resources.Load("wasteNetSounds/Organic") as AudioClip;
                messageBehavior.ShowMessage("Organic");
            }
            else
            {
                audioSource.clip = Resources.Load("wasteNetSounds/Inorganic") as AudioClip;
                messageBehavior.ShowMessage("Inorganic");
            }
            audioSource.Play();

            dictVal = "";
            yield return new WaitForSeconds(1.0f);
            dictVal = "";
        }
    }

    private IEnumerator HandleIt3()
    {

            audioSource.clip = Resources.Load("imageNetSounds/" + label3) as AudioClip;
            audioSource.Play();
            yield return new WaitForSeconds(audioSource.clip.length + 0.5f);
            audioSource.clip = Resources.Load("wasteNetSounds/Found") as AudioClip;
            audioSource.Play();
            messageBehavior.ShowMessage(label3 + " found!");
            search = false;
            tempsearch = false;
            label3 = "x";
            yield return new WaitForSeconds(1.0f);
            label3 = "x";

    }

    void onActivityResult(string recognizedText)
    {
        char[] delimiterChars = { '~' };
        string[] result = recognizedText.Split(delimiterChars);

        //You can get the number of results with result.Length
        //And access a particular result with result[i] where i is an int
        //I have just assigned the best result to UI text
        GameObject.Find("Text").GetComponent<Text>().text = result[0].ToLower();
        if (result[0].ToLower() == "scan object")
            ProcessImage();
        else if (result[0].ToLower() == "classify object")
            ProcessImage2();
        else if (result[0].ToLower() == "classify trash")
            hc.takePhoto();
        else if (result[0].ToLower().Split(new[] { ' ' }, 2)[0] == "find")
            ProcessImage3(result[0].ToLower().Split(new[] { ' ' }, 2)[1]);
        else if (result[0] == "stop find")
        {
            audioSource.clip = Resources.Load("wasteNetSounds/Abort") as AudioClip;
            audioSource.Play();
            modelThread3.Abort();
            search = false;
            tempsearch = false;
        }
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
    public void proimg3()
    {
        ProcessImage3("mouse");
    }

    public void ProcessImage3(string obj)
    {
        console.text = obj;
        obj3 = obj;

        string[] lm = labelMap.text.Split("\n"[0]);
        Debug.Log(lm[5]);
        //console.text = lm[15];
        Debug.Log(lm[10]);
        if (!lm.Contains(obj)|| !(Array.IndexOf(lm, obj3) > -1))
        {
            audioSource.clip = Resources.Load("wasteNetSounds/404") as AudioClip;
            audioSource.Play();
            messageBehavior.ShowMessage("Not found!");
            return;
        }
        else
        {
            audioSource.clip = Resources.Load("wasteNetSounds/Searching") as AudioClip;
            audioSource.Play();
            messageBehavior.ShowMessage("Searching...");
            search = true;
            tempsearch = true;
        }

        //pass in input tensor
        tensor3 = TransformInput(camFeed.GetImage(), INPUT_SIZE, INPUT_SIZE);


        //modelThread3 = new Thread(RunModel3);
        //modelThread3.Start();
        //RunModel3();
    }

    void RunModel3()
    {
        int n = 0;
        console.text = search.ToString();
        while (search)
        {
            console.text = "1";
            Debug.Log(n);
            //tensor = TransformInput(camFeed.GetImage(), INPUT_SIZE, INPUT_SIZE);
            console.text = "2";
            var runner = session.GetRunner();
            runner.AddInput(graph[INPUT_TENSOR][0], tensor3).Fetch(graph[OUTPUT_TENSOR][0]);
            console.text = "3";
            var output = runner.Run();
            //put results into one dimensional array
            float[] probs = ((float[][])output[0].GetValue(jagged: true))[0];
            console.text = "4";
            //get max value of probabilities and find its associated label index
            float maxValue = probs.Max();
            int maxIndex = probs.ToList().IndexOf(maxValue);
            console.text = "5";
            //print label with highest probability
            Debug.Log(n++);
            label3 = labels[maxIndex];
            console.text = "6";
            console.text = label3;
            console.text = "7";
            n = n + 1;
            console.text = n.ToString();
            //Thread.Sleep(2000);
        }
    }

    //---------------------------------------------------------------------------------------

    public static TFTensor TransformInput (Color32 [] pic, int width, int height) {
        float [] floatValues = new float [width * height * 3];

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

