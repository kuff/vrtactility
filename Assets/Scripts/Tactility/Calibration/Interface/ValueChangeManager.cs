// Copyright (C) 2024 Peter Leth

#region
using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
#endregion

namespace Tactility.Calibration.Interface
{
    public class ValueChangeManager : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The text field to display the value")]
        private InputField inputField;
        [SerializeField]
        [Tooltip("The amount to increment or decrement")]
        private float changeAmount = 1;
        [SerializeField]
        private float defaultValue;

        private float _currentValue; // Internal value to keep track of the current number

        private void Start()
        {
            try
            {
                _currentValue = defaultValue;
                inputField.text = FloatToText(_currentValue);
            }
            catch (Exception _)
            {
                // Do nothing...
            }
        }

        // Invoked when the increment button is clicked
        public void IncrementValue()
        {
            if (inputField == null)
            {
                return;
            }
            
            var currentValue = float.Parse(inputField.text, CultureInfo.InvariantCulture);
            currentValue += changeAmount;
            inputField.text = FloatToText(currentValue);
        }

        // Invoked when the decrement button is clicked
        public void DecrementValue()
        {
            if (inputField == null)
            {
                return;
            }
            
            var currentValue = float.Parse(inputField.text, CultureInfo.InvariantCulture);
            currentValue -= changeAmount;
            Debug.Log($"{inputField.text}...{currentValue.ToString(CultureInfo.InvariantCulture)}");
            inputField.text = FloatToText(currentValue);
        }

        public static float TextToFloat(string text)
        {
            var normalizedString = text.Replace(",", ".");
            return float.Parse(normalizedString, CultureInfo.InvariantCulture);
        }
        
        public static string FloatToText(float value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }
    }
}
