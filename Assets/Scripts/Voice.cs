using UnityEngine;
using System.Collections;

public class Voice : MonoBehaviour
{
    [SerializeField]
    private int VoiceSteps;
    [SerializeField][Range(0, 2)]
    private float Intensity;
    [SerializeField]
    private AudioClip VoiceSound;

    private float m_voiceTime;
    private AudioSource m_source;
    private Coroutine m_voiceIteration;

    public void SaySomething()
    {
        if (m_voiceIteration != null)
        {
            StopCoroutine(m_voiceIteration);
            m_voiceIteration = null;
        }

        Intensity -= 0.1f;
        Intensity = Mathf.Clamp(Intensity, 0.3f, 2f);

        m_voiceIteration = StartCoroutine(VoiceIteration());
    }

    public void ShoutSomething()
    {
        if (m_voiceIteration != null)
        {
            StopCoroutine(m_voiceIteration);
            m_voiceIteration = null;
        }

        Intensity += 0.1f;
        Intensity = Mathf.Clamp(Intensity, 0.3f, 2f);

        m_voiceIteration = StartCoroutine(VoiceIteration());
    }

    private IEnumerator VoiceIteration()
    {
        for (int i = 0; i < VoiceSteps; i++)
        {
            m_source.pitch = Random.Range(1f, 3f);
            m_source.PlayOneShot(VoiceSound);
            yield return new WaitForSeconds(m_voiceTime * Intensity);
        }

        m_voiceIteration = null;
    }

    private void Start()
    {
        bool result;

        result = Initialize();
        if (!result)
        {
            Debug.LogError("Failed to initialize Voice");
            Debug.Break();
        }
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.F1))
        {
            SaySomething();
        }
    }

    private bool Initialize()
    {
        if (VoiceSound == null)
        {
            return false;
        }

        m_source = GetComponent<AudioSource>();
        if (m_source == null)
        {
            return false;
        }

        if (Intensity < 0)
        {
            return false;
        }

        m_voiceTime = VoiceSound.length;

        return true;
    }
}
