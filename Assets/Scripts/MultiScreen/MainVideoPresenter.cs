using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RenderHeads.Media.AVProVideo;
using System.IO;
using System;
using System.Threading.Tasks;
public class MainVideoPresenter : MonoBehaviour
{
    [SerializeField]
    public List<string> fullcamVideoList = new List<string>();    
    private readonly List<string> inputDate = new List<string>();
    [SerializeField]
    public MediaPlayer fullcamPlayer;
    public event Action<string> OnVcamSetting = videoPath => { };
    
    //private CalendarController _calendarController;

    public int videoCount = 0;
    public bool checkEndVideo;
    private int minus = 0;
       
    // Start is called before the first frame update
    void Start()
    {
        //_calendarController = new CalendarController();
        if (Display.displays.Length > 1)
            Display.displays[1].Activate();
        if (Display.displays.Length > 2)
            Display.displays[2].Activate();
        if (Display.displays.Length > 3)
            Display.displays[3].Activate();
        if (Display.displays.Length > 4)
            Display.displays[4].Activate();
    }
    private void Update()
    {
        if (fullcamPlayer.Control.IsFinished())
            checkEndVideo = true;

    }
    public void MainVideoPath()
    {
        fullcamVideoList.Clear();
        int helpCount = int.Parse(CalendarController._calendarInstance.firstVideoDate.text);
        if (CalendarController._calendarInstance.lastVideoDate.text != "")
            minus = int.Parse(CalendarController._calendarInstance.lastVideoDate.text) - int.Parse(CalendarController._calendarInstance.firstVideoDate.text);
        if (minus >= 0)
        {
            for (int i = 0; i <= minus; i++)
            {
                inputDate.Add(helpCount.ToString());
                helpCount++;
            }
        }
        else if (minus < 0)
        {
            inputDate.Add(helpCount.ToString());
        }
        for (int i = 0; i < inputDate.Count; i++)
        {
            DirectoryInfo di = new DirectoryInfo(@"C:\2_fullcam_Acut\" + inputDate[i]);
            if (di.Exists == true)
            {
                foreach (var item in di.GetFiles())
                {
                    fullcamVideoList.Add(di + "\\" + item.Name);                    
                }
            }
        }
        FileOpenReady();
    }    
    public void FileOpenReady()
    {        
        OnVcamSetting.Invoke(fullcamVideoList[videoCount]);
        fullcamPlayer.OpenVideoFromFile(0, "file://" + fullcamVideoList[videoCount], false);
        fullcamPlayer.Pause();
    }    
}
