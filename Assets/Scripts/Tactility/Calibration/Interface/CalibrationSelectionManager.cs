using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Tactility.Calibration.Interface
{
    public class CalibrationSelectionManager : DropdownManager<string>
    {
        [Tooltip("Indicates if the 'Start New Calibration' option is selected.")]
        public bool isNewCalibrationSelected;

        // Add a constant for the 'Start New Calibration' option
        private const string StartNewCalibrationOption = "Start New Calibration";

        protected override List<string> GetAllItems()
        {
            // Define the directory to search for calibration files
            var directoryPath = Application.persistentDataPath;
            var fileInfo = new DirectoryInfo(directoryPath).GetFiles();

            // Pattern to identify calibration files
            const string pattern = @"^[a-zA-Z0-9]+_calibration_vrt\d+\.\d+.*\.txt$";

            // Filtering files according to the regex pattern
            var calibrationFiles = fileInfo.Where(file => Regex.IsMatch(file.Name, pattern))
                                                    .Select(file => file.Name)
                                                    .ToList();

            // Add the option for starting new calibration at the beginning of the list
            calibrationFiles.Insert(0, StartNewCalibrationOption);

            return calibrationFiles;
        }

        protected override void Start()
        {
            base.Start();
            // Check if there's a default value set, if not, make 'Start New Calibration' the default
            if (string.IsNullOrEmpty(defaultItem) || dropdown.options.All(opt => opt.text != defaultItem))
            {
                dropdown.value = dropdown.options.FindIndex(opt => opt.text == StartNewCalibrationOption);
                UpdateSelectedItem();
            }
        }

        public override void UpdateSelectedItem()
        {
            if (dropdown.options.Count <= dropdown.value)
            {
                return;
            }
            
            var selectedItem = dropdown.options[dropdown.value].text;
            if (selectedItem == StartNewCalibrationOption)
            {
                isNewCalibrationSelected = true;
                Debug.Log("Starting new calibration process...");
                // Placeholder for any function that you would call to start a new calibration
            }
            else
            {
                isNewCalibrationSelected = false;
                SetSelectedItem(selectedItem);
            }
        }

        protected override void SetSelectedItem(string fileName)
        {
            // Load the selected calibration file
            CalibrationManager.LoadCalibrationDataFromFile(fileName);
        }

        protected override string GetItemName(string item)
        {
            // Return the name of the file as the item name in the dropdown
            return item;
        }
    }
}
