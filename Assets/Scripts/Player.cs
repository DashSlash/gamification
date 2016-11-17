using UnityEngine;
using System.Collections;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;

public class Player : MovingObj
{
    public int wallDamage = 1;
    private int pointPerFood = 10;
    private int pointPerSoda = 20;
    public float restartLevelDelay = 1f;
    public int playerNumber = 0;
    public string[] right_arr = { "d", "right"};
    public string[] left_arr = { "a", "left" };
    public string[] up_arr = { "w", "up" };
    public string[] down_arr = { "s", "down" };
    public Text foodText;
    public Sprite lighton;
    public Sprite lightoff;
    public int fooddistance = 6;
    public static Player instance = null;
    public float playerTime = 0.1f;

    public AudioClip moveSound1;
    public AudioClip moveSound2;
    public AudioClip eatSound1;
    public AudioClip eatSound2;
    public AudioClip drinkSound1;
    public AudioClip drinkSound2;
    public AudioClip gameOverSound;

    //private List<string> inputstring = new List<string>();
    private Dictionary<string,int> input_count= new Dictionary<string, int>();
    private Animator animator;
    private int food;
    
    
    protected override void Start()
    {
        animator = GetComponent<Animator>();
        food = GameManager.instance.points[playerNumber];
        foodText = GameObject.Find("FoodText" + playerNumber).GetComponent<Text>();
        foodText.text = "Player" + (playerNumber + 1) + Environment.NewLine + " food: " + food;
        input_count.Add(right_arr[playerNumber], 0);
        input_count.Add(left_arr[playerNumber], 0);
        input_count.Add(up_arr[playerNumber], 0);
        input_count.Add(down_arr[playerNumber], 0);

        base.Start();
    }

    protected void ReStart()
    {
        GameManager.instance.level++;
        SceneManager.LoadScene(0);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log("collider");
        if (other.tag == "Exit")
        {
            Invoke("ReStart", restartLevelDelay);
            enabled = false;
        }
        else if(other.tag == "Food")
        {
            food += pointPerFood;
            foodText.text = "Player" + (playerNumber + 1) + Environment.NewLine + " + " + pointPerFood + " food: " + food;
            SoundManager.instance.RandomizeSfx(eatSound1, eatSound2);
            other.gameObject.SetActive(false);
        }
        else if (other.tag == "Soda")
        {
            //Debug.Log("soda");
            food += pointPerSoda;
            foodText.text = "Player" + (playerNumber + 1) + Environment.NewLine + " + " + pointPerSoda + " food: " + food;
            SoundManager.instance.RandomizeSfx(drinkSound1, drinkSound2);
            other.gameObject.SetActive(false);
        }
        else if (other.tag == "Light")
        {
            Light light = other.gameObject.GetComponent<Light>();
            light.IsOn = true;
            SpriteRenderer sr = other.gameObject.GetComponent<SpriteRenderer>();
            sr.sprite = lighton;
        }
    }

    public void LoseFood(int loss)
    {
        animator.SetTrigger("PlayerHit");
        food -= loss;
        foodText.text = "Player" + (playerNumber + 1) + Environment.NewLine + " - " + loss + " food: " + food;
        IsGameOver();
    }

    protected override void OnCantMove<T>(T component)
    {
        Wall hitWall = component as Wall;
        hitWall.Damege(wallDamage);
        animator.SetTrigger("PlayerChop");
    }

    protected override void AttempMove<T>(int x, int y)
    {
        //
        food--;
        foodText.text = "Player" + (playerNumber + 1) + Environment.NewLine + " food: " + food;

        RaycastHit2D hit;
        if (CanMove( x,  y, out hit) == true)
        {
            SoundManager.instance.RandomizeSfx(moveSound1, moveSound2);
        }
        else
        {

        }
        base.AttempMove<T>(x,y);
        //RaycastHit2D hit;
        IsGameOver();

        GameManager.instance.playerTurn = false;
        //base.Move(x, y, out hit);
    }

    private void OnDisable()
    {
        GameManager.instance.points[playerNumber] = food;
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(right_arr[playerNumber]))
            input_count[right_arr[playerNumber]]++;
        if (Input.GetKeyDown(left_arr[playerNumber]))
            input_count[left_arr[playerNumber]]++;
        if (Input.GetKeyDown(up_arr[playerNumber]))
            input_count[up_arr[playerNumber]]++;
        if (Input.GetKeyDown(down_arr[playerNumber]))
            input_count[down_arr[playerNumber]]++;
        
        //Debug.Log(up_arr[playerNumber] + input_count[up_arr[playerNumber]]);
        if (!GameManager.instance.playerTurn) return;

        // this controls whether enemy can move with or without move of player
        GameManager.instance.playerTurn = false;

        //Debug.Log("Move");
        int horizontal = 0;
        int vertical = 0;

        
        //Debug.Log(Input.inputString);
        
        if((input_count[right_arr[playerNumber]]) > 0)
        {
            input_count[right_arr[playerNumber]]--;
            horizontal = +1;
        }
        else if (Input.GetKey(right_arr[playerNumber]))
        {
            horizontal = +1;
        }
        if ((input_count[left_arr[playerNumber]]) > 0)
        {
            input_count[left_arr[playerNumber]]--;
            horizontal = -1;
        }
        else if(Input.GetKey(left_arr[playerNumber]))
        {
            horizontal = -1;
        }
        if ((input_count[up_arr[playerNumber]]) > 0)
        {
            input_count[up_arr[playerNumber]]--;
            vertical = +1;
        }
        else if (Input.GetKey(up_arr[playerNumber]))
        {
            vertical = +1;
        }
        if ((input_count[down_arr[playerNumber]]) > 0)
        {
            input_count[down_arr[playerNumber]]--;
            vertical = -1;
        }
        else if (Input.GetKey(down_arr[playerNumber]))
        {
            vertical = -1;
        }


        if (horizontal != 0)
            vertical = 0;
        if (horizontal != 0 || vertical != 0)
        {
            AttempMove<Wall>(horizontal, vertical);
            GameManager.instance.UpdatePlayer(gameObject);
        }
        //if( horizontal == 0 && vertical == 0)
        StartCoroutine(WaitPlayer());
    }

    IEnumerator WaitPlayer()
    {
        
        yield return new WaitForSeconds(playerTime);
        
    }

    private void IsGameOver(){
        if (food <= 0)
        {
            GameManager.instance.GameOver();
            SoundManager.instance.PlaySingle(gameOverSound);
            SoundManager.instance.back.Stop();
            enabled = false;
        }
    }   


}
