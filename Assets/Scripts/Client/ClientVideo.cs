using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using RenderHeads.Media.AVProVideo;
using System.IO;
using UnityEngine.UI;
using System.Net;

public class ClientVideo : MonoBehaviour
{
    [SerializeField]
    private string _ipAddr;
    [SerializeField]
    private InputField serverIP;
    [SerializeField]
    private int _portNum;
    public List<string> fanVideoList = new List<string>(); // 메인 동영상의 직캠 배열
    [SerializeField]
    private MediaPlayer[] fanCamPlayer = new MediaPlayer[4];
    [SerializeField]
    private VideoPlayer[] fanCamUnityPlayer = new VideoPlayer[4];
    [SerializeField]
    private bool pause = false;
    [SerializeField]
    private GameObject fanScreen;
    [SerializeField]
    private GameObject inputServerUI;
    private bool avProVideoCheck = false;
    private bool start = false;
    private ClientSocket<PayloadType> clientSocket;

    // Start is called before the first frame update
    private void Start()
    {

        if (Display.displays.Length > 1)
            Display.displays[1].Activate();
        if (Display.displays.Length > 2)
            Display.displays[2].Activate();
        if (Display.displays.Length > 3)
            Display.displays[3].Activate();
    }
    // Update is called once per frame
    void Update()
    {
        if (start)
        {
            if (clientSocket.payload != null)
                Debug.Log(clientSocket.payload.Body);
            CheckPayloadData();
        }
    }
    public void StartServerSocket()
    {
        _ipAddr = serverIP.text;
        clientSocket = new ClientSocket<PayloadType>(IPAddress.Parse(_ipAddr), _portNum);
        start = true;
        fanScreen.SetActive(true);
        Destroy(inputServerUI);
    }
    private void CheckPayloadData()
    {
        if (clientSocket.checkReceive == true && clientSocket.payload.Type == PayloadType.Play && clientSocket.payload.Body != "Start")
        {
            FanCamReady();
            Debug.Log("Ready");
        }
        else if (clientSocket.checkReceive == true && clientSocket.payload.Type == PayloadType.Play && clientSocket.payload.Body == "Start")
        {
            PlayerRateStart();
            Debug.Log("Video Start");
        }
        else if (clientSocket.checkReceive == true && clientSocket.payload.Type == PayloadType.Pause && clientSocket.payload.Body == "Pause" ||
                 clientSocket.checkReceive == true && clientSocket.payload.Type == PayloadType.Pause && clientSocket.payload.Body == "Restart")
        {
            CheckPause();
        }
        else if (clientSocket.checkReceive == true && clientSocket.payload.Type == PayloadType.Stop && clientSocket.payload.Body == "Stop")
        {
            CheckStop();
        }


        //else if (clientSocket.checkReceive == true && clientSocket.payload.Type == PayloadType.Pause && clientSocket.payload.Body == "Restart")
        //{
        //    CheckPause();            
        //}
        clientSocket.checkReceive = false;
    }
    private void CheckStop()
    {
        fanVideoList.Clear();
        for (int i = 0; i < fanCamUnityPlayer.Length; i++)
            fanCamUnityPlayer[i].Stop();
    }

    private void CheckPause()
    {
        pause = !pause;

        if (pause)
        {
            for (int i = 0; i < fanCamUnityPlayer.Length; i++)
                fanCamUnityPlayer[i].Pause();
        }
        else if (pause == false)
        {
            for (int i = 0; i < fanCamUnityPlayer.Length; i++)
                fanCamUnityPlayer[i].Play();
        }
    }
    public void FanCamReady()
    {
        pause = false;
        var videoPath = clientSocket.payload.ToString();
        if (avProVideoCheck == false)
        {
            fanVideoList.Clear();

            string[] date = videoPath.Split('\\');
            string[] mainCamName = date[date.Length - 1].Split('_');
            //DirectoryInfo di = new DirectoryInfo(@"\\10.30.16.100\vertigo\" + "\\" + "TestVcam" + "\\" + date[5] + "\\" + mainCamName[4]); // 직캠 모아두는 폴더로 들어감
            DirectoryInfo di = new DirectoryInfo(@"D:\3_vcam_complete" + "\\" + date[2] + "\\" + mainCamName[4]); // 직캠 모아두는 폴더로 들어감
            foreach (var item in di.GetFiles())
            {
                if (fanVideoList.Count >= fanCamUnityPlayer.Length)
                    break;
                fanVideoList.Add(di + "\\" + item.Name);

            }
            for (int i = 0; i < fanVideoList.Count; i++)
            {
                fanCamUnityPlayer[i].url = "file://" + fanVideoList[i];
                fanCamUnityPlayer[i].Play();
                fanCamUnityPlayer[i].Pause();
            }
            //switch (fanVideoList.Count)
            //{
            //    case 1:
            //        fanCamUnityPlayer[0].url = "file://" + fanVideoList[0];
            //        break;
            //    case 2:
            //        fanCamUnityPlayer[0].url = "file://" + fanVideoList[0];
            //        fanCamUnityPlayer[1].url = "file://" + fanVideoList[1];
            //        break;
            //    case 3:
            //        fanCamUnityPlayer[0].url = "file://" + fanVideoList[0];
            //        fanCamUnityPlayer[1].url = "file://" + fanVideoList[1];
            //        fanCamUnityPlayer[2].url = "file://" + fanVideoList[2];
            //        break;
            //    case 4:
            //        fanCamUnityPlayer[0].url = "file://" + fanVideoList[0];
            //        fanCamUnityPlayer[1].url = "file://" + fanVideoList[1];
            //        fanCamUnityPlayer[2].url = "file://" + fanVideoList[2];
            //        fanCamUnityPlayer[3].url = "file://" + fanVideoList[3];
            //        break;
            //}

            //clientSocket.Send(new RawPayload<PayloadType>(PayloadType.Play, fanCamCheck));
            //StartCoroutine(PlayerRateStart());

        }
        else
        {
            fanVideoList.Clear();

            string[] date = videoPath.Split('\\');
            string[] mainCamName = date[date.Length - 1].Split('_');
            //DirectoryInfo di = new DirectoryInfo(@"\\10.30.16.100\vertigo\" + "\\" + "TestVcam" + "\\" + date[5] + "\\" + mainCamName[4]); // 직캠 모아두는 폴더로 들어감
            DirectoryInfo di = new DirectoryInfo(@"D:\3_vcam_complete" + "\\" + date[2] + "\\" + mainCamName[4]); // 직캠 모아두는 폴더로 들어감
            foreach (var item in di.GetFiles())
            {
                if (fanVideoList.Count >= fanCamPlayer.Length)
                    break;
                fanVideoList.Add(di + "\\" + item.Name);

            }
            switch (fanVideoList.Count)
            {
                case 1:
                    fanCamPlayer[0].OpenVideoFromFile(0, "file://" + fanVideoList[0], false);
                    break;
                case 2:
                    fanCamPlayer[0].OpenVideoFromFile(0, "file://" + fanVideoList[0], false);
                    fanCamPlayer[1].OpenVideoFromFile(0, "file://" + fanVideoList[1], false);
                    break;
                case 3:
                    fanCamPlayer[0].OpenVideoFromFile(0, "file://" + fanVideoList[0], false);
                    fanCamPlayer[1].OpenVideoFromFile(0, "file://" + fanVideoList[1], false);
                    fanCamPlayer[2].OpenVideoFromFile(0, "file://" + fanVideoList[2], false);
                    break;
                case 4:
                    fanCamPlayer[0].OpenVideoFromFile(0, "file://" + fanVideoList[0], false);
                    fanCamPlayer[1].OpenVideoFromFile(0, "file://" + fanVideoList[1], false);
                    fanCamPlayer[2].OpenVideoFromFile(0, "file://" + fanVideoList[2], false);
                    fanCamPlayer[3].OpenVideoFromFile(0, "file://" + fanVideoList[3], false);
                    break;
            }
            for (int i = 0; i < fanVideoList.Count; i++)
                fanCamPlayer[i].Pause();
            //StartCoroutine(PlayerRateStart());
        }
    }
    //public void FanMonitorCountCheck()
    //{
    //    if (fanCamPlayer.Count > int.Parse(fanMonitorCount.text))
    //    {
    //        fanCamPlayer.RemoveRange(int.Parse(fanMonitorCount.text), fanCamPlayer.Count - int.Parse(fanMonitorCount.text));
    //    }
    //    if (fanCamUnityPlayer.Count > int.Parse(fanMonitorCount.text))
    //    {
    //        fanCamUnityPlayer.RemoveRange(int.Parse(fanMonitorCount.text), fanCamUnityPlayer.Count - int.Parse(fanMonitorCount.text));
    //    }
    //}
    //public void MainCamFrameSync()
    //{
    //    for (int i = 0; i < fanCamPlayer.Count; i++)
    //    {
    //        if (fanCamPlayer[i].Control.GetCurrentTimeMs() != fullCamPlayer.Control.GetCurrentTimeMs())
    //            fanCamPlayer[i].Control.Seek(fullCamPlayer.Control.GetCurrentTimeMs());
    //    }
    //}
    private void PlayerRateStart()
    {
        //videoPlaying = true;
        //if (fanVideoList.Count > 0 && fanCamUnityPlayer.Length > 0)
        //{
        //if (pause == false)
        //{
        if (avProVideoCheck == false)
        {

            if (fanVideoList.Count > 0 && fanCamUnityPlayer.Length > 0)
            {
                for (int i = 0; i < fanVideoList.Count; i++)
                    fanCamUnityPlayer[i].Play();
            }
        }
        else
        {
            if (fanVideoList.Count > 0 && fanCamPlayer.Length > 0)
            {
                for (int i = 0; i < fanVideoList.Count; i++)
                    fanCamPlayer[i].Play();
            }
        }
        //}


        // }

    }
}
