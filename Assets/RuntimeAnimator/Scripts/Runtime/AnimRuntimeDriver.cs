using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Unity.Mathematics;
using UnityEngine;
using static WeaponController;

public class AnimRuntimeDriver : MonoBehaviour, IAnimRuntimeDriver
{
    [SerializeField]
    private string fileName = "AnimData.dat";

    [SerializeField]
    private List<RigTamplate> rigTamplates;

    [SerializeField]
    private List<AnimTypeRigView> animTypeRigViews;
    public List<AnimTypeRigView> AnimTypeRigViews() => this.animTypeRigViews;

    [SerializeField]
    private WeaponSelector weaponCore;

    private List<AnimTypeRigView> allRigs = new List<AnimTypeRigView>();
    public List<AnimTypeRigView> AllRigs() => this.allRigs;

    private Dictionary<string, WeaponActionsData> weaponActions = new Dictionary<string, WeaponActionsData>();
    public Dictionary<string, WeaponActionsData> WeaponActions() => this.weaponActions;

    public event Action AnimInit;

    private bool forseStop;

    private string path;

    public void Init(IAnimationController animationController)
    {
        this.animTypeRigViews.ForEach(x => x.Init(this.rigTamplates.FirstOrDefault(y => x.Tamplate == y.Tamplate).rigTransforms));
        this.allRigs.Add(this.animTypeRigViews.FirstOrDefault(x => x.Tamplate == ERigTamplate.Full));

        if (Application.isEditor)
        {
            this.path = Application.dataPath + "../../../SaveData/";
        }
        else
        {
            this.path = "SaveData/";
        }

        this.LoadFile();

        animationController.Load();

        this.AnimInit?.Invoke();
    }

    private void LoadFile()
    {
        BinaryFormatter binaryFormatter = new BinaryFormatter();

        if (!File.Exists(this.path))
        {
            Directory.CreateDirectory(this.path);
        }

        using (FileStream fs = new FileStream(Path.Combine(this.path, this.fileName), FileMode.OpenOrCreate))
        {
            if (fs.Length != 0)
            {
                this.weaponActions = binaryFormatter.Deserialize(fs) as Dictionary<string, WeaponActionsData>;
            }
        }
    }

    public void SaveFile(Dictionary<string, WeaponActionsData> saveActions)
    {
        BinaryFormatter binaryFormatter = new BinaryFormatter();

        using (FileStream fs = new FileStream(Path.Combine(this.path, this.fileName), FileMode.Create))
        {
            binaryFormatter.Serialize(fs, saveActions);
        }
    }

    public TimeLineData SetAnimTime(float timeValue, AnimAction animAction, AnimTypeRigView rigView, bool notFirst = false, bool moveAnimY = false)
    {
        if (this.forseStop) return new TimeLineData();

        bool first = false;

        TimeLineData timeLineData = new TimeLineData();

        if (animAction.AnimData == null) return new TimeLineData();

        animAction.AnimData = animAction.AnimData.OrderBy(x => x.Time).ToList();

        for (int i = 0; i < animAction.AnimData.Count; i++)
        {
            if (animAction.AnimData.Count >= 2 && i != 0)
            {
                if (animAction.AnimData[i].Time >= timeValue && !first)
                {
                    first = true;

                    timeLineData.RootMotion = new float2(math.abs(animAction.AnimData[i].Position[0].x - animAction.AnimData[i - 1].Position[0].x) / (animAction.AnimData[i].Time - animAction.AnimData[i - 1].Time),
                        math.abs(animAction.AnimData[i].Position[0].y - animAction.AnimData[i - 1].Position[0].y) / (animAction.AnimData[i].Time - animAction.AnimData[i - 1].Time));

                    float time = (timeValue - animAction.AnimData[i - 1].Time) / (animAction.AnimData[i].Time - animAction.AnimData[i - 1].Time);

                    if (timeValue < animAction.AnimData[i - 1].Time)
                        time = 0;

                    if (!notFirst)
                    {
                        for (int pointIndex = 0; pointIndex < animAction.AnimData[i].Position.Count; pointIndex++)
                        {
                            if (pointIndex < rigView.rigTransforms.Count)
                            {
                                rigView.rigTransforms[pointIndex].localPosition = animAction.AnimData[i - 1].Position[pointIndex] * (1 - time) + animAction.AnimData[i].Position[pointIndex] * time;

                                quaternion rotation = math.slerp(animAction.AnimData[i - 1].Rotation[pointIndex], animAction.AnimData[i].Rotation[pointIndex], time / 1);

                                rigView.rigTransforms[pointIndex].localRotation = rotation;
                            }
                        }
                    }
                    else
                    {
                        // Y only Track for root motion
                        if (moveAnimY)
                        {
                            float3 localPos = rigView.rigTransforms[0].localPosition;
                            localPos.y = animAction.AnimData[i - 1].Position[0].y * (1 - time) + animAction.AnimData[i].Position[0].y * time;
                            rigView.rigTransforms[0].localPosition = localPos;
                        }

                        for (int pointIndex = 1; pointIndex < animAction.AnimData[i].Position.Count; pointIndex++)
                        {
                            if (pointIndex < rigView.rigTransforms.Count)
                            {
                                rigView.rigTransforms[pointIndex].localPosition = animAction.AnimData[i - 1].Position[pointIndex] * (1 - time) + animAction.AnimData[i].Position[pointIndex] * time;

                                quaternion rotation = math.slerp(animAction.AnimData[i - 1].Rotation[pointIndex], animAction.AnimData[i].Rotation[pointIndex], time / 1);

                                rigView.rigTransforms[pointIndex].localRotation = rotation;
                            }
                        }
                    }
                }
                else
                {
                    // For Stay the last frame after all is done
                    if (!first && timeValue >= animAction.AnimData[animAction.AnimData.Count - 1].Time)
                    {
                        first = true;
                        for (int pointIndex = 0; pointIndex < animAction.AnimData[animAction.AnimData.Count - 1].Position.Count; pointIndex++)
                        {
                            rigView.rigTransforms[pointIndex].localPosition = animAction.AnimData[animAction.AnimData.Count - 1].Position[pointIndex];

                            rigView.rigTransforms[pointIndex].localRotation = animAction.AnimData[animAction.AnimData.Count - 1].Rotation[pointIndex];
                        }
                    }
                }
            }
            else
            {
                // For Just One Key
                if (!notFirst)
                {
                    for (int pointIndex = 0; pointIndex < animAction.AnimData[0].Position.Count; pointIndex++)
                    {
                        if (pointIndex < rigView.rigTransforms.Count)
                        {
                            rigView.rigTransforms[pointIndex].localPosition = animAction.AnimData[0].Position[pointIndex];
                            rigView.rigTransforms[pointIndex].localEulerAngles = ((Quaternion)animAction.AnimData[0].Rotation[pointIndex]).eulerAngles;
                        }
                    }
                }
                else
                {
                    for (int pointIndex = 1; pointIndex < animAction.AnimData[0].Position.Count; pointIndex++)
                    {
                        if (pointIndex < rigView.rigTransforms.Count)
                        {
                            rigView.rigTransforms[pointIndex].localPosition = animAction.AnimData[0].Position[pointIndex];
                            rigView.rigTransforms[pointIndex].localEulerAngles = ((Quaternion)animAction.AnimData[0].Rotation[pointIndex]).eulerAngles;
                        }
                    }
                }
            }
        }

        return timeLineData;
    }

    public WeaponActionsData GetWeaponActionData(string weaponGuid)
    {
        if (this.weaponActions.Values.Count == 0 || string.IsNullOrEmpty(weaponGuid)) return null;

        WeaponActionsData weaponActionsData = this.weaponActions.Values.FirstOrDefault(x => x.WeaponGuid == weaponGuid);

        return weaponActionsData;
    }

    public WeaponActionsData GetCoreActionData()
    {
        if (this.weaponActions.Values.Count == 0) return null;

        WeaponActionsData weaponActionsData = this.weaponActions.Values.FirstOrDefault(x => x.WeaponGuid == weaponCore.CurrentWeapon());

        return weaponActionsData;
    }

    public AnimTypeRigView GetAnimType(AnimType animType)
    {
        return this.animTypeRigViews.FirstOrDefault(x => x.AnimType == animType);
    }

    public void ForceStop()
    {
        this.forseStop = true;
    }
}

public interface IAnimRuntimeDriver
{
    void Init(IAnimationController animationController);

    event Action AnimInit;

    WeaponActionsData GetWeaponActionData(string weaponGuid);
    WeaponActionsData GetCoreActionData();

    Dictionary<string, WeaponActionsData> WeaponActions();

    TimeLineData SetAnimTime(float timeValue, AnimAction animAction, AnimTypeRigView rigView, bool notFirst = false, bool moveAnimY = false);

    AnimTypeRigView GetAnimType(AnimType animType);

    List<AnimTypeRigView> AnimTypeRigViews();

    List<AnimTypeRigView> AllRigs();

    void SaveFile(Dictionary<string, WeaponActionsData> saveActions);

    void ForceStop();
}