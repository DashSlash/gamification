using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour {
    public AudioSource eff;
    public AudioSource back;
    public static SoundManager instance = null;

    public float low = 0.95f;
    public float high = 1.05f;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != null && this != instance)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        //Debug.Log("reload music");
        back.loop = true;
        back.enabled = true;
        back.Play();
    }

    public void PlaySingle(AudioClip clip)
    {
        eff.clip = clip;
        eff.Play();
    }

    public void RandomizeSfx(params AudioClip [] clips)
    {
        int random = Random.Range(0,clips.Length);
        float pitch = Random.Range(low, high);

        eff.pitch = pitch;
        eff.clip = clips[random];
        eff.Play();
    }

   
}
