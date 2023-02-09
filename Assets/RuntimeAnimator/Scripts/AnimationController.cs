using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class AnimData
{
    public List<float3> Position { get; set; } = new List<float3>();
    public List<quaternion> Rotation { get; set; } = new List<quaternion>();

    public float Time { get; set; }

    public void SetupPoint(List<Transform> selected, float time)
    {
        this.Position.Clear();
        this.Rotation.Clear();

        selected.ForEach(x =>
        {
            this.Position.Add(x.localPosition);
            this.Rotation.Add(x.localRotation);
        });

        this.Time = time;
    }

    public void UpdateTime(float time)
    {
        this.Time = time;
    }

    public AnimData DeepCopy()
    {
        AnimData animData = (AnimData)this.MemberwiseClone();

        animData.Position = new List<float3>();
        animData.Rotation = new List<quaternion>();

        for (int i = 0; i < this.Position.Count; i++)
        {
            animData.Position.Add(this.Position[i]);
            animData.Rotation.Add(this.Rotation[i]);
        }

        return animData;
    }
}

// Before WeapnController
[DefaultExecutionOrder(-2)]
public class AnimationController : MonoBehaviour, IAnimationController
{
    [SerializeReference]
    private AnimRuntimeDriver animDriverRef;
    public void SetAnimationDriver(AnimRuntimeDriver driver)
    {
        animDriverRef = driver;
    }
    public IAnimRuntimeDriver AnimDriver => this.animDriverRef;

    [SerializeField]
    private Camera camera;
    public Camera Camera => camera;

    private AnimTypeRigView shotRig;
    private AnimTypeRigView shotIdleRig;
    private AnimTypeRigView shotStartRig;
    private AnimTypeRigView shotEndRig;
    private AnimTypeRigView weaponIdleRig;
    private AnimTypeRigView weaponRig;
    private AnimTypeRigView angleRig;
    private AnimTypeRigView moveRig;
    private AnimTypeRigView aimWeaponRig;
    private AnimTypeRigView reloadRig;
    private AnimTypeRigView fastKillRig;
    private AnimTypeRigView slideRig;
    private AnimTypeRigView startClimbRig;
    private AnimTypeRigView endClimbRig;
    private AnimTypeRigView jumpRig;

    [SerializeField]
    private bool rootMootionMove;
    [SerializeField]
    private Transform playerMotionNode;

    private WeaponActionsData weaponActionData;
    private WeaponActionsData coreActionData;

    private bool moving;

    private AnimAction angleAction;
    private AnimAction shotAction;
    private AnimAction shotIdleAction;
    private AnimAction shotStartAction;
    private AnimAction shotEndAction;
    private AnimAction weaponIdleAction;
    private AnimAction rigAction;
    private AnimAction moveAction;
    private AnimAction idleAction;
    private AnimAction runAction;
    private AnimAction fastKillAction;
    private AnimAction slideAction;
    private AnimAction climbStartAction;
    private AnimAction climbEndAction;
    private AnimAction jumpAction;

    private AnimAction weaponAimAction;
    private AnimAction weaponReloadAction;

    private float moveTime;
    private IEnumerator moveAnim;
    private IEnumerator runAnim;

    private IEnumerator shotAnim;
    private IEnumerator shotStartAnim;
    private IEnumerator shotEndAim;

    private bool shot;
    private bool move;
    private bool lastMove;
    private bool run;
    private bool lastRun;
    private float angle;
    private float lastAngle;

    private float aimTime;

    private bool reloading;

    private bool fastKill;

    private bool slide;
    private bool lastSlide;

    private bool climbStart;
    private bool climbEnd;
    private bool climbState;

    private bool jump;
    private bool lastJump;

    private bool shotStart;
    private bool shotIdle;
    private bool weaponIdle;
    private bool shotEnd;

    private UnityEvent reloadFinish = new UnityEvent();

    private string weaponGuid;
    private WeaponRig weaponRigLoad;

    // EndFrame to TimeDeltaTime? for fix fast show anims

    private Action<float> angleChanged;
    private Action<bool> moveChanged;
    private Action<bool> runChanged;
    private Action<bool> slideChanged;
    private Action<bool> jumpChanged;

    private Action<float> endMoveAnim;

    public void Init()
    {
        this.AnimDriver.Init(this);

        this.shotRig = this.AnimDriver.GetAnimType(AnimType.Shot);

        this.shotIdleRig = this.AnimDriver.GetAnimType(AnimType.ShotIdle);
        this.shotStartRig = this.AnimDriver.GetAnimType(AnimType.ShotStart);
        this.weaponIdleRig = this.AnimDriver.GetAnimType(AnimType.WeaponIdle);
        this.shotEndRig = this.AnimDriver.GetAnimType(AnimType.ShotEnd);

        this.weaponRig = this.AnimDriver.GetAnimType(AnimType.Rig);

        this.angleRig = this.AnimDriver.GetAnimType(AnimType.Angle);

        this.moveRig = this.AnimDriver.GetAnimType(AnimType.Move);

        this.aimWeaponRig = this.AnimDriver.GetAnimType(AnimType.Aim);

        this.reloadRig = this.AnimDriver.GetAnimType(AnimType.Reload);

        this.fastKillRig = this.AnimDriver.GetAnimType(AnimType.FastKill);

        this.slideRig = this.AnimDriver.GetAnimType(AnimType.Slide);

        this.startClimbRig = this.AnimDriver.GetAnimType(AnimType.Climb);
        ///TODO: Optimize This
        this.endClimbRig = this.AnimDriver.GetAnimType(AnimType.EndClimb);

        this.jumpRig = this.AnimDriver.GetAnimType(AnimType.Jump);
    }

    public void SetupWeapon(string weaponGuid, WeaponRig weaponRig)
    {
        this.weaponGuid = weaponGuid;
        this.weaponRigLoad = weaponRig;

        this.weaponActionData = this.AnimDriver.GetWeaponActionData(weaponGuid);
        this.coreActionData = this.AnimDriver.GetCoreActionData();

        // WeaponLayer
        this.shotAction = this.CheckExsist(AnimType.Shot);
        this.weaponIdleAction = this.CheckExsist(AnimType.WeaponIdle);
        this.shotEndAction = this.CheckExsist(AnimType.ShotEnd);
        this.shotIdleAction = this.CheckExsist(AnimType.ShotIdle);
        this.shotStartAction = this.CheckExsist(AnimType.ShotStart);

        this.rigAction = this.CheckExsist(AnimType.Rig);

        this.weaponAimAction = this.CheckExsist(AnimType.Aim);

        this.weaponReloadAction = this.CheckExsist(AnimType.Reload);

        // Move Layer
        this.moveAction = this.CheckExsist(AnimType.Move); //CheckExistMove
        this.idleAction = this.CheckExsist(AnimType.Idle); //CheckExistMove

        this.angleAction = this.CheckExsist(AnimType.Angle);

        this.runAction = this.CheckExsist(AnimType.Run);

        this.fastKillAction = this.CheckExsist(AnimType.FastKill);

        this.slideAction = this.CheckExsist(AnimType.Slide);

        this.climbStartAction = this.CheckExsist(AnimType.Climb);

        this.climbEndAction = this.CheckExsist(AnimType.EndClimb);

        this.jumpAction = this.CheckExsist(AnimType.Jump);

        if (weaponRig != null && this.shotRig != null)
        {
            this.shotRig.ToStartRigs();
            this.shotRig.AddWeaponRig(weaponRig.Rigs);

            if (this.shotIdleRig != null)
                this.shotIdleRig.AddWeaponRig(weaponRig.Rigs);
            if (this.shotStartRig != null)
                this.shotStartRig.AddWeaponRig(weaponRig.Rigs);
            if (this.weaponIdleRig != null)
                this.weaponIdleRig.AddWeaponRig(weaponRig.Rigs);
            if (this.shotEndRig != null)
                this.shotEndRig.AddWeaponRig(weaponRig.Rigs);
        }
        else
        {
            // ResetWeaoponRig For others
            if (this.shotRig != null)
                this.shotRig.ToStartRigs();

            if (this.weaponRig != null)
                this.weaponRig.ToStartRigs();

            if (this.shotIdleRig != null)
                this.shotIdleRig.ToStartRigs();

            if (this.shotStartRig != null)
                this.shotStartRig.ToStartRigs();

            if (this.weaponRig != null)
                this.weaponRig.ToStartRigs();

            if (this.weaponIdleRig != null)
                this.weaponIdleRig.ToStartRigs();

            if (this.shotEndRig != null)
                this.shotEndRig.ToStartRigs();
        }
    }

    public void Load()
    {
        if (string.IsNullOrEmpty(this.weaponGuid)) return;

        this.SetupWeapon(this.weaponGuid, this.weaponRigLoad);
    }    

    public void ShotStart(Action finish)
    {
        if (!this.shotStart && this.shotStartAction != null)
        {
            this.shotStartAnim = this.ShotStartAnimAction(finish);
            StartCoroutine(this.shotStartAnim);
        }
    }

    private IEnumerator ShotStartAnimAction(Action finish)
    {
        float time = 0;

        this.shotStart = true;

        while (this.shotStartAction != null && time <= this.shotStartAction.Time)
        {
            this.AnimDriver.SetAnimTime(time, this.shotStartAction, this.shotStartRig);

            time += Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }

        finish?.Invoke();

        this.shotStart = false;
    }

    public void ShotEnd(Action finish)
    {
        if (!this.shotEnd && this.shotEndAction != null)
        {
            this.shotEndAim = this.ShotEndAnimAction(finish);
            StartCoroutine(this.shotEndAim);
        }
    }

    private IEnumerator ShotEndAnimAction(Action finish)
    {
        float time = 0;

        this.shotEnd = true;

        while (this.shotEndAction != null && time <= this.shotEndAction.Time)
        {
            this.AnimDriver.SetAnimTime(time, this.shotEndAction, this.shotEndRig);

            time += Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }

        finish?.Invoke();

        this.shotEnd = false;
    }

    public void StopShot()
    {
        if (this.shotAnim != null)
            StopCoroutine(this.shotAnim);
        if (this.shotStartAnim != null)
            StopCoroutine(this.shotStartAnim);
        if (this.shotEndAim != null)
            StopCoroutine(this.shotEndAim);
    }

    public void ShotIdle()
    {
        if (!this.shotIdle && this.shotIdleAction != null)
        {
            StartCoroutine(this.ShotIdleAnimAction());
        }
    }

    private IEnumerator ShotIdleAnimAction()
    {
        float time = 0;

        this.shotIdle = true;

        while (this.shotIdleAction != null && time <= this.shotIdleAction.Time)
        {
            this.AnimDriver.SetAnimTime(time, this.shotIdleAction, this.shotIdleRig);

            time += Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }

        this.shotIdle = false;
    }

    public void WeaponIdle()
    {
        if (!this.weaponIdle && this.weaponIdleAction != null)
        {
            StartCoroutine(this.WeapoinIdleAction());
        }
    }

    private IEnumerator WeapoinIdleAction()
    {
        this.weaponIdle = true;

        float time = 0;

        while (this.weaponIdleAction != null && time <= this.weaponIdleAction.Time)
        {
            this.AnimDriver.SetAnimTime(time, this.weaponIdleAction, this.weaponIdleRig);

            time += Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }

        this.weaponIdle = false;
    }


    public void AngleAnim(float angle)
    {
        if (this.angleAction == null) return;

        this.angle = angle;

        Debug.Log($"fpsAngle {angle}");

        if (this.lastAngle != angle)
        {
            this.lastAngle = angle;

            this.angleChanged?.Invoke(this.angle);
        }

        this.AnimDriver.SetAnimTime(angle, this.angleAction, this.angleRig);
    }

    public void RigAnim()
    {
        if (this.rigAction == null) return;

        this.camera.fieldOfView = 60;
        this.AnimDriver.AllRigs().ForEach(x => x.Reset());

        this.AnimDriver.SetAnimTime(1, this.rigAction, this.weaponRig);
    }

    private IEnumerator MoveAnim(AnimTypeRigView rig, AnimAction animAction, Action<float> speed)
    {
        this.moving = true;

        if (animAction != null)
        {
            while(animAction.Time > this.moveTime)
            {
                float speedValue = this.AnimDriver.SetAnimTime(this.moveTime, animAction, rig, this.rootMootionMove).RootMotion.x;

                if (this.rootMootionMove)
                {
                    speed?.Invoke(speedValue);
                    this.endMoveAnim?.Invoke(speedValue);
                }

                this.moveTime += Time.deltaTime;

                if (!this.moving)
                    break;

                yield return new WaitForEndOfFrame();
            }

            if (this.moving)
            {
                this.moveTime = 0;
            }
        }

        this.moving = false;
    }

    public void SubscribeEndMove(Action<float> speed)
    {
        this.endMoveAnim = speed;
    }

    private AnimAction CheckExsist(AnimType animType)
    {
        if (this.weaponActionData == null) return null;

        if (this.weaponActionData.ActionsContainer.Count == 0) return null;

        AnimAction animAction = this.weaponActionData.ActionsContainer.FirstOrDefault(x => x.Value.AnimType == animType).Value;

        if (animAction == null && this.coreActionData != null)
        {
            animAction = this.coreActionData.ActionsContainer.FirstOrDefault(x => x.Value.AnimType == animType).Value;
        }

        return animAction;
    }

    public void MoveAnim(Action<float> speed)
    {
        if (!this.moving && this.moveAction != null)
        {
            this.moveAnim = this.MoveAnim(this.moveRig, this.moveAction, speed);
            StartCoroutine(this.moveAnim);

            this.move = true;

            if (this.move != lastMove)
            {
                this.lastMove = this.move;

                this.moveChanged?.Invoke(this.move);
            }
        }
    }

    public void StopMove()
    {
        if (this.moveAnim == null) return; 

        this.moving = false;
        StopCoroutine(this.moveAnim);

        this.move = false;

        if (this.move != this.lastMove)
        {
            this.lastMove = this.move;
            this.moveChanged?.Invoke(this.move);
        }
    }

    public void RunAnim(Action<float> speed)
    {
        if (!this.moving && this.moveAction != null)
        {
            this.runAnim = this.MoveAnim(this.moveRig, this.runAction, speed);
            StartCoroutine(this.runAnim);

            this.run = true;

            if(this.run != this.lastRun)
            {
                this.lastRun = this.run;

                this.runChanged?.Invoke(this.run);
            }
        }
    }

    public void StopRun()
    {
        if (this.moveAnim == null) return;

        StopCoroutine(this.moveAnim);
        this.moving = false;

        this.run = false;

        if (this.run != this.lastRun)
        {
            this.lastRun = this.run;

            this.runChanged?.Invoke(this.run);
        }
    }

    public void WeaponAim(bool value)
    {
        if (this.weaponAimAction == null) return;

        if (value)
        {
            if (this.aimTime <= this.weaponAimAction.Time)
            {
                this.aimTime += Time.deltaTime;
                this.AnimDriver.SetAnimTime(this.aimTime, weaponAimAction, aimWeaponRig);
            }
        }
        else
        {
            if (this.aimTime >= 0)
            {
                this.aimTime -= Time.deltaTime;
                this.AnimDriver.SetAnimTime(this.aimTime, weaponAimAction, aimWeaponRig);
            }
        }
    }

    public void ReloadAim(Action finish)
    {
        if (this.weaponReloadAction == null)
        {
            finish?.Invoke();
            this.aimTime = 0;
            return;
        }

        if (!reloading)
            StartCoroutine(ReloadWeapon(finish));
    }

    private IEnumerator ReloadWeapon(Action finish)
    {
        float time = 0;
        reloading = true;

        while (time <= this.weaponReloadAction.Time)
        {
            this.AnimDriver.SetAnimTime(time, this.weaponReloadAction, this.reloadRig);
            time += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        finish?.Invoke();

        this.reloadFinish?.Invoke();

        reloading = false;
        this.aimTime = 0;
    }

    public void FastKillAnim(Action finish)
    {
        if (this.fastKillAction == null)
        {
            finish?.Invoke();
            return;
        }

        if (!fastKill)
        {
            StartCoroutine(FastKill(this.fastKillAction,finish));
        }
    }

    private IEnumerator FastKill(AnimAction fskAction,Action finish)
    {
        float time = 0;
        fastKill = true;

        while (time <= fskAction.Time)
        {
            this.AnimDriver.SetAnimTime(time, fskAction, this.fastKillRig);
            time += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        finish?.Invoke();

        fastKill = false;
    }

    public void SlideAnim(Action<float> speed, Action finish)
    {
        if (this.slideAction == null)
            return;

        if (!this.slide)
        {
            StartCoroutine(SlideAction(speed, finish));
        }
    }

    private IEnumerator SlideAction(Action<float> speed, Action finish)
    {
        float time = 0;

        this.slide = true;

        if(this.slide != this.lastSlide)
        {
            this.lastSlide = this.slide;

            this.slideChanged?.Invoke(this.slide);
        }

        while (time < this.slideAction.Time)
        {
            float speedValue = this.AnimDriver.SetAnimTime(time, this.slideAction, this.slideRig, this.rootMootionMove).RootMotion.x;

            speed?.Invoke(speedValue);

            time += Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }

        finish?.Invoke();
        this.slide = false;

        if (this.slide != this.lastSlide)
        {
            this.lastSlide = this.slide;

            this.slideChanged?.Invoke(this.slide);
        }
    }

    public void ClimbStartAnim(Action finish)
    {
        if (this.climbStartAction == null)
        {
            finish?.Invoke();
            return;
        }

        if (!this.climbStart)
        {
            StartCoroutine(this.ClimbAction(finish));
        }
    }

    public void ClimbEndAnim(Action<float2> rootMotion, Action finish)
    {
        if (this.climbEndAction == null)
        {
            finish?.Invoke();
            return;
        }

        if (!this.climbEnd)
        {
            StartCoroutine(this.ClimbEndAction(rootMotion, finish));
        }
    }

    private IEnumerator ClimbAction(Action finish)
    {
        float timer = 0;

        this.climbStart = true;

        while(timer < this.climbStartAction.Time)
        {
            this.AnimDriver.SetAnimTime(timer, this.climbStartAction, this.startClimbRig);

            timer += Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }

        this.climbStart = false;

        finish?.Invoke();
    }

    private IEnumerator ClimbEndAction(Action<float2> rootMotion, Action finish)
    {
        float timer = 0;

        this.climbEnd = true;

        while (timer < this.climbEndAction.Time)
        {
            TimeLineData timeLineData = this.AnimDriver.SetAnimTime(timer, this.climbEndAction, this.endClimbRig, this.rootMootionMove);

            rootMotion?.Invoke(timeLineData.RootMotion);

            timer += Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }

        this.climbEnd = false;

        finish?.Invoke();
    }

    public void JumpAnim(Action<float> speed)
    {
        if (this.jumpAction == null)
            return;

        if (!this.jump)
        {
            this.StartCoroutine(this.JumpAction(speed));
        }
    }

    private IEnumerator JumpAction(Action<float> speed)
    {
        float timer = 0;

        this.jump = true;

        if(this.jump != this.lastJump)
        {
            this.lastJump = this.jump;

            this.jumpChanged?.Invoke(this.jump);
        }

        while (timer < this.jumpAction.Time && !this.climbStart)
        {
            float speedValue = this.AnimDriver.SetAnimTime(timer, this.jumpAction, this.jumpRig, this.rootMootionMove).RootMotion.x;

            speed?.Invoke(speedValue);

            timer += Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }

        this.jump = false;

        if (this.jump != this.lastJump)
        {
            this.lastJump = this.jump;

            this.jumpChanged?.Invoke(this.jump);
        }
    }

    public void IdleAnim()
    {
        if (this.idleAction != null)
        {
            this.AnimDriver.SetAnimTime(1, this.idleAction, this.moveRig);
        }
    }
}

public interface IAnimationController
{
    void Load();

    void SetupWeapon(string weaponGuid, WeaponRig weaponRig);

    void ShotAnim(Action startDamage, Action finishAnim);

    void ShotStart(Action finish);
    void ShotEnd(Action finish);

    void ShotIdle();

    void StopShot();

    void WeaponIdle();

    void AngleAnim(float angle);

    void RigAnim();

    void MoveAnim(Action<float> speed);

    void SubscribeEndMove(Action<float> speed);

    void StopMove();

    void IdleAnim();

    void RunAnim(Action<float> speed);

    void StopRun();

    void SlideAnim(Action<float> speed, Action finish);

    void ClimbStartAnim(Action finish);

    void ClimbEndAnim(Action<float2> rootMotion, Action finish);

    void JumpAnim(Action<float> speed);

    void WeaponAim(bool v);

    void ReloadAim(Action finish);

    void FastKillAnim(Action finish);
  
}