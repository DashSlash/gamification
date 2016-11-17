using UnityEngine;
using System.Collections;

public abstract class MovingObj : MonoBehaviour {

    public float moveTime = 0.01f;
    public LayerMask blockingLayer;

    private BoxCollider2D boxCollider;
    private Rigidbody2D rb2D;
    private float inverseMoverTime;

	// Use this for initialization
	protected virtual void Start () {
        boxCollider = GetComponent<BoxCollider2D>();
        rb2D = GetComponent<Rigidbody2D>();
        inverseMoverTime = 1.0f / moveTime;

    }
	
    protected IEnumerator SmoothMovement(Vector3 end)
    {
        float distance = (transform.position - end).sqrMagnitude; // squarMag is faster than Mag

        while ( distance >  float.Epsilon)
        {
            Vector3 newPos = Vector3.MoveTowards(rb2D.position,end, inverseMoverTime * Time.deltaTime);
            rb2D.MovePosition(newPos);
            distance = (transform.position - end).sqrMagnitude;
            yield return null; // wait for a new frame before evaluating the while loop
        }
    }

    protected bool CanMove(int x, int y, out RaycastHit2D hit)
    {
        Vector2 current_pos = (Vector2)transform.position;
        Vector2 end = current_pos + new Vector2(x, y);

        boxCollider.enabled = false;
        hit = Physics2D.Linecast(current_pos, end, blockingLayer);
        boxCollider.enabled = true;
        
        if (hit.transform == null)
        {
            return true;
        }
        else
        {
            return false; 
        }
    }

    protected bool Move(int x, int y, out RaycastHit2D hit)
    {
        Vector2 current_pos = (Vector2)transform.position;
        Vector2 end = current_pos + new Vector2(x, y);

        if (CanMove(x, y, out hit) == true && hit.transform == null)
        {
            StartCoroutine(SmoothMovement(end));
            return true;
        }
        else
        {
            return false;
        }
            
    }

    protected virtual void AttempMove<T>(int x, int y) where T:Component
    {
        RaycastHit2D hit;
        bool canMove = Move(x,y, out hit);

        if(hit.transform == null)
        {
            return;
        }

        T hitComponent = hit.transform.GetComponent<T>();
        
        if(!canMove && hitComponent != null)
        {
            OnCantMove(hitComponent);
        }

    }
    protected abstract void OnCantMove<T>(T component) where T : Component;
}
