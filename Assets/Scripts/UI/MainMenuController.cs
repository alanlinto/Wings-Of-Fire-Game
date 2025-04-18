using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private string seedText;
    public static int seed { get; set; }

    public void PlayGame() {
        GameObject qButton = GameObject.Find("QuitButton");
        GameObject sButton = GameObject.Find("PlayButton");
        qButton.gameObject.SetActive(false);
        sButton.gameObject.SetActive(false);
    }

    public void GenerateWorld() {
        seed = seedText != "" ? int.Parse(seedText) : new System.Random().Next();
        SceneManager.LoadScene("MainScene");
    }

    public void QuitGame() {
        Application.Quit();
    }

    public void HandleInputChange(InputField seedTextInputField) {
        seedText = seedTextInputField.text;
    }

}
