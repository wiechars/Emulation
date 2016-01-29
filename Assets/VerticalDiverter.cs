using UnityEngine;
using System.Collections;

public class VerticalDiverter : MonoBehaviour {
	
	public bool diverteron =false;
	private float diverterperiod = 500f;
	private bool stateup = false;
	private bool statedown = false;
	
	
	public string IO_name_disconnect = "";
	public string IO_name_cycle_cmd = "";
	public string IO_name_up_prox = "";
	public string IO_name_dn_prox = "";
	

	void Start () {
		if(diverteron)
		{
			gameObject.transform.FindChild("DiverterSwitch").GetComponent<Renderer>().material.color = Color.green;
			stateup = true;
		}else{
			statedown = true;
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
		return stateup;
	}
	
	public bool GetRetracted()
	{
		return statedown;
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
			foreach(Transform child in gameObject.transform)
			{
				if(child.name=="VerticalDiverter1")
				{
					float deltaz = child.transform.localEulerAngles.z;
					if(deltaz!=348f)
					{				
						child.transform.Find("Conveyor Belt").gameObject.GetComponent<ConveyorForward>().ReleaseKinematicBags();
						stateup = false;
						statedown = false;
						if(deltaz<=12f||deltaz>348f)
						{							
							deltaz = child.transform.localEulerAngles.z -(24000f*Time.deltaTime/diverterperiod);
						}
						if(deltaz<348f&&deltaz>100f){
							deltaz = 348f;
						}
						
						if(deltaz==348f)
						{	
							stateup = true;
						}
						child.transform.localEulerAngles = new Vector3(0f,0f,deltaz);					
					}
				}
				else if(child.name=="VerticalDiverter2")
				{
					float deltaz = child.transform.localEulerAngles.z;
					if(deltaz!=12f)
					{			
						child.transform.Find("Conveyor Belt").gameObject.GetComponent<ConveyorForward>().ReleaseKinematicBags();
						if(deltaz>300f||deltaz<12f)
						{							
							deltaz = child.transform.localEulerAngles.z +(24000f*Time.deltaTime/diverterperiod);
						}
						if(deltaz<300f&&deltaz>12f){
							deltaz = 12f;
						}
						child.transform.localEulerAngles = new Vector3(0f,180f,deltaz);					
					}				
				}
			}	
		}
		else
		{
			foreach(Transform child in gameObject.transform)
			{
				if(child.name=="VerticalDiverter1")
				{
					float deltaz = child.transform.localEulerAngles.z;
					if(deltaz!=12f)
					{		
						child.transform.Find("Conveyor Belt").gameObject.GetComponent<ConveyorForward>().ReleaseKinematicBags();
						stateup = false;
						statedown = false;					
						if(deltaz>300f||deltaz<12f)
						{							
							deltaz = child.transform.localEulerAngles.z +(24000f*Time.deltaTime/diverterperiod);
						}
						if(deltaz<300f&&deltaz>12f){
							deltaz = 12f;
						}
						
						if(deltaz==12f)
						{							
							statedown = true;
						}
						child.transform.localEulerAngles = new Vector3(0f,0f,deltaz);					
					}
				}
				else if(child.name=="VerticalDiverter2")
				{
					float deltaz = child.transform.localEulerAngles.z;
					if(deltaz!=348f)
					{			
						child.transform.Find("Conveyor Belt").gameObject.GetComponent<ConveyorForward>().ReleaseKinematicBags();
						if(deltaz<=12f||deltaz>348f)
						{							
							deltaz = child.transform.localEulerAngles.z -(24000f*Time.deltaTime/diverterperiod);
						}
						if(deltaz<348f&&deltaz>100f){
							deltaz = 348f;
						}
						
						child.transform.localEulerAngles = new Vector3(0f,180f,deltaz);					
					}
				}
			}		
		}
	}
}
