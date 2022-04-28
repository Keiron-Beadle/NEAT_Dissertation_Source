using Base_NEAT;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

class AIController : MonoBehaviour
{
    private Phenotype _phenotype;
    private Renderer[] renderers;
    private Transform _thisTransform;
    private Vector3 spawnPoint;
    private const float SPEED = 4.4f;
    private const float TURN_FORCE = 65.0f;

    public AIController()
    {
    }

    public void SetPhenotype(Phenotype pPhenotype)
    {
        _phenotype = pPhenotype;
    }

    public Phenotype GetPhenotype() { return _phenotype; }

    public void AIControllerConstructor(Vector3 pSpawnPoint)
    {
        //GetComponentsInChildren(Material, materials);
        spawnPoint = pSpawnPoint;
        renderers = GetComponentsInChildren<Renderer>();
        _thisTransform = GetComponent<Transform>();
        float ranX = UnityEngine.Random.value;
        float ranY = UnityEngine.Random.value;
        float ranZ = UnityEngine.Random.value;
        foreach (Renderer r in renderers)
        {
            r.material.color = new Color(ranX, ranY, ranZ, 1.0f);
        }
    }

    public void ResetTransform()
    {
        transform.position = spawnPoint;
        transform.rotation = Quaternion.identity;
        float ranX = UnityEngine.Random.value;
        float ranY = UnityEngine.Random.value;
        float ranZ = UnityEngine.Random.value;
        foreach (Renderer r in renderers)
        {
            r.material.color = new Color(ranX, ranY, ranZ, 1.0f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 8)
        {
            _phenotype.Score += NEATConfig.GOAL_SCORE; 
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.layer == 7)
        {
            _phenotype.IsDead = true;
            return;
        }
    }

    public void UpdateBasedOnNetwork()
    {
        foreach (var output in _phenotype.ActivatedOutputs)
        {
            switch (output)
            {
                case RESPONSES.LEFT:
                    _thisTransform.Rotate(Vector3.up, -TURN_FORCE * Time.deltaTime);
                    break;
                case RESPONSES.RIGHT:
                    _thisTransform.Rotate(Vector3.up, TURN_FORCE * Time.deltaTime);
                    break;
            }
        }
        _thisTransform.position += -_thisTransform.forward * SPEED * Time.deltaTime;
    }

    internal float[] GatherInput()
    {
        var mask = LayerMask.GetMask("death");
        float[] input = new float[8]; //8 directions raycasts
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
        return input;
    }
}
