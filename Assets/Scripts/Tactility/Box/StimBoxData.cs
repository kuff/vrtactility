// Copyright (C) 2024 Peter Leth

#region
using System;
using UnityEditor;
using UnityEngine;
#endregion

namespace Tactility.Box
{
    public class StimBoxData : ScriptableObject
    {
        private const string ResourcePath = "Tactility/StimBoxData";

        private static StimBoxData _instance;
        public string capacity;
        public string voltage;
        public string current;
        public string temperature;

        // Unix timestamp for last updated time
        [SerializeField]
        private long lastUpdatedUnix;

        // Property to get and set the last updated time as DateTime
        public DateTime LastUpdated
        {
            get => DateTimeOffset.FromUnixTimeSeconds(lastUpdatedUnix).UtcDateTime;
            private set => lastUpdatedUnix = ((DateTimeOffset)value).ToUnixTimeSeconds();
        }

        public static StimBoxData Instance
        {
            get
            {
                if (_instance)
                {
                    return _instance;
                }

                // Try to load the asset from the Resources folder
                _instance = Resources.Load<StimBoxData>(ResourcePath);
                if (_instance)
                {
                    return _instance;
                }

                // Only if it doesn't exist, create a new instance and save it
                _instance = CreateInstance<StimBoxData>();
                _instance.LastUpdated = DateTime.Now;
                SaveAsset(_instance, "StimBoxData");
                return _instance;
            }
        }

        public void UpdateData(string newCapacity, string newVoltage, string newCurrent, string newTemperature)
        {
            capacity = newCapacity;
            voltage = newVoltage;
            current = newCurrent;
            temperature = newTemperature;
            LastUpdated = DateTime.UtcNow; // Update the timestamp to current time
            SaveAsset(_instance, "StimBoxData");
        }

        public string GetTimeSinceLastUpdate()
        {
            // Get the current Unix timestamp and the last updated Unix timestamp
            var currentUnix = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds();
            var unix = lastUpdatedUnix;

            // Calculate the time difference in seconds
            var timeDifference = currentUnix - unix;

            // Convert the time difference to TimeSpan for easy manipulation
            var timeSinceUpdate = TimeSpan.FromSeconds(timeDifference);

            if (timeSinceUpdate.TotalDays >= 1)
            {
                return $">{(int)timeSinceUpdate.TotalDays} day(s) ago";
            }
            if (timeSinceUpdate.TotalHours >= 1)
            {
                return $">{(int)timeSinceUpdate.TotalHours} hour(s) ago";
            }
            if (timeSinceUpdate.TotalMinutes >= 1)
            {
                return $">{(int)timeSinceUpdate.TotalMinutes} minute(s) ago";
            }
            if (timeSinceUpdate.TotalSeconds >= 1)
            {
                return $">{(int)timeSinceUpdate.TotalSeconds} second(s) ago";
            }

            return "just now";
        }

        private static void SaveAsset(StimBoxData asset, string name)
        {
#if UNITY_EDITOR
            // Update the timestamp when modified
            asset.LastUpdated = DateTime.UtcNow;

            // Define the path where the asset will be saved
            var assetPath = $"Assets/Resources/Tactility/{name}.asset";

            // Check if the asset already exists
            var existingAsset = AssetDatabase.LoadAssetAtPath<StimBoxData>(assetPath);
            if (existingAsset == null)
            {
                // If the asset does not exist, create it
                AssetDatabase.CreateAsset(asset, assetPath);
                // Set the asset's name to match the file name
                asset.name = name;
            }
            else
            {
                // If it exists, update the existing asset to ensure any changes are saved
                EditorUtility.CopySerialized(asset, existingAsset);
                asset = existingAsset;
            }

            // Mark the asset as dirty to ensure it gets saved
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
#endif
        }
    }
}
