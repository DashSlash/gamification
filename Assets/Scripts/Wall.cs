using UnityEngine;
using System.Collections;

public class Wall : MonoBehaviour {
    public Sprite spr;
    private SpriteRenderer sprr;
    private int hp = 4;
    public AudioClip chop1;
    public AudioClip chop2;

	void Awake () {
        sprr = GetComponent<SpriteRenderer>();
	}
	
	public void Damege( int loss)
    {
        SoundManager.instance.RandomizeSfx(chop1, chop2);
        sprr.sprite = spr;
        hp -= loss;
        if (hp <= 0)
            gameObject.SetActive(false);
    }
}
