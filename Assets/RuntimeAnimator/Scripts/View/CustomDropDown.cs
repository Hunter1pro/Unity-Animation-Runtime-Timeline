using System.Collections.Generic;
using TMPro;
using UnityEngine.Events;

public class CustomDropDown<T>
{
    private TMP_Dropdown dropdown;
    private UnityAction<T> action;

    private List<T> values = new List<T>();

    private T lastValue;

    public CustomDropDown(TMP_Dropdown dropdown, UnityAction<T> action, T defaultValue)
    {
        this.dropdown = dropdown;
        this.dropdown.ClearOptions();
        this.action = action;
        this.dropdown.onValueChanged.AddListener(num => { this.lastValue = this.values[num]; this.action?.Invoke(this.values[num]); });
        this.lastValue = defaultValue;
    }

    public void Clear()
    {
        this.dropdown.ClearOptions();
    }

    public void AddValue(string name, T returnValue)
    {
        this.values.Add(returnValue);
        this.dropdown.options.Add(new TMP_Dropdown.OptionData { text = name });

        if (this.dropdown.options.Count == 1)
        {
            this.dropdown.SetValueWithoutNotify(0);
        }
    }

    public void SetValueWithoutNotify(T value, string name = "")
    {
        this.dropdown.SetValueWithoutNotify(this.values.IndexOf(value));

        this.lastValue = value;

        if (!string.IsNullOrEmpty(name))
            this.dropdown.options[this.values.IndexOf(value)].text = name;
    }

    public void SetValue(T value, string name = "")
    {
        this.dropdown.value = this.values.IndexOf(value);

        this.lastValue = value;

        if (!string.IsNullOrEmpty(name))
            this.dropdown.options[this.values.IndexOf(value)].text = name;
    }

    public T LastChousen() => this.lastValue;
}
