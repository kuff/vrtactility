using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

// ReSharper disable UnusedMemberHierarchy.Global
// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Internal.Box
{
    [RequireComponent(typeof(SerialController))]
    public abstract class AbstractBoxController : MonoBehaviour
    {
        [FormerlySerializedAs("logMode")] [Tooltip("Controls the logging of serial messages. Outbound: Logs messages sent from the device. Inbound: Logs messages received by the device. All: Logs all messages, both inbound and outbound. Use this to debug and monitor serial communication. Only logs when running through the Editor.")]
        public SerialLogMode logMode;

        public string Port { get; protected set; }
        public string Battery { get; protected set; }
        public string Voltage { get; protected set; }
        public string Current { get; protected set; }
        public string Temperature { get; protected set; }
        public bool IsConnected { get; protected set; }
        
        public abstract void Connect(string port);
        public abstract void EnableStimulation();
        public abstract void DisableStimulation();
        
        protected SerialController Sc;

        protected virtual void Start()
        {
            Sc = GetComponent<SerialController>();
        
            DontDestroyOnLoad(this);
        }
        
        public void Send(in string message)
        {
#if UNITY_EDITOR
            if (logMode is SerialLogMode.Outbound or SerialLogMode.All)
                Debug.Log($"{this} Outbound message queued: {message}");
#endif
            Sc.SendSerialMessage($"{message}\r");
        }
        
        public IEnumerator SendAsync(string message, float delayInSeconds, Action callback = null)
        {
            yield return new WaitForSeconds(delayInSeconds);
            Send(message);
            callback?.Invoke();
        }

        public void SendMany(in IEnumerable<string> messages)
        {
            foreach (var msg in messages)
                Send(msg);
        }
        
        public IEnumerator SendManyAsync(IEnumerable<string> messages, float delayBetweenMessagesInSeconds, Action callback = null)
        {
            foreach (var message in messages)
                yield return SendAsync(message, delayBetweenMessagesInSeconds);
            
            callback?.Invoke();
        }

        public void SendManyDelayed(in IEnumerable<string> messages)
        {
            SendMany(messages);
        }
        
        public IEnumerator SendManyDelayedAsync(IEnumerable<string> messages, float initialDelayInSeconds, float delayBetweenMessagesInSeconds, Action callback = null)
        {
            yield return new WaitForSeconds(initialDelayInSeconds);
            foreach (var message in messages)
                yield return SendAsync(message, delayBetweenMessagesInSeconds);
            
            callback?.Invoke();
        }
        
        protected virtual void OnMessageArrived(string message)
        {
#if UNITY_EDITOR
            if (logMode is SerialLogMode.Inbound or SerialLogMode.All)
                Debug.Log($"{this} Inbound message received: {message}");
#endif
        }
        
        protected virtual void OnConnectionEvent(bool wasSuccessful)
        {
#if UNITY_EDITOR
            Debug.Log(this + (wasSuccessful ? ": Connection established" : ": Connection attempt failed or disconnection detected"));
#endif
        }
    }
}
