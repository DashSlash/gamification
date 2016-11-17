using UnityEngine;
using System.Collections;
using System.ComponentModel;
using System;

public class Light : MonoBehaviour, INotifyPropertyChanged{
    public Vector3 pos;
    public bool isOn = false;
    public BorderManager boardScript;

    public event PropertyChangedEventHandler PropertyChanged;
    protected void NotifyPropertyChanged(String propertyName)
    {
        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
    }

    public bool IsOn{
        get { return isOn; }
        set {
            if ( isOn != value)
            {
                //Debug.Log("changeison");
                isOn = value;
                //GetComponent<Image>().
                NotifyPropertyChanged("isOn"+isOn.ToString());
            }
        }
    }

    // Use this for initialization
    void Start () {
        pos = GetComponent<Transform>().position;
        isOn = false;
    }
	
	// Update is called once per frame
	void Update () {
	}
}
