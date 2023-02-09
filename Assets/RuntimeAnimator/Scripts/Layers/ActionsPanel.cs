using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[Serializable]
public class AnimAction
{
    public List<AnimData> AnimData { get; set; } = new List<AnimData>();

    public string Name { get; set; }

    public AnimType AnimType { get; set; }

    public float Time { get; set; } = 1;

    public AnimAction DeepCopy()
    {
        AnimAction animAData = (AnimAction) this.MemberwiseClone();

        animAData.AnimData = new List<AnimData>();

        this.AnimData.ForEach(x =>
        {
            animAData.AnimData.Add(x.DeepCopy());
        });

        return animAData;
    }
}

public enum AnimType {None, Shot, Angle, Rig, Move, Aim, Reload,
    Run, FastKill, Slide, Climb, EndClimb, Jump, ShotIdle, ShotStart, WeaponIdle, ShotEnd, Idle
}

public class AnimTypeEvent : UnityEvent<AnimType> { }

[DefaultExecutionOrder(-4)]
public class ActionsPanel : MonoBehaviour
{
    [Header("View")]
    [SerializeField]
    private TMP_InputField animName;

    [SerializeField]
    private Button createBtn;

    [SerializeField]
    private Button nameBtn;

    [SerializeField]
    private Button copyBtn;

    [SerializeField]
    private Button pasteBtn;

    [SerializeField]
    private Button removeBtn;

    [SerializeField]
    private TMP_Dropdown animListDropdown;

    [SerializeField]
    private TMP_Dropdown animTypeDropdown;

    [Header("Systems")]
    [SerializeField]
    private TimeLine timeLine;

    [SerializeField]
    private AnimTypeSetup animTypeSetup;

    // From WeaponSetup
    public WeaponActionsData animActions = new WeaponActionsData();

    private KeyValuePair<string, AnimAction> currentAnim;

    private AnimAction copyAnim;

    private AnimTypeEvent animTypeEvent = new AnimTypeEvent();
    private Dictionary<int, AnimType> animTypeFromDropDown = new Dictionary<int, AnimType>();
    private Dictionary<AnimType, int> animIndexFromDropDown = new Dictionary<AnimType, int>();

    private IAnimRuntimeDriver animDriver;

    public void Init(IAnimRuntimeDriver animDriver)
    {
        this.animTypeFromDropDown.Clear();
        this.animIndexFromDropDown.Clear();
        this.animTypeDropdown.options.Clear();
        this.animTypeDropdown.onValueChanged.RemoveAllListeners();

        this.animDriver = animDriver;

        int indexType = 0;

        this.animDriver.AnimTypeRigViews().ForEach(x =>
        {
            this.animTypeFromDropDown.Add(indexType, x.AnimType);
            this.animIndexFromDropDown.Add(x.AnimType, indexType);
            this.animTypeDropdown.options.Add(new TMP_Dropdown.OptionData { text = x.AnimType.ToString() });
            indexType++;
        });

        if (this.animActions.ActionsContainer.Count > 0)
        {
            // Because Load Actions is firstly
            this.animListDropdown.ClearOptions();

            this.animActions.ActionsContainer.Values.ToList().ForEach(x =>
            {
                this.animListDropdown.AddOptions(new List<string> { x.Name });
            });

            var currentAnimAction = this.animActions.ActionsContainer.ToList()[0];


            this.animTypeDropdown.value = this.animIndexFromDropDown[(currentAnimAction.Value).AnimType];

            this.currentAnim = currentAnimAction;

            this.animTypeEvent?.Invoke(currentAnimAction.Value.AnimType);

            this.timeLine.animAction = currentAnimAction.Value;

            this.timeLine.LoadFrames();

            print($"Loaded {(currentAnimAction.Value).AnimType}");

            this.animName.SetTextWithoutNotify(this.currentAnim.Value.Name);
        }

        this.createBtn.onClick.AddListener(() =>
        {
            if (this.animName.text != string.Empty)
            {
                // Check UniqueName
                AnimAction animAction = new AnimAction() { Name = this.animName.text, AnimType = this.DropDownAnimType(), AnimData = new List<AnimData>() };

                this.animActions.ActionsContainer.Add(this.animName.text, animAction);

                this.timeLine.animAction = animAction;

                this.animListDropdown.AddOptions(new List<string> { this.animName.text });

                this.animTypeEvent?.Invoke(animAction.AnimType);

                this.currentAnim = this.animActions.ActionsContainer.ToList()[0];
            }
        });

        this.copyBtn.onClick.AddListener(() =>
        {
            this.copyAnim = this.currentAnim.Value.DeepCopy();
            this.copyAnim.Name += $"{UnityEngine.Random.Range(0, 100)}";
        });

        this.pasteBtn.onClick.AddListener(() =>
        {
            if (this.copyAnim != null)
            {
                this.animActions.ActionsContainer.Add(this.copyAnim.Name, this.copyAnim);

                this.LoadActions();
            }
        });

        this.animListDropdown.onValueChanged.AddListener(index =>
        {
            var animAction = this.animActions.ActionsContainer.First(x => x.Value.Name == this.animListDropdown.options[index].text);

            this.currentAnim = animAction;

            this.timeLine.animAction = animAction.Value;

            this.animTypeDropdown.value = this.animIndexFromDropDown[(animAction.Value).AnimType];

            this.timeLine.LoadFrames();

            this.animTypeEvent?.Invoke(animAction.Value.AnimType);

            this.animName.SetTextWithoutNotify(this.currentAnim.Value.Name);
        });

        this.animTypeDropdown.onValueChanged.AddListener(index =>
        {
            if (this.currentAnim.Value.Name != string.Empty)
            {
                this.animActions.ActionsContainer[this.currentAnim.Key].AnimType = this.animTypeFromDropDown[index];

                this.animTypeEvent?.Invoke(this.animTypeFromDropDown[index]);
            }
        });

        this.nameBtn.onClick.AddListener(() =>
        {
            if (this.animName.text != string.Empty && this.currentAnim.Value != null)
            {
                this.currentAnim.Value.Name = this.animName.text;

                this.animListDropdown.ClearOptions();

                this.animActions.ActionsContainer.Values.ToList().ForEach(x =>
                {
                    this.animListDropdown.AddOptions(new List<string> { x.Name });
                });
            }
        });

        this.removeBtn.onClick.AddListener(() =>
        {
            if (this.animActions.ActionsContainer.Count == 0) return;

            this.animActions.ActionsContainer.Remove(this.currentAnim.Key);

            this.animListDropdown.ClearOptions();

            this.animActions.ActionsContainer.Values.ToList().ForEach(x =>
            {
                this.animListDropdown.AddOptions(new List<string> { x.Name });
            });

            if (this.animActions.ActionsContainer.Count > 0)
            {
                this.currentAnim = this.animActions.ActionsContainer.ToList()[0];
            }
            else
            {
                this.currentAnim = new KeyValuePair<string, AnimAction>();
                this.timeLine.animAction = this.currentAnim.Value;
                this.LoadActions();
            }
        });
    }

    private AnimType DropDownAnimType()
    {
        return this.animTypeFromDropDown[this.animTypeDropdown.value];
    }

    public void LoadActions()
    {
        this.animListDropdown.ClearOptions();

        if (this.animActions.ActionsContainer.Count > 0)
        {
            this.animActions.ActionsContainer.Values.ToList().ForEach(x =>
            {
                this.animListDropdown.AddOptions(new List<string> { x.Name });
            });

            var currentAnimAction = this.animActions.ActionsContainer.ToList()[0];

            this.timeLine.animAction = currentAnimAction.Value;

            this.timeLine.LoadFrames();

            this.currentAnim = currentAnimAction;

            if (animIndexFromDropDown.Count > 0 && currentAnimAction.Value != null)
                this.animTypeDropdown.value = this.animIndexFromDropDown[(currentAnimAction.Value).AnimType]; 

            this.timeLine.animAction = currentAnimAction.Value;
        }
        else
        {
            this.timeLine.ClearFrames();
        }
    }

    public void SubscribeChangedAnimType(UnityAction<AnimType> animType)
    {
        this.animTypeEvent.AddListener(animType);
    }

    public void RemoveListenersAnimType()
    {
        this.animTypeEvent.RemoveAllListeners();
    }
}
