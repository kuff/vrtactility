using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SerialMessenger : SerialController
{
    public bool logInboundMessages;
    public bool logOutboundMessages;
    
    public int Port { get; private set; }
    public int Battery { get; private set; }
    public bool IsConnected { get; private set; }

    public void Connect(int portNumber, Func<bool> callback = null)
    {
        Port = portNumber;

        portName = $"COM{Port}";
        SetTearDownFunction(() => SendSerialMessage("stim off\r"));
        enabled = true;
        
        DontDestroyOnLoad(this);

        StartCoroutine(SendDelayed(200f, new[]
        {
            "iam TACTILITY\r",
            "elec 1 *pads_qty 32\r",
            "battery ?\r",
            "freq 50\r"
        }, callback: callback));
    }

    public IEnumerator SendDelayed(float millisecondsDelay, IEnumerable<string> messages, Func<bool> callback = null)
    {
        yield return new WaitForSeconds(millisecondsDelay / 1000f);
        
        foreach (var msg in messages)
            SendSerialMessage(msg);

        callback?.Invoke();
    }
    
    private void OnMessageArrived(string msg)
    {
#if UNITY_EDITOR
        // ... 
#endif
    }
}
