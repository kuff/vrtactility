// Copyright (C) 2024 Peter Leth

#region
using System.IO;
using UnityEditor;
using UnityEngine;
#endregion

namespace Editor
{
    public class EmulatorSettings : ScriptableObject
    {
        private const string AssetFolderPath = "Assets/Resources/Tactility";
        private const string AssetPath = AssetFolderPath + "/EmulatorSettings.asset";

        private static EmulatorSettings _instance;
        public string comPort = "COM2";
        public int baudRate = 115200;
        public bool enableLogging = true;
        public int reconnectionDelay = 1000;
        public int maxUnreadMessages = 10;

        public static EmulatorSettings Instance
        {
            get
            {
                if (_instance)
                {
                    return _instance;
                }
#if UNITY_EDITOR
                _instance = AssetDatabase.LoadAssetAtPath<EmulatorSettings>(AssetPath);
                if (_instance)
                {
                    return _instance;
                }

                if (!Directory.Exists(AssetFolderPath))
                {
                    Directory.CreateDirectory(AssetFolderPath);
                }

                _instance = CreateInstance<EmulatorSettings>();
                AssetDatabase.CreateAsset(_instance, AssetPath);
                AssetDatabase.SaveAssets();
#endif
                return _instance;
            }
        }
    }
}
