using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class onMouseDownScript : MonoBehaviour,IPointerDownHandler
{
    protected static onMouseDownScript Self;
    protected bool onClicked = false;
    // Use this for initialization
    void Start ()
    {
        Self = this;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
    //Instance
    public static onMouseDownScript Instance()
    {
        return Self;
    }
    //
    public void OnPointerDown(PointerEventData eventData)
    {
        if (Instance().onClicked == false)
        {
            //Debug.Log(this.gameObject.name + " was clicked");
            main.Instance().Click(this.gameObject.name);
            Instance().onClicked = true;
        }
        else
        {
            //Debug.Log(this.gameObject.name + " was check..");
            main.Instance().Check(this.gameObject.name);
        }
    }
    //
    public void Reset()
    {
        onClicked = false;
    }
}
