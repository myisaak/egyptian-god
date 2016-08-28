using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pyramid : MonoBehaviour
{
    [SerializeField]
    private string PyramidTag;
    [SerializeField]
    private float BlockScale;
    [SerializeField]
    private int PyramidHeight, PyramidLength, PyramidWidth;
    [SerializeField]
    private GameObject DebugBlockPrefab;

    private List<Vector3> m_pyramidBlocks;
    private int m_currentPyramidIteration;
    private static Pyramid m_instance;
    private float SpawnDelay;

    public static Pyramid Instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = FindObjectOfType<Pyramid>();
            }

            return m_instance;
        }
    }

    public void UpdatePlaceBlockPosition()
    {
        if (m_currentPyramidIteration >= m_pyramidBlocks.Count - 1)
        {
            Environnement.Instance.NextLevel();
            return;
        }
        m_currentPyramidIteration++;
    }

    public Vector3 GetCurrentBlockPosition()
    {
        return m_pyramidBlocks[Mathf.Min(m_currentPyramidIteration, m_pyramidBlocks.Count - 1)];
    }

	private void Start ()
    {
        bool result;

        result = Initialize();
        if (!result)
        {
            Debug.LogError("Failed to initialize Pyramid");
            Debug.Break();
        }
    }

    private bool Initialize()
    {
        bool result;
        int currentLevel;
        
        currentLevel = PlayerPrefs.GetInt("Level", 1);
        PyramidHeight += Mathf.RoundToInt(0.5f * currentLevel * currentLevel);
        PyramidLength += Mathf.RoundToInt(2f * currentLevel * currentLevel);
        PyramidWidth += Mathf.RoundToInt(2f * currentLevel * currentLevel);

        result = GeneratePyramid();
        if (!result)
        {
            return false;
        }

        StartCoroutine(DebugPyramid());

        return true;
    }

    private IEnumerator DebugPyramid()
    {
        GameObject cube;

        for (int i = 0; i < m_pyramidBlocks.Count; i++)
        {
            cube = (GameObject)Instantiate(DebugBlockPrefab);
            cube.transform.position = m_pyramidBlocks[i];
            cube.transform.localScale = Vector3.one * BlockScale;
            cube.name = m_pyramidBlocks[i].ToString();
            yield return new WaitForSeconds(0.1f);
        }
    }

    private bool GeneratePyramid()
    {
        int blockCount;
        float volume;

        if (PyramidHeight <= 0)
        {
            return false;
        }

        if (PyramidLength <= 0)
        {
            return false;
        }

        if (PyramidWidth <= 0)
        {
            return false;
        }
       
        volume = (PyramidHeight * PyramidLength * PyramidWidth) / 3;
        m_pyramidBlocks = new List<Vector3>(Mathf.RoundToInt(volume));

        blockCount = 0;
        for (int y = 0; y < PyramidHeight; y++)
        {
            for (int z = 0 + y; z < PyramidWidth - y; z++)
            {
                for (int x = 0 + y; x < PyramidLength - y; x++)
                {
                    m_pyramidBlocks.Add(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f) * BlockScale);
                    blockCount++;
                }
            }
        }

        Debug.Log("Block count: " + blockCount + ", Volume: " + volume);

        return true;
    }

    // Hope the m_pyramidBlocks array is sorted!
    private bool CheckPyramidBlocks(out int index)
    {
        RaycastHit hit;

        for (int i = 0; i < m_pyramidBlocks.Count; i++)
        {
            if (!Physics.BoxCast(m_pyramidBlocks[i], Vector3.one * 0.5f, Vector3.up, out hit, Quaternion.identity, 0.5f))
            {
                if (hit.transform.tag == PyramidTag)
                {
                    index = i;
                    return false;
                }
            }
        }

        index = -1;

        return true;
    }
}
