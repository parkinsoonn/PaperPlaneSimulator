using UnityEngine;

// A car, bus or truck driving along the street. Flying into one ends the game.
public class Vehicle : MonoBehaviour
{
    public float s;              // distance along the street
    public float lane;           // lateral offset from the center line
    public int direction;        // +1 drives with the plane, -1 comes toward it
    public float speed;          // desired speed, m/s
    public float currentSpeed;   // after keeping distance to the vehicle ahead
    public float length;

    void OnTriggerEnter(Collider other)
    {
        var plane = other.attachedRigidbody != null ? other.attachedRigidbody.GetComponent<PaperPlaneController>() : null;
        if (plane != null && plane.IsFlying) plane.Crash();
    }
}
