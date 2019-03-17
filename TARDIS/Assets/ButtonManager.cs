using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour {

public void toAR()
    {
        StartCoroutine(changescene());
    }
    IEnumerator changescene()
    {
        yield return new WaitForSeconds(3);
        SceneManager.LoadScene("AR");
    }
}
