using UnityEngine;

namespace AI
{
    public class Worker : MonoBehaviour
    {
        [SerializeField]
        private float MaxHealth, MaxThirst;
        [SerializeField][Range(0,1)]
        private float WorkEthic;
        [SerializeField][Range(0, 1)]
        private float Talkative;
        [SerializeField][Range(0, 1)]
        private float WanderRate;
        [SerializeField]
        private float LookRadius;
        [SerializeField]
        private float PlaceRadius;
        [SerializeField]
        private float PickupRadius;
        [SerializeField]
        private float FuckRadius;
        [SerializeField]
        private float CarryBlockHeight;
        [SerializeField]
        private float JumpForce;
        [SerializeField]
        private string PickupTag;
        [SerializeField]
        private string WorkerTag;
        [SerializeField]
        private GameObject DeathPrefab;
        [SerializeField]
        private GameObject Love;

        private State m_state;
        private Vector3 m_target;
        private Movement m_movement;
        private GameObject m_carryingBlock;
        private float m_timer;
        private float m_health;
        private float m_thirst;
        private GameObject m_lover;
        private float m_age;
        private float m_loveTimer;

        private void Start()
        {
            bool result;

            result = Initialize();
            if (!result)
            {
                Debug.LogError("Failed to initialize Worker.");
                Debug.Break();
            }
        }

        private bool Initialize()
        {
            m_target = transform.position;

            m_carryingBlock = null;

            m_movement = GetComponent<Movement>();
            if (m_movement == null)
            {
                return false;
            }

            if (LookRadius <= 0)
            {
                return false;
            }

            if (PickupRadius <= 0)
            {
                return false;
            }

            m_state = State.Idling;
            m_health = MaxHealth;
            m_thirst = MaxThirst;
            m_timer = 0;
            m_age = 1;
            m_lover = null;

            return true;
        }

        private void Idle()
        {
            bool result;
            RaycastHit[] hits;
            
            result = ScanAround(out hits);
            if (!result)
            {
                return;
            }

            for (int i = 0; i < hits.Length; i++)
            {
                string name = hits[i].transform.tag;

                if(name == WorkerTag)
                {
                    if (hits[i].transform.gameObject != gameObject)
                    {
                        if (Random.value < Talkative && m_loveTimer > 10)
                        {
                            Debug.Log("I want to make a baby!");
                            m_target = hits[i].transform.position;
                            m_lover = hits[i].transform.gameObject;
                            var love = Instantiate(Love, transform.position + Vector3.up * 5, Quaternion.identity);
                            Destroy(love, 2);

                            m_state = State.LoveSeeking;
                            
                            return;
                        }
                    }
                }
                else if (name == PickupTag)
                {
                    if (Random.value < WorkEthic)
                    {
                        Debug.Log("Going to work!");
                        m_target = hits[i].transform.position;
                        m_state = State.Working;
                        return;
                    }
                }
            }

            if (Random.value < WanderRate)
            {
                Debug.Log("Wandering!");
                m_target = new Vector3(Random.value * 10, 0, Random.value * 10);
            }
        }

        public void StopWorking()
        {
            m_lover = null;
            m_state = State.Idling;
            if (m_carryingBlock != null)
            {
                m_carryingBlock.tag = PickupTag;
                m_carryingBlock = null;
            }
        }

        private void Loving()
        {
            if (m_lover != null)
            {
                if (Vector3.Distance(transform.position, m_lover.transform.position) < FuckRadius && m_loveTimer > 10)
                {
                    m_loveTimer = 0;
                    Environnement.Instance.SpawnBaby(transform.position + Vector3.up * 3);
                    var love = Instantiate(Love, transform.position + Vector3.up * 5, Quaternion.identity);
                    Destroy(love, 2);
                    m_lover = null;
                    m_state = State.Idling;
                }
                else
                {
                    m_target = m_lover.transform.position;
                }
            }
            else
            {
                m_state = State.Idling;
            }
        }

        private void Work()
        {
            Vector3 blockTarget;
            bool result;

            // Can we pickup the object if we are holding nothing
            if (m_carryingBlock == null)
            {
                RaycastHit[] hits;

                result = ScanAround(out hits);
                if (!result)
                {
                    return;
                }
                
                for (int i = 0; i < hits.Length; i++)
                {
                    if (hits[i].transform.tag != PickupTag)
                    {
                        continue;
                    }
                    if (Vector3.Distance(transform.position, hits[i].transform.position) < PickupRadius)
                    {
                        m_carryingBlock = hits[i].transform.gameObject;
                        hits[i].transform.gameObject.tag = "Untagged";
                        break;
                    }
                }

                m_thirst -= 3 * Time.deltaTime;
            }
            else
            {
                // Can we drop object in pyramid position
                blockTarget = Pyramid.Instance.GetCurrentBlockPosition();
                if (Vector3.Distance(transform.position, blockTarget) < PlaceRadius)
                {
                    Pyramid.Instance.UpdatePlaceBlockPosition();

                    m_carryingBlock.tag = "Untagged";
                    m_carryingBlock.transform.position = blockTarget;
                    m_carryingBlock.transform.eulerAngles = Vector3.zero;
                    m_carryingBlock.GetComponent<Rigidbody>().isKinematic = true;
                    m_carryingBlock = null;
                    m_state = State.Idling;
                }
                // Else just carry object and move to pyramid
                else
                {
                    m_carryingBlock.transform.position = transform.position + Vector3.up * CarryBlockHeight;
                    m_target = blockTarget;
                }

                m_thirst -= 6 * Time.deltaTime;
            }
        }

        // TODO: SOrt collisions by distance
        private bool ScanAround(out RaycastHit[] hits)
        {
            hits = Physics.SphereCastAll(transform.position, LookRadius, transform.forward);
            if (hits == null || hits.Length == 0)
            {
                return false;
            }

            return true;
        }

        private void Update()
        {
            UpdateDebug();

            if (m_health <= 0 || transform.position.y < -10 || m_thirst <= 0 || m_age > 100)
            {
                var particle = Instantiate(DeathPrefab, transform.position, Quaternion.identity);
                if (m_carryingBlock != null)
                {
                    m_carryingBlock.tag = PickupTag;
                    m_carryingBlock = null;
                }

                Destroy(particle, 3);
                Destroy(gameObject);
                return;
            }

            m_age += Time.deltaTime;
            transform.localScale = Vector3.one * 3 * m_thirst/100;
            m_loveTimer += Time.deltaTime;

            m_thirst -= 1 * Time.deltaTime;

            switch (m_state)
            {
                case State.Idling:
                    Idle();
                    break;
                case State.LoveSeeking:
                    Loving();
                    break;
                case State.Working:
                    Work();
                    break;
                default:
                    break;
            }
        }

        private void UpdateDebug()
        {
            Debug.DrawLine(transform.position, m_target);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, LookRadius);
        }

        private void FixedUpdate()
        {
            if (Physics.Raycast(transform.position, (m_target - transform.position).normalized, 3))
            {
                m_movement.Move(Vector3.up * JumpForce);
            }
            m_movement.Move((m_target - transform.position).normalized * ((100-m_age)/100));
        }

        public GameObject GetCarryingObject()
        {
            return m_carryingBlock;
        }

        public void Damage()
        {
            m_health -= 3;
        }

        public void RefreshThirst()
        {
            m_thirst = MaxThirst;
        }
    }

    public enum State
    {
        Idling,
        LoveSeeking,
        Working
    }
}