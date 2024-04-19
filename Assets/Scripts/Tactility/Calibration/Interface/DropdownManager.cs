// Copyright (C) 2024 Peter Leth

#region
using JetBrains.Annotations;
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
            dropdown.ClearOptions();
            var items = GetAllItems();

            if (defaultItem != null && items.Contains(defaultItem))
            {
                dropdown.options.Add(new Dropdown.OptionData(GetItemName(defaultItem)));
                items.Remove(defaultItem); // Prevent adding it twice
            }

            foreach (var item in items)
            {
                dropdown.options.Add(new Dropdown.OptionData(GetItemName(item)));
            }
        }

        protected abstract List<T> GetAllItems();
        protected abstract void SetSelectedItem(T item);
        protected abstract string GetItemName(T item);

        public virtual void UpdateSelectedItem()
        {
            if (dropdown.options.Count <= dropdown.value)
            {
                return;
            }

            var selectedItem = dropdown.options[dropdown.value].text;
            var item = GetAllItems().Find(i => GetItemName(i) == selectedItem);
            SetSelectedItem(item);
        }
    }
}
