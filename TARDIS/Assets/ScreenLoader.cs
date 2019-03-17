using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio;

public class ScreenLoader : MonoBehaviour
{
    public Transform LoadingBar;
    public Image screen;
    [SerializeField] private float currentAmount = 0;
    [SerializeField] private float speed = 17.0f;

    void Update()
    {
        if (currentAmount < 100)
        {
            currentAmount += speed * Time.deltaTime;
            Debug.Log((int)currentAmount);
        }
        else
        {
            screen.CrossFadeColor(Color.black, 1.0f, true, true);
            SceneManager.LoadScene("AppName");
        }

        LoadingBar.GetComponent<Image>().fillAmount = currentAmount / 100;
    }

}
