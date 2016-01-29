using UnityEngine;
using System.Collections;

public class ArrowViews : MonoBehaviour {
	
	private int viewindex = 0;
	private int totalviews = 3;
	
	private float lastview = 0;	
	private float delay =0.25f;
	
	
	private Vector3[] viewpositions;
	private Vector3[] viewangles;
	// Use this for initialization
	void Start () {
		viewpositions = new Vector3[3];
		viewangles = new Vector3[3];
		
		//CBIS1
		viewpositions[0] = new Vector3(69.39411f,56.44371f,-58.71655f);
		viewangles[0] = new Vector3(57.66f,90.48f,0f);
		
		//Ticket Counter
		viewpositions[1] = new Vector3(-12.06624f,54.63894f,-27.19841f);
		viewangles[1] = new Vector3(57.66f,90.48f,0f);
		
		// TC Merges
		viewpositions[2] = new Vector3(49.73474f,32.97554f,-28.8889f);
		viewangles[2] = new Vector3(90f,90f,0f);
		
		
		
		
		/*
		gameObject.transform.position = viewpositions[0];
		gameObject.transform.localEulerAngles = viewangles[0];*/
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKey (KeyCode.RightArrow)){
			
			if((Time.time - lastview) > delay)
			{
				lastview = Time.time;
				viewindex ++;
				if(viewindex>totalviews-1)
				{
					viewindex = 0;
				}
				
		
				gameObject.transform.position = viewpositions[viewindex];
				gameObject.transform.localEulerAngles = viewangles[viewindex];
				
				
			}
		}
		if (Input.GetKey (KeyCode.LeftArrow)){
			
			if((Time.time - lastview) > delay)
			{
				lastview = Time.time;
				viewindex --;
				if(viewindex<0)
				{
					viewindex = totalviews-1;
				}
				
		
				gameObject.transform.position = viewpositions[viewindex];
				gameObject.transform.localEulerAngles = viewangles[viewindex];
				
				
			}
		}
		
		
	}
}