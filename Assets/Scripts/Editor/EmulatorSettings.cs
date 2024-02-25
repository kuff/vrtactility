using UnityEngine;

namespace Editor
{
    public class EmulatorSettings : ScriptableObject
    {
        public string comPort = "COM2";
        public int baudRate = 115200;
        public bool enableLogging = true;

        private static EmulatorSettings _instance;
        private const string AssetFolderPath = "Assets/Resources/Tactility";
        private const string AssetPath = AssetFolderPath + "/EmulatorSettings.asset";

        public static EmulatorSettings Instance
        {
            get
            {
                if (_instance) return _instance;
#if UNITY_EDITOR
                _instance = UnityEditor.AssetDatabase.LoadAssetAtPath<EmulatorSettings>(AssetPath);
                if (_instance) 
                    return _instance;
                
                if (!System.IO.Directory.Exists(AssetFolderPath)) 
                    System.IO.Directory.CreateDirectory(AssetFolderPath);

                _instance = CreateInstance<EmulatorSettings>();
                UnityEditor.AssetDatabase.CreateAsset(_instance, AssetPath);
                UnityEditor.AssetDatabase.SaveAssets();
#endif
                return _instance;
            }
        }
    }
}