using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Text;
using System.Linq;
[Serializable]
public class ClientSocket<T> : IDisposable where T : Enum
{
    public event Action<RawPayload<T>> OnReceive = _ => { };
    //통신할 소켓
    private readonly Socket _clinetScoket;
    private readonly int _timeOutMsceonds;
    private readonly CancellationTokenSource _cts;
    private readonly EndPoint _endPoint;
    public RawPayload<T> payload;
    public bool checkReceive;


    public ClientSocket(IPAddress ipAddr, int portNum, int timeOutMsceonds = 10000)
    {

        _timeOutMsceonds = timeOutMsceonds;
        _clinetScoket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _cts = new CancellationTokenSource();
        _endPoint = new IPEndPoint(ipAddr, portNum);
        _ = ConnectAssync(_clinetScoket, _cts.Token);

    }

    private async Task ConnectAssync(Socket socket, CancellationToken token)
    {
        await socket.ConnectAsync(_endPoint);

        while (!token.IsCancellationRequested && socket.Connected)
        {
            var buffer = new byte[1024 * 1024];
            var bufferArraySegment = new ArraySegment<byte>(buffer);
            var size = await socket.ReceiveAsync(bufferArraySegment, SocketFlags.None);
            var response = Encoding.Default.GetString(bufferArraySegment.Array);
            payload = JsonUtility.FromJson<RawPayload<T>>(response);
            OnReceive.Invoke(payload);
            checkReceive = true;

        }
    }
    public void Send(RawPayload<T> payload)
    {
        var rawPayload = JsonUtility.ToJson(payload);
        var request = Encoding.Default.GetBytes(rawPayload);
        var requestArraySegment = new ArraySegment<byte>(request);

        _clinetScoket.SendAsync(requestArraySegment, SocketFlags.None).Wait(_timeOutMsceonds);
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
        _clinetScoket.Dispose();
    }
}
