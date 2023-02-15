using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using Unity.Mathematics;

public class TimeLinePoint
{
    public KeyHandle PointBtn { get; set; }
    public float Time { get; set; }
    public AnimData animData { get; set; }
}

public class TimeLineData
{
    public float2 RootMotion { get; set; }
}

public class TimeLine : MonoBehaviour
{
    [Header("View")]
    [SerializeField]
    private SourceHandle _sourceHandle;

    [SerializeField]
    private Button addKeyBtn;
    [SerializeField]
    private Button removeKeyBtn;
    [SerializeField]
    private Button updateBtn;
    [SerializeField]
    private Button fixBtn;

    [SerializeField]
    private TMP_InputField animInput;

    [SerializeField]
    private Button playBtn;
    [SerializeField]
    private Button stopBtn;
    [SerializeField]
    private Button circleBtn;
    
    [SerializeField]
    private KeyHandle keyHandlePrefab;

    [SerializeField]
    private Transform rootPoints;

    private List<TimeLinePoint> currentTimeLinePoints = new List<TimeLinePoint>();

    public AnimAction animAction = new AnimAction();

    public AnimTypeRigView animRigView { get; private set; }

    private float currentTime;
    private AnimData currentAnimData;

    private bool playAnim;
    private bool circleAnim;

    public void Init(IAnimRuntimeDriver animDriver)
    {
        _sourceHandle.SubscribePointerDown(() =>
        {
            _sourceHandle.ResetHandle(); 
            currentAnimData = null; 
            playAnim = false; 
            circleAnim = false; 
        });

        this.animInput.onValueChanged.AddListener(value =>
        {
            if (float.TryParse(value, out float inputValue))
            {
                if (inputValue == 0) return;

                _sourceHandle.Slider.maxValue = inputValue;

                // Recalculate Anim Time
                this.animAction.AnimData.ForEach(x =>
                {
                    if (x.Time == 0)
                    {
                        x.UpdateTime(0);
                    }
                    else
                    {
                        float percent = x.Time / this.animAction.Time;

                        x.UpdateTime(percent * inputValue);
                    }
                });

                this.animAction.Time = inputValue;

                this.LoadFrames();
            }
        });

        this.addKeyBtn.onClick.AddListener(() =>
        {
            AnimData localAnimData = new AnimData();

            localAnimData.SetupPoint(this.animRigView.rigTransforms, this.currentTime);

            KeyHandle keyHandle = Instantiate(this.keyHandlePrefab, this._sourceHandle.Slider.handleRect.position, Quaternion.identity, this.rootPoints);

            this.currentTimeLinePoints.Add(
                new TimeLinePoint 
                { 
                    Time = this.currentTime,
                    PointBtn = keyHandle,
                    animData = localAnimData
                }
            );

            this.animAction.AnimData.Add(localAnimData);

            RectTransform keyHandleRect = keyHandle.GetComponent<RectTransform>();

            keyHandleRect.anchoredPosition = this._sourceHandle.Slider.handleRect.anchoredPosition;
            keyHandleRect.anchorMax = this._sourceHandle.Slider.handleRect.anchorMax;
            keyHandleRect.anchorMin = this._sourceHandle.Slider.handleRect.anchorMin;

            keyHandle.SubscribePointerDown(() => KeyHandleMove(keyHandle.GetComponent<RectTransform>(), localAnimData));
        });

        this.fixBtn.onClick.AddListener(() =>
        {
            this.animAction.AnimData.ForEach(animData =>
            {
                animDriver.SetAnimTime(animData.Time, this.animAction, this.animRigView);
                animData.SetupPoint(this.animRigView.rigTransforms, animData.Time);
            });
        });

        this.removeKeyBtn.onClick.AddListener(() =>
        {
            TimeLinePoint timeLinePoint = this.currentTimeLinePoints.FirstOrDefault(x => x.animData == this.currentAnimData);
           
            if (timeLinePoint != null)
            {
                Destroy(timeLinePoint.PointBtn.gameObject);

                this.animAction.AnimData.Remove(this.currentAnimData);

                this.currentTimeLinePoints.Remove(timeLinePoint);
            }
        });

        _sourceHandle.Slider.onValueChanged.AddListener(value =>
        {
            this.currentTime = value;

            // TODO Make in normal way
            if (animRigView != null && animRigView.rigTransforms != null && animRigView.rigTransforms[0] != null)
                animDriver.SetAnimTime(this.currentTime, this.animAction, this.animRigView);

            if (this.currentAnimData != null)
                this.currentAnimData.UpdateTime(this.currentTime);
        });

        this.updateBtn.onClick.AddListener(() =>
        {
            if (this.currentAnimData != null)
            {
                this.currentAnimData.SetupPoint(this.animRigView.rigTransforms, this.currentTime);
            }
        });

        this.playBtn.onClick.AddListener(() =>
        {
            currentAnimData = null;

            _sourceHandle.ResetHandle();

            _sourceHandle.Slider.value = 0;

            playAnim = true;
        });

        this.stopBtn.onClick.AddListener(() =>
        {
            playAnim = false;
            circleAnim = false;
        });

        this.circleBtn.onClick.AddListener(() =>
        {
            circleAnim = true;
        });
    }

    private void KeyHandleMove(RectTransform keyHandle, AnimData animData)
    {
        _sourceHandle.Slider.handleRect = keyHandle;
        currentAnimData = animData;

        currentAnimData.UpdateTime(currentTime);
    }

    public void LoadFrames()
    {
        if (this.currentTimeLinePoints.Count > 0)
        {
            this.currentTimeLinePoints.ForEach(timeLinePoint =>
            {
                Destroy(timeLinePoint.PointBtn.gameObject);
            });

            this.currentTimeLinePoints.Clear();
        }

        if (this.animAction.AnimData == null) return;

        _sourceHandle.Slider.maxValue = this.animAction.Time;

        _sourceHandle.ResetHandle();

        this.animAction.AnimData.ForEach(x =>
        {
            // For Setup Correct TimeBtnSPawn
            _sourceHandle.Slider.SetValueWithoutNotify(x.Time);
            KeyHandle keyHandle = Instantiate(this.keyHandlePrefab, _sourceHandle.Slider.handleRect.position, Quaternion.identity, this.rootPoints);

            RectTransform keyHandleRect = keyHandle.GetComponent<RectTransform>();

            keyHandleRect.anchoredPosition = _sourceHandle.Slider.handleRect.anchoredPosition;
            keyHandleRect.anchorMax = _sourceHandle.Slider.handleRect.anchorMax;
            keyHandleRect.anchorMin = _sourceHandle.Slider.handleRect.anchorMin;

            keyHandle.SubscribePointerDown(() => this.KeyHandleMove(keyHandle.RectTransform, x));

            this.currentTimeLinePoints.Add(new TimeLinePoint { animData = x, Time = x.Time, PointBtn = keyHandle });
        });

        this.animInput.SetTextWithoutNotify(this.animAction.Time.ToString());
    }

    public void ClearFrames()
    {
        this.animAction = new AnimAction();

        if (this.currentTimeLinePoints.Count > 0)
        {
            this.currentTimeLinePoints.ForEach(timeLinePoint =>
            {
                Destroy(timeLinePoint.PointBtn.gameObject);
            });

            this.currentTimeLinePoints.Clear();
        }
    }

    public void UpdateRigLayer(AnimTypeRigView animTypeRigView)
    {
        this.animRigView = animTypeRigView;
    }

    private void Update()
    {
        if (this.playAnim)
        {
            float value = _sourceHandle.Slider.value;

            value += Time.deltaTime;

            if (value < _sourceHandle.Slider.maxValue)
            {
                _sourceHandle.Slider.value = value;
            }
            else
            {
                if (!this.circleAnim)
                {
                    this.playAnim = false;
                }
                else
                {
                    _sourceHandle.Slider.value = Time.deltaTime;
                }
            }
        }
    }
}
