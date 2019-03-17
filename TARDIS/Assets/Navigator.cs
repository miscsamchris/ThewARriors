using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Navigator : MonoBehaviour {

    string next;
    public Image screen,image;
    void Start()
    {
        next = "MainMenu";
        var a = image.color;
        a.a = 1f;
        image.color = a;
        image.CrossFadeAlpha(0f, 0f, true);
        StartCoroutine(change_scene(next));
    }

    IEnumerator change_scene(string s)
    {
        image.CrossFadeAlpha(1, 3.0f, false);
        yield return new WaitForSeconds(3);
        screen.CrossFadeColor(Color.black, 0.7f, true, true);
        image.CrossFadeColor(Color.black, 0.7f, true, true);
        yield return new WaitForSeconds(2);
        SceneManager.LoadScene(s);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}
