// Copyright (C) 2024 Peter Leth

#region
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#endregion

// ReSharper disable UnusedMemberHierarchy.Global
// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Tactility.Box
{
    public enum SerialLogMode
    {
        Inbound,
        Outbound,
        All,
        None
    }

    [RequireComponent(typeof(SerialController))]
    public abstract class AbstractBoxController : MonoBehaviour
    {
        [Tooltip("Controls the logging of serial messages. Outbound: Logs messages sent from the device. Inbound: Logs messages received by the device. All: Logs all messages, both inbound and outbound. Use this to debug and monitor serial communication. Only logs when running in DEBUG mode.")]
        public SerialLogMode logMode = SerialLogMode.None;

        [Tooltip("The delay in milliseconds between sending messages. This is useful for devices that require a delay between sending commands. The delay is in milliseconds. The default value is 50 second.")]
        public float messageDelay = 50f;
        [Tooltip("The maximum number of messages that can be queued. If the queue is full, messages will be dropped.")]
        public int maxQueueSize = 10;
        protected readonly Queue<string> MessageQueue = new Queue<string>();
        protected bool IsSendingMessages;

        protected SerialController Sc;

        public string Port { get; protected set; }
        public string Battery { get; protected set; }
        public string Voltage { get; protected set; }
        public string Current { get; protected set; }
        public string Temperature { get; protected set; }
        public bool IsConnected { get; protected set; }


        protected virtual void Start()
        {
            Sc = GetComponent<SerialController>();
            Sc.SetTearDownFunction(DisableStimulation);

            // DontDestroyOnLoad(this);
        }

        protected virtual void Update()
        {
            if (!IsSendingMessages && MessageQueue.Count > 0)
            {
                StartCoroutine(SendMessagesFromQueue());
            }
        }

        public abstract void Connect(string port);
        public abstract void EnableStimulation();
        public abstract void DisableStimulation();
        public abstract void ResetAllPads();
        public abstract string GetStimString(int[] pads, float[] amps, int[] widths);
        public abstract string GetFreqString(int frequency);

        private IEnumerator SendMessagesFromQueue()
        {
            IsSendingMessages = true;
            while (MessageQueue.Count > 0)
            {
                var message = MessageQueue.Dequeue();
                Sc.SendSerialMessage($"{message}\r");
                yield return new WaitForSeconds(messageDelay / 1_000f);
            }
            IsSendingMessages = false;
        }

        protected void QueueMessage(string message, bool ignoreQueueSize = false)
        {
            if (MessageQueue.Count >= maxQueueSize && !ignoreQueueSize)
            {
                Debug.LogWarning($"Message queue is full, dropping message: {message}");
                return;
            }
            MessageQueue.Enqueue(message);

#if DEBUG
            if (logMode is SerialLogMode.Outbound or SerialLogMode.All)
            {
                Debug.Log($"{this} Outbound message queued: {message}");
            }
#endif
        }

        public void Send(in string message)
        {
            QueueMessage(message);
        }

        public IEnumerator SendDelayed(string message, float delay, Action callback = null)
        {
            yield return new WaitForSeconds(delay);
            Send(message);
            callback?.Invoke();
        }

        public void SendMany(in IEnumerable<string> messages)
        {
            foreach (var msg in messages)
            {
                Send(msg);
            }
        }

        public IEnumerator SendManyEachDelayed(IEnumerable<string> messages, float delay, Action callback = null)
        {
            foreach (var message in messages)
            {
                yield return SendDelayed(message, delay);
            }

            callback?.Invoke();
        }

        protected virtual void OnMessageArrived(string message)
        {
#if DEBUG
            if (logMode is SerialLogMode.Inbound or SerialLogMode.All)
            {
                Debug.Log($"{this} Inbound message received: {message}");
            }
#endif
        }

        protected virtual void OnConnectionEvent(bool wasSuccessful)
        {
#if DEBUG
            Debug.Log(this + (wasSuccessful
                ? ": Connection established"
                : ": Connection attempt failed or disconnection detected"));
#endif
        }
    }
}
