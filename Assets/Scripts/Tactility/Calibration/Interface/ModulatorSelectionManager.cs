using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Tactility.Calibration.Interface
{
    public class ModulatorSelectionManager : DropdownManager<string>
    {
        private string _selectedModulatorName;
        public GameObject tactilityManager;
        public Text textBox_modality;

        protected override List<string> GetAllItems()
        {
            var modulatorNames = new List<string>();
            Transform[] children = tactilityManager.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in children)
            {
                // Skip the parent object itself
                if (child == tactilityManager.transform)
                    continue;

                // Check if the child is active
                bool isActive = child.gameObject.activeSelf;
                if (child.gameObject.name != "ConstantModulator")
                {
                    modulatorNames.Add(child.gameObject.name);
                }
            }
            return modulatorNames;
        }

        protected override void SetSelectedItem(string modulatorName)
        {
            _selectedModulatorName = modulatorName;
        }

        protected override string GetItemName(string item)
        {
            return item;
        }

        public void ChangeSelectedModulator()
        {
            UpdateSelectedItem();
            if (!string.IsNullOrEmpty(_selectedModulatorName))
            {
                Transform[] children = tactilityManager.GetComponentsInChildren<Transform>(true);
                foreach (Transform child in children)
                {
                    if (child == tactilityManager.transform)
                        continue;

                    if (child.gameObject.name == _selectedModulatorName)
                    {
                        child.gameObject.SetActive(true);
                        textBox_modality.text = child.gameObject.name;
                    }
                    else
                    {
                        child.gameObject.SetActive(false);
                    }
                }
            }
        }
    }
}
