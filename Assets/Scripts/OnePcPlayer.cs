using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using RenderHeads.Media.AVProVideo;
using System.IO;


public class OnePcPlayer : MonoBehaviour
{
    public static OnePcPlayer Instance { get; private set; }
    public List<string> fanVideoList = new List<string>(); // 메인 동영상의 직캠 배열
    public List<MediaPlayer> fanCamPlayer = new List<MediaPlayer>(); // 메인 동영상의 직캠 재생
    public List<VideoPlayer> fanCamUnityPlayer = new List<VideoPlayer>();
    public List<string> videoList = new List<string>();//메인 동영상 리스트 배열
    private List<string> userInput = new List<string>();//날짜 입력 
    [SerializeField]
    private MediaPlayer fullCamPlayer;//날짜별 메인으로 띄울 동영상을 재생
    [SerializeField]
    private VideoPlayer fullCamUnityPlayer;
    [SerializeField]
    private GameObject calender;

    public Text firstVideoDate;
    public Text lastVideoDate;
    [SerializeField]
    private bool avProVideoCheck = false;
    private bool calenderOpen;
    private bool mainCheck;
    private bool pause;
    public bool fanCamCheck = false;
    
    public int fullCamVideoCount = 0;
    private int helpCount, minus;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }
    // Start is called before the first frame update
    void Start()
    {
        if (Display.displays.Length > 1)
            Display.displays[1].Activate();
        if (Display.displays.Length > 2)
            Display.displays[2].Activate();
        if (Display.displays.Length > 3)
            Display.displays[3].Activate();
        if (Display.displays.Length > 4)
            Display.displays[4].Activate();
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
            //fullCamPlayer.Play();
            fullCamPlayer.Pause();
            FanCamStart("file://" + videoList[fullCamVideoCount]);
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
            FanCamStart(fullCamUnityPlayer.url);
            //StartCoroutine(PlayerRateStart());
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
                //fullCamPlayer.Play();
                fullCamPlayer.Pause();
                mainCheck = true;
                FanCamStart("file://" + videoList[fullCamVideoCount]);
            }
            else
            {
                fullCamUnityPlayer.url = "file://" + videoList[fullCamVideoCount];
                fullCamUnityPlayer.Play();
                fullCamUnityPlayer.Pause();

                mainCheck = true;
                
                FanCamStart(fullCamUnityPlayer.url);
            }
            
        }
        else
        {
            if(avProVideoCheck)
            {
                pause = !pause;
                FanCamStart("file://" + videoList[fullCamVideoCount]);
            }
            else
                pause = !pause;
                FanCamStart(fullCamUnityPlayer.url);
        }
    }

    public void FanCamStart(string videoPath)
    {
        if (avProVideoCheck == true)
        {
            fanVideoList.Clear();

            string[] date = videoPath.Split('\\');
            string[] mainCamName = date[date.Length - 1].Split('_');
            //DirectoryInfo di = new DirectoryInfo(@"\\10.30.16.100\vertigo\" + "\\" + "TestVcam" + "\\" + date[5] + "\\" + mainCamName[4]); // 직캠 모아두는 폴더로 들어감
            DirectoryInfo di = new DirectoryInfo(@"D:\3_vcam_complete" + "\\" + date[2] + "\\" + mainCamName[4]); // 직캠 모아두는 폴더로 들어감
            foreach (var item in di.GetFiles())
            {
                if (fanVideoList.Count >= fanCamPlayer.Count)
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
            {
                //fanCamPlayer[i].Play();
                fanCamPlayer[i].Pause();
            }
            fanCamCheck = true;
            StartCoroutine(PlayerRateStart());
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
                if (fanVideoList.Count >= fanCamPlayer.Count)
                    break;
                fanVideoList.Add(di + "\\" + item.Name);

            }
            switch (fanVideoList.Count)
            {
                case 1:
                    fanCamUnityPlayer[0].url = "file://" + fanVideoList[0];
                    break;
                case 2:
                    fanCamUnityPlayer[0].url = "file://" + fanVideoList[0];
                    fanCamUnityPlayer[1].url = "file://" + fanVideoList[1];
                    break;
                case 3:
                    fanCamUnityPlayer[0].url = "file://" + fanVideoList[0];
                    fanCamUnityPlayer[1].url = "file://" + fanVideoList[1];
                    fanCamUnityPlayer[2].url = "file://" + fanVideoList[2];
                    break;
                case 4:
                    fanCamUnityPlayer[0].url = "file://" + fanVideoList[0];
                    fanCamUnityPlayer[1].url = "file://" + fanVideoList[1];
                    fanCamUnityPlayer[2].url = "file://" + fanVideoList[2];
                    fanCamUnityPlayer[3].url = "file://" + fanVideoList[3];
                    break;
            }
            for (int i = 0; i < fanVideoList.Count; i++)
            {
                fanCamUnityPlayer[i].Play();
                fanCamUnityPlayer[i].Pause();
            }
            fanCamCheck = true;
            StartCoroutine(PlayerRateStart());
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
            for (int i = 0; i < fanVideoList.Count; i++)
            {
                fanCamPlayer[i].Stop();
                fanCamPlayer[i].CloseVideo();
            }
        }
        else
        {
            fullCamVideoCount = 0;
            helpCount = 0;
            minus = 0;
            videoList.Clear();
            userInput.Clear();
            fullCamUnityPlayer.Stop();
            for (int i = 0; i < fanVideoList.Count; i++)
            {                
                fanCamUnityPlayer[i].Stop();
            }
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
                for (int i = 0; i < fanVideoList.Count; i++)
                    fanCamPlayer[i].Pause();
            }
            else
            {
                fullCamPlayer.Play();
                for (int i = 0; i < fanVideoList.Count; i++)
                    fanCamPlayer[i].Play();

            }
        }
        else
        {
            if (pause == true)
            {
                fullCamUnityPlayer.Pause();
                for (int i = 0; i < fanVideoList.Count; i++)
                    fanCamUnityPlayer[i].Pause();
            }
            else
            {
                fullCamUnityPlayer.Play();
                for (int i = 0; i < fanVideoList.Count; i++)
                    fanCamUnityPlayer[i].Play();

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
                mainCheck = true;
                //StartCoroutine(FrameSkip());
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                fullCamPlayer.Pause();
                fullCamPlayer.Control.Seek(fullCamPlayer.Control.GetCurrentTimeMs() - 10000);
                mainCheck = true;
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
                
                
                StartCoroutine(PlayerRateStart());
            }
        }
    }
    public void AVproVideoTurn()
    {
        avProVideoCheck = !avProVideoCheck;
    }
    IEnumerator PlayerRateStart()
    {        
        if (fanCamCheck && mainCheck)
        {            
            yield return new WaitForSeconds(3f);
            if (avProVideoCheck == true)
            {
                if (fanVideoList.Count > 0 && fanCamPlayer.Count > 0)
                {
                    fullCamPlayer.Play();
                    for (int i = 0; i < fanVideoList.Count; i++)
                        fanCamPlayer[i].Play();
                }
            }
            else
            {
                if (fanVideoList.Count > 0 && fanCamUnityPlayer.Count > 0)
                {
                    fullCamUnityPlayer.Play();
                    for (int i = 0; i < fanVideoList.Count; i++)
                        fanCamUnityPlayer[i].Play();
                }
            }
            mainCheck = false;
            fanCamCheck = false;
            //}
        }        
    }
}
