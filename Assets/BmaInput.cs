using UnityEngine;
using System.Collections;

public class BmaInput : MonoBehaviour {
	
	public string IO_name_BMA_OK;
	public string IO_name_BMA_OOG;
	public string IO_name_photoeye_trigger;
	
	private PhotoEye trigger_photoeye;
	
	private string BMA_setting = "OK";
	
	public int public_percent = 1;
	private int remember_public_percent =0;	
	private int randompercent = 0;	
	private int remainingpercent = 0;	
	private int picks_remaining = 100;	
	private bool randomstate = false; 
	
	
	
	private int GUIopen = 0; // GUI_open
	private Rect windowRect;
	private int windowID=1;
	
	
	// Update is called once per frame
	void Update () {
		
		if(remember_public_percent != public_percent)
		{
			randompercent = public_percent;
			remember_public_percent = public_percent;
			picks_remaining =0;
		}
		
		if(trigger_photoeye!=null&&trigger_photoeye.checkTrigger())
		{
			if(picks_remaining ==0) // resets
			{
				picks_remaining=100;
				remainingpercent = randompercent;
			}		
			if(Random.Range(0,picks_remaining)<remainingpercent) // positivepick
			{
				remainingpercent --;
				randomstate = true;
			}
			else
			{
				randomstate = false;
			}			
			picks_remaining --;
		}
	}
	
	void OnMouseDown(){ 
		if(GUIopen==1)
		{
			GUIopen=0;
		}
		else{
			GUIopen=1;
		}
	}
	
	void OnGUI(){
		if(GUIopen==1)
		{
			windowRect = GUI.Window (windowID, windowRect, WindowFunction, "BMA Settings" );
		}
	}
	
	void WindowFunction (int windowID) {
	    if (GUI.Button (new Rect (170, 0,30, 30), "X")) {
	    	GUIopen = 0;
	  	}
		if (GUI.Button (new Rect (10,30,60, 30), "In Gauge")) {
	    	BMA_setting="OK";
	  	}
		if (GUI.Button (new Rect (70,30,60, 30), "Out of Gauge")) {
	    	BMA_setting="OOG";
	  	}
		if (GUI.Button (new Rect (130,30,60, 30), "Random")) {
	    	BMA_setting="Random";
	  	}
		GUI.Label(new Rect (5, 65, 170, 20),"State:" + BMA_setting);
		
	}
	
	// Use this for initialization
	void Start () {
		
		
		windowID = Random.Range(0,100000);
		windowRect= new Rect (50, 50, 200, 150);
		
		
		object[] obj = GameObject.FindSceneObjectsOfType(typeof (GameObject));
		
		foreach (object o in obj)
		{
			GameObject g = (GameObject) o;
			if(g.GetComponent<PhotoEye>()!=null)
			{
				if(g.GetComponent<PhotoEye>().IO_name_photoeye==IO_name_photoeye_trigger)
				{
					trigger_photoeye = g.GetComponent<PhotoEye>();
				}
			}
		}
		randompercent = public_percent;
		remainingpercent = randompercent;
		remember_public_percent = public_percent;
		
	}
	
	
	public bool getBMA_OK(){		
		if(BMA_setting=="OK")
		{
			return true;
		}
		else if(BMA_setting == "OOG")
		{
			return false;
		}
		else//(BMA_setting == "Random")
		{
			return !randomstate;
		}	
	}
	
	public bool getBMA_OOG(){		
		if(BMA_setting=="OK")
		{
			return false;
		}
		else if(BMA_setting == "OOG")
		{
			return true;
		}
		else// if(BMA_setting == "Random")
		{
			return randomstate;
		}
	}
}
