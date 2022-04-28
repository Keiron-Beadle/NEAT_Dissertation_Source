using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandController : MonoBehaviour
{
    private Renderer[] renderers;
    private Transform _thisTransform;
    private const float SPEED = 4.4f;
    private const float TURN_FORCE = 65.0f;

    // Start is called before the first frame update
    void Start()
    {
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

    // Update is called once per frame
    void Update()
    {
        bool moveX = Random.value > 0.5f;
        bool moveZ = Random.value > 0.5f;
        if (moveX)
        {
            _thisTransform.Rotate(Vector3.up, -TURN_FORCE * Time.deltaTime);
        }
        if (moveZ)
        {
            _thisTransform.Rotate(Vector3.up, TURN_FORCE * Time.deltaTime);
        }
        _thisTransform.position += -_thisTransform.forward * SPEED * Time.deltaTime;
    }
}
