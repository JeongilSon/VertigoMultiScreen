using System;
using UnityEngine;

[Serializable]
public class RawPayload<T> where T : Enum
{
    public T Type => _type;
    public string Body => _body;    
    [SerializeField]
    private T _type;
    [SerializeField]
    private string _body;    
    public RawPayload(T type, string body)
    {
        _type = type;
        _body = body;
    }    
   
    public override string ToString()
    {
        return $"Type: {Type}, Body: {Body}";
    }

}
