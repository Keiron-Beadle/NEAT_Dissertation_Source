using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandController : MonoBehaviour
{
    private SpriteRenderer colorComp;
    private Transform _thisTransform;
    private Transform[] _poi = new Transform[5];
    private Transform _target;
    private const float SPEED = 8.0f;
    private float _nodeDist = 0.1f;
    private Pathfinding.Path _path;
    private Seeker _seeker;
    private int _nodeCount = 0;
    private bool _pathFinding = false;

    public void RandControllerConstructor(Transform placesOfInterest)
    {
        for (int i = 0; i < placesOfInterest.childCount; i++)
        {
            _poi[i] = placesOfInterest.GetChild(i).transform;
        }
        _seeker = GetComponent<Seeker>();
    }

    // Start is called before the first frame update
    void Start()
    {
        colorComp = GetComponent<SpriteRenderer>();
        _thisTransform = GetComponent<Transform>();
        float ranX = UnityEngine.Random.value;
        float ranY = UnityEngine.Random.value;
        float ranZ = UnityEngine.Random.value;
        colorComp.color = new Color(ranX, ranY, ranZ, 1.0f);
    }

    // Update is called once per frame
    void Update()
    {
        if (_path == null || _nodeCount >= _path.vectorPath.Count)
        {
            _target = _poi[Random.Range(0, _poi.Length)];
            StartPathToTarget();
            return;
        }
        Vector3 dir = (_path.vectorPath[_nodeCount] - _thisTransform.position).normalized;
        _thisTransform.position += dir * Time.deltaTime * SPEED;
        float dist = Vector2.Distance(_thisTransform.position, _path.vectorPath[_nodeCount]);
        if (dist < _nodeDist)
        {
            _nodeCount++;
        }
    }

    private void StartPathToTarget()
    {
        if (_pathFinding) return;
        _seeker.StartPath(_thisTransform.position, _target.position, OnPathComplete);
        _pathFinding = true;
    }

    private void OnPathComplete(Pathfinding.Path p)
    {
        _pathFinding = false;
        if (!p.error)
        {
            _path = p;
            _nodeCount = 0;
        }
    }
}
