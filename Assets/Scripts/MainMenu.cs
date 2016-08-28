using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Text Title;
    public Text HighScore;

    public string description;
    void Start()
    {
        HighScore.text = "Highest Level: " + PlayerPrefs.GetInt("HighScore", 1);

        StartCoroutine(StartGame());
    }

    private IEnumerator StartGame()
    {
        yield return new WaitForSeconds(2);
        Title.text = description;
        yield return new WaitForSeconds(3);
        Title.text = "Have fun!";
        yield return new WaitForSeconds(1);
        Title.text = "Level " + PlayerPrefs.GetInt("Level", 1);
        yield return new WaitForSeconds(1);
        SceneManager.LoadScene(1);
    }
}
