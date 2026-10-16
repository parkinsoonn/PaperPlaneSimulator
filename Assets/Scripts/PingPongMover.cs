using UnityEngine;

public class PingPongMover : MonoBehaviour
{
    public float distance = 3f;
    public float speed = 2f;

    Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        float x = Mathf.Sin(Time.time * speed) * distance;
        transform.position = startPos + Vector3.right * x;
    }
}
