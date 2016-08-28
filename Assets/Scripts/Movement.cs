using UnityEngine;

public class Movement : MonoBehaviour
{
    [SerializeField]
    private float Speed;
    [SerializeField]
    private float Rotation;

    private Rigidbody m_rigidbody;
    private Vector3 m_velocity;
    private Vector3 m_torque;

    public void Move(Vector3 velocity)
    {
        m_velocity = velocity;
    }

    public void Rotate(Vector3 torque)
    {
        m_torque = torque;
    }

    private void Start()
    {
        bool result;

        result = Initialize();
        if (!result)
        {
            Debug.LogError("Failed to initialize Movement");
            Debug.Break();
        }
    }

    private bool Initialize()
    {
        m_rigidbody = GetComponent<Rigidbody>();
        if (m_rigidbody == null)
        {
            return false;
        }

        return true;
    }

    private void FixedUpdate()
    {
        m_rigidbody.AddForce(m_velocity.normalized * Speed * PlayerPrefs.GetInt("Level", 1));
        m_rigidbody.AddTorque(m_torque.normalized * Rotation);
    }
}
