using Base_NEAT;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

class AIController : MonoBehaviour
{
    private Phenotype _phenotype;
    private Renderer[] _thisRenderers;
    private Transform _thisTransform;
    private BoxCollider _thisCollider;
    private Vector3 spawnPoint;
    private const float SPEED = 1.2f;
    private Transform[] waypoints;
    private int waypointPointer = 0;
    private float elapsedTime = 0;

    public AIController()
    {
    }

    public void SetPhenotype(Phenotype pPhenotype)
    {
        _phenotype = pPhenotype;
    }

    public Phenotype GetPhenotype() { return _phenotype; }

    public void AIControllerConstructor(Vector3 pSpawnPoint, Transform pObstacles)
    {
        //GetComponentsInChildren(Material, materials);
        spawnPoint = pSpawnPoint;
        _thisRenderers = GetComponentsInChildren<Renderer>();
        _thisTransform = GetComponent<Transform>();
        _thisCollider = GetComponent<BoxCollider>();
        float ranX = UnityEngine.Random.value;
        float ranY = UnityEngine.Random.value;
        float ranZ = UnityEngine.Random.value;
        foreach (Renderer r in _thisRenderers)
        {
            r.material.color = new Color(ranX, ranY, ranZ, 1.0f);
        }
        FindWaypointsFromObstacles(pObstacles);
    }

    private void FindWaypointsFromObstacles(Transform pObstacles)
    {
        List<Transform> waypointsList = new List<Transform>();
        //Loops through obstacles
        for (int i = 0; i < pObstacles.childCount; i++)
        {
            var obstacle = pObstacles.GetChild(i);
            //Then loop through each obstacle, looking for waypoints
            for (int j = 0; j < obstacle.childCount; j++)
            {
                if (obstacle.GetChild(j).gameObject.layer == 7)
                {
                    waypointsList.Add(obstacle.GetChild(j));
                }
            }
        }
        waypoints = waypointsList.ToArray();
    }

    public void ResetTransform()
    {
        transform.position = spawnPoint;
        transform.rotation = Quaternion.identity;
        waypointPointer = 0;
        float ranX = UnityEngine.Random.value;
        float ranY = UnityEngine.Random.value;
        float ranZ = UnityEngine.Random.value;
        foreach (Renderer r in _thisRenderers)
        {
            r.material.color = new Color(ranX, ranY, ranZ, 1.0f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 7)
        {
            var parentGroup = other.gameObject.transform.parent;
            //find which waypoint we just collided with 
            for (int i = 0; i < parentGroup.childCount; i++)
            {
                if (parentGroup.GetChild(i).gameObject == other.gameObject)
                {
                    _phenotype.Score += NEATConfig.GOAL_SCORE;
                    waypointPointer++;
                }
            }
            elapsedTime = 0;
            Physics.IgnoreCollision(other, _thisCollider);
        }
        else if (other.gameObject.layer == 6)
        {
            foreach (Renderer r in _thisRenderers)
            {
                r.material.color = new Color(0, 0, 0, 0.0f);
            }
            _phenotype.IsDead = true;
            return;
        }
    }

    public void UpdateBasedOnNetwork()
    {
        //Phenotype dies after X time if not reached next checkpoint,
        //means it'll constantly be looking to move instead of sitting in same place 
        elapsedTime += Time.deltaTime;
        if (elapsedTime >= NEATConfig.KILL_TIME)
        {
            _phenotype.IsDead = true;
            elapsedTime = 0;
            return;
        }

        Vector3 movement = new Vector3(0,0,0);
        foreach (var output in _phenotype.ActivatedOutputs)
        {
            switch (output)
            {
                case RESPONSES.LEFT:
                    movement.z += SPEED;
                    break;
                case RESPONSES.RIGHT:
                    movement.z -= SPEED;
                    break;
                case RESPONSES.FORWARD:
                    movement.x += SPEED;
                    break;
                case RESPONSES.BACKWARD:
                    movement.x -= SPEED;
                    break;
            }
        }
        _thisTransform.Translate(movement * Time.deltaTime);
    }

    internal float[] GatherInput()
    {
        var mask = LayerMask.GetMask("death");
        float[] input = new float[10]; //8 directions raycasts
        Physics.Raycast(_thisTransform.position, _thisTransform.forward, out RaycastHit hitInfo, 100.0f, mask);
        input[0] = 1 / hitInfo.distance;
        Physics.Raycast(_thisTransform.position, -_thisTransform.forward, out hitInfo, 100.0f, mask);
        input[1] = 1 / hitInfo.distance;
        Physics.Raycast(_thisTransform.position, _thisTransform.right, out hitInfo, 100.0f, mask);
        input[2] = 1 / hitInfo.distance;
        Physics.Raycast(_thisTransform.position, -_thisTransform.right, out hitInfo, 100.0f, mask);
        input[3] = 1 / hitInfo.distance;
        Physics.Raycast(_thisTransform.position, _thisTransform.forward + _thisTransform.right, out hitInfo, 100.0f, mask);
        input[4] = 1 / hitInfo.distance;
        Physics.Raycast(_thisTransform.position, _thisTransform.forward - _thisTransform.right, out hitInfo, 100.0f, mask);
        input[5] = 1 / hitInfo.distance;
        Physics.Raycast(_thisTransform.position, -_thisTransform.forward + _thisTransform.right, out hitInfo, 100.0f, mask);
        input[6] = 1 / hitInfo.distance;
        Physics.Raycast(_thisTransform.position, -_thisTransform.forward - _thisTransform.right, out hitInfo, 100.0f, mask);
        input[7] = 1 / hitInfo.distance;

        //input 8->10 are decided by vector to next checkpoint & distance 
        input[8] = waypoints[waypointPointer].position.x - _thisTransform.position.x;
        input[9] = waypoints[waypointPointer].position.z - _thisTransform.position.z;
        //distance to last waypoint is the score each frame, should encourage AI to keep moving forward
        float dist = Vector3.Distance(waypoints[2].position, _thisTransform.position);
        _phenotype.Score += 1.0f / dist;
        return input;
    }
}
