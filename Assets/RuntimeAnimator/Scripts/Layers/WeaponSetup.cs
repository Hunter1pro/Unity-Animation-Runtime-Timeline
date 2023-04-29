using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class WeaponActionsData
{
    public string Name { get; set; }

    public string WeaponGuid { get; set; }

    public Dictionary<string, AnimAction> ActionsContainer { get; set; } = new Dictionary<string, AnimAction>();

    public WeaponActionsData DeepCopy()
    {
        WeaponActionsData weaponActionsData = (WeaponActionsData)this.MemberwiseClone();
        weaponActionsData.ActionsContainer = new Dictionary<string, AnimAction>();

        this.ActionsContainer.ToList().ForEach(action =>
        {
            weaponActionsData.ActionsContainer.Add(action.Key, action.Value.DeepCopy());
        });

        return weaponActionsData;
    }
}

// Before AnimationController
[DefaultExecutionOrder(-3)]
public class WeaponSetup : MonoBehaviour
{
    [Header("View")]
    [SerializeField]
    private TMP_InputField weaponInput;

    [SerializeField]
    private Button weaponBtn;

    [SerializeField]
    private Button nameBtn;

    [SerializeField]
    private TMP_Dropdown weaponAnimDropdown;

    [SerializeField]
    private Button saveBtn;

    [SerializeField]
    private Button dublicateBtn;

    [SerializeField]
    private Button removeBtn;

    [SerializeField]
    private TMP_Dropdown weaponTypeDropdown;
    private CustomDropDown<string> weaponDropdown;

    [Header("Systems")]
    [SerializeField]
    private ActionsPanel actionsPanel;

    private Dictionary<string, WeaponActionsData> weaponActions = new Dictionary<string, WeaponActionsData>();

    private int weaponIndex;
    private Action changes;

    private WeaponContext weaponContext;

    private WeaponController weaponController;

    private GameObject lastSelected;

    private KeyValuePair<string, WeaponActionsData> currentWeaoponData;

    private bool inited;

    private IAnimRuntimeDriver animDriver;

    // Интегрировать систему оружия

    public void Init(IAnimRuntimeDriver animDriver, WeaponController weaponController, WeaponContext weaponContext)
    {
        this.animDriver = animDriver;
        this.weaponController = weaponController;
        this.weaponContext = weaponContext;

        this.Init();
    }

    private void Init()
    {
        this.weaponBtn.onClick.RemoveAllListeners();
        this.weaponAnimDropdown.onValueChanged.RemoveAllListeners();
        this.weaponTypeDropdown.onValueChanged.RemoveAllListeners();
        this.saveBtn.onClick.RemoveAllListeners();
        this.dublicateBtn.onClick.RemoveAllListeners();
        this.removeBtn.onClick.RemoveAllListeners();
        this.nameBtn.onClick.RemoveAllListeners();

        this.weaponActions = this.animDriver.WeaponActions();

        if (this.weaponActions.Count > 0)
        {
            this.weaponAnimDropdown.ClearOptions();

            this.weaponActions.Values.ToList().ForEach(x =>
            {
                this.weaponAnimDropdown.AddOptions(new List<string> { x.Name });
            });

            WeaponActionsData weaponActionsData = this.weaponActions.First(x => x.Value.Name == this.weaponAnimDropdown.options[0].text).Value;

            this.actionsPanel.animActions = weaponActionsData;

            this.actionsPanel.LoadActions();

            this.currentWeaoponData = this.weaponActions.First(y => y.Value.Name == this.weaponAnimDropdown.options[0].text);

            this.weaponInput.text = this.currentWeaoponData.Value.Name;
        }

        this.weaponBtn.onClick.AddListener(() =>
        {
            if (this.weaponInput.text != string.Empty)
            {
                if (this.weaponActions.ContainsKey(this.weaponInput.text)) return;

                WeaponActionsData weaponActionsData = new WeaponActionsData { Name = this.weaponInput.text, ActionsContainer = new Dictionary<string, AnimAction>() };

                this.weaponActions.Add(this.weaponInput.text, weaponActionsData);

                this.weaponAnimDropdown.AddOptions(new List<string> { this.weaponInput.text });

                this.currentWeaoponData = this.weaponActions.First(y => y.Value.Name == this.weaponAnimDropdown.options[0].text);

                this.currentWeaoponData.Value.WeaponGuid = this.weaponContext.WeaponViews.First().GuidString;

                // Setup Action Panel Callback
                this.actionsPanel.animActions = this.currentWeaoponData.Value;

                this.changes?.Invoke();
            }
        });

        this.weaponAnimDropdown.onValueChanged.AddListener(x =>
        {
            this.currentWeaoponData = this.weaponActions.First(y => y.Value.Name == this.weaponAnimDropdown.options[x].text);

            this.actionsPanel.animActions = this.currentWeaoponData.Value;

            this.actionsPanel.LoadActions();

            this.weaponIndex = x;

            this.changes?.Invoke();

            this.weaponDropdown.SetValue(this.currentWeaoponData.Value.WeaponGuid);

            this.weaponInput.text = this.currentWeaoponData.Value.Name;
        });

        this.saveBtn.onClick.AddListener(() =>
        {
            this.animDriver.SaveFile(this.weaponActions);
        });

        this.dublicateBtn.onClick.AddListener(() =>
        {
            if (this.currentWeaoponData.Value != null)
            {
                string newName = $"{this.currentWeaoponData.Key}Copy{UnityEngine.Random.Range(0, 100)}";

                if (this.weaponActions.ContainsKey(newName)) return;

                WeaponActionsData newActionData = this.currentWeaoponData.Value.DeepCopy();

                newActionData.Name = newName;

                this.weaponActions.Add(newName, newActionData);

                this.weaponAnimDropdown.AddOptions(new List<string> { newName });
            }
        });

        this.removeBtn.onClick.AddListener(() =>
        {
            if (this.weaponActions.Count == 0) return;

            this.weaponActions.Remove(this.currentWeaoponData.Key);

            this.weaponAnimDropdown.ClearOptions();

            this.weaponActions.Values.ToList().ForEach(x =>
            {
                this.weaponAnimDropdown.AddOptions(new List<string> { x.Name });
            });

            if (weaponActions.Count > 0)
            {
                this.currentWeaoponData = this.weaponActions.First(y => y.Value.Name == this.weaponAnimDropdown.options[0].text);
            }
            else
            {
                this.currentWeaoponData = new KeyValuePair<string, WeaponActionsData>();
            }
        });



        //this.lastSelected = this.weaponTypes[0].Weapon.gameObject;
        //*this.lastSelected.SetActive(true);

        weaponDropdown = new CustomDropDown<string>(weaponTypeDropdown, chousenValue =>
        {
            this.currentWeaoponData.Value.WeaponGuid = chousenValue;
            
            this.weaponController.SetupEditorWeaponType(chousenValue);
        }, weaponContext.WeaponViews.First().GuidString);

        this.weaponContext.WeaponViews.ForEach(weaponItem =>
        {
            weaponDropdown.AddValue(weaponItem.DisplayName, weaponItem.GuidString);
        });

        this.nameBtn.onClick.AddListener(() =>
        {
            if (this.currentWeaoponData.Value == null || this.weaponInput.text == string.Empty) return;

            this.currentWeaoponData.Value.Name = this.weaponInput.text;

            this.weaponAnimDropdown.ClearOptions();

            this.weaponActions.Values.ToList().ForEach(x =>
            {
                this.weaponAnimDropdown.AddOptions(new List<string> { x.Name });
            });

            int setWeaponIndex = 0;

            for (int i = 0; i < this.weaponActions.Keys.Count; i++)
            {
                if (this.weaponActions.Keys.ToList()[i] == this.currentWeaoponData.Key)
                {
                    setWeaponIndex = i;
                    break;
                }
            }

            this.weaponAnimDropdown.SetValueWithoutNotify(setWeaponIndex);
        });

        this.inited = true;
    }

    public WeaponActionsData GetWeaponActionData()
    {
        if (this.weaponActions.Values.Count == 0) return null;

        return this.weaponActions.Values.ToList()[this.weaponIndex];
    }

    public WeaponActionsData GetWeaponActionData(string weaponGuid)
    {
        if (this.weaponActions.Values.Count == 0 || string.IsNullOrEmpty(weaponGuid)) return null;

        WeaponActionsData weaponActionsData = this.weaponActions.Values.FirstOrDefault(x => x.WeaponGuid == weaponGuid);

        return weaponActionsData;
    }

    public void SubscribeChanges(Action changes)
    {
        this.changes = changes;
    }
}
