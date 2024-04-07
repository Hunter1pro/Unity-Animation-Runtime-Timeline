using System;
using System.Collections.Generic;
using UnityEngine;

public class OpenAnimArgs : EventArgs
{
    public bool Open { get; set; }
}

public class UIControll : MonoBehaviour
{
    [SerializeField]
    private Camera camera;

    [SerializeField]
    private Camera fpsCamera;

    [SerializeField]
    private Transform root;

    [SerializeField]
    private List<GameObject> animSetupPanel;
    public List<GameObject> AnimSetupPanel => this.animSetupPanel;

    [SerializeField, Range(0.1f, 1)]
    private float scroolStep = 0.2f;

    [SerializeField]
    private KeyCode resetKey = KeyCode.R;

    [SerializeField]
    private KeyCode enterAnimSetup = KeyCode.Q;

    [SerializeField]
    private KeyCode secondKey = KeyCode.LeftControl;
    public enum Project { ThreeD, TwoD }

    [SerializeField]
    private Project projectType;

    private Transform player;

    private Vector3 lastAngle;
    private Vector2 lastPos;

    private Vector3 lastMousePos;

    private Vector3 startPos;
    private Quaternion startRotation;
    private Vector3 startCameraPos;

    private bool playModeTimes;

    public event EventHandler<OpenAnimArgs> OpenAnimEvent;

    public void Init(Transform player)
    {
        this.player = player;

        this.startPos = this.player.transform.localPosition;
        this.startRotation = this.root.localRotation;
        this.startCameraPos = this.camera.transform.localPosition;
    }

    private void Update()
    {
        if (Input.GetKeyDown(this.enterAnimSetup) && Input.GetKey(this.secondKey))
        {
            EnterAnimSetup();
        }

        if (this.player == null) return;

        if (Input.GetMouseButton(1))
        {
            if (this.projectType == Project.ThreeD)
            {
                this.root.eulerAngles = this.lastAngle - this.ScreenToAngle();
            }
            else if (this.projectType == Project.TwoD)
            {
                this.camera.transform.localPosition = new Vector3(this.camera.transform.localPosition.x - Input.mousePosition.x - this.lastPos.x,
                    this.camera.transform.localPosition.y - Input.mousePosition.y - this.lastPos.y, this.camera.transform.localPosition.z);
            }
        }
        else
        {
            if (this.projectType == Project.ThreeD)
            {
                this.lastAngle = this.ScreenToAngle() + this.root.eulerAngles;
            }
            else if (this.projectType == Project.TwoD)
            {
                this.lastPos = Input.mousePosition;
            }
        }

        if (Input.mouseScrollDelta.y > 0)
        {
            if (this.projectType == Project.ThreeD)
            {
                this.camera.transform.localPosition = new Vector3(this.camera.transform.localPosition.x,
                    this.camera.transform.localPosition.y, this.camera.transform.localPosition.z + this.scroolStep);
            }
            else if (this.projectType == Project.TwoD)
            {
                this.camera.orthographicSize -= this.scroolStep;
            }

        }
        else if (Input.mouseScrollDelta.y < 0)
        {
            if (this.projectType == Project.ThreeD)
            {
                this.camera.transform.localPosition = new Vector3(this.camera.transform.localPosition.x,
                this.camera.transform.localPosition.y, this.camera.transform.localPosition.z - this.scroolStep);
            }
            else if (this.projectType == Project.TwoD)
            {
                this.camera.orthographicSize += this.scroolStep;
            }
        }

        if (Input.GetMouseButton(2))
        {
            this.camera.transform.position = (this.lastMousePos + this.MouseWorldPos());

            this.lastMousePos = this.camera.transform.position - this.MouseWorldPos();
        }
        else
        {
            this.lastMousePos = this.camera.transform.position - this.MouseWorldPos();
        }

        if (Input.GetKeyDown(this.resetKey) && Input.GetKey(this.secondKey))
        {
            this.player.transform.localPosition = this.startPos;
            this.player.transform.localRotation = this.startRotation;

            this.camera.transform.localPosition = this.startCameraPos;
        }
    }

    public void EnterAnimSetup()
    {
        if (!this.playModeTimes)
        {
            this.camera.gameObject.SetActive(true);
            if (this.fpsCamera)
                this.fpsCamera.gameObject.SetActive(false);

            this.playModeTimes = true;

            this.OpenAnimEvent?.Invoke(this, new OpenAnimArgs { Open = true });
        }
        else
        {
            this.camera.gameObject.SetActive(false);
            if (this.fpsCamera)
                this.fpsCamera.gameObject.SetActive(true);

            this.playModeTimes = false;

            this.OpenAnimEvent?.Invoke(this, new OpenAnimArgs { Open = false });

            this.animSetupPanel.ForEach(x => x.SetActive(false));
        }
    }

    private Vector3 ScreenToAngle()
    {
        float slowDown = 0.5f;
        return new Vector3((Input.mousePosition.y / Screen.height) * 360 * slowDown, (-Input.mousePosition.x / Screen.height) * 360 * slowDown, 0);
    }

    private Vector3 MouseWorldPos()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = this.camera.nearClipPlane;

        return this.camera.ScreenToWorldPoint(mousePos);
    }
}
