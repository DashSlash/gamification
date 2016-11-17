using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;
using UnityEngine.Networking;

public class GameManager : MonoBehaviour {
    public float turnDelay = 0.1f;
    public float levelStartDelay = 2f;
    public static GameManager instance = null;
    public BorderManager boardScript;
    public int[] points;
    public Sprite[] beautyimage;
    public Sprite[] scaryimage;
    public bool showscary = false;
    public float imageshowtime = 0.5f;
    public float lettertime = 0.1f;


    string[] story_text = { "!!! Where am I!!!" + Environment.NewLine + "It's so dark here! Let Me Out!   " + Environment.NewLine
            + Environment.NewLine + "wasd control move, enter key restart.    " + Environment.NewLine + "Watch out your food. Dont Starve!   ",
        "Life is tough here. " + Environment.NewLine + "Can I get out here?",
        "I dont want to die!!!",
        "Go for it! Exit is ahead!" };
    [HideInInspector] public bool playerTurn = true;
    [HideInInspector] public bool enemiesMoving = false;

    private List<Enemy> enemies;
    private List<Light> lights;
    private Text levelText;
    public int level = 1;
    private GameObject levelImage;
    private bool doingSetup;
    private bool isGameOver;

	// Use this for initialization
	void Awake () {
        //Debug.Log("Awake");
        if (instance == null)
        {
            instance = this;
        }
        else if(instance != null && this != instance)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
        Initialize();
        boardScript = GetComponent<BorderManager>();
        //InitGame();
	}

    void Initialize()
    {
        points = new int[2] { 100, 100 };
        enemies = new List<Enemy>();
        isGameOver = false;
        level = 1;
    }

    //This is called each time a scene is loaded.
    void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        
        //Call InitGame to initialize our level.
        InitGame();
    }
    void OnEnable()
    {
        //Tell our ‘OnLevelFinishedLoading’ function to start listening for a scene change event as soon as this script is enabled.
        //Debug.Log("En");
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
    }
    void OnDisable()
    {
        //Tell our ‘OnLevelFinishedLoading’ function to stop listening for a scene change event as soon as this script is disabled.
        //Remember to always have an unsubscription for every delegate you subscribe to!
        //Debug.Log("Dn");
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }

    IEnumerator AnimateText(string strComplete)
    {
        levelText.enabled = false;
        //yield return new WaitForSeconds(imageshowtime);
        if (levelImage.GetComponent<Image>() != null)
        {
            levelImage.GetComponent<Image>().sprite = null;
            levelImage.GetComponent<Image>().color = Color.black;
        }
        levelImage.SetActive(true);
        levelText.enabled = true;


        int i = 0;
        string str = "";
        while (i < strComplete.Length)
        {
            str += strComplete[i++];
            levelText.text = str;
            yield return new WaitForSeconds(lettertime);
        }
        //yield return new WaitForSeconds(0.2f);
    }

    void InitGame()
    {
        //Debug.Log("Init");
        enemies.Clear();
        doingSetup = true;
        levelImage = GameObject.Find("LevelImage");
        levelText = GameObject.Find("LevelText").GetComponent<Text>();
        levelText.text = string.Empty;
        //levelText.text = "Day: " + level;

        //Debug.Log(story_text[0]);
        if (level % 5 == 0 && levelImage.GetComponent<Image>() != null)
        {
            System.Random a = new System.Random();
            levelImage.GetComponent<Image>().sprite = beautyimage[a.Next(beautyimage.Length)];
            levelImage.GetComponent<Image>().color = Color.white;
            levelText.enabled = false;
            levelText.text = story_text[Math.Min(level / 5, story_text.Length - 1)];
            levelImage.SetActive(true);
        }
        else if(levelImage.GetComponent<Image>() != null)
        {
            levelImage.GetComponent<Image>().sprite = null;
            levelImage.GetComponent<Image>().color = Color.black;
            levelImage.SetActive(true);
            if (level == 1)
            {
                levelText.text = story_text[0];
            }
            else
            {
                levelText.text = "Day: " + level;
            }
            levelText.enabled = false;
        }
        Invoke("ShowLevelText", 1f);
        Invoke("HideLevelImage", 2f + lettertime* levelText.text.Length);

        boardScript.row = 10 + level;
        boardScript.col = 10 + level;
        boardScript.wallCount = new Count((8 + level*2)/2, 8 + level*2);
        boardScript.foodCount = new Count((8 + level) / 2, 8 + level);
        boardScript.lightCount = new Count((8 + level) / 2, 8 + level);
        Camera camera = GameObject.Find("Main Camera").GetComponent<Camera>();
        camera.transform.position = new Vector3(boardScript.row/2, boardScript.col/2, -10);
        camera.orthographicSize = boardScript.row/2 + 1;

        boardScript.SetupScene(level);
            
    }

    void ShowLevelText()
    {
        if(levelImage.GetComponent<Image>() != null)
        {
            levelImage.GetComponent<Image>().sprite = null;
            levelImage.GetComponent<Image>().color = Color.black;
        }
        levelImage.SetActive(true);
        levelText.enabled = true;
        if (level % 5 == 0)
            StartCoroutine(AnimateText(levelText.text)); 
        else
            StartCoroutine(AnimateText(levelText.text));
    }

    void HideLevelImage()
    {
        levelImage.SetActive(false);
        doingSetup = false;
    }

    public static double Distance(GameObject a, GameObject b)
    {
        if (a != null && b != null)
            return Math.Abs(a.transform.position.x - b.transform.position.x) + Math.Abs(a.transform.position.y - b.transform.position.y);
        else
            return int.MaxValue;
    }

    void SendWebData()
    {
        StartCoroutine(UploadData());
    }

    // future group members need to edit this function to send the data to server in a desired way
    IEnumerator UploadData()
    {
        //yield return null;
        WWWForm form = new WWWForm();
        form.AddField("Player-Survival-Days", level);

        UnityWebRequest client = UnityWebRequest.Post("http://ccain.eecs.wsu.edu/", form);
        yield return client.Send();
        if (client.isError)
        {
            Debug.Log(client.error);
        }
        else
        {
            Debug.Log("Form upload complete!");
        }

    }

    public void GameOver()
    {
        if (showscary)
        {
            if (levelImage.GetComponent<Image>() != null)
            {
                System.Random a = new System.Random();
                levelImage.GetComponent<Image>().sprite = scaryimage[a.Next(scaryimage.Length)];
                levelImage.GetComponent<Image>().color = Color.white;
                levelText.enabled = false;
                levelImage.SetActive(true);
                if (GameObject.Find("Player0") != null)
                    GameObject.Find("Player0").SetActive(false);
            }
            isGameOver = true;
            // send server the user's content
            SendWebData();
            return;
        }
        levelImage.SetActive(true);
        levelText.text = "After " + level + " Days," + Environment.NewLine + " you starved.";
        // send server the user's content
        SendWebData();

        //enabled = false;
        isGameOver = true;
    }
    // Update is called once per frame
    void Update () {
        if(isGameOver)
        {
            if (Input.GetKey("enter") || Input.GetKey("return"))
            {
                SceneManager.LoadScene(0);
                Initialize();
                showscary = false;
                SoundManager.instance.back.Play();
                InitGame();
            }
            else
                return;
        }
        else if(playerTurn || enemiesMoving || doingSetup)
        {
            return;
        }
        StartCoroutine(MoveEnemy());
	}

    public void AddEnemyToList(Enemy emy)
    {
        enemies.Add(emy);
    }

    IEnumerator MoveEnemy()
    {
        enemiesMoving = true;
        yield return new WaitForSeconds(turnDelay);

        if(enemies.Count == 0)
        {
            yield return new WaitForSeconds(turnDelay);
        }
        for (int i = 0; i< enemies.Count; i++)
        {
            enemies[i].MoveEnemy();
            //yield return new WaitForSeconds(enemies[i].moveTime);//enemies.Count * 
        }
        yield return new WaitForSeconds( 0.3f);
        //Debug.Log("Enemy num: " + enemies.Count);
        playerTurn = true;
        enemiesMoving = false;
    }

    internal void UpdatePlayer(GameObject gameobj)
    {
        boardScript.UpdatePlayer(gameobj);
    }
}
