using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Threading;

public class ButtonController : MonoBehaviour, IDisposable
{
    [SerializeField]
    private GameObject calender;
    private bool checkOpen = true;
    private MainVideoPresenter _main;
    private VCamVideoPresenter _vcam;
    private bool pause = false;
    private CancellationTokenSource _cts = new CancellationTokenSource();
    private void Start()
    {
        _main = FindObjectOfType<MainVideoPresenter>();
        _vcam = FindObjectOfType<VCamVideoPresenter>();
        
    }
    private void Update()
    {
        MoveVideo();
        if (_main.checkEndVideo && _main.fullcamPlayer.Control.IsFinished())
        {
            ++_main.videoCount;
            if (_main.videoCount >= _main.fullcamVideoList.Count)
                _main.videoCount = 0;
            StartCoroutine(RestartVideo());
        }
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            checkOpen = !checkOpen;
            calender.SetActive(checkOpen);
        }
        else if (Input.GetKeyDown(KeyCode.Space))
            OnPausePress();
    }
    public void OnDatePress() => _main.MainVideoPath();
    public void StartReady() =>_main.FileOpenReady();
    public void OnPausePress()
    {
        pause = !pause;
        if (pause)
        {
            _main.fullcamPlayer.Pause();
            foreach (var player in _vcam.vcamPlayer)
                player.Pause();
        }
        else if (!pause)
        {
            _main.fullcamPlayer.Play();
            foreach (var player in _vcam.vcamPlayer)
                player.Play();
        }
    }
    public void OnEndButtonPress()
    {
        //_main.fullcamPlayer.Stop();
        _main.fullcamPlayer.CloseVideo();
        foreach (var player in _vcam.vcamPlayer)
            player.CloseVideo();
        _main.fullcamVideoList.Clear();
        _vcam.vcamVideoList.Clear();
    }
    public void OnStartPress()
    {
        _main.fullcamPlayer.Play();
        foreach(var player in _vcam.vcamPlayer)
        {
            player.Play();
        }
        _ = FrameCheck();
    }
    IEnumerator RestartVideo()
    {     
        StartReady();
        yield return new WaitForSeconds(3f);
        OnStartPress();
        _main.checkEndVideo = false;                            
    }    
    private async Task FrameCheck()
    {
        while (!_cts.IsCancellationRequested)
        {
            float _mainTime = _main.fullcamPlayer.Control.GetCurrentTimeMs();
            float _vcamTime = _vcam.vcamPlayer[0].Control.GetCurrentTimeMs();
            float time = _mainTime - _vcamTime;
            if (time > 300 || time < -300)
            {
                _main.fullcamPlayer.Pause();
                foreach (var vcam in _vcam.vcamPlayer)
                    vcam.Pause();
                foreach(var vcam in _vcam.vcamPlayer)
                {
                    vcam.Control.Seek(_main.fullcamPlayer.Control.GetCurrentTimeMs());
                }
                await Task.Delay(3000);
                _main.fullcamPlayer.Play();
                foreach (var vcam in _vcam.vcamPlayer)
                    vcam.Play();
            }
            await Task.Delay(10000);
        }
    }

    
    private void MoveVideo()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            Debug.Log("Right");
            _main.videoCount++;
            if (_main.videoCount >= _main.fullcamVideoList.Count)
                _main.videoCount = 0;
            StartCoroutine(RestartVideo());
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Debug.Log("Left");
            _main.videoCount--;
            if (_main.videoCount < 0)
                _main.videoCount = _main.fullcamVideoList.Count - 1;
            StartCoroutine(RestartVideo());
        }
    }
    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();

    }
}
