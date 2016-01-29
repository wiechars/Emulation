using UnityEngine;
using System.Collections;

public class ControlStationButton : MonoBehaviour {

	
	public string IO_input ="";
	public string IO_output = "";
	
	public bool toggle = false;
	
	public string shinecolor = "";
	
	private int uptime = 2000;
	private bool pushed =false;
	private bool lighted= false;
	private float lastpush = 0f;
	
	
	private bool one_click = false;
	private float last_click = 0f;
	private float click_delay =0.50f;
	
	private float button_original_height;
	
	
	// Use this for initialization
	void Start () {
		button_original_height = gameObject.transform.localPosition.y;
		setButtonColor(false); // initializes to off.
	}
	
	public bool getState(){
		return this.pushed;	
	}
	
	public void setState(bool lighted){
		setButtonColor(lighted);
	}
	
	private void setButtonColor(bool lighted)
	{
		if(lighted)
		{
			if(shinecolor=="red")
			{
				gameObject.GetComponent<Renderer>().material.color = Color.red;
			}else if(shinecolor=="green")
			{
				gameObject.GetComponent<Renderer>().material.color = Color.green;
			}else if(shinecolor=="amber")
			{
				gameObject.GetComponent<Renderer>().material.color = Color.yellow;
			}else{
				gameObject.GetComponent<Renderer>().material.color = Color.red;//new Color(1f,0.1f,0.1f,1f);
			}
			//Debug.Log ("Button lighting changed to " + lighted.ToString());	
		}else{
			gameObject.GetComponent<Renderer>().material.color = new Color(0.25f,0.25f,0.25f,1f);////Color.;//  new Color(0.4627450980392157f,0.4627450980392157f,0.4627450980392157f,1f);
		}
	}
	
	void LateUpdate () {
		if(!toggle&&(Time.time - lastpush) > uptime/1000f)
		{
			pushed = false;
			gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x,button_original_height,gameObject.transform.localPosition.z);
		
		}
		
		
	}
	void OnMouseDown(){
		
		if(!one_click)
		{
			one_click =true;
			last_click = Time.time;
			return;
		}else if((Time.time - last_click)>click_delay){ //too long
			last_click = Time.time;
			return;
		}
		
		one_click = false;
		
		if(toggle){
			pushed = !pushed;
			if(pushed)
			{
				gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x,button_original_height-0.1f,gameObject.transform.localPosition.z);
			
			}else
			{
				gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x,button_original_height,gameObject.transform.localPosition.z);
		
			}
			//Temporary Testing
			//setButtonColor(pushed);
			return;	
		}
		
		pushed = true;
		
		gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x,button_original_height-0.1f,gameObject.transform.localPosition.z);
		
		lastpush = Time.time;
		
		
	}
}
