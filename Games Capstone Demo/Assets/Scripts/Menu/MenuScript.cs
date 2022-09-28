using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class MenuScript : MonoBehaviour
{
	//Variables
	///public Object gameScene; This currently doesnt work when building, comes up as null
	public List<Texture2D> introImages;
	public GameObject cutsceneUI;
	public AudioSource audioSource;
	public AudioClip cutsceneMusic;
    public AudioClip pickClip;
    public Button playBtn;
    public Button quitBtn;
    public Image transitionFader;
    public GameObject loadingNum;
    public GameObject loadingText;
    public GameObject btnHighlight;
    public bool enableQuitBtn = false;

    private int curScene = -1;
    private float fadeTimer = 0;
    private bool loaded = false;

    // button select varaibles
    private int numberOfOptions = 2;
	private int selectedOption;
	private bool inMenu = true;

	private float inputTimer = 0;
	[SerializeField]
	private float waitTime = 0.3f;

	//Startup stuff
	void Start()
    {
		audioSource.Play(0);

		selectedOption = 1;
        btnHighlight.transform.position =
                    playBtn.transform.position;
        btnHighlight.SetActive(true);

        if(!enableQuitBtn) {
            GameObject quitButton = GameObject.FindWithTag("QuitBtn");

            quitButton.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
	{
        SwapMenu();
        if (loaded)
        {
            fadeTimer += Time.deltaTime;
            transitionFader.color = new Color(0, 0, 0, fadeTimer);
        }
        if (curScene > 0)
        {
            cutsceneUI.gameObject.transform.Find("ProgressText").gameObject.SetActive(false);
            cutsceneUI.gameObject.transform.Find("SkipText").gameObject.SetActive(false);
        }
	}

    void SwapMenu()
    {
        if (inMenu)
        {
            inputTimer += Time.unscaledDeltaTime;

            if (inputTimer >= waitTime)
            {
                // select options code
                if (Input.GetAxisRaw("Swap") > 0)
                {
                    selectedOption += 1;

                    if (enableQuitBtn)
                    {
                        if (selectedOption > numberOfOptions)
                        {
                            selectedOption = 1;
                        }
                    }
                    else
                    {
                        if (selectedOption > (numberOfOptions - 1))
                        {
                            selectedOption = 1;
                        }
                    }

                    // reset selected highlight
                    SwapSelected(selectedOption);
                }

                if (Input.GetAxisRaw("Swap") < 0)
                {
                    selectedOption -= 1;

                    if (selectedOption < 1)
                    {
                        if (enableQuitBtn)
                        {
                            selectedOption = numberOfOptions;
                        }
                        else
                        {
                            selectedOption = (numberOfOptions - 1);
                        }
                    }

                    // reset selected highlight
                    SwapSelected(selectedOption);
                }

                if (Input.GetButton("Jump") ||
                    Input.GetButton("Enter"))
                {
                    // quit button needs to be last in the switch case
                    switch (selectedOption)
                    {
                        case 1:
                            OnPButtonPress();
                            break;
                        case 2:
                            OnQButtonPress();
                            break;
                    }
                }
            }
        }
        else
        {
            //Progress when jump is pressed, loading when hitting the end of the images
            if (Input.GetButtonDown("Jump") && curScene >= 0)
            {
                curScene++;
                if (curScene >= introImages.Count)
                {
                    curScene = -1;
                    StartCoroutine(LoadAsyncScene());
                }
                else
                    cutsceneUI.GetComponent<RawImage>().texture = introImages[curScene];
            }
            if (Input.GetButtonDown("Dash"))
            {
                curScene = -1;
                StartCoroutine(LoadAsyncScene());
            }
        }
    }

	void SwapSelected(int option)
	{
		// reset selected highlight

		Debug.Log("Picked: " + selectedOption);
		switch (selectedOption)
		{
			case 1:
                btnHighlight.transform.position =
                    playBtn.transform.position;
                playBtn.Select();
                break;
			case 2:
                btnHighlight.transform.position =
                    quitBtn.transform.position;
                quitBtn.Select();
                break;
		}
        audioSource.PlayOneShot(pickClip);
		inputTimer = 0;
	}

	//Play Button pressed
	public void OnPButtonPress()
	{
        curScene = 0;
		audioSource.clip = cutsceneMusic;
		audioSource.Play(0);
		cutsceneUI.SetActive(true);
		inMenu = false;
        btnHighlight.SetActive(false);
    }

	//Quit Button pressed
	public void OnQButtonPress()
	{
		inMenu = false;
        btnHighlight.SetActive(false);
        Application.Quit();
	}

	//Load scene (asynchronous)
	IEnumerator LoadAsyncScene()
    {
        cutsceneUI.gameObject.transform.Find("LoadingTextH").gameObject.SetActive(true);
        AsyncOperation asyncLoad = 
			SceneManager.LoadSceneAsync(1);
        asyncLoad.allowSceneActivation = false;
		//Wait until scene fully loads
		while (!asyncLoad.isDone)
		{
            //Whatever happened to just getting Text.text? seriously this is dumb
            loadingNum.GetComponent<TextMeshProUGUI>().GetComponent<TMP_Text>().text = Mathf.Round((asyncLoad.progress * 100)) + "%";
            loadingNum.gameObject.transform.Find("LoadingNum").GetComponent<TextMeshProUGUI>().GetComponent<TMP_Text>().text = loadingNum.GetComponent<TextMeshProUGUI>().GetComponent<TMP_Text>().text;
            if(asyncLoad.progress >= 0.9f)
            {
                loadingNum.gameObject.SetActive(false);
                loadingText.GetComponent<TextMeshProUGUI>().GetComponent<TMP_Text>().text = "Done!";
                loadingText.gameObject.transform.Find("LoadingText").GetComponent<TextMeshProUGUI>().GetComponent<TMP_Text>().text = "Done!";
                if (asyncLoad.allowSceneActivation == false)
                {
                    loaded = true;
                    asyncLoad.allowSceneActivation = true;
                }
            }
            yield return null;
		}
    }

    // Mouse over Play button
    public void MOPlayBtn()
    {
        selectedOption = 1;
        SwapSelected(selectedOption);
    }

    // Mouse over Quit button
    public void MOQuitBtn()
    {
        selectedOption = 2;
        SwapSelected(selectedOption);
    }
}
