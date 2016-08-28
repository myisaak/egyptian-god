using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    private float HitForce = 1;
    [SerializeField]
    private string PickableTag;
    [SerializeField]
    private GameObject Line;
    [SerializeField]
    private GameObject Cloud;
    [SerializeField]
    private GameObject Rain;
    [SerializeField]
    private AudioClip ShockSound;
    [SerializeField]
    private AudioClip RainSound;

    private Movement m_movement;
    private Camera m_cam;
    private AudioSource m_audioSource;

    private void Start()
    {
        bool result;

        result = Initialize();
        if (!result)
        {
            Debug.LogError("Failed to initialize Player");
            Debug.Break();
        }
    }

    private bool Initialize()
    {
        m_movement = GetComponent<Movement>();
        if (m_movement == null)
        {
            return false;
        }

        m_cam = GetComponent<Camera>();
        if (m_cam == null)
        {
            return false;
        }

        m_audioSource = GetComponent<AudioSource>();
        if (m_audioSource == null)
        {
            return false;
        }

        if (Line == null)
        {
            return false;
        }

        return true;
    }

    private void Update()
    {
        // Handle input
        Vector3 axisInputMovement = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        Vector3 axisInputRotation = new Vector3(0, Input.GetAxis("Yaw"), 0);
        axisInputMovement = transform.TransformDirection(axisInputMovement);
        axisInputMovement.y = 0;
        if (Input.mouseScrollDelta.sqrMagnitude > 0)
        {
            axisInputMovement.y = Input.mouseScrollDelta.y * 100;
        }

        m_movement.Move(axisInputMovement);
        m_movement.Rotate(axisInputRotation);

        Vector3 pos = transform.position;
        transform.position = new Vector3(Mathf.Clamp(pos.x, -320, 320), pos.y, Mathf.Clamp(pos.z, -320, 320));

        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);

        if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
        {
            RaycastHit hit;
            Ray ray = m_cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.tag == PickableTag)
                {
                    Vector3[] linePositions = new Vector3[]
                    {
                        hit.point + Vector3.up * 20f,
                        hit.point + (Vector3.up * 18 + new Vector3(Random.value, 0, Random.value)),
                        hit.point + (Vector3.up * 16 + new Vector3(Random.value, 0, Random.value)),
                        hit.point + (Vector3.up * 14 + new Vector3(Random.value, 0, Random.value)),
                        hit.point + (Vector3.up * 12 + new Vector3(Random.value, 0, Random.value)),
                        hit.point + (Vector3.up * 10 + new Vector3(Random.value, 0, Random.value)),
                        hit.point + (Vector3.up * 8 + new Vector3(Random.value, 0, Random.value)),
                        hit.point + (Vector3.up * 6 + new Vector3(Random.value, 0, Random.value)),
                        hit.point + (Vector3.up * 4 + new Vector3(Random.value, 0, Random.value)),
                        hit.point + (Vector3.up * 2 + new Vector3(Random.value, 0, Random.value)),
                        hit.point
                    };

                    if (Input.GetMouseButton(0))
                    {
                        var line = (GameObject)Instantiate(Line);
                        line.GetComponent<LineRenderer>().SetPositions(linePositions);
                        Destroy(line, 0.1f);

                        m_audioSource.PlayOneShot(ShockSound);

                        hit.collider.GetComponent<Rigidbody>().AddForce(ray.direction.normalized * HitForce, ForceMode.Impulse);
                        hit.collider.GetComponent<Rigidbody>().AddForce(Vector3.up * HitForce, ForceMode.Impulse);
                        hit.collider.GetComponent<Voice>().ShoutSomething();
                        hit.collider.GetComponent<AI.Worker>().StopWorking();
                        hit.collider.GetComponent<AI.Worker>().Damage();
                    }
                    else
                    {
                        var rain = (GameObject)Instantiate(Rain, hit.point + (Vector3.up * 20f), Rain.transform.rotation);
                        Destroy(rain, 1.5f);

                        m_audioSource.PlayOneShot(RainSound);

                        hit.collider.GetComponent<AI.Worker>().RefreshThirst();
                    }

                    var cloud = (GameObject)Instantiate(Cloud, hit.point + (Vector3.up * 20f), Cloud.transform.rotation);
                    Destroy(cloud, 2f);
                }
            }
        }
    }
}
