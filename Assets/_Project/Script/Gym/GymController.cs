using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ModularTreasures;

public class GymController : MonoBehaviour
{
    public static GymController instance;

    [Header("Player")]
    [SerializeField] private PlayerController _player;
    private Animator _playerAnimator;

    [Header("Gym Entry Button")]
    [SerializeField] private GameObject _gymInteractionButton;
    [SerializeField] private Button _enterGymButton;
    [SerializeField] private GameObject _exitGymButtonObj;
    [SerializeField] private Button _exitGymButton;

    [Header("Gym Display UI")]
    [SerializeField] private GameObject _gymDisplay;
    [SerializeField] private GameObject _buttonDisplay;
    [SerializeField] private RectTransform _touchText;
    [SerializeField] private GameObject _touchGymButton;
    [SerializeField] private TMP_Text _speedAmountText;
    [SerializeField] private TMP_Text _jumpAmountText;

    [Header("Stat Popups")]
    [SerializeField] private GymStatPopupPool _speedPopupPool;
    [SerializeField] private GymStatPopupPool _jumpPopupPool;

    [Header("Barbell (Jump Training)")]
    [SerializeField] private Transform _barbell;
    [SerializeField] private Transform _barbellHolder;
    [SerializeField] private Vector3 _barbellPositionOffset;
    [SerializeField] private Vector3 _barbellRotationOffset;

    private bool _barbellFollowing;

    [Header("Gym Settings")]
    [SerializeField] private float _speedStatPerTap = 0.1f;
    [SerializeField] private float _jumpStatPerTap = 0.1f;
    [SerializeField] private float _gymCameraDistance = 3f;
    [SerializeField] private float _gymRunningAnimSpeed = 1.5f;
    [SerializeField] private float _exitBackDistance = 2.5f;

    private bool _isInGym;
    private GymType _currentGymType;
    private GymZone _currentZone;
    private Coroutine _touchTextCoroutine;
    private float _savedCameraZoom;

    private Vector3 _barbellOriginalWorldPos;
    private Quaternion _barbellOriginalWorldRot;
    private Transform _barbellOriginalParent;

    private bool _forceRunning;
    private Coroutine _runOnceCoroutine;
    [SerializeField] private float _runAnimDuration = 0.8f;

    private bool _squatPlaying;
    private bool _waitingToStop;
    private float _lastTapTime;
    [SerializeField] private float _tapTimeout = 0.5f;
    private const string SquatStateName = "Squatting";

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        if (_enterGymButton != null)
            _enterGymButton.onClick.AddListener(EnterGym);

        if (_exitGymButton != null)
            _exitGymButton.onClick.AddListener(ExitGym);

        _gymInteractionButton?.SetActive(false);
        _exitGymButtonObj?.SetActive(false);
        _gymDisplay?.SetActive(false);
    }

    private void Update()
    {
        if (_isInGym && _currentGymType == GymType.Speed && _forceRunning && _playerAnimator != null)
        {
            _playerAnimator.SetFloat("Speed", 1f);
        }

        if (_isInGym && _currentGymType == GymType.Jump && _squatPlaying && _playerAnimator != null)
        {
            if (!_waitingToStop && (Time.time - _lastTapTime) >= _tapTimeout)
                _waitingToStop = true;

            var info = _playerAnimator.GetCurrentAnimatorStateInfo(0);

            if (_waitingToStop)
            {
                if (!info.IsName(SquatStateName) || (info.normalizedTime % 1f) >= 0.9f)
                {
                    _playerAnimator.speed = 0f;
                    _playerAnimator.Play(SquatStateName, 0, 0f);
                    _playerAnimator.Update(0f);
                    _squatPlaying = false;
                    _waitingToStop = false;
                }
            }
            else if (info.IsName(SquatStateName) && info.normalizedTime >= 0.95f)
            {
                _playerAnimator.Play(SquatStateName, 0, 0f);
            }
        }

        if (_barbellFollowing && _barbell != null && _barbellHolder != null)
        {
            _barbell.position = _barbellHolder.position
                                + _barbellHolder.TransformDirection(_barbellPositionOffset);
            _barbell.rotation = _barbellHolder.rotation
                                * Quaternion.Euler(_barbellRotationOffset);
        }
    }

    public void OnPlayerEnterZone(GymZone zone)
    {
        if (_isInGym) return;
        _currentZone = zone;
        _gymInteractionButton?.SetActive(true);
    }

    public void OnPlayerExitZone(GymZone zone)
    {
        if (_isInGym || zone != _currentZone) return;
        _currentZone = null;
        _gymInteractionButton?.SetActive(false);
    }

    private void EnterGym()
    {
        if (_currentZone == null || _isInGym) return;

        _isInGym = true;
        _currentGymType = _currentZone.gymType;

        if (_player != null)
            _player.canMove = false;

        if (_currentZone.spawnPoint != null)
        {
            _player.Teleport(_currentZone.spawnPoint.position, _currentZone.spawnPoint.rotation);
        }

        var cameraController = FindObjectOfType<CameraLookController>();
        if (cameraController != null)
        {
            _savedCameraZoom = cameraController.saveZoom;
            cameraController.ForceZoomTo(_gymCameraDistance);
        }

        _exitGymButtonObj?.SetActive(true);
        _gymDisplay?.SetActive(true);
        _buttonDisplay?.SetActive(false);
        _touchGymButton?.SetActive(true);

        StartTouchTextAnimation();
        UpdateStatDisplays();

        if (_currentGymType == GymType.Jump)
        {
            AttachBarbell();
            _playerAnimator.applyRootMotion = false;
            _playerAnimator.speed = 0f;
            _playerAnimator.Play(SquatStateName, 0, 0f);
            _playerAnimator.Update(0f);
            _squatPlaying = false;
            _waitingToStop = false;
        }
        else
        {
            _playerAnimator.speed = _gymRunningAnimSpeed;
        }
    }

    public void SetPlayerAnimator(Animator animator) => _playerAnimator = animator;

    public void ExitGym()
    {
        if (!_isInGym) return;

        _isInGym = false;

        if (_currentGymType == GymType.Speed)
        {
            if (_runOnceCoroutine != null)
            {
                StopCoroutine(_runOnceCoroutine);
                _runOnceCoroutine = null;
            }
            _forceRunning = false;
            _playerAnimator.SetFloat("Speed", 0f);
        }

        if (_player != null)
            _player.canMove = true;

        if (_currentZone != null && _currentZone.spawnPoint != null)
        {
            Vector3 exitPos = _currentZone.spawnPoint.position
                              - _currentZone.spawnPoint.forward * _exitBackDistance;
            _player.Teleport(exitPos, _player.transform.rotation);
        }

        var cameraControllerExit = FindObjectOfType<CameraLookController>();
        if (cameraControllerExit != null)
        {
            cameraControllerExit.ForceZoomTo(_savedCameraZoom);
        }

        StopTouchTextAnimation();
        _gymDisplay?.SetActive(false);
        _buttonDisplay?.SetActive(true);
        _gymInteractionButton?.SetActive(false);
        _exitGymButtonObj?.SetActive(false);
        _touchGymButton?.SetActive(false);

        if (_currentGymType == GymType.Jump)
        {
            _squatPlaying = false;
            _waitingToStop = false;
            _playerAnimator.applyRootMotion = true;
            _playerAnimator.speed = 1f;
            _playerAnimator.CrossFade("Idle", 0.15f);
            DetachBarbell();
        }
        else
        {
            _playerAnimator.SetFloat("Speed", 0f);
        }

        _currentZone = null;
    }

    public void OnGymTap()
    {
        if (!_isInGym) return;

        if (_currentGymType == GymType.Speed)
        {
            if (_player != null)
                _player.AddSpeed(_speedStatPerTap/1000);
            _speedPopupPool?.Show(_speedStatPerTap);

            if (_runOnceCoroutine != null)
                StopCoroutine(_runOnceCoroutine);
            _runOnceCoroutine = StartCoroutine(PlayRunOnce());
        }
        else
        {
            if (_player != null)
                _player.AddJump(_jumpStatPerTap/1000);
            _jumpPopupPool?.Show(_jumpStatPerTap);

            _lastTapTime = Time.time;
            _waitingToStop = false;

            if (!_squatPlaying)
            {
                _squatPlaying = true;
                _playerAnimator.speed = 1f;
                _playerAnimator.Play(SquatStateName, 0, 0f);
            }
        }

        UpdateStatDisplays();
    }

    private IEnumerator PlayRunOnce()
    {
        _forceRunning = true;
        yield return new WaitForSeconds(_runAnimDuration);
        _forceRunning = false;
        _playerAnimator.SetFloat("Speed", 0f);
        _runOnceCoroutine = null;
    }

    private void UpdateStatDisplays()
    {
        if (_player == null)
            return;

        if (_speedAmountText != null)
        {
            float speedVal = 500 + (_player.additionalSpeed * 1000);
            _speedAmountText.text = NumberFormatter.Format(speedVal, NumberFormatMode.Abbreviated);
        }

        if (_jumpAmountText != null)
        {
            float jumpVal = 500 + (_player.additionalJump * 1000);
            _jumpAmountText.text = NumberFormatter.Format(jumpVal, NumberFormatMode.Abbreviated);
        }
    }

    private void StartTouchTextAnimation()
    {
        if (_touchText == null) return;
        StopTouchTextAnimation();
        _touchText.localScale = Vector3.one;
        _touchTextCoroutine = StartCoroutine(AnimateTouchText());
    }

    private void StopTouchTextAnimation()
    {
        if (_touchTextCoroutine != null)
        {
            StopCoroutine(_touchTextCoroutine);
            _touchTextCoroutine = null;
        }
        if (_touchText != null)
            _touchText.localScale = Vector3.one;
    }

    private IEnumerator AnimateTouchText()
    {
        float duration = 0.5f;
        float targetScale = 1.12f;
        bool growing = true;
        
        while (true)
        {
            float elapsed = 0f;
            Vector3 startScale = _touchText.localScale;
            Vector3 endScale = growing ? Vector3.one * targetScale : Vector3.one;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                _touchText.localScale = Vector3.Lerp(startScale, endScale, t);
                yield return null;
            }
            
            _touchText.localScale = endScale;
            growing = !growing;
        }
    }

    private void AttachBarbell()
    {
        // Try to find barbell and holder in the current zone's children
        Debug.Log(_currentZone.PlayerController);
        if (_barbell == null && _currentZone != null)
        {
            _barbell = _currentZone.transform.parent.Find("Barbell");
            Debug.Log(_barbell);
        }
        
        if (_barbellHolder == null && _currentZone != null)
        {
            //_barbellHolder = _currentZone.PlayerController.transform.GetChild(1).Find("BarbellHolder");
        }

        if (_barbell != null)
        {
            _barbellOriginalParent = _barbell.parent;
            _barbellOriginalWorldPos = _barbell.position;
            _barbellOriginalWorldRot = _barbell.rotation;
        }

        if (_barbell == null || _barbellHolder == null) return;
        _barbellFollowing = true;
    }

    private void DetachBarbell()
    {
        if (_barbell == null) return;
        _barbellFollowing = false;
        _barbell.position = _barbellOriginalWorldPos;
        _barbell.rotation = _barbellOriginalWorldRot;
    }
}