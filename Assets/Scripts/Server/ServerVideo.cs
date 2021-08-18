using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using RenderHeads.Media.AVProVideo;
using System.Net;
public class ServerVideo : MonoBehaviour
{    
    private string _ipAddr;
    [SerializeField]
    private int _portNum;
    [SerializeField]
    private InputField fanMonitorCount;
    [SerializeField]
    private MediaPlayer fullCamPlayer;//날짜별 메인으로 띄울 동영상을 재생
    [SerializeField]
    private VideoPlayer fullCamUnityPlayer;        
    [SerializeField]
    private GameObject calender;
    [SerializeField]
    private Text ipText;
    
    public List<string> videoList = new List<string>();//메인 동영상 리스트 배열
    private List<string> userInput = new List<string>();//날짜 입력    
    private SocketServer<PayloadType> socketServer;

    public Text firstVideoDate;
    public Text lastVideoDate;
    public bool pause = false;
    public bool avProVideoCheck = false;
    //private bool videoPlaying = false;
    //private bool checkEndMainVideo = false;
    private bool mainCheck, calenderOpen = true;

    public int fullCamVideoCount = 0;
    private int helpCount, minus;

    private void Awake()
    {
        GetIP();
        
    }
    private void Start()
    {
        Debug.Log(_ipAddr);
        ipText.text = "Server IP : " + _ipAddr;
        socketServer = new SocketServer<PayloadType>(IPAddress.Parse(_ipAddr), _portNum);
    }
    // Update is called once per frame
    void Update()
    {
        
        if (avProVideoCheck == true)
        {
            if (fullCamPlayer.Control.IsFinished() == true)
            {
                CheckStopVideo();
            }
        }
        else
        {
            if ((ulong)fullCamUnityPlayer.frame == fullCamUnityPlayer.frameCount - 3 && fullCamUnityPlayer.isPlaying)
            {
                //videoPlaying = false;
                CheckStopVideo();
            }
        }
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            calenderOpen = !calenderOpen;
            calender.SetActive(calenderOpen);
        }
        if (Input.GetKeyDown(KeyCode.Space))
            PauseButtonClick();
        MovingSecond();

        //MainCamFrameSync();
    }
    public void CheckStopVideo()
    {
        if (avProVideoCheck == true)
        {
            //checkEndMainVideo = true;
            if (fullCamVideoCount < videoList.Count - 1)
                fullCamVideoCount++;
            else
                fullCamVideoCount = 0;
            fullCamPlayer.OpenVideoFromFile(0, "file://" + videoList[fullCamVideoCount], false);
            mainCheck = true;
            fullCamPlayer.Pause();
            StartCoroutine(PlayerRateStart());
        }
        else
        {
            //checkEndMainVideo = true;
            if (fullCamVideoCount < videoList.Count - 1)
                fullCamVideoCount++;
            else
                fullCamVideoCount = 0;
            fullCamUnityPlayer.url = "file://" + videoList[fullCamVideoCount];
            mainCheck = true;
            fullCamUnityPlayer.Play();
            fullCamUnityPlayer.Pause();
            socketServer.BroadCast(new RawPayload<PayloadType>(PayloadType.Play, videoList[fullCamVideoCount]));
            StartCoroutine(PlayerRateStart());
        }
    }
    public void StartBUttonClick()
    {
        if (pause == false)
        {
            helpCount = int.Parse(firstVideoDate.text);
            if (lastVideoDate.text != "")
                minus = int.Parse(lastVideoDate.text) - int.Parse(firstVideoDate.text);
            if (minus >= 0)
            {
                for (int i = 0; i <= minus; i++)
                {
                    userInput.Add(helpCount.ToString());
                    helpCount++;
                }
            }
            else if (minus < 0)
            {
                userInput.Add(helpCount.ToString());
            }

            for (int i = 0; i < userInput.Count; i++)
            {
                //DirectoryInfo di = new DirectoryInfo(@"\\10.30.16.100\vertigo\8_fullcam_Acut_4k\" + userInput[i] + "\\4k\\");                    
                DirectoryInfo di = new DirectoryInfo(@"D:\2_fullcam_Acut\" + userInput[i]);
                if (di.Exists == true)
                {
                    foreach (var item in di.GetFiles())
                    {
                        videoList.Add(di + "\\" + item.Name);
                    }
                }
            }
            if (avProVideoCheck == true)
            {
                fullCamPlayer.OpenVideoFromFile(0, "file://" + videoList[fullCamVideoCount], false);
                fullCamPlayer.Pause();
                mainCheck = true;
                //StartCoroutine(PlayerRateStart());
            }
            else
            {
                fullCamUnityPlayer.url = "file://" + videoList[fullCamVideoCount];
                fullCamUnityPlayer.Play();
                fullCamUnityPlayer.Pause();

                mainCheck = true;
                //StartCoroutine(PlayerRateStart());
            }

            socketServer.BroadCast(new RawPayload<PayloadType>(PayloadType.Play, videoList[fullCamVideoCount]));
            StartCoroutine(PlayerRateStart());
        }
        else
        {
            pause = !pause;
            fullCamUnityPlayer.Play();
        }
    }
    public void StopButtonClick()
    {
        StopAllCoroutines();
        if (avProVideoCheck == true)
        {
            fullCamVideoCount = 0;
            helpCount = 0;
            minus = 0;
            videoList.Clear();
            userInput.Clear();
            fullCamPlayer.Stop();
            fullCamPlayer.CloseVideo();
            socketServer.BroadCast(new RawPayload<PayloadType>(PayloadType.Stop, "Stop"));
        }
        else
        {
            fullCamVideoCount = 0;
            helpCount = 0;
            minus = 0;
            videoList.Clear();
            userInput.Clear();
            fullCamUnityPlayer.Stop();
            socketServer.BroadCast(new RawPayload<PayloadType>(PayloadType.Stop, "Stop"));
        }
    }
    public void PauseButtonClick()
    {
        pause = !pause;
        if (avProVideoCheck == true)
        {
            if (pause == true)
            {
                fullCamPlayer.Pause();
                socketServer.BroadCast(new RawPayload<PayloadType>(PayloadType.Pause, "Pause"));
            }
            else
            {
                fullCamPlayer.Play();
                socketServer.BroadCast(new RawPayload<PayloadType>(PayloadType.Pause, "Restart"));
            }
        }
        else
        {
            if (pause == true)
            {
                fullCamUnityPlayer.Pause();
                socketServer.BroadCast(new RawPayload<PayloadType>(PayloadType.Pause, "Pause"));
            }
            else
            {
                fullCamUnityPlayer.Play();
                socketServer.BroadCast(new RawPayload<PayloadType>(PayloadType.Pause, "Restart"));
            }
        }
        
    }
    public void CloseCalenderButton()
    {
        calenderOpen = false;
        calender.SetActive(calenderOpen);
    }
    public void MovingSecond()
    {
        if (avProVideoCheck == true)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                fullCamPlayer.Pause();
                fullCamPlayer.Control.Seek(fullCamPlayer.Control.GetCurrentTimeMs() + 10000);
                socketServer.BroadCast(new RawPayload<PayloadType>(PayloadType.Play, videoList[fullCamVideoCount]));
                //StartCoroutine(FrameSkip());
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                fullCamPlayer.Pause();
                fullCamPlayer.Control.Seek(fullCamPlayer.Control.GetCurrentTimeMs() - 10000);
                socketServer.BroadCast(new RawPayload<PayloadType>(PayloadType.Play, videoList[fullCamVideoCount]));
                //StartCoroutine(FrameSkip());
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                ++fullCamVideoCount;
                pause = false;
                if (fullCamVideoCount > videoList.Count - 1)
                    fullCamVideoCount = 0;

                fullCamUnityPlayer.url = "file://" + videoList[fullCamVideoCount];
                mainCheck = true;
                fullCamUnityPlayer.Play();
                fullCamUnityPlayer.Pause();
                //socketServer.BroadCast(new RawPayload<PayloadType>(PayloadType.Stop, "Stop"));
                socketServer.BroadCast(new RawPayload<PayloadType>(PayloadType.Play, videoList[fullCamVideoCount]));
                StartCoroutine(PlayerRateStart());
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                pause = false;
                --fullCamVideoCount;
                if (fullCamVideoCount < 0)
                    fullCamVideoCount = videoList.Count - 1;
                fullCamUnityPlayer.url = "file://" + videoList[fullCamVideoCount];
                mainCheck = true;
                fullCamUnityPlayer.Play();
                fullCamUnityPlayer.Pause();
                //socketServer.BroadCast(new RawPayload<PayloadType>(PayloadType.Stop, "Stop"));
                socketServer.BroadCast(new RawPayload<PayloadType>(PayloadType.Play, videoList[fullCamVideoCount]));
                StartCoroutine(PlayerRateStart());
            }
        }
    }
    private void GetIP()
    {
        IPHostEntry entry = Dns.GetHostEntry(Dns.GetHostName());
        IPAddress[] ip = entry.AddressList;
        _ipAddr = ip[ip.Length - 1].ToString();
    }
    IEnumerator PlayerRateStart()
    {
        //videoPlaying = true;

        if(mainCheck)
        { 
            yield return new WaitForSeconds(5f);
            //if (pause == false)
            //{
            socketServer.BroadCast(new RawPayload<PayloadType>(PayloadType.Play, "Start"));
            if (avProVideoCheck == true)
            {
                fullCamPlayer.Play();
            }
            else
            {
                fullCamUnityPlayer.Play();                                
            }
            
            Debug.Log("BroadCasting");
            mainCheck = false;
            //}
        }        
        //IEnumerator FrameSkip()
        //{
        //    yield return new WaitForSeconds(3f);
        //    fullCamPlayer.Play();
        //    for (int i = 0; i < fanCamPlayer.Count; i++)
        //    {
        //        fanCamPlayer[i].Play();
        //    }
        //}
    }
    public void OnAVProVideo()
    {
        avProVideoCheck = !avProVideoCheck;
    }
}
