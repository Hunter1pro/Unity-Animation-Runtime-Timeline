using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class AnimAvatar
{
    [SerializeField]
    private GameObject avatar;

    [SerializeField]
    private string avatarType;
    public string AvatarName => this.avatarType;

    private GameObject initedObj;
    public GameObject InitedObj => this.initedObj;

    public IAnimRuntimeDriver animDriver { get; private set; }
    //public InputController inputController { get; private set; }

    public void Init(Transform root)
    {
        this.initedObj = GameObject.Instantiate(avatar, root.position, Quaternion.identity, root);
        this.initedObj.GetComponent<AnimationController>().Init();
        var weaponController = this.initedObj.GetComponent<WeaponController>();
        weaponController.Initialize();
        weaponController.InitEditorWeaponType();
        this.animDriver = initedObj.GetComponentInChildren<IAnimRuntimeDriver>();
        //this.inputController = initedObj.GetComponent<InputController>();
    }

    public void Destroy()
    {
        GameObject.Destroy(this.initedObj);
        this.animDriver = null;
    }
}

public class AnimRuntimeController : MonoBehaviour
{
    [SerializeField]
    private Transform initRoot;

    [SerializeField]
    private List<AnimAvatar> animAvatars;

    [SerializeField]
    private WeaponContext database;

    [SerializeField]
    private TimeLine timeLine;

    [SerializeField]
    private AnimTypeSetup animType;

    [SerializeField]
    private ActionsPanel actionsPanel;

    [SerializeField]
    private WeaponSetup weaponSetup;

    [SerializeField]
    private UIControll uiController;

    [SerializeField]
    private ChouseAvatarBtn p_AvatarBtn;

    [SerializeField]
    private Transform btnsSpawnPos;

    [SerializeField]
    private GameObject avatarPanel;

    [SerializeField]
    private Button backToAvatarBtn;

    private void Start()
    {
        this.animAvatars.ForEach(x =>
        {
            ChouseAvatarBtn avatarBtn = ChouseAvatarBtn.Instantiate(this.p_AvatarBtn, this.btnsSpawnPos);
            avatarBtn.SetText(x.AvatarName);

            avatarBtn.SubscribeAction(() =>
            {
                x.Init(this.initRoot);

                this.Setup(x.animDriver, x.InitedObj.GetComponent<WeaponController>(), database);
                this.uiController.Init(x.InitedObj.transform/*, x.inputController*/);
                this.uiController.AnimSetupPanel.ForEach(x => x.gameObject.SetActive(true));
                this.avatarPanel.SetActive(false);
            });
        });

        this.uiController.OpenAnimEvent += this.UiController_OpenAnimEvent;

        this.backToAvatarBtn.onClick.AddListener(() =>
        {
            this.animAvatars.ForEach(x => x.Destroy());
            this.avatarPanel.gameObject.SetActive(true);
            this.uiController.AnimSetupPanel.ForEach(x => x.gameObject.SetActive(false));
        });
    }

    private void UiController_OpenAnimEvent(object sender, OpenAnimArgs e)
    {
        if (e.Open)
        {
            this.avatarPanel.gameObject.SetActive(true);
            //playerSpawn.Player.Destroy();
            //_cursorLock.LoockCursor(false);
        }
        else
        {
            this.avatarPanel.gameObject.SetActive(false);
            this.animAvatars.ForEach(x => x.Destroy());
            //playerSpawn.Respawn();
            //_cursorLock.LoockCursor(true);
        }
    }

    private async void Setup(IAnimRuntimeDriver animDriver, WeaponController weaponController, WeaponContext weaponContext)
    {
        // Wait one frame for init Unity UI
        await Task.Delay(TimeSpan.FromSeconds(0.1f));

        this.timeLine.Init(animDriver);
        this.animType.Init(animDriver);
        this.actionsPanel.Init(animDriver);
        this.weaponSetup.Init(animDriver, weaponController, weaponContext);
    }
}
