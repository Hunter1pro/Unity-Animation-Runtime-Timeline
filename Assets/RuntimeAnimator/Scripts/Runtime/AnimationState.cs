using System;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class AnimationState : Updatable
{
    public bool IsRunning { get; private set; }

    private IAnimRuntimeDriver _runtimeDriver;
    private WeaponActionsData _weaponAction;
    private WeaponActionsData _coreAction;
    private AnimTypeRigView _rigView;
    private AnimAction _animAction;
    private Action _finish;
    private Action<float2> _speed;
    private float _time;

    private bool _circle;

    private bool _rootMoution;

    public AnimationState(IAnimRuntimeDriver runtimeDriver, AnimType animType, string weaponGuid, bool circle = true, WeaponActionsData coreAction = null)
    {
        _runtimeDriver = runtimeDriver;
        _rigView = _runtimeDriver.GetAnimType(animType);
        _coreAction = coreAction;
        _circle = circle;

        _weaponAction = _runtimeDriver.GetWeaponActionData(weaponGuid);
        _animAction = CheckExsist(animType, _weaponAction, _coreAction);
    }

    private AnimAction CheckExsist(AnimType animType, WeaponActionsData weaponActionData, WeaponActionsData coreAction)
    {
        if (weaponActionData == null) return null;

        if (weaponActionData.ActionsContainer.Count == 0) return null;

        AnimAction animAction = weaponActionData.ActionsContainer.FirstOrDefault(x => x.Value.AnimType == animType).Value;

        if (animAction == null && coreAction != null)
        {
            animAction = coreAction.ActionsContainer.FirstOrDefault(x => x.Value.AnimType == animType).Value;
        }

        return animAction;
    }

    public void Play()
    {
        IsRunning = true;
        _finish = null;
    }

    public void Play(Action finish)
    {
        IsRunning = true;
        _finish = finish;
    }

    public void PlayRootMotion(Action finish, Action<float2> speed)
    {
        IsRunning = true;
        _finish = finish;
    }

    public void Stop()
    {
        IsRunning = false;
        Dispose();
        _time = 0;
    }

    public void Reset()
    {
        Reinit();
        IsRunning = false;
        _time = 0;
    }

    public override void Update()
    {
        if (_weaponAction == null || _animAction == null || _rigView == null)
        {
            Debug.LogError($"Animation can't be played, check fields: _weaponAction is null: {_weaponAction is null} _animAction is null: {_animAction is null} _rigView is null {_rigView is null}");
        }

        if (!_rootMoution)
        {
            if (IsRunning && _time <= _animAction.Time)
            {
                _runtimeDriver.SetAnimTime(_time, _animAction, _rigView);
                _time += Time.deltaTime;
            }
            else
            {
                IsRunning = false;
                _finish?.Invoke();

                if (_circle)
                    _time = 0;
            }
        }
        else
        {
            if (IsRunning && _time <= _animAction.Time)
            {
                var speed = _runtimeDriver.SetAnimTime(_time, _animAction, _rigView, true).RootMotion;
                _time += Time.deltaTime;
                _speed?.Invoke(speed);
            }
            else
            {
                IsRunning = false;
                _finish?.Invoke();

                if (_circle)
                    _time = 0;
            }
        }

    }
}
