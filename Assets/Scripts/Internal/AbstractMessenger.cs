using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// ReSharper disable MemberCanBePrivate.Global

namespace Internal
{
    [RequireComponent(typeof(SerialController))]
    public abstract class AbstractMessenger : MonoBehaviour
    {
        [Tooltip("Controls the logging of serial messages. Outbound: Logs messages sent from the device. Inbound: Logs messages received by the device. All: Logs all messages, both inbound and outbound. Use this to debug and monitor serial communication. Only logs when running through the Editor.")]
        public SerialLogModes logMode;

        public string Port { get; protected set; }
        public string Battery { get; protected set; }
        public bool IsConnected { get; protected set; }
        
        public abstract void Connect(string port);
        public abstract void EnableStimulation();
        public abstract void DisableStimulation();
        
        protected SerialController Sc;

        protected virtual void Start()
        {
            Sc = GetComponent<SerialController>();
            // _sc.SetTearDownFunction(() => Send("stim off"));
        
            DontDestroyOnLoad(this);
        }
        
        public IEnumerator Send(string message, Action callback = null, float invokeWait = 0f)
        {
#if UNITY_EDITOR
            if (logMode is SerialLogModes.Outbound or SerialLogModes.All)
                Debug.Log($"{this}: Outbound message queued: {message}");
#endif
            Sc.SendSerialMessage($"{message}\r");
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
        
        // ReSharper disable once UnusedMember.Global
        protected virtual void OnMessageArrived(string message)
        {
#if UNITY_EDITOR
            if (logMode is SerialLogModes.Inbound or SerialLogModes.All)
                Debug.Log($"{this}: Inbound message received: {message}");
#endif
        }

        // ReSharper disable once UnusedMember.Global
        protected virtual void OnConnectionEvent(bool value)
        {
#if UNITY_EDITOR
            Debug.Log(this + (value ? ": Connection established" : ": Connection attempt failed or disconnection detected"));
#endif
        }
    }
}
