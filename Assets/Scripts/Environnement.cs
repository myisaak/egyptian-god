using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Environnement : MonoBehaviour
{
    [SerializeField]
    private Slider TimeBar;
    [SerializeField]
    private GameObject BlockPrefab, WorkerPrefab;
    [SerializeField]
    private float SpawnDelay;
    [SerializeField]
    private float TimeMultiplier;
    [SerializeField]
    private Light SunLight;
    [SerializeField][Range(0, 24)]
    private float TimeOfDayInHours;

    // In seconds
    private float m_time;
    private float m_spawnTimer;
    private static Environnement m_instance;

    public static Environnement Instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = FindObjectOfType<Environnement>();
            }

            return m_instance;
        }
    }

    private void Start ()
    {
        bool result;

        result = Initialize();
        if (!result)
        {
            Debug.LogError("Failed to initialize Environnement");
            Debug.Break();
        }
	}

    private bool Initialize ()
    {
        if (SunLight == null)
        {
            return false;
        }

        if (TimeBar == null)
        {
            return false;
        }

        if (TimeOfDayInHours < 0)
        {
            return false;
        }

        if (BlockPrefab == null)
        {
            return false;
        }

        m_time = TimeOfDayInHours * 60 * 60;
        return true;
    }

    private void Update ()
    {
        m_time -= Time.deltaTime * TimeMultiplier / PlayerPrefs.GetInt("Level", 1);

        TimeBar.value = m_time/(TimeOfDayInHours * 60 * 60);
        SunLight.transform.eulerAngles = new Vector3(180 - (m_time/86400.0f) * 180.0f, 0, 0);

        if (m_time <= 0)
        {
            int currentLevel = PlayerPrefs.GetInt("Level", 1);
            int highscore = PlayerPrefs.GetInt("HighScore", 1);
            if (currentLevel > highscore)
            {
                PlayerPrefs.SetInt("HighScore", currentLevel + 1);
            }
            PlayerPrefs.SetInt("Level", 1);
            SceneManager.LoadScene(0);
            return;
        }

        m_spawnTimer += Time.deltaTime;

        if (m_spawnTimer > SpawnDelay/PlayerPrefs.GetInt("Level", 1))
        {
            m_spawnTimer = 0;

            var terrain = FlatTerrain.Instance.GetTerrainSize();
            Debug.Log(terrain.ToString());
            var pos = new Vector3(Random.Range(-250, 250), 10, Random.Range(-250, 250));
            SpawnBaby(pos);
            pos = new Vector3(Random.Range(-250, 250), 10, Random.Range(-250, 250));
            Instantiate(BlockPrefab, pos, Quaternion.identity);
        }
    }

    public void SpawnBaby(Vector3 position)
    {
        Instantiate(WorkerPrefab, position, Quaternion.identity);
    }

    public void NextLevel()
    {
        int currentLevel = PlayerPrefs.GetInt("Level", 1);
        int highscore = PlayerPrefs.GetInt("HighScore", 1);
        if (currentLevel > highscore)
        {
            PlayerPrefs.SetInt("HighScore", currentLevel + 1);
        }
        PlayerPrefs.SetInt("Level", PlayerPrefs.GetInt("Level", 1) + 1);
        SceneManager.LoadScene(0);
    }
}
