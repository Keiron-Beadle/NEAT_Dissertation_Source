using Base_NEAT;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Pathfinding;

class AIController : MonoBehaviour
{
    private Phenotype _phenotype;

    public float[] _status = new float[] { 100.0f, 100.0f, 100.0f, 100.0f, 100.0f};
    private SpriteRenderer colorComp;
    private Transform _thisTransform;
    private Transform[] _poi = new Transform[5];
    private Transform _target;
    private float _speed = 8.0f;
    private float _nodeDist = 0.1f;
    bool _pathFinding = false;
    private Pathfinding.Path _path;
    private int _nodeCount = 0;

    private Seeker _seeker;

    public AIController()
    {
    }

    public void SetPhenotype(Phenotype pPhenotype)
    {
        _phenotype = pPhenotype;
    }

    public Phenotype GetPhenotype() { return _phenotype; }

    public void AIControllerConstructor(Transform placesOfInterest)
    {
        colorComp = GetComponent<SpriteRenderer>();
        _thisTransform = GetComponent<Transform>();
        float ranX = UnityEngine.Random.value;
        float ranY = UnityEngine.Random.value;
        float ranZ = UnityEngine.Random.value;
        colorComp.color = new Color(ranX, ranY, ranZ, 1.0f);
        for (int i = 0; i < placesOfInterest.childCount; i++)
            _poi[i] = placesOfInterest.GetChild(i).transform;
        _seeker = GetComponent<Seeker>();
        
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

    public void ResetTransform()
    {
        transform.position = new Vector2(-0.15f, 2.21f);
        _status[0] = 100;
        _status[1] = 100;
        _status[2] = 100;
        _status[3] = 100;
        _status[4] = 100;
        float ranX = UnityEngine.Random.value;
        float ranY = UnityEngine.Random.value;
        float ranZ = UnityEngine.Random.value;
        colorComp.color = new Color(ranX, ranY, ranZ, 1.0f);
        _path = null;
        _nodeCount = 0;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        switch (collision.tag)
        {
            case "Happiness":
                if (_status[0] < 99.9)
                    _status[0] += NEATConfig.INCREASE_STATUS[(int)RESPONSES.Happiness] * Time.deltaTime;
                break;
            case "Hunger":
                if (_status[1] < 99.9)
                    _status[1] += NEATConfig.INCREASE_STATUS[(int)RESPONSES.Hunger] * Time.deltaTime;
                break;
            case "Sleep":
                if (_status[2] < 99.9)
                    _status[2] += NEATConfig.INCREASE_STATUS[(int)RESPONSES.Sleep] * Time.deltaTime;
                break;
            case "Thirst":
                if (_status[3] < 99.9)
                    _status[3] += NEATConfig.INCREASE_STATUS[(int)RESPONSES.Thirst] * Time.deltaTime;
                break;
            case "Social":
                if (_status[4] < 99.9)
                    _status[4] += NEATConfig.INCREASE_STATUS[(int)RESPONSES.Social] * Time.deltaTime;
                break;
            default:
                break;
        }
    }

    public void UpdateBasedOnNetwork()
    {
        for (int i = 0; i <_status.Length; i++)
        {
            //check if dead
            if (_status[i] <= 0)
            {
                //_status[i] = 100;
                _phenotype.IsDead = true;
                colorComp.color = new Color(0, 0, 0, 0);
            }
        }

        if (_path == null || _nodeCount >= _path.vectorPath.Count)
        {
            switch (_phenotype.ActivatedOutputs[0])
            {
                case RESPONSES.Hunger:
                    _target = _poi[(int)RESPONSES.Hunger];
                    break;
                case RESPONSES.Happiness:
                    _target = _poi[(int)RESPONSES.Happiness];
                    break;
                case RESPONSES.Sleep:
                    _target = _poi[(int)RESPONSES.Sleep];
                    break;
                case RESPONSES.Social:
                    _target = _poi[(int)RESPONSES.Social];
                    break;
                case RESPONSES.Thirst:
                    _target = _poi[(int)RESPONSES.Thirst];
                    break;
                default:
                    break;
            }
            StartPathToTarget();
            return;
        }
        Vector3 direction = (_path.vectorPath[_nodeCount] - _thisTransform.position).normalized;
        _thisTransform.position += direction * Time.deltaTime * _speed;
        float distance = Vector2.Distance(_thisTransform.position, _path.vectorPath[_nodeCount]);
        if (distance < _nodeDist)
        {
            _nodeCount++;
        }
    }

    internal float[] GatherInput()
    {
        //_status[5] = Vector2.Distance(_thisTransform.position, _poi[0].position);
        //_status[6] = Vector2.Distance(_thisTransform.position, _poi[1].position);
        //_status[7] = Vector2.Distance(_thisTransform.position, _poi[2].position);
        //_status[8] = Vector2.Distance(_thisTransform.position, _poi[3].position);
        //_status[9] = Vector2.Distance(_thisTransform.position, _poi[4].position);

        float[] inputData = new float[10];
        for (int i = 0; i < 5; i++) //Give status as first set of input
        {
            if (_status[i] <= 0)
            {
                inputData[i] = 0;
                continue;
            }
            inputData[i] =  1/_status[i];
        }
        for (int i = 5; i < 10; i++) //Give a factor of time til full status, assuming AI is stood on a place of interest
        {
            inputData[i] = (NEATConfig.INCREASE_STATUS[i - 5] - NEATConfig.DECREASE_STATUS[i - 5]) / (100-_status[i - 5]);
        }
        return inputData;
    }
}
