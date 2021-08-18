using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Text;
[Serializable]
public class SocketServer<T> : IDisposable where T : Enum
{
    public event Action<RawPayload<T>> OnReceive = _ => { };    
    //통신할 소켓
    private readonly Socket _videoSocket;
    public ConcurrentDictionary<Guid, Socket> _clientSessions;
    private int _timeOutMsceonds;
    private readonly CancellationTokenSource _cts;
    public bool CheckConnect { get; private set; }
    public SocketServer(IPAddress ipAddr, int portNum, int timeOutMsceonds = 10000)
    {
        _timeOutMsceonds = timeOutMsceonds;
        _videoSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _videoSocket.Bind(new IPEndPoint(ipAddr, portNum));
        _cts = new CancellationTokenSource();     
        _clientSessions = new ConcurrentDictionary<Guid, Socket>();        
        Debug.Log("SocketBInd");
        _ = Task.Run(() => StartServer(_videoSocket, _cts.Token));                
    }
    private async Task StartServer(Socket server, CancellationToken token)
    {
        server.Listen(20);
        Debug.Log("ListeningServer");
        while(!token.IsCancellationRequested)
        {
            var session = await server.AcceptAsync(); // 들어오는 연결시도 받을때까지 대기
            Debug.Log("Connect");
            var id = new Guid();
            _clientSessions[id] = session;            
            _ = Task.Run(() => ReceiveClientAsync(id,session));
        }
    }
    private async Task ReceiveClientAsync(Guid id, Socket session)
    {
        using(session)
        {
            var buffer = new byte[1024 * 1024];
            var bufferArraySegment = new ArraySegment<byte>(buffer); // buffer로 들어온 값 저장
            while(session.Connected)
            {                    
                Debug.Log("Session true");                
                var size = await session.ReceiveAsync(bufferArraySegment, SocketFlags.None);
                var response = Encoding.UTF8.GetString(bufferArraySegment.Array);
                await session.ReceiveAsync(bufferArraySegment, SocketFlags.None);
                var payload = JsonUtility.FromJson<RawPayload<T>>(response);

                OnReceive.Invoke(payload);
            }                
            _clientSessions.TryRemove(id, out _);
        }
    }
    public void BroadCast(RawPayload<T> payload)
    {
        var rawPayload = JsonUtility.ToJson(payload);
        var request = Encoding.UTF8.GetBytes(rawPayload);
        var requestArraySegment = new ArraySegment<byte>(request);        
        foreach(var session in _clientSessions.Values)
        {
            Task.Run(() => session.SendAsync(requestArraySegment, SocketFlags.None).Wait(_timeOutMsceonds));            
        }
    }
    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
        _videoSocket.Dispose();
        foreach(var session in _clientSessions.Values)
        {
            session.Dispose();
        }
        
    }
}
