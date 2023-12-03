using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Internal
{
    [RequireComponent(typeof(SerialController))]
    public abstract class AbstractMessenger : MonoBehaviour
    {
        public SerialLogModes logMode;

        public string Port { get; private set; }
        public string Battery { get; private set; }
        public bool IsConnected { get; private set; }
        
        public abstract void Connect();
        public abstract void EnableStimulation();
        public abstract void DisableStimulation();
        
        private SerialController _sc;

        protected virtual void Start()
        {
            _sc = GetComponent<SerialController>();
            _sc.SetTearDownFunction(() => Send("stim off"));
        
            DontDestroyOnLoad(this);
        }
        
        public IEnumerator Send(string message, Action callback = null, float invokeWait = 0f)
        {
#if UNITY_EDITOR
            if (logMode is SerialLogModes.Outbound or SerialLogModes.Both)
                Debug.Log($"Outbound message queued: {message}");
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
        
        // ReSharper disable once UnusedMember.Global
        protected virtual void OnMessageArrived(string message)
        {
#if UNITY_EDITOR
            if (logMode is SerialLogModes.Inbound or SerialLogModes.Both)
                Debug.Log($"Inbound message received: {message}");
#endif
        }

        // ReSharper disable once UnusedMember.Global
        protected virtual void OnConnectionEvent(bool value)
        {
#if UNITY_EDITOR
            Debug.Log(value ? "Connection established" : "Connection attempt failed or disconnection detected");
#endif
        }
    }
}
