using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ConveyorForward : MonoBehaviour {
	
	
	public float speed = 220; // speed
	private float orig_speed_backup;
	private int direction = 1; // direction
	public bool forward = true;
	public bool bag_centering = true;
	public bool release_fixed_box_height = false; // release_fixed_box_height
	public bool set_fixed_box_height = false;	
	public float fixed_box_height_value = 0; // setting_fixed_box_height_values
	public bool is_incline = false;
	private int GUIopen = 0; // GUI_open
	private Rect windowRect;
	private int windowID=1;
	public bool override_speed=false; // override_speed
	public bool delete_bag = false; // delete_bag
	public bool running = false; // running
	
	private bool excel_override = false;
	private bool excel_val = false;
	
	private bool disconnectstate = true;
	
	public string IO_name_motor_run = null;
	public string IO_name_motor_direction = null;
	public string IO_name_motor_speed = null;
	public string IO_name_estop_circuit = null;
	public string IO_name_disconnect = null;
	
	private float static_friction = 0.2f;  //0.2f
	private float dynamic_friction = 0.5f; // 0.5f
	
	private float uncatchangle = 0.5f;
	
	private List<GameObject> kinbags = new List<GameObject>();
	
		
	// Use this for initialization
	void Start () {
		
		orig_speed_backup = speed;
		
		windowID = Random.Range(0,100000);
		windowRect= new Rect (50, 50, 200, 150);
		if(override_speed)
		{
			//this.setState(true);						
			if(speed<0.001)
			{
				gameObject.GetComponent<Collider>().material.staticFriction = ConveyorConstants.static_friction;
				gameObject.GetComponent<Collider>().material.dynamicFriction = ConveyorConstants.dynamic_friction;
			}
			else
			{
				gameObject.GetComponent<Collider>().material.staticFriction = 0f;
				gameObject.GetComponent<Collider>().material.dynamicFriction = 0f;
			}				
			gameObject.GetComponent<Renderer>().material.color= Color.blue;			
			
		}else{
			this.setState(false);			
		}
	}
	
	void setBeltColor()
	{
		if(excel_override)
		{
			if(excel_val)
			{
				gameObject.GetComponent<Renderer>().material.color= Color.cyan;
				gameObject.GetComponent<Collider>().material.staticFriction = 0f;
				gameObject.GetComponent<Collider>().material.dynamicFriction = 0f;
				if(is_incline)
				{
					ReleaseKinematicBags();
				}
			}
			else
			{
				gameObject.GetComponent<Renderer>().material.color = Color.magenta;
				gameObject.GetComponent<Collider>().material.staticFriction = ConveyorConstants.static_friction;
				gameObject.GetComponent<Collider>().material.dynamicFriction = ConveyorConstants.dynamic_friction;
			}
			
		}
		else
		{
			if(this.running)
			{
				gameObject.GetComponent<Renderer>().material.color= Color.green;
				gameObject.GetComponent<Collider>().material.staticFriction = 0f;
				gameObject.GetComponent<Collider>().material.dynamicFriction = 0f;
				if(is_incline)
				{
					ReleaseKinematicBags();
				}
			}
			else
			{
				gameObject.GetComponent<Renderer>().material.color = Color.red;
				gameObject.GetComponent<Collider>().material.staticFriction = ConveyorConstants.static_friction;
				gameObject.GetComponent<Collider>().material.dynamicFriction = ConveyorConstants.dynamic_friction;
			}	
		}
	}
	
	void FixedUpdate(){
		// belt rotation animation - unused
		//gameObject.renderer.material.SetTextureOffset("_MainTex",new Vector2(Time.time*speed*direction/10,0));
	}
	
	public bool getState(){
		return this.running;	
	}
	
	public bool getDisconnect(){
		return this.disconnectstate;
	}
	
	public void setState(bool running){
		this.running = running;
		setBeltColor();
	}
	
	public void setExcelOverride(bool excel_override,bool excel_val)
	{
		this.excel_override = excel_override;
		this.excel_val = excel_val;
		setBeltColor();
	}
	
	void OnGUI(){
		if(GUIopen==1)
		{
			windowRect = GUI.Window (windowID, windowRect, WindowFunction, "Device Name: " + gameObject.transform.parent.name );
		}
	}
	
	// this function continually fires when the popup is open
	void WindowFunction (int windowID) {
	    if (GUI.Button (new Rect (170, 0,30, 30), "X")) {
	    	GUIopen = 0;
	  	}
	  	override_speed = GUI.Toggle(new Rect(5, 35, 150, 30), override_speed, "Override Speed");
	  	if(override_speed)
	  	{
	  		speed = GUI.HorizontalSlider (new Rect (55, 70, 115, 30), speed, 0.0f, 280.0f);
		  	GUI.Label(new Rect (5, 65, 50, 20),"Speed");
		  	GUI.Label(new Rect (175, 65, 25, 30),speed.ToString());
		}
		disconnectstate = GUI.Toggle (new Rect (5, 95,115, 30),disconnectstate,"Disconnect State");
		
		// display blue if the belt is in override mode
		if(override_speed)
		{
			if(speed<0.001)
			{
				gameObject.GetComponent<Collider>().material.staticFriction = ConveyorConstants.static_friction;
				gameObject.GetComponent<Collider>().material.dynamicFriction = ConveyorConstants.dynamic_friction;
			}
			else
			{
				gameObject.GetComponent<Collider>().material.staticFriction = 0f;
				gameObject.GetComponent<Collider>().material.dynamicFriction = 0f;
				if(is_incline)
				{
					ReleaseKinematicBags();
				}
			}
				
			gameObject.GetComponent<Renderer>().material.color= Color.blue;
			
		}else{ //or restore it back to how it should be
			speed = orig_speed_backup;
			setBeltColor();	
		}
		
		GUI.DragWindow (new Rect (0,0, 10000, 10000));
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
	
	public void ReleaseKinematicBags(){
		//if(((this.running||override_speed)&&excel_override==false)||(excel_override&&excel_val)){
		//	return;
		//}
		foreach(GameObject rb in kinbags)
		{
			try{
				if (rb == null) {
					continue;
				}
				rb.GetComponent<Rigidbody>().isKinematic = false;	
				rb.GetComponent<Rigidbody>().velocity = transform.right*.0001f;
			}catch(UnityException exc)
			{
				Debug.Log("kinematic release fault");	
			}
		}
		kinbags = new List<GameObject>();
	}
	
	
	void OnCollisionStay(Collision collision){
	
		
		if(collision.gameObject.GetComponent<Rigidbody>().velocity.magnitude<.01f)
		{
			//Debug.Log("bagID:"+collision.gameObject.GetInstanceID()+" stopped");
		}
		
		if(delete_bag)
		{
			Destroy(collision.gameObject);	
		}
		if(release_fixed_box_height)
		{
			collision.rigidbody.constraints = 0;		
		}
		
		if(forward)
		{
			direction =1;
		}
		else{
			direction = -1;
		}
		
		//overrides speed off if it's not running
		float tempspeed = 0;
		if(((this.running||override_speed)&&excel_override==false)||(excel_override&&excel_val)){
			tempspeed = this.speed;			
		}
		
		// gets the rigid body of the colliding bag
	    Rigidbody rigidbody = collision.gameObject.GetComponent<Rigidbody>();
		// this gets the position of the box in relation to the belt.
	    Vector3 relativeposition = gameObject.transform.InverseTransformPoint(rigidbody.transform.position);
	    
		// Generate a ray from the box straight down- used to see if the center of the box is above the belt or not.
		// This next if condition makes sure of this.
	    Ray ray = new Ray (rigidbody.position, new Vector3(0,-1,0));
		RaycastHit hit;
		if (Physics.Raycast (ray, out hit, 1)) {
			
			//if the ray from the box that is hitting something, is actually hitting US (the conveyer)
			if(hit.collider==gameObject.GetComponent<Collider>())
			{
				if(is_incline&&tempspeed==0)
				{			
					
					rigidbody.isKinematic = true;
					kinbags.Add(collision.gameObject);
//					Debug.Log("in 0");
					
					
					return;
				}
				
				
				
				// This section is responsible for attempting to constrain the y axis of the bag.
				// It makes sure that the bag is on the belt(from above) and that it's rotations have "steadied" out.
				// It will eventually make it into here early on as it continually collides with the belt.
				if(set_fixed_box_height&&(rigidbody.rotation.eulerAngles.x <1f||rigidbody.rotation.eulerAngles.x >359f)&&(rigidbody.rotation.eulerAngles.z <1f||rigidbody.rotation.eulerAngles.z >359f))
				{
					// adjust the y position precisely as desired.
					rigidbody.position = new Vector3(rigidbody.position.x,fixed_box_height_value + collision.transform.localScale.y/2,rigidbody.position.z);
					//freezes Y 
					rigidbody.constraints = RigidbodyConstraints.FreezePositionY;			
				}
				
				//get the magnitude of the velocity
			    float rigidbodyspeed = rigidbody.velocity.magnitude;
				
				// adjust it's direction-  :
				// 0* it's current direction
				// +
				// 2* toward the center of the belt.
				// then normalized back to it's previous magnitude.
				float centering = 0f;
				if(bag_centering)
				{
					centering = 1f;	
				}
			    rigidbody.velocity = (0*rigidbody.velocity.normalized + 2*direction*transform.right - centering*transform.up*(gameObject.transform.InverseTransformPoint(rigidbody.transform.position).y-0.5f) ).normalized * rigidbodyspeed;
				
				// if the bag is stopped, just set it exactly in the speed and direction it should be
			   
			   if(rigidbody.velocity.magnitude<0.01)
			   {
			   	   rigidbody.velocity = transform.right*tempspeed/60;
			   }
				// if it's moving, divide the speed by the normalized dotproduct of it's direction onto where it SHOULD be going
				// what this effectively does, is increase the bags speed ABOVE what it should be, when it's being
				// corrected towards the center of the belt
				// additionally, this attempts to speed up boxes that are being diverted,
				// which bounces them off the divert walls, and effectively keeps the speed up correctly.
				else 
			   {
					
			       rigidbody.velocity = direction*rigidbody.velocity.normalized/(Vector3.Dot(rigidbody.velocity.normalized,transform.right)*60/tempspeed);
			   }
				
				//Debug.Log (gameObject.transform.rotation.z.ToString());  //gets important angle- depending on this angle,
				// determine the required z and y rotations from this.
				
				
					
				if(!collision.gameObject.GetComponent<Rigidbody>().freezeRotation)
				{
					collision.gameObject.transform.Rotate(new Vector3(-rigidbody.velocity.normalized.z*ConveyorConstants.beltbagrotateback,0,rigidbody.velocity.normalized.x*ConveyorConstants.beltbagrotateback)  ,Space.World);	
				}		
					
				//collision.gameObject.transform.Rotate(  ,Space.World);//new Vector3(uncatchangle,0f,0f),Space.World); // TODO, a command here could fix the catching boxes
			}
			else{
				
				
				
			}
	    }
	}
}
