using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

enum CameraState {
    WorldGenAnimation,
    FindPlayer,
    FindPlayerSlow,
    PlayerDied,
    BossDied
}

public class CameraFollow : MonoBehaviour {
    public GameObject player;
    public GameObject boss;
    public GameObject worldGenUI;
    public LevelGenerator levelGenerator;

    public float panAnimationRate;
    public float panAnimationRateSlow;
    public float sizeAnimationRate;
    public float sizeAnimationRateSlow;
    public float defaultSize;
    public float bossDeathSize;
    public float playerDeathSize;
    public float bossDeathTime;

    public float worldGenUIImageSize;
    
    // "new" keyword to override deprecated "camera" field in base class.
    new Camera camera;

    CameraState state;
    float stateTimer;

    Func<Vector2> _panTarget;
    Vector2 _panOffset;
    float _panSpeed;
    bool _panLock;

    float _sizeTarget;
    float _sizeCurrent;
    float _sizeSpeed;
    bool _sizeLock;


    void Start() {
        // Camera starts where player currently is (no animation).
        camera = this.GetComponent<Camera>();

        boss.GetComponent<BossHealth>().ProvideCamera(this);
        player.GetComponent<PlayerStats>().ProvideCamera(this);
        worldGenUI.GetComponent<WorldGenUI>().ProvideCamera(this);

        SetState(CameraState.WorldGenAnimation);
    }

    void FixedUpdate() {
        stateTimer += Time.fixedDeltaTime;

        UpdateState();

        Vector2 panDesired = _panTarget();
        if (_panLock) {
            SetPosition(panDesired);
        }
        else {
            _panOffset *= 1 - (_panSpeed * Time.fixedDeltaTime);
            SetPosition(panDesired + _panOffset);
        }

        if (_sizeLock) {
            _sizeCurrent = _sizeTarget;
            SetSize(_sizeCurrent);
        }
        else {
            _sizeCurrent += (_sizeTarget - _sizeCurrent) * _sizeSpeed * Time.fixedDeltaTime;
            SetSize(_sizeCurrent);
        }
    }

    void SetState(CameraState state) {
        this.state = state;
        this.stateTimer = 0;

        if (state == CameraState.WorldGenAnimation) {
            LockPan(() => new Vector2(levelGenerator.mazeWidth, levelGenerator.mazeHeight) / 2f);
            LockSize(0.5f * levelGenerator.mazeHeight * (768 / worldGenUIImageSize));
        }
        else if (state == CameraState.FindPlayer) {
            AnimatePan(() => player.transform.position, panAnimationRate);
            AnimateSize(defaultSize, sizeAnimationRate);
        }
        else if (state == CameraState.FindPlayerSlow) {
            AnimatePan(() => player.transform.position, panAnimationRateSlow);
            AnimateSize(defaultSize, sizeAnimationRateSlow);
        }
        else if (state == CameraState.FindPlayer) {
            AnimatePan(() => player.transform.position, panAnimationRate);
            AnimateSize(defaultSize, sizeAnimationRate);
        }
        else if (state == CameraState.PlayerDied) {
            AnimatePan(() => player.transform.position, panAnimationRate);
            AnimateSize(playerDeathSize, sizeAnimationRate);
        }
        else if (state == CameraState.BossDied) {
            AnimatePan(() => boss.transform.position, panAnimationRate);
            AnimateSize(bossDeathSize, sizeAnimationRate);
        }
        else {
            throw new Exception("Unknown state");
        }
    }

    void UpdateState() {
        if (state == CameraState.WorldGenAnimation) {
            // Do nothing.
        }
        else if (state == CameraState.FindPlayer) {
            // Do nothing.
        }
        else if (state == CameraState.FindPlayerSlow) {
            // Do nothing.
        }
        else if (state == CameraState.PlayerDied) {
            // Do nothing.
        }
        else if (state == CameraState.BossDied) {
            if (stateTimer > bossDeathTime) {
                SetState(CameraState.FindPlayer);
            }
        }
        else {
            throw new Exception("Unknown state");
        }
    }

    void AnimatePan(Func<Vector2> target, float speed) {
        Vector2 current = _panTarget() + _panOffset;

        _panTarget = target;
        _panOffset = current - target();
        _panSpeed = speed;
        _panLock = false;
    }

    void LockPan(Func<Vector2> target) {
        _panTarget = target;
        _panOffset = Vector2.zero;
        _panLock = true;
    }

    void AnimateSize(float target, float speed) {
        _sizeTarget = target;
        _sizeSpeed = speed;
        _sizeLock = false;
    }

    void LockSize(float target) {
        _sizeTarget = target;
        _sizeLock = true;
    }

    public void OnBossDeath() {
        if (state != CameraState.PlayerDied) {
            SetState(CameraState.BossDied);
        }
    }

    public void OnPlayerDeath() {
        SetState(CameraState.PlayerDied);
    }

    public void OnWorldGenDone() {
        SetState(CameraState.FindPlayerSlow);
    }

    void SetPosition(Vector2 newPosition) {
        gameObject.transform.position = new Vector3(
            Mathf.Round(newPosition.x * 100) / 100, 
            Mathf.Round(newPosition.y * 100) / 100, 
            -10
        );
    }

    void SetSize(float size) {
        camera.orthographicSize = size;
    }
}
