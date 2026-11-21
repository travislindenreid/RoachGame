/*
 * File: RoachWeapon.cs
 * Created: 28/05/2026, 12:52:38 PM
 * Author: Travis Reid
 * Copyright 2019 - 2026 Studio Tilia
 */

using UnityEngine;

public class RoachWeapon : MonoBehaviour
{
    // ------------------------------------------------------------------------
    // Methods
    // ------------------------------------------------------------------------
    [Header("Obj references")]
    [SerializeField] private Roach _owner;
    [SerializeField] private Transform _pivot;
    [SerializeField] private GameObject _bangText;
    [SerializeField] private AudioSource _audioSource;
    [Header("Tuning")]
    [SerializeField] private float _textAppearTime;
    [SerializeField] private float _damage = 1;
    [SerializeField] private AudioClip _shootClip;

    private float _textTime;
    private bool _skipReloadAudio;
    private Vector3 _playerPos => Player._Instance._CameraPosition;
    private Ray _roachToPlayer => new Ray(_pivot.position, _playerPos - _pivot.position);

    // ------------------------------------------------------------------------
    // Methods
    // ------------------------------------------------------------------------
    private void Update ()
    {
        if(_textTime <= _textAppearTime)
        {
            _textTime += Time.deltaTime;
            if(_textTime >= _textAppearTime)
            {
                _bangText.SetActive(false);
            }
        }
    }

    // ------------------------------------------------------------------------
    private void Start()
    {
        // play reloading audio when activated
        // unless we've been told not to (because of cinematic reload)
        if(!_skipReloadAudio)
        {
            _audioSource.Play();
        }
    } 

    // ------------------------------------------------------------------------
    public void SkipFirstReloadAudio ()
    {
        _skipReloadAudio = true;
    }

    // ------------------------------------------------------------------------
    public void PointAtPlayer ()
    {
        _pivot.LookAt(_playerPos);
    }

    // ------------------------------------------------------------------------
    public void Use ()
    {
        RaycastHit hitInfo;
        if(Physics.Raycast(_roachToPlayer, out hitInfo))
        {
            Debug.Log("hit " + hitInfo.collider.gameObject);
            if(hitInfo.collider.gameObject.CompareTag("Player"))
            {
                Debug.Log("hit player");
                _textTime = 0.0f;
                _bangText.SetActive(true);

                // play on the persistent SFX source so the shot isn't cut off
                // when this weapon gets deactivated
                AudioController._Instance.PlaySfx(_shootClip);

                if(Player._Instance.DamageAndTryKill(_damage, true, _owner.transform.position))
                {
                    _owner.KilledPlayer();
                }
            }
        }
        else
        {
            Debug.Log("hit nothing");
        }
    }

    // ------------------------------------------------------------------------
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(_roachToPlayer);
    }
}