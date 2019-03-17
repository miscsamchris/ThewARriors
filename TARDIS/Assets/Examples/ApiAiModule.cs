//
// API.AI Unity SDK Sample
// =================================================
//
// Copyright (C) 2015 by Speaktoit, Inc. (https://www.speaktoit.com)
// https://www.api.ai
//
// ***********************************************************************************************************************
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with
// the License. You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on
// an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the
// specific language governing permissions and limitations under the License.
//
// ***********************************************************************************************************************

using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Collections;
using System.Reflection;
using ApiAiSDK;
using ApiAiSDK.Model;
using ApiAiSDK.Unity;
using Newtonsoft.Json;
using System.Net;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class ApiAiModule : MonoBehaviour
{

    private ApiAiUnity apiAiUnity;
    private AudioSource aud;
    private TextToSpeechDemo t;

    private readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings
    {
        NullValueHandling = NullValueHandling.Ignore,
    };

    private readonly Queue<Action> ExecuteOnMainThread = new Queue<Action>();
    public IEnumerator newpage()
    {
        yield return new WaitForSeconds(2);

        SceneManager.LoadScene("AROP");

    }
    // Use this for initialization
    IEnumerator Start()
    {
        // check access to the Microphone
        yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);
        if (!Application.HasUserAuthorization(UserAuthorization.Microphone))
        {
            throw new NotSupportedException("Microphone using not authorized");
        }

        ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) =>
        {
            return true;
        };

        const string ACCESS_TOKEN = "420982a3aa344adcb44ed8a6c883bc61  ";

        var config = new AIConfiguration(ACCESS_TOKEN, SupportedLanguage.English);

        apiAiUnity = new ApiAiUnity();
        apiAiUnity.Initialize(config);

        apiAiUnity.OnError += HandleOnError;
        apiAiUnity.OnResult += HandleOnResult;
        t = transform.GetComponent<TextToSpeechDemo>();
    }

    void HandleOnResult(object sender, AIResponseEventArgs e)
    {
        RunInMainThread(() => {
            var aiResponse = e.Response;
            if (aiResponse != null)
            {
                Debug.Log(aiResponse.Result.ResolvedQuery);
                var outText = aiResponse.Result.Fulfillment.Speech;
                if (outText == "OBJECT PLACEMENT")
                {
                    if (SceneManager.GetActiveScene().name!="AROP")
                    {
                        StartCoroutine(newpage());
                    }
                }
                if (outText.Substring(outText.IndexOf(' ')) == "ENTER")
                {
                    var r = new System.Random();
                    PlayerPrefs.SetString("LID", r.Next().ToString());
                    StartCoroutine(Upload(PlayerPrefs.GetString("LID","001"), outText.Split(' ')[1], outText.Split(' ')[2], outText.Split(' ')[3], outText.Split(' ')[4]));
                }
                if (outText.Substring(outText.IndexOf(' ')) == "INFO")
                {
                    WWW req = new WWW("http://192.168.43.164:5000/getinfo/" + outText.Substring(outText.IndexOf(' ') + 1) + "/");
                    StartCoroutine(request(req));
                }
                if (outText.Substring(outText.IndexOf(' ')) == "BUY")
                {
                    WWW req = new WWW("http://192.168.43.164:5000/buy/" + "Order for "+outText.Substring(outText.IndexOf(' ') + 1) + "/"+PlayerPrefs.GetString("LID","001")+"/");
                    StartCoroutine(request(req));
                }
                if (outText == "HELP")
                {
                    WWW req = new WWW("http://192.168.43.164:5000/buy/" + "Need Help. Interior Decorator wanted. " + "/" + PlayerPrefs.GetString("LID", "001") + "/");
                    StartCoroutine(request(req));
                }
                else
                {
                    t.SpeakOut(outText);
                }

            }
            else
            {
                Debug.LogError("Response is null");
            }
        });
    }
    public IEnumerator request(WWW req)
    {
        yield return req;
        if (req.text.Length >= 5)
        {
            t.SpeakOut(req.text);
        }
    }
    IEnumerator Upload(string ID, string NAME, string AGE, string PHONE, string EMAIL)
    {
        WWWForm form = new WWWForm();

        form.AddField("ID", ID);
        form.AddField("NAME", NAME);
        form.AddField("AGE", AGE);
        form.AddField("PHONE", PHONE);
        form.AddField("EMAIL", EMAIL);

        UnityWebRequest www = UnityWebRequest.Post("http://192.168.43.164:5000/upload-inquiry/", form);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            t.SpeakOut("Saved to Database");
        }
    }
    void HandleOnError(object sender, AIErrorEventArgs e)
    {
        RunInMainThread(() => {
            Debug.LogException(e.Exception);
            Debug.Log(e.ToString());
        });
    }

    // Update is called once per frame
    void Update()
    {
        if (apiAiUnity != null)
        {
            apiAiUnity.Update();
        }

        // dispatch stuff on main thread
        while (ExecuteOnMainThread.Count > 0)
        {
            ExecuteOnMainThread.Dequeue().Invoke();
        }
    }

    private void RunInMainThread(Action action)
    {
        ExecuteOnMainThread.Enqueue(action);
    }

    public void PluginInit()
    {

    }

    public void StartListening()
    {
        Debug.Log("StartListening");



        aud = GetComponent<AudioSource>();
        apiAiUnity.StartListening(aud);

    }

    public void StopListening()
    {
        try
        {
            Debug.Log("StopListening");


            apiAiUnity.StopListening();
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    public void SendText()
    {

        Debug.Log("");

        AIResponse response = apiAiUnity.TextRequest("");

        if (response != null)
        {
            Debug.Log("Resolved query: " + response.Result.ResolvedQuery);
            var outText = JsonConvert.SerializeObject(response, jsonSettings);

            Debug.Log("Result: " + outText);

        }
        else
        {
            Debug.LogError("Response is null");
        }

    }

    public void StartNativeRecognition()
    {
        try
        {
            t.utilsPlugin.UnMuteBeep();
            t.Stop();
            apiAiUnity.StartNativeRecognition();
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }
}
