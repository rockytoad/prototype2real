using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movetoward : MonoBehaviour
{
    public float speed = 1.0f;
    public Transform target;
    // Update is called once per frame
    void Update()
    {
        var step = speed * Time.deltaTime; // calculate distance to move
        transform.position = Vector3.MoveTowards(transform.position, target.position, step);
    }
}
