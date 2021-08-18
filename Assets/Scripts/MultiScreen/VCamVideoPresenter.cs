using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RenderHeads.Media.AVProVideo;
using System.IO;
public class VCamVideoPresenter : MonoBehaviour
{
    private MainVideoPresenter _mainVideoPresenter;
    [SerializeField]
    public List<string> vcamVideoList = new List<string>();
    [SerializeField]
    public List<MediaPlayer> vcamPlayer = new List<MediaPlayer>();
    private void Start()
    {
        _mainVideoPresenter = FindObjectOfType<MainVideoPresenter>();        
        _mainVideoPresenter.OnVcamSetting += VcamPathSetting;
    }

    public void VcamPathSetting(string videoPath)
    {
        vcamVideoList.Clear();
        string[] groupName = videoPath.Split('_');
        string[] date = videoPath.Split('\\');
        DirectoryInfo di = new DirectoryInfo(@"C:\3_vcam_complete\" + date[2] + "\\" + groupName[6]);
        if (di.Exists == true)
        {
            foreach (var item in di.GetFiles())
            {
                vcamVideoList.Add(di + "\\" + item.Name);
            }
        }
        for(int i = 0; i < vcamPlayer.Count; i++)
        {
            vcamPlayer[i].OpenVideoFromFile(0, "file://" + vcamVideoList[i], false);
            vcamPlayer[i].Pause();
        }
    }    
}
