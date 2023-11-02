using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SerialController))]
public class SerialManager : MonoBehaviour
{
    [Tooltip("Whether inbound and/or outbound messages should be logged to the console (only available when running through the Editor.)")]
    public SerialLogMode logMode;
    
    public string Port { get; private set; }
    public string Battery { get; private set; }
    public bool IsConnected { get; private set; }
    
    private SerialController _sc;
    private bool _receivedValidGreeting;

    private void Start()
    {
        _sc = GetComponent<SerialController>();
        _sc.SetTearDownFunction(() => Send("stim off"));
        
        DontDestroyOnLoad(this);
    }

    public void Connect(string portNumber, Action callback = null)
    {
        Port = portNumber;
        _sc.portName = $"COM{Port}";
        enabled = true;
        
        StartCoroutine(SendManyDelayed(new[]
        {
            "iam TACTILITY",
            "elec 1 *pads_qty 32",
            "battery ?",
            "freq 50"
        }
            , millisecondsDelay: 200f
            , callback: callback)
        );
    }

    public IEnumerator Send(string message, Action callback = null, float invokeWait = 0f)
    {
#if UNITY_EDITOR
        if (logMode is SerialLogMode.Outbound or SerialLogMode.Both)
            Debug.Log("Outbound message queued: " + message);
#endif
        _sc.SendSerialMessage($"{message}\r");
        yield return new WaitForSeconds(invokeWait / 1_000f);
        callback?.Invoke();
    }

    public IEnumerator SendMany(IEnumerable<string> messages, Action callback = null, float invokeWait = 0f)
    {
        foreach (var msg in messages)
            Send(msg);
        yield return new WaitForSeconds(invokeWait / 1_000f);
        callback?.Invoke();
    }

    public IEnumerator SendManyDelayed(IEnumerable<string> messages, float millisecondsDelay, Action callback = null)
    {
        yield return new WaitForSeconds(millisecondsDelay / 1_000f);
        SendMany(messages);
        callback?.Invoke();
    }

    public void EnableStimulation()
    {
        Send("stim on");
    }
    
    public void ResetStimulation()
    {
        Send("velec 11 *selected 0");
    }
    
    public void DisableStimulation()
    {
        Send("stim off");
    }

    // ReSharper disable once UnusedMember.Global
    protected void OnMessageArrived(string message)
    {
#if UNITY_EDITOR
        if (logMode is SerialLogMode.Inbound or SerialLogMode.Both)
            Debug.Log("Inbound message received: " + message);
#endif
        CheckAndSetBattery(message);
        if (_receivedValidGreeting || !IsConnected) return;
        
        _receivedValidGreeting = message is "Re:[] new connection" or "Re:[] re-connection" or "Re:[] ok";
        IsConnected = _receivedValidGreeting;
    }

    // ReSharper disable once UnusedMember.Global
    protected void OnConnectionEvent(bool value)
    {
#if UNITY_EDITOR
        Debug.Log(value ? "Connection established" : "Connection attempt failed or disconnection detected");
#endif
    }
    
    private void CheckAndSetBattery(string message)
    {
        // Check if response string contains the known battery response sequence and ignore it if it does not
        const string checkString = "Re:[] battery *capacity=";
        if (message.Length < checkString.Length || message[..checkString.Length] != checkString)
            return;

        Battery = message;
    }
}
