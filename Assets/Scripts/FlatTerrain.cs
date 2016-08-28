using UnityEngine;

public class FlatTerrain : MonoBehaviour
{
    [SerializeField]
    private bool perlin = true;
    [SerializeField][Range(0.00001f, 1)]
    private float perlinScale = 1;
    [SerializeField]
    private string filename = "";
    [SerializeField]
    private float m_heightScale = 300;
    [SerializeField]
    private int m_terrainHeight = 1025;
    [SerializeField]
    private int m_terrainWidth = 1025;
    [SerializeField]
    private Texture2D colorMap;

    private int m_vertexCount;
    private HeightMapPoint[] m_heightMap;
    private TerrainPoint[] m_terrainPointData;
    private MeshFilter m_meshFilter;
    private MeshCollider m_meshCollider;
    private static FlatTerrain m_instance;

    public static FlatTerrain Instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = FindObjectOfType<FlatTerrain>();
            }

            return m_instance;
        }
    }

    private void Start()
    {
        bool result;
        result = Initialize();
        if (!result)
        {
            Debug.LogError("Failed to initialize Terrain.");
            Debug.Break();
        }
    }

    private void Update()
    {
        bool result;

        if (Input.GetKeyUp(KeyCode.F2))
        {
            result = Initialize();
            if (!result)
            {
                Debug.LogError("Failed to initialize Terrain.");
                Debug.Break();
            }
        }
    }

    private bool Initialize()
    {
        bool result;

        // Initialize the terrain height map with the data from a generated perlin noise.
        if (perlin)
        {
            result = LoadPerlinHeightmap();
            if (!result)
            {
                return false;
            }
        }
        else
        {
            // Initialize the terrain height map with the data from the raw file.
            result = LoadRawHeightMap(filename);
            if (!result)
            {
                return false;
            }
        }

        // Setup the X and Z coordinates for the height map as well as scale the terrain height by the height scale value.
        SetTerrainCoordinates();

        if (perlin)
        {
            // Load in a random color map for the terrain.
            result = LoadRandomColorMap();
            if (!result)
            {
                return false;
            }
        }
        else
        {
            // Load in the color map for the terrain.
            result = LoadColorMap(colorMap);
            if (!result)
            {
                return false;
            }
        }

        // Now build the 3D model data of the terrain.
        result = BuildTerrainModel();
        if (!result)
        {
            Debug.LogError("Failed to build the 3D model data of the terrain.");
            return false;
        }

        // Create a mesh out of the model data
        result = BuildTerrainMesh();
        if (!result)
        {
            Debug.LogError("Failed to build the 3D mesh of the terrain.");
            return false;
        }

        return true;
    }

    private bool LoadRawHeightMap(string filename)
    {
        int i, j, index;

        // Create the array to hold the height map data.
        m_heightMap = new HeightMapPoint[m_terrainWidth * m_terrainHeight];
        if (m_heightMap == null)
        {
            return false;
        }

        // Make sure the file name is specified.
        if (string.IsNullOrEmpty(filename))
        {
            Debug.LogError("Filename must be specified.");
            return false;
        }

        // Check if the map file exists
        if (!System.IO.File.Exists(Application.streamingAssetsPath + "/" + filename))
        {
            Debug.LogError("File located at " + Application.streamingAssetsPath + "/" + filename + " doesnt exist.");
            return false;
        }

        // Copy the image data into the height map array.
        using (var file = System.IO.File.OpenRead(Application.streamingAssetsPath + "/" + filename))
        using (var reader = new System.IO.BinaryReader(file))
        {
            for (j = 0; j < m_terrainHeight; j++)
            {
                for (i = 0; i < m_terrainWidth; i++)
                {
                    index = (m_terrainWidth * j) + i;

                    // Store the height at this point in the height map array.
                    m_heightMap[index].y = (float)reader.ReadUInt16() / 0xFFFF;
                }
            }
        }

        return true;
    }

    private bool LoadPerlinHeightmap()
    {
        int i, j, index;

        // Create the array to hold the height map data.
        m_heightMap = new HeightMapPoint[m_terrainWidth * m_terrainHeight];
        if (m_heightMap == null)
        {
            return false;
        }

        for (j = 0; j < m_terrainHeight; j++)
        {
            for (i = 0; i < m_terrainWidth; i++)
            {
                index = (m_terrainWidth * j) + i;

                // Store the height at this point in the height map array.
                m_heightMap[index].y = Mathf.PerlinNoise(i * perlinScale, j * perlinScale);
                if (m_heightMap[index].y < 0.5)
                {
                    m_heightMap[index].y = 0.5f;
                }
            }
        }

        return true;
    }

    private void SetTerrainCoordinates()
    {
        int i, j, index;

        // Loop through all the elements in the height map array and adjust their coordinates correctly.
        for (j = 0; j < m_terrainHeight; j++)
        {
            for (i = 0; i < m_terrainWidth; i++)
            {
                index = (m_terrainWidth * j) + i;

                // Set the X and Z coordinates.
                m_heightMap[index].x = i;
                m_heightMap[index].z = -j;

                // Move the terrain depth into the positive range.  For example from (0, -256) to (256, 0).
                m_heightMap[index].z += (m_terrainHeight - 1);

                // Scale the height.
                m_heightMap[index].y *= m_heightScale;
            }
        }

        return;
    }

    private bool LoadColorMap(Texture2D bitmap)
    {
        int imageSize, i, j, index;
        int count;
        int bitmapHeight, bitmapWidth;
        Color[] bitmapImage;

        if (bitmap == null)
        {
            Debug.LogError("A colormap must be assigned in the inspector");
            return false;
        }

        bitmapHeight = bitmap.height;
        bitmapWidth = bitmap.width;

        // Make sure the height map dimensions are the same as the terrain dimensions for easy 1 to 1 mapping.
        if ((bitmapHeight != m_terrainHeight - 1) || (bitmapWidth != m_terrainWidth - 1))
        {
            Debug.LogError("Make sure the color map dimensions are the same as the terrain dimensions for easy 1 to 1 mapping.");
            return false;
        }

        // Calculate the size of the bitmap image data.  
        // Since we use non-divide by 2 dimensions (eg. 257x257) we need to add an extra byte to each line.
        imageSize = (m_terrainHeight - 1) * (m_terrainWidth - 1);

        // Read in the bitmap image data.
        bitmapImage = bitmap.GetPixels();

        count = bitmapImage.Length;
        if (count != imageSize)
        {
            Debug.LogError("Failed to read in the bitmap image data.");
            return false;
        }

        // Read the image data into the height map array.
        for (j = 0; j < bitmapHeight; j++)
        {
            for (i = 0; i < bitmapWidth; i++)
            {
                // Bitmaps are upside down so load bottom to top into the height map array.
                index = (j * bitmapWidth) + i;

                m_heightMap[index].b = bitmapImage[index].b;
                m_heightMap[index].g = bitmapImage[index].g;
                m_heightMap[index].r = bitmapImage[index].r;
            }
        }

        // Release the bitmap image data now that the height map array has been loaded.
        bitmapImage = null;

        // Release the terrain filename now that is has been read in.
        bitmap = null;

        return true;
    }

    private bool LoadRandomColorMap()
    {
        int i, j, index;
        int bitmapHeight, bitmapWidth;
        Color[] colors;

        bitmapHeight = m_terrainHeight;
        bitmapWidth = m_terrainWidth;

        colors = new Color[bitmapHeight * bitmapWidth];
        if (colors.Length == 0)
        {
            return false;
        }

        // Read the image data into the height map array.
        for (j = 0; j < bitmapHeight; j++)
        {
            for (i = 0; i < bitmapWidth; i++)
            {
                index = (j * bitmapWidth) + i;

                colors[index] = Color.white;

                m_heightMap[index].b = colors[index].b;
                m_heightMap[index].g = colors[index].g;
                m_heightMap[index].r = colors[index].r;
            }
        }

        return true;
    }

    private bool BuildTerrainModel()
    {
        int i, j, index, index1, index2, index3, index4;

        // Calculate the number of vertices in the 3D terrain model.
        m_vertexCount = (m_terrainHeight - 1) * (m_terrainWidth - 1) * 6;

        // Create the 3D terrain model array.
        m_terrainPointData = new TerrainPoint[m_vertexCount];
        if (m_terrainPointData.Length == 0)
        {
            return false;
        }

        // Initialize the index into the height map array.
        index = 0;

        // Load the 3D terrain model with the height map terrain data.
        // We will be creating 2 triangles for each of the four points in a quad.
        for (j = 0; j < (m_terrainHeight - 1); j++)
        {
            for (i = 0; i < (m_terrainWidth - 1); i++)
            {
                // Get the indexes to the four points of the quad.
                index1 = (m_terrainWidth * j) + i;          // Upper left.
                index2 = (m_terrainWidth * j) + (i + 1);      // Upper right.
                index3 = (m_terrainWidth * (j + 1)) + i;      // Bottom left.
                index4 = (m_terrainWidth * (j + 1)) + (i + 1);  // Bottom right.

                // Now create two triangles for that quad.
                // Triangle 1 - Upper left.
                m_terrainPointData[index].x = m_heightMap[index1].x;
                m_terrainPointData[index].y = m_heightMap[index1].y;
                m_terrainPointData[index].z = m_heightMap[index1].z;
                m_terrainPointData[index].r = m_heightMap[index1].r;
                m_terrainPointData[index].g = m_heightMap[index1].g;
                m_terrainPointData[index].b = m_heightMap[index1].b;
                index++;

                // Triangle 1 - Upper right.
                m_terrainPointData[index].x = m_heightMap[index2].x;
                m_terrainPointData[index].y = m_heightMap[index2].y;
                m_terrainPointData[index].z = m_heightMap[index2].z;
                m_terrainPointData[index].r = m_heightMap[index2].r;
                m_terrainPointData[index].g = m_heightMap[index2].g;
                m_terrainPointData[index].b = m_heightMap[index2].b;
                index++;

                // Triangle 1 - Bottom left.
                m_terrainPointData[index].x = m_heightMap[index3].x;
                m_terrainPointData[index].y = m_heightMap[index3].y;
                m_terrainPointData[index].z = m_heightMap[index3].z;
                m_terrainPointData[index].r = m_heightMap[index3].r;
                m_terrainPointData[index].g = m_heightMap[index3].g;
                m_terrainPointData[index].b = m_heightMap[index3].b;
                index++;

                // Triangle 2 - Bottom left.
                m_terrainPointData[index].x = m_heightMap[index3].x;
                m_terrainPointData[index].y = m_heightMap[index3].y;
                m_terrainPointData[index].z = m_heightMap[index3].z;
                m_terrainPointData[index].r = m_heightMap[index3].r;
                m_terrainPointData[index].g = m_heightMap[index3].g;
                m_terrainPointData[index].b = m_heightMap[index3].b;
                index++;

                // Triangle 2 - Upper right.
                m_terrainPointData[index].x = m_heightMap[index2].x;
                m_terrainPointData[index].y = m_heightMap[index2].y;
                m_terrainPointData[index].z = m_heightMap[index2].z;
                m_terrainPointData[index].r = m_heightMap[index2].r;
                m_terrainPointData[index].g = m_heightMap[index2].g;
                m_terrainPointData[index].b = m_heightMap[index2].b;
                index++;

                // Triangle 2 - Bottom right.
                m_terrainPointData[index].x = m_heightMap[index4].x;
                m_terrainPointData[index].y = m_heightMap[index4].y;
                m_terrainPointData[index].z = m_heightMap[index4].z;
                m_terrainPointData[index].r = m_heightMap[index4].r;
                m_terrainPointData[index].g = m_heightMap[index4].g;
                m_terrainPointData[index].b = m_heightMap[index4].b;
                index++;
            }
        }

        return true;
    }

    private bool BuildTerrainMesh()
    {
        int index;
        Vector3[] vertices;
        int[] indices;
        Color[] colors;
        Mesh mesh;

        m_meshFilter = GetComponent<MeshFilter>();
        if (m_meshFilter == null)
        {
            return false;
        }

        m_meshCollider = GetComponent<MeshCollider>();
        if (m_meshCollider == null)
        {
            return false;
        }

        vertices = new Vector3[m_vertexCount];
        if (vertices.Length == 0)
        {
            return false;
        }

        indices = new int[vertices.Length];
        if (indices.Length == 0)
        {
            return false;
        }

        colors = new Color[vertices.Length];
        if (colors.Length == 0)
        {
            return false;
        }

        index = 0;

        for (int i = 0; i < m_vertexCount; i++)
        {
            var data = m_terrainPointData[i];

            vertices[index] = new Vector3(data.x, data.y, data.z);
            indices[index] = i;
            colors[index] = new Color(data.r, data.g, data.b);

            index++;
        }

        mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.SetIndices(indices, MeshTopology.Triangles, 0);
        mesh.colors = colors;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        m_meshFilter.sharedMesh = mesh;
        m_meshCollider.sharedMesh = mesh;

        return true;
    }

    public Vector3 GetTerrainSize()
    {
        return m_meshFilter.sharedMesh.bounds.size * transform.localScale.x;
    }
}

public struct TerrainPoint
{
    public float x, y, z;
    public float r, g, b;
}

public struct HeightMapPoint
{
    public float x, y, z;
    public float r, g, b;
}