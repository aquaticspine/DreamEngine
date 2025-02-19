using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class CutSceneScript : MonoBehaviour
{
    //Variables
    ///public Object gameScene; This currently doesnt work when building, comes up as null
    public List<Texture2D> introImages;
    public GameObject cutsceneUI;
    public AudioSource audioSource;
    public AudioClip cutsceneMusic;
    public AudioClip pickClip;
    public int nextSceneNum = -1;

    public Image transitionFader;
    public GameObject loadingThrobber;

    private float fadeTimer = 1;
    private bool loaded = false;

    private int curScene = -1;

    //Startup stuff
    void Start()
    {
        transitionFader.gameObject.SetActive(true);
        audioSource.Play(0);

        curScene = 0;
        audioSource.clip = cutsceneMusic;
        audioSource.Play(0);
        cutsceneUI.SetActive(true);
        cutsceneUI.GetComponent<RawImage>().texture = introImages[curScene];
    }

    // Update is called once per frame
    void Update()
    {
        ProceedScene();

        if (loaded)
        {
            fadeTimer += Time.deltaTime;
            transitionFader.color = new Color(0, 0, 0, fadeTimer);
        }
        else if (fadeTimer > 0)
        {
            fadeTimer -= Time.deltaTime;
            transitionFader.color = new Color(0, 0, 0, fadeTimer);
        }

        if (curScene > 0)
        {
            cutsceneUI.gameObject.transform.Find("ProgressText").gameObject.SetActive(false);
            cutsceneUI.gameObject.transform.Find("SkipText").gameObject.SetActive(false);
        }
    }

    void ProceedScene()
    {
        if (Input.GetButtonDown("Jump") && curScene >= 0)
        {
            curScene++;
            if (curScene >= introImages.Count)
            {
                curScene = -1;
                if (nextSceneNum != -1)
                {
                    loadingThrobber.SetActive(true);
                    StartCoroutine(LoadAsyncScene());
                }
            }
            else
                cutsceneUI.GetComponent<RawImage>().texture = introImages[curScene];
        }
        if (Input.GetButtonDown("Dash"))
        {
            curScene = -1;
            if (nextSceneNum != -1)
            {
                loadingThrobber.SetActive(true);
                StartCoroutine(LoadAsyncScene());
            }
        }
    }

    //Load scene (asynchronous)
    IEnumerator LoadAsyncScene()
    {
        AsyncOperation asyncLoad =
            SceneManager.LoadSceneAsync(nextSceneNum);
        asyncLoad.allowSceneActivation = false;

        //Wait until scene fully loads
        while (!asyncLoad.isDone)
        {
            if (asyncLoad.progress >= 0.9f)
            {
                if (asyncLoad.allowSceneActivation == false)
                {
                    loaded = true;
                }
            }
            //Allow loading when faded
            if (transitionFader.color.a >= 1)
                asyncLoad.allowSceneActivation = true;
            yield return null;
        }

        //AsyncOperation asyncLoad =
        //    SceneManager.LoadSceneAsync(nextSceneNum);

        ////Wait until scene fully loads
        //while (!asyncLoad.isDone)
        //{
        //    yield return null;
        //}
    }
}
