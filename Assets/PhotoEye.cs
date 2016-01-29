using UnityEngine;
using System.Collections;
using System;
using System.Data;
using System.Data.Odbc;

public class PhotoEye : MonoBehaviour {
	
	private bool security_log_enabled = true;
	
	// Use this for initialization
	private enum security_status_type { Clear, Alarmed, Error, Pending, Unscanned }
	public int set_bag_security_status = (int)security_status_type.Unscanned;
	
	private GameObject connectedATR;
	
	private bool enabled = true;
	//private int statechanged = false;
	private bool laststate = false;	
	private Boolean hitstate = false;	
	public string IO_name_photoeye="";
	
	private bool excel_override = false;
	private bool excel_val = false;
	
	private bool risingedge = false;
	private bool risinglatch = false;
	
	
	public bool IO_assign_bag_enabled = false;
	public string IO_assign_bag_clear = "";
	public string IO_assign_bag_alarmed = "";
	public string IO_assign_bag_pending = "";
	
	
	
	private bool bag_clear = false; //green
	private bool bag_alarmed = false; //red
	private bool bag_pending = false;  //blue
	
	public bool checkTrigger(){
		if(risingedge)
		{
			risingedge = false;
			return true;
		}
		return false;
	}
	
	/*void setSecurityStatusAssigner(int status)
	{
		this.set_bag_security_status = status;	
	}*/
	
	public void setAssignClear(bool security_state_value)
	{
		if(security_state_value)
		{
			if(security_log_enabled)
			{
				Logger.WriteLine("[INFO]","PhotoEye","SS state CLEAR",true);
			}
			this.set_bag_security_status = (int)security_status_type.Clear;
		}
		else if(this.set_bag_security_status == (int)security_status_type.Clear)
		{
			if(security_log_enabled)
			{
				Logger.WriteLine("[INFO]","PhotoEye","SS state Unscanned -clear",true);
			}
			this.set_bag_security_status = (int)security_status_type.Unscanned;
		}
	}	
	public void setAssignAlarmed(bool security_state_value)
	{
		if(security_state_value)
		{
			
			if(security_log_enabled)
			{
				Logger.WriteLine("[INFO]","PhotoEye","SS state ALARMED",true);
			}
			this.set_bag_security_status = (int)security_status_type.Alarmed;
		}
		else if(this.set_bag_security_status == (int)security_status_type.Alarmed)
		{
			if(security_log_enabled)
			{
				Logger.WriteLine("[INFO]","PhotoEye","SS state Unscanned -alarmed",true);
			}
			this.set_bag_security_status = (int)security_status_type.Unscanned;
		}
	}
	public void setAssignPending(bool security_state_value)
	{
		if(security_state_value)
		{
			if(security_log_enabled)
			{
				Logger.WriteLine("[INFO]","PhotoEye","SS state PENDING",true);
			}
			this.set_bag_security_status = (int)security_status_type.Pending;
		}
		else if(this.set_bag_security_status == (int)security_status_type.Pending)
		{
			if(security_log_enabled)
			{
				Logger.WriteLine("[INFO]","PhotoEye","SS state Unscanned -pending",true);
			}
			this.set_bag_security_status = (int)security_status_type.Unscanned;
		}
	}
	
	
	public void ConnectATR(GameObject atrin)
	{
		this.connectedATR = atrin;
	}
	
	void Start () {
		if(!enabled)
		{
			gameObject.GetComponent<Renderer>().material.color = Color.black;	
		}
	}
	
	public void setExcelOverride(bool excel_override,bool excel_val)
	{
		this.excel_override = excel_override;
		this.excel_val = excel_val;
	}
	
	public bool getState(){
		if(excel_override)
		{
			return excel_val;	
		}
		return !this.laststate;	
	}
	
	void OnMouseDown()
	{
		if(enabled)
		{
			gameObject.GetComponent<Renderer>().material.color = Color.black;	
			enabled = false;
		}else{
			gameObject.GetComponent<Renderer>().material.color = Color.gray;	
			enabled = true;			
		}
		
		
		
	}
	
	// Update is called once per frame
	void Update () {
		
		if(!enabled)
		{
			return;
		}
		
		RaycastHit hit;
       
		Ray photoray = new Ray(transform.position-transform.forward*0.15f,-transform.forward);
		
		// Communicates over the the ATR what the passing bag's IATA tag is.
		if(Physics.Raycast(photoray, out hit, 2.8f))
		{
			if(connectedATR!=null)
			{
				if(hit.collider.gameObject.GetComponent<Bag>().getIATA()=="")  // bag hasn't got an IATA tag yet.
				{
					hit.collider.gameObject.GetComponent<Bag>().setIATA(IataTags.popIATA());
					Logger.WriteLine("[INFO]","PhotoEye","IATA "+(hit.collider.gameObject.GetComponent<Bag>()).getIATA()+" by photoeye " + IO_name_photoeye,true);
				}
				connectedATR.GetComponent<AtrsCom>().SetLastIATA(hit.collider.gameObject.GetComponent<Bag>().getIATA());
			}
		}
		/// assign bag color
		if(IO_assign_bag_enabled&&Physics.Raycast(photoray, out hit, 2.8f))
		{
			hit.collider.gameObject.GetComponent<Bag>().primeSecurityStatus(set_bag_security_status);
			
		}
		
		//Debug.DrawRay(transform.position, -transform.forward);//new Vector3(0,0,-1));
        if ((Physics.Raycast(photoray, out hit, 2.8f)&&!excel_override)||(excel_override&&!excel_val))
		{
			if(risinglatch==false)
			{
				risinglatch = true;
				risingedge =true;
			}
			
			if(excel_override)
			{
				gameObject.GetComponent<Renderer>().material.color = Color.magenta;
			}else{
				gameObject.GetComponent<Renderer>().material.color = Color.red;				
			}
			foreach(Transform child in gameObject.transform)
			{
				if(child.name=="ray")
				{
					child.transform.localPosition = new Vector3(0f,0f,-2.85f);
					child.transform.localScale =new Vector3(0.1f,2.55f,0.1f);
				}	
				if(child.name=="reflector")
				{
					if(excel_override)
			        {
				        child.GetComponent<Renderer>().material.color = Color.magenta;
			        }else{
				        child.GetComponent<Renderer>().material.color = Color.red;				
			        }
				}
			        //D// your stuff
			}

			laststate = true;
		}
		else
		{
			risinglatch = false;
			if(excel_override)
			{
				gameObject.GetComponent<Renderer>().material.color = Color.black;
			}else{
				gameObject.GetComponent<Renderer>().material.color = Color.white;
			}
			foreach(Transform child in gameObject.transform)
			{
				if(child.name=="ray")
				{
					child.transform.localPosition = new Vector3(0f,0f,-5.7f);
					child.transform.localScale = new Vector3(0.1f,5.1f,0.1f);
				}
				if(child.name=="reflector")
				{
					if(excel_override)
			        {
				        child.GetComponent<Renderer>().material.color = Color.black;
			        }else{
				        child.GetComponent<Renderer>().material.color = Color.white;				
			        }
				}
			        //Do your stuff
			}
			laststate = false;
		}
	}
}
