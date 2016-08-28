using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    [SerializeField]
    private AudioClip Sound;

    private float m_timer = 0;

    private void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
    }

    private void Update()
    {
        m_timer += Time.deltaTime;

        if (m_timer%1 < 0.1f)
        {
            GetComponent<AudioSource>().PlayOneShot(Sound);
        }

        if (m_timer > 3)
        {
            m_timer = 0;
            GetComponent<AudioSource>().pitch = Random.Range(1.0f, 2.0f);
        }
    }
}
