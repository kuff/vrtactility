// Copyright (C) 2024 Peter Leth

#region
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#endregion

namespace Tactility.Calibration.Interface
{
    public abstract class DropdownManager<T> : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The dropdown menu for selecting items.")]
        protected Dropdown dropdown;

        [SerializeField]
        [CanBeNull]
        [Tooltip("The default item to use if no other is selected. It is not required that this field be specified.")]
        protected T defaultItem;

        protected virtual void Start()
        {
            UpdateItems();
        }

        protected abstract List<T> GetAllItems();
        protected abstract void SetSelectedItem(T item);
        protected abstract string GetItemName(T item);

        public void UpdateItems()
        {
            dropdown.ClearOptions();
            var items = GetAllItems();

            if (defaultItem != null && items.Contains(defaultItem))
            {
                dropdown.options.Add(new Dropdown.OptionData(GetItemName(defaultItem)));
                items.Remove(defaultItem); // Prevent adding it twice
            }

            // Get the items
            foreach (var item in items)
            {
                dropdown.options.Add(new Dropdown.OptionData(GetItemName(item)));
            }

            try
            {
                // Use either the default item or the first item in the list for the dropdown label
                var itemText = string.IsNullOrEmpty(GetItemName(defaultItem)) ? GetItemName(items[0]) : GetItemName(defaultItem);
                dropdown.GetComponentInChildren<Text>()!.text = itemText;
            }
            catch (IndexOutOfRangeException e)
            {
                // Notify that no items were found
                Debug.LogError(e.Message);
            }
        }
        
        public virtual void UpdateSelectedItem()
        {
            if (dropdown.options.Count <= dropdown.value)
            {
                return;
            }

            // If no explicit item was selected yet, use the dropdown label
            var selectedItem = string.IsNullOrEmpty(dropdown.options[dropdown.value]!.text) ? dropdown.GetComponentInChildren<Text>()!.text : dropdown.options[dropdown.value]!.text;
            var item = GetAllItems().Find(i => GetItemName(i) == selectedItem);
            SetSelectedItem(item);
        }
    }
}
