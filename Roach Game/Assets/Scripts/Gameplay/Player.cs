/*
 * File: Player.cs
 * Created: 25/05/2026, 11:14:18 PM
 * Author: Travis Reid
 * Copyright 2019 - 2026 Studio Tilia
 */

using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Splines;

public enum PlayerMovementType
{
    Walking, Running, Still
}

public enum PlayerWeaponType
{
    Shoe, Pistol
}

public class Player : MonoBehaviour
{
    // ------------------------------------------------------------------------
    // Variables
    // ------------------------------------------------------------------------
    [Header("Movement")]
    [SerializeField] private float _walkSpeed = 5.0f;
    [SerializeField] private float _runSpeed = 7.0f;
    [SerializeField] private float _mouseSensitivity;
    [SerializeField] private CharacterController _cc;
    [Header("Shoe animation")]
    [SerializeField] private Collider _shoeCollider;
    [SerializeField] private MeshRenderer _shoeRenderer;
    [SerializeField] private Transform _shoeDefaultPos;
    [SerializeField] private SplineContainer _shoeSpline;
    [SerializeField] private SplineAnimate _splineAnimator;
    [Header("Aiming reticle")]
    [SerializeField] private MeshRenderer _reticleRenderer;
    [SerializeField] private Transform _defaultReticlePos;
    [SerializeField] private Transform _aimReticle;
    [SerializeField] private float _maxAimDistance;
    [SerializeField] private LayerMask _aimLayers;
    [Header("Roach collection")]
    [SerializeField] private LayerMask _roachLayer;
    [SerializeField] private Transform _roachHoldLoc;
    [Header("Combat")]
    [SerializeField] private int _maxHealth = 5;
    [SerializeField] private PlayerWeaponType _currentWeapon = PlayerWeaponType.Shoe;
    [Header("Cinematics")]
    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private Transform _manager;
    [SerializeField] private float _lookAtManagerSpeed = 0.1f;
    [Header("Pistol")]
    [SerializeField] private GameObject _pistol;
    [SerializeField] private AudioSource _pistolAudio;
    [SerializeField] private LayerMask _bulletHoleLayers;
    [SerializeField] private GameObject _bulletHoleObj;

    // movement and aiming
    private bool _inputEnabled;
    private Transform _cameraTrans;
    private float _rotationX;
    private float _rotationY;
    private bool _needsAnimRestart;
    private int _health;

    // aiming reticle
    private Vector3 _reticleOffset = new Vector3(0, 0.1f, 0);
    private Material _reticleMat;
    private float _reticleAlpha;
    private Color _reticleHit;
    private Color _reticleMiss;
    private Color _reticleInvalid;

    // cinematics
    private bool _rotateToTarget;
    private bool _endSequenceWhenDoneRotation = false;
    private float _rotateStartTime;
    private float _lookSpeed;
    private Transform _lookTarget;

    private PlayerMovementType _movementType = PlayerMovementType.Still;

    // ------------------------------------------------------------------------
    // Properties
    // ------------------------------------------------------------------------
    public static Player _Instance { get; private set; }

    public Vector3 _Position => transform.position;
    public int _Health => _health;
    public bool _AtMaxHealth => _health == _maxHealth;
    public Transform _CameraTransform => _cameraTransform;
    public Vector3 _CameraPosition => _cameraTransform.position;

    // ------------------------------------------------------------------------
    // Methods
    // ------------------------------------------------------------------------
    private void Awake()
    {
        if (_Instance != null && _Instance != this)
        {
            Destroy(this);
            return;
        }

        _Instance = this;

        DontDestroyOnLoad(gameObject);

        _health = _maxHealth;
        _inputEnabled = true;
    }

    // ------------------------------------------------------------------------
    private void Start ()
    {
        _cameraTrans = Camera.main.transform;
        _cameraTrans.localPosition = Vector3.zero;

#if UNITY_WEBGL
        _mouseSensitivity = _mouseSensitivity / 2.0f;
#endif

        EventBus._Instance.RoachCollected += HandleRoachCollected;

        _reticleMat = _reticleRenderer.material;
        _reticleAlpha = _reticleMat.color.a;

        _reticleHit = new Color(0, 1, 0, _reticleAlpha);
        _reticleMiss = new Color(1, 1, 0, _reticleAlpha);
        _reticleInvalid = new Color(1, 0, 0, _reticleAlpha);

        SetShoeSplineStartPos();
        ResetShoeAnim();

        MatchMouseToRotation();
    }

    // ------------------------------------------------------------------------
    private void Update ()
    {
        if(_inputEnabled)
        {
            LookAndMove();   

            switch(_currentWeapon)
            {
                case PlayerWeaponType.Shoe: UpdateShoe(); break;
                case PlayerWeaponType.Pistol: UpdatePistol(); break;
            }
        }

        if(_rotateToTarget)
        {
            float t =(Time.time - _rotateStartTime)/_lookSpeed;

            if(t >= 1.0f)
            {
                _rotateToTarget = false;
                if(_endSequenceWhenDoneRotation)
                {
                    SequenceController._Instance.EndCurrentSequence();
                }
            }

            // turn player towards manager on Y axis
            Vector3 managerPosXZ = new Vector3(_lookTarget.position.x, 0, _lookTarget.position.z);
            Vector3 posXZ = new Vector3(transform.position.x, 0, transform.position.z);
            Quaternion targetRotation = Quaternion.LookRotation(managerPosXZ - posXZ);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, t);

            // turn camera towards manager on X axis
            Vector3 cameraTargetXRot = _lookTarget.position - _cameraTrans.position;
            Quaternion cameraTargetQuat = Quaternion.LookRotation(cameraTargetXRot);
            Vector3 currentCameraXRot = _cameraTrans.rotation.eulerAngles;
            currentCameraXRot.y = 0;
            currentCameraXRot.z = 0;
            _cameraTrans.rotation = Quaternion.Lerp(Quaternion.Euler(currentCameraXRot), cameraTargetQuat, t);
        }

#if UNITY_EDITOR
        if(Input.GetKey(KeyCode.K))
        {
            GameController._Instance.DebugKillAllRoaches();
        }
#endif
    }

    // ------------------------------------------------------------------------
    // timeline callback
    public void TurnAndLookAtManager ()
    {
        _rotateToTarget = true;
        _endSequenceWhenDoneRotation = true;
        _lookTarget = _manager;
        _lookSpeed = _lookAtManagerSpeed;
        _rotateStartTime = Time.time;
    }

    // ------------------------------------------------------------------------
    public void TurnAndLookAt (Transform t, float speed)
    {
        _rotateToTarget = true;
        _endSequenceWhenDoneRotation = false;
        _lookTarget = t;
        _lookSpeed = speed;
        _rotateStartTime = Time.time;
    }

    // ------------------------------------------------------------------------
    public void RemoveRoachesFromHand ()
    {
        foreach(Transform t in _roachHoldLoc)
        {
            Destroy(t.gameObject);
        }
    }

    // ------------------------------------------------------------------------
    // timeline callback
    public void SwitchToPistol ()
    {
        _currentWeapon = PlayerWeaponType.Pistol;
    }

    // ------------------------------------------------------------------------
    public void SetInputEnabled(bool enabled)
    {
        _inputEnabled = enabled;
        _reticleRenderer.enabled = enabled;

        if(enabled)
        {
            switch(_currentWeapon)
            {
                case PlayerWeaponType.Shoe: 
                    _shoeRenderer.enabled = true;
                    _shoeCollider.enabled = true;
                break;
                case PlayerWeaponType.Pistol:
                    _pistol.SetActive(true);
                break;
            }   
        }
        else
        {
            _shoeRenderer.enabled = false;
            _shoeCollider.enabled = false;
            _pistol.SetActive(false);
        }

        if(!enabled)
        {
            if(_movementType != PlayerMovementType.Still)
            {
                _movementType = PlayerMovementType.Still;
                EventBus._Instance.InvokePlayerMovementChanged(PlayerMovementType.Still);
            }
        }
        else
        {
            MatchMouseToRotation();
        }

        SetShoeSplineStartPos();
        ResetShoeAnim();
    }

    // ------------------------------------------------------------------------
    private void MatchMouseToRotation ()
    {
        _rotationY = -_cameraTrans.localEulerAngles.x;
        _rotationX = transform.localEulerAngles.y;
    }

    // ------------------------------------------------------------------------
    public bool DamageAndTryKill (int damage)
    {
        _health -= damage;
        EventBus._Instance.InvokePlayerHealthChanged();

        if(_health <= 0)
        {
            Cursor.lockState = CursorLockMode.None;
            SetInputEnabled(false);
            EventBus._Instance.InvokePlayerDied();
        }

        return _health == 0;
    }

    // ------------------------------------------------------------------------
    public void SetupForActionSequence ()
    {
        SetInputEnabled(true);
        Cursor.lockState = CursorLockMode.Locked;
        _health = _maxHealth;
        EventBus._Instance.InvokePlayerHealthChanged();
    }

    // ------------------------------------------------------------------------
    private void LookAndMove ()
    {
        float mouseY = Input.GetAxis("Mouse Y");
        _rotationY += mouseY * _mouseSensitivity;
        _rotationY = Mathf.Clamp(_rotationY, -60.0f, 60.0f);

        float mouseX = Input.GetAxis("Mouse X");
        _rotationX += mouseX * _mouseSensitivity;

        _cameraTrans.localEulerAngles = new Vector3(-_rotationY, 0, 0);
        transform.localEulerAngles = new Vector3(0, _rotationX, 0);

        float vertical = Input.GetAxis("Vertical");
        float horizontal = Input.GetAxis("Horizontal");

        float speed = _walkSpeed;
        bool running = Input.GetKey(KeyCode.LeftShift);
        if(running)
        {
            speed = _runSpeed;
        }

        Vector3 playerForward = transform.TransformDirection(Vector3.forward);
        Vector3 playerRight = transform.TransformDirection(Vector3.right);
        _cc.Move(((vertical * playerForward) + (horizontal * playerRight))
            * speed * Time.deltaTime);

        if(vertical != 0 || horizontal != 0)
        {
            // TODO: running
            if(_movementType != PlayerMovementType.Walking)
            {
                _movementType = PlayerMovementType.Walking;
                EventBus._Instance.InvokePlayerMovementChanged(PlayerMovementType.Walking);
            }
        }
        else if(_movementType != PlayerMovementType.Still)
        {
            _movementType = PlayerMovementType.Still;
            EventBus._Instance.InvokePlayerMovementChanged(PlayerMovementType.Still);
        }
    }

    // ------------------------------------------------------------------------
    public void TeleportTo(Transform newLoc)
    {
        _cc.enabled = false;
        transform.position = newLoc.position;
        transform.rotation = newLoc.rotation;
        _cc.enabled = true;
    }

    private struct RoachHitInfo
    {
        public bool hit;
        public GameObject roachObj;
    }

    // ------------------------------------------------------------------------
    private void UpdatePistol ()
    {
        RoachHitInfo hitInfo = UpdateTargetReticle();

        if(Input.GetMouseButtonDown(0))
        {
            // find the nearest walkable surface, and put a bullet hole there
            RaycastHit raycastHit;
            Vector3 raycastStart = Camera.main.transform.position;
            bool aimRaycastHit = Physics.Raycast(
                    raycastStart,
                    Camera.main.transform.forward,
                    out raycastHit,
                    100.0f,
                    _bulletHoleLayers
            );
            if(aimRaycastHit)
            {
                Instantiate(_bulletHoleObj, raycastHit.point, Quaternion.LookRotation(-raycastHit.normal));
            }

            _pistolAudio.Play();

            if(hitInfo.hit && hitInfo.roachObj != null)
            {
                Roach roach = hitInfo.roachObj.GetComponent<Roach>();
                Assert.IsNotNull(roach);
                roach.Hit();
            }
        }
    }

    // ------------------------------------------------------------------------
    private void UpdateShoe ()
    {
        SetShoeSplineStartPos();

        bool weaponCanReachTarget = UpdateTargetReticle().hit;

        if(weaponCanReachTarget && Input.GetMouseButtonDown(0))
        {
            _splineAnimator.Play();
            _needsAnimRestart = true;
        }

        if(_needsAnimRestart && _splineAnimator.ElapsedTime >= _splineAnimator.Duration*2)
        {
            ResetShoeAnim();
        }
    }

    // ------------------------------------------------------------------------
    private RoachHitInfo UpdateTargetReticle ()
    {
        bool weaponCanReachTarget = false;
        GameObject hitRoach = null;

        RaycastHit raycastHit;
        Vector3 raycastStart = Camera.main.transform.position;
        bool aimRaycastHit = Physics.Raycast(
                raycastStart,
                Camera.main.transform.forward,
                out raycastHit,
                100.0f,
                _aimLayers
        );
        if(aimRaycastHit)
        {
            if(Vector3.Distance(raycastHit.point, raycastStart) <= _maxAimDistance)
            {
                if(((1<<raycastHit.collider.gameObject.layer) & _roachLayer) != 0)
                {
                    hitRoach = raycastHit.collider.gameObject;

                    weaponCanReachTarget = true;
                    _reticleMat.color = _reticleHit;
                    SetTargetPosition(raycastHit.point);
                }
                else
                {
                    weaponCanReachTarget = true;
                    _reticleMat.color = _reticleMiss;
                    SetTargetPosition(raycastHit.point);
                }
            }
            else
            {
                _reticleMat.color = _reticleInvalid;
                SetTargetPosition(raycastHit.point);
            }
        }
        else
        {
            _reticleMat.color = _reticleInvalid;
            SetTargetPosition(_defaultReticlePos.position);
        }

        return new RoachHitInfo()
        {
            hit = weaponCanReachTarget,
            roachObj = hitRoach
        };
    }

    // ------------------------------------------------------------------------
    private void SetTargetPosition (Vector3 position)
    {
        _aimReticle.position = position + _reticleOffset;
        SetShoeSplineDestination(position);
    }

    // ------------------------------------------------------------------------
    private void ResetShoeAnim()
    {
        _needsAnimRestart = false;
        _splineAnimator.Restart(false);
    }

    // ------------------------------------------------------------------------
    private void SetShoeSplineStartPos()
    {
        var targetKnot = _shoeSpline.Spline.Knots.ToArray()[0];
        targetKnot.Position = _shoeSpline.transform.InverseTransformPoint(_shoeDefaultPos.position);
        _shoeSpline.Spline.SetKnot(0, targetKnot);
    }

    // ------------------------------------------------------------------------
    private void SetShoeSplineDestination(Vector3 targetWorldPos)
    {
        var targetKnot = _shoeSpline.Spline.Knots.ToArray()[1];
        targetKnot.Position = _shoeSpline.transform.InverseTransformPoint(targetWorldPos);
        _shoeSpline.Spline.SetKnot(1, targetKnot);
    }

    // ------------------------------------------------------------------------
    private void HandleRoachCollected(Roach roach)
    {
        roach.transform.position = _roachHoldLoc.position;
        roach.transform.SetParent(_roachHoldLoc);
    }
}
