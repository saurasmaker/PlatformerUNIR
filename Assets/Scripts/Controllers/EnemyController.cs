using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField]
    DetectorController _detectIfFollow, _detectIfAttack;
    [SerializeField]
    PlayerController _playerController;
    [SerializeField]
    Transform[] _targetToAttack, _targetToFollow;



    [SerializeField]
    Transform[] _path;

    [SerializeField]
    float _speed = 2f, _speedOnFollow = 3f, _followingTime = 2f, _followMaxDistanceFromOrigin = 3f;

    bool _canMove = true, _following = false;
    Vector2 _initPos = Vector2.zero, _targetDir = Vector2.zero;
    Vector3 _initLocalScale;


    public Vector2 TargetDir
    {
        get => _targetDir;
        set
        {
            if(_targetDir != value)
            {
                _targetDir = value;
                if (_targetDir.x < 0)
                    transform.localScale = new Vector3(-_initLocalScale.x, _initLocalScale.y, _initLocalScale.z);
                else if(_targetDir.x > 0)
                    transform.localScale = new Vector3(_initLocalScale.x, _initLocalScale.y, _initLocalScale.z);
            }
        }
    }

    private void Awake()
    {
        _initPos = transform.position;
        _initLocalScale = transform.localScale;

        if(_detectIfFollow == null)
            _detectIfFollow = GetComponentInChildren<DetectorController>();

        _detectIfFollow.OnDetect.AddListener((_)=> StartFollowPlayer());
        _detectIfFollow.OnUndetect.AddListener((_)=> StartFollowPath());

        //_detectIfAttack.OnDetect.AddListener((_)=> StartFollowPlayer());
        //_detectIfAttack.OnUndetect.AddListener((_)=> StartFollowPlayer());
    }

    private void Start()
    {
        transform.position = _path[0].position;
        StartFollowPath();
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < (_path.Length - 1); ++i)
            Debug.DrawLine(_path[i].position, _path[i + 1].position, Color.yellow);
    }

    void StartAttack()
    {

    }


    void StartFollowPlayer()
    {
        StopFollowPath();
        StopFollowPlayer();
        _following = true;
        _followPlayer = StartCoroutine(FollowPlayer());
    }

    private void StopFollowPlayer()
    {
        if(_followPlayer != null)
        {
            StopCoroutine(_followPlayer);
            _followPlayer = null;
        }

        _following = false;
    }
    Coroutine _followPlayer = null;
    IEnumerator FollowPlayer()
    {
        Transform target = _targetToFollow[Random.Range(0, _targetToFollow.Length)];

        while (_following)
        {
            TargetDir = target.position - transform.position;
            Vector2 newPos = _speedOnFollow * Time.deltaTime * TargetDir.normalized;
            transform.position += new Vector3(newPos.x, newPos.y, transform.position.z);

            if(Vector2.Distance(_initPos, target.position) > _followMaxDistanceFromOrigin)
                StartFollowPath();

            yield return new WaitForEndOfFrame();
        }
    }


    void StartFollowPath()
    {
        StopFollowPath();
        StopFollowPlayer();
        _followPath = StartCoroutine(FollowPath());

    }
    private void StopFollowPath()
    {
        if (_followPath != null)
        {
            StopCoroutine(_followPath);
            _followPath = null;
        }
    }
    Coroutine _followPath = null;
    IEnumerator FollowPath()
    {
        Vector3 prevPos = transform.position;
        
        int pos = 1;
        float t = 0;

        TargetDir = prevPos - _path[pos].position;
        while (true)
        {   
            if (_canMove)
            {
                t += Time.deltaTime * _speed;

                transform.position = Vector3.LerpUnclamped(prevPos, _path[pos].position, t);

                if(t >= 1)
                {
                    t = 0;
                    ++pos;
                    pos %= _path.Length;
                    prevPos = transform.position;
                    TargetDir = prevPos - _path[pos].position;
                }
            }

            yield return new WaitForEndOfFrame();
        }
    }
}   
