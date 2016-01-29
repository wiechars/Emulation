using UnityEngine;
using System.Collections;

public class DiverterRightC : MonoBehaviour {
	
	
	public int speed = 220;
	public bool diverteron;
	public string IO_name_disconnect = "";
	public string IO_name_cycle_command = "";
	public string IO_name_extended_prox = "";
	public string IO_name_retracted_prox = "";
	
	
	// Use this for initialization
	void Start () {
		if(diverteron)
		{
			gameObject.transform.FindChild("DiverterSwitch").GetComponent<Renderer>().material.color = Color.green;
		}
	}
	
	void SetState(bool diverteron)
	{
		this.diverteron = diverteron;	
	}
	
	bool GetExtended()
	{
		return diverteron;
	}
	
	bool GetRetracted()
	{
		return !diverteron;
	}
	
	// Update is called once per frame
	void Update () {
		if(diverteron)
		{
			gameObject.transform.FindChild("DiverterSwitch").GetComponent<Renderer>().material.color = Color.green;
		}else{
			gameObject.transform.FindChild("DiverterSwitch").GetComponent<Renderer>().material.color = Color.red;
		}
	}
	
	
	void FixedUpdate(){
		//offset = offset + 0.5;
		if(diverteron)
		{
			foreach(Transform child in gameObject.transform)
			{
				if(child.name=="1PaddleDiverter")
				{
					foreach(Transform grandchild in child.transform)
					{
						//grandchild.renderer.material.SetTextureOffset("_MainTex",new Vector2(-Time.time*speed/10,0));
					}
					//child.transform.localRotation = new Quaternion(0f,1f,0f,0f);
					child.transform.localRotation = new Quaternion(0f,0.9238796f,0f,-0.3826834f);
					//child.transform.localRotation = new Quaternion(0f,-0.3826834f,0f,0.9238796f);
				}
				else if(child.name=="2PaddleDiverter")
				{
					foreach(Transform grandchild in child.transform)
					{
						//grandchild.renderer.material.SetTextureOffset("_MainTex",new Vector2(-Time.time*speed/10,0));
					}
					child.transform.localRotation = new Quaternion(0f,0.3826834f,0f,0.9238796f);
					//child.transform.localRotation = new Quaternion(0f,0.9238796f,0f,0.3826834f);
					//child.transform.localRotation = new Quaternion(0f,0f,0f,0f);
				}
			        //Do your stuff
			}
			
			
		
		}
		else
		{
			foreach(Transform child in gameObject.transform)
			{
				if(child.name=="1PaddleDiverter")
				{
					//child.transform.localRotation = new Quaternion(0f,0.9238796f,0f,0.3826834f);
					
					child.transform.localRotation = new Quaternion(0f,1f,0f,0f);
	
				}
				else if(child.name=="2PaddleDiverter")
				{
					
					//child.transform.localRotation = new Quaternion(0f,-0.3826834f,0f,0.9238796f);
					child.transform.localRotation = new Quaternion(0f,0f,0f,0f);
	
				}
				
					
			}
		
		}
	}
	
}





