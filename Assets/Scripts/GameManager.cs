using UnityEngine;

public class GameManager : MonoBehaviour
{
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

    private bool Initialize()
    {
        return true;
    }
	
	void Update ()
    {
	
	}
}
