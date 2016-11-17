using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class Enemy : MovingObj {

    public int playerDamage;
    public AudioClip attack1;
    public AudioClip attack2;
    public AudioClip scaryscream1;
    public AudioClip scaryscream2;
    public int distance = 3;

    private Animator animator;
    private Transform target;
    private bool skipMove;
    private new List<GameObject> light;
    private new Renderer renderer;


    protected override void OnCantMove<T>(T component)
    {
        Player hitPlayer = component as Player;
        hitPlayer.LoseFood(playerDamage);
        animator.SetTrigger("EnemyHit");
        SoundManager.instance.RandomizeSfx(attack1, attack2);
        System.Random a = new System.Random();
        if(a.Next(100) < 5)
        {
            GameManager.instance.showscary = true;
            SoundManager.instance.RandomizeSfx(scaryscream1, scaryscream1);
            GameManager.instance.GameOver();
        }
    }

    // Use this for initialization
    protected override void Start ()
    {
        animator = GetComponent<Animator>();
        //System.Random a = new System.Random();
        string s = "Player0";
        if (GameObject.FindGameObjectWithTag(s) == null)
            target = null;
        //Debug.Log(s);
        target = GameObject.FindGameObjectWithTag(s).transform;
        base.Start();
        GameManager.instance.AddEnemyToList(this);
        BorderManager boardScript = GameManager.instance.GetComponent<BorderManager>();
        light = boardScript.lights;
        renderer = GetComponent<Renderer>();
        renderer.enabled = false;
    }

    protected override void AttempMove<T>(int x, int y)
    {
        if (skipMove)
        {
            skipMove = false;
            return;
        }
        base.AttempMove<T>(x, y);

        skipMove = true;
    }

    public void MoveEnemy()
    {
        int x = 0;
        int y = 0;
        if (target == null)
            return;
        y = (int) (target.position.y - gameObject.transform.position.y);

        x = (int) (target.position.x - gameObject.transform.position.x);

        //System.Random a = new System.Random();
        x = (Math.Abs(x) < 1e-5) ? 0 : Math.Sign(x);
        y = (Math.Abs(y) < 1e-5) ? 0 : Math.Sign(y);

        // try to design strategy to aviod any obstacles
        RaycastHit2D hit = new RaycastHit2D();
        //System.Random a = new System.Random();
        bool CanMoveWithoutDijkstra = false;
        //if (y == 0)
        //{
        //    if(CanMove(x, 0, out hit))
        //        AttempMove<Player>(x, 0);
        //    else
        //        CanMoveWithoutDijkstra = false;
        //}
        //else
        //{
        //    if (CanMove(0, y, out hit))
        //        AttempMove<Player>(0, y);
        //    else
        //        CanMoveWithoutDijkstra = false;
        //}

        if (!CanMoveWithoutDijkstra)
        {
            BorderManager boardScript = GameManager.instance.GetComponent<BorderManager>();
            //FileStream fs = new FileStream("Running.log", FileMode.Create);
            //string s = " Enemy is at " + gameObject.transform.position.x + gameObject.transform.position.y + "\n";
            //Debug.Log(s);
            //byte[] buffer = Encoding.ASCII.GetBytes(s);
            //fs.Write(buffer, 0, buffer.Length);
            //s = " Player is at " + target.transform.position.x + target.transform.position.y + "\n";
            //Debug.Log(s);
            //buffer = Encoding.ASCII.GetBytes(s);
            //fs.Write(buffer, 0, buffer.Length);
            //fs.Close();
            List<GraphNode> steps = boardScript.Dijkstra(new Vector2_int(transform.position.x, transform.position.y), new Vector2_int(target.position.x, target.position.y));
            if (steps != null && steps.Count > 1)
            {
                x = steps[1].pos.x - (int)gameObject.transform.position.x;
                y = steps[1].pos.y - (int)gameObject.transform.position.y;
                AttempMove<Player>(x, y);
            }
            if (steps == null)
                Debug.Log("steps is null");
            //for(int i = 0; i < steps.Count; i++)
            //{
            //    Debug.Log(steps[i].pos.x + " " + steps[i].pos.y);
            //}
        }


        //Vector2 current_pos = (Vector2)transform.position;
        for(int i = 0; i < light.Count; i++)
        {
            if(GameManager.Distance(gameObject, light[i]) < distance
                && light[i].GetComponent<Light>().IsOn)
            {
                renderer.enabled = true;
                animator.enabled = true;
                return;
            }
        }
        renderer.enabled = false;
        animator.enabled = false;
        return;
    }

    
}
