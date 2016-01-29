using UnityEngine;
using System.Collections;

public class DiverterLeftC : MonoBehaviour {
	
	
	//public int speed = 10;
	public bool diverteron =false;
	private float diverterperiod = 300f;
	public bool right_divert = false;
	private bool extended = false;
	private bool retracted = false;
	
	//public bool release_fixed_box_height = false; // release_fixed_box_height
	
	public string IO_name_disconnect = "";
	public string IO_name_cycle_command = "";
	public string IO_name_extended_prox = "";
	public string IO_name_retracted_prox = "";
	
	// Use this for initialization
	void Start () {
		if(diverteron)
		{
			gameObject.transform.FindChild("DiverterSwitch").GetComponent<Renderer>().material.color = Color.green;
			extended = true;
		}
		else{
			retracted = true;
		}
	}
	
	public void setState(bool diverteron)
	{
		this.diverteron = diverteron;
	}
	public bool getState()
	{
		return diverteron;	
	}
	public bool GetExtended()
	{
		return extended;
	}
	
	public bool GetRetracted()
	{
		return retracted;
	}
	

	
	void Update(){
		
		if(diverteron)
		{
			gameObject.transform.FindChild("DiverterSwitch").GetComponent<Renderer>().material.color = Color.green;
		}else{
			gameObject.transform.FindChild("DiverterSwitch").GetComponent<Renderer>().material.color = Color.red;
		}
		if(diverteron)
		{
			if(right_divert)
			{
				foreach(Transform child in gameObject.transform)
				{
					if(child.name=="1PaddleDiverter")
					{
						float deltay = child.transform.localEulerAngles.y;
						if(deltay!=225f)
						{				
							extended = false;
							retracted = false;
							if(deltay<225f)
							{							
								deltay = 	child.transform.localEulerAngles.y +(45000f*Time.deltaTime/diverterperiod);
							}
							if(deltay>225f){
								deltay = 225f;
							}
							
							if(deltay==225f)
							{		
								extended = true;
								// put flag up that the transition is completed.
							}
							child.transform.localEulerAngles = new Vector3(0f,deltay,0f);					
						}
		
					}
					else if(child.name=="2PaddleDiverter")
					{
						float deltay = child.transform.localEulerAngles.y;
						if(deltay!=45f)
						{				
							if(deltay<45f)
							{							
								deltay = 	child.transform.localEulerAngles.y +(45000f*Time.deltaTime/diverterperiod);
							}
							if(deltay>45f){ // ie. 359.2 degrees
								deltay = 45f;
							}
							child.transform.localEulerAngles = new Vector3(0f,deltay,0f);					
						}
					}
				}
				
			}
			else
			{
				foreach(Transform child in gameObject.transform)
				{
					if(child.name=="1PaddleDiverter")
					{
						float deltay = child.transform.localEulerAngles.y;
						if(deltay!=135f)
						{				
							extended = false;
							retracted = false;
							if(deltay>135f)
							{							
								deltay = 	child.transform.localEulerAngles.y -(45000f*Time.deltaTime/diverterperiod);
							}
							if(deltay<135f){
								deltay = 135f;
							}
							
							if(deltay==135f)
							{		
								extended = true;// put flag up that the transition is completed.								
							}
							child.transform.localEulerAngles = new Vector3(0f,deltay,0f);					
						}
					}
					else if(child.name=="2PaddleDiverter")
					{
						float deltay = child.transform.localEulerAngles.y;
						if(deltay!=315f)
						{	
							if(deltay>315f||deltay<0.001f)
							{								
								deltay = 	child.transform.localEulerAngles.y -(45000f*Time.deltaTime/diverterperiod);
								if(deltay<0)
								{
									deltay = deltay+360f;
								}
							}
							if(deltay<315f){
								deltay = 315f;
							}
							child.transform.localEulerAngles = new Vector3(0f,deltay,0f);					
						}
					}
				}
			}
		}
		else
		{
			if(right_divert)
			{
				foreach(Transform child in gameObject.transform)
				{
					if(child.name=="1PaddleDiverter")
					{
						float deltay = child.transform.localEulerAngles.y;
						if(deltay!=180f)
						{				
							extended = false;
							retracted = false;
							if(deltay>180f)
							{							
								deltay = 	child.transform.localEulerAngles.y -(45000f*Time.deltaTime/diverterperiod);
							}
							if(deltay<180f){
								deltay = 180f;
							}
							
							if(deltay==180f)
							{		
								retracted = true;// put flag up that the transition is completed.
							}
							child.transform.localEulerAngles = new Vector3(0f,deltay,0f);					
						}
		
					}
					else if(child.name=="2PaddleDiverter")
					{
						float deltay = child.transform.localEulerAngles.y;
						if(deltay!=0f)
						{				
							if(deltay>0f)
							{							
								deltay = 	child.transform.localEulerAngles.y -(45000f*Time.deltaTime/diverterperiod);
							}
							if(deltay>100f){ // ie. 359.2 degrees
								deltay = 0f;
							}
							child.transform.localEulerAngles = new Vector3(0f,deltay,0f);					
						}
					}
				}
			}
			else
			{				
				foreach(Transform child in gameObject.transform)
				{
					if(child.name=="1PaddleDiverter")
					{
						float deltay = child.transform.localEulerAngles.y;
						if(deltay!=180f)
						{			
							extended = false;
							retracted = false;
							if(deltay<180f)
							{							
								deltay = 	child.transform.localEulerAngles.y +(45000f*Time.deltaTime/diverterperiod);
							}
							if(deltay>180f){
								deltay = 180f;
							}
							
							if(deltay==180f)
							{		
								retracted = true;
							}
							child.transform.localEulerAngles = new Vector3(0f,deltay,0f);					
						}
					}
					else if(child.name=="2PaddleDiverter")
					{
						float deltay = child.transform.localEulerAngles.y;
						if(deltay!=0f)
						{							
							if(deltay<360f)
							{								
								deltay = 	child.transform.localEulerAngles.y +(45000f*Time.deltaTime/diverterperiod);
	
							}
							if(deltay>360f||deltay<315f){
								deltay = 0f;
							}
							child.transform.localEulerAngles = new Vector3(0f,deltay,0f);					
						}
					}
				}
			}
			
		
		}
	}
	
}





