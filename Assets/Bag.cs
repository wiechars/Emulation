using UnityEngine;
using System.Collections;

public class Bag : MonoBehaviour {
	
	private enum security_status_type { Clear, Alarmed, Error, Pending,Unscanned }
	private int security_status = (int)security_status_type.Unscanned;
	private int prime_security_status = (int)security_status_type.Unscanned;
	private bool security_status_primed = false;
	private float prime_security_status_timer = 0;
	private float falling_photoeye_delay = 0.2f;
	
	public bool one_click = false;
	private bool timer_running;
	private float timer_for_double_click;
	private float delay =0.50f;
	private int windowID=1;
	private int GUIopen = 0;
	private Rect windowRect;
	
	private string IATA_tag = "";

	// Use this for initialization
	void Start () {
		windowID = Random.Range(0,100000);
		windowRect= new Rect (10, 10, 250, 150);
	
	}
	
	void OnGUI(){
		if(GUIopen==1)
		{
			windowRect = GUI.Window (windowID, windowRect, WindowFunction, "Bag ID: " + gameObject.GetInstanceID());
		}
	}
	
	void WindowFunction (int windowID) {
	    if (GUI.Button (new Rect (220, 0,30, 30), "X")) {
	    	GUIopen = 0;
	  	}
		GUI.Label(new Rect (5, 35, 120, 20),"Sequence ID:");
		GUI.Label(new Rect (130, 35, 100, 20),this.IATA_tag);
		
		string alarmedString = "";
		if(security_status == (int)security_status_type.Clear)
		{
			alarmedString= "Clear"; 
		}
		else if(security_status == (int)security_status_type.Alarmed)
		{
		}
		else if(security_status == (int)security_status_type.Error)
		{
			alarmedString= "Back"; 
		}
		else if(security_status == (int)security_status_type.Pending)
		{ 
		}
		else if(security_status == (int)security_status_type.Unscanned)
		{
			alarmedString= "Unscanned";
		}
		
		GUI.Label(new Rect (5, 65, 120, 20),"Alarmed State:");
		GUI.Label(new Rect (130, 65, 100, 20),alarmedString);
	}
	
	public void setIATA(string iata)
	{
		this.IATA_tag = iata;	
	}
	
	public string getIATA()
	{
		return this.IATA_tag;
	}
	
		
	public void primeSecurityStatus(int set_status)
	{
		prime_security_status = set_status;
		security_status_primed = true;
		prime_security_status_timer = Time.time;
	}
	
	public void setSecurityStatus(int set_status)
	{
		security_status = set_status;
		fixSecurityColors();
	}
	
	private void fixSecurityColors(){
		if(security_status == (int)security_status_type.Clear)
		{
			gameObject.GetComponent<Renderer>().material.color = Color.green;
		}
		else if(security_status == (int)security_status_type.Alarmed)
		{
			gameObject.GetComponent<Renderer>().material.color = Color.red;
		}
		else if(security_status == (int)security_status_type.Error)
		{
			gameObject.GetComponent<Renderer>().material.color = Color.black;
		}
		else if(security_status == (int)security_status_type.Pending)
		{
			gameObject.GetComponent<Renderer>().material.color = Color.blue;
		}
		else if(security_status == (int)security_status_type.Unscanned)
		{
			gameObject.GetComponent<Renderer>().material.color = Color.white;
		}
		
	}
	
	void OnMouseOver(){
		
		if (Input.GetKey (KeyCode.I)){
			GUIopen = 1;
	    }
		//Debug.Log(this.IATA_tag);
		if(Input.GetMouseButtonDown(0))
		{
		   if(!one_click) // first click no previous clicks
		   {
		     one_click = true;
		
		     timer_for_double_click = Time.time; // save the current time
		     // do one click things;
		   } 
		   else
		   {
		     one_click = false; // found a double click, now reset
			
				//Debug.Log("doubleclick");;
				//Component.Destroy(gameObject.GetComponent<DragRigidbody>());
				Destroy(gameObject);	
		   }
		}
		
		if(one_click)
		{
		   // if the time now is delay seconds more than when the first click started. 
		   if((Time.time - timer_for_double_click) > delay)
			{
			    one_click = false;			
			}
		}
		
	}
	
	// Update is called once per frame
	void Update () {
		if(gameObject.transform.position.y<-10)
		{
			Destroy(gameObject);	
		}
		
		if(security_status_primed&&(Time.time -prime_security_status_timer )> falling_photoeye_delay)
		{
			security_status_primed = false;
			setSecurityStatus(prime_security_status);
		}
		

	}
}
