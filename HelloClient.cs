using System.Collections;
using System.IO;
using UnityEngine;

public class HelloClient : MonoBehaviour
{
    public HelloRequester _helloRequester;
    WebCamTexture webCamTexture;
    public static byte[] photoBytes;

    private void Start()
    {
        _helloRequester = new HelloRequester();

        webCamTexture = new WebCamTexture();
        GetComponent<Renderer>().material.mainTexture = webCamTexture; //Add Mesh Renderer to the GameObject to which this script is attached to
        webCamTexture.Play();
    }

    public void sendReq()
    {
        _helloRequester.Start();
    }

    private void OnDestroy()
    {
        _helloRequester.Stop();
    }

    public void takePhoto()
    {
        StartCoroutine(TakePhoto());
    }

    IEnumerator TakePhoto()  // Start this Coroutine on some button click
    {

        // NOTE - you almost certainly have to do this here:

        yield return new WaitForEndOfFrame();


        Texture2D photo = new Texture2D(webCamTexture.width, webCamTexture.height);
        photo.SetPixels(webCamTexture.GetPixels());
        photo.Apply();

        //Encode to a PNG
        photoBytes = photo.EncodeToPNG();
        Debug.Log(photoBytes);
        //Write out the PNG. Of course you have to substitute your_path for something sensible
        //File.WriteAllBytes("photo.png", photoBytes);
        Debug.Log("Photu taken");

        sendReq();

        //yield return new WaitForSeconds(4);

        //OnDestroy();
        _helloRequester = new HelloRequester();
    }
}