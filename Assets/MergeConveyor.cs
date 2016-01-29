using UnityEngine;
using System.Collections;

public class MergeConveyor : MonoBehaviour {
	
	public float speed = 220;
	private float orig_speed_backup;
	public bool merge = true;
	private int direction = 1;
	public bool release_fixed_box_height = false; // release_fixed_box_height
	public bool set_fixed_box_height = false;
	public float fixed_box_height_value = 0; // fixed_box_height_values
	private int GUIopen = 0;
	private Rect windowRect;
	private int windowID=1;
	public bool delete_bag = false; // delete_bag
	private bool override_speed=false;
	public bool running = false;
	private bool excel_override = false;
	private bool excel_val = false;
	
	private bool disconnectstate = true;
	
	public string IO_name_motor_run = null;
	public string IO_name_motor_direction = null;
	public string IO_name_motor_speed = null;
	public string IO_name_estop_circuit = null;
	public string IO_name_disconnect = null;
	
	private float uncatchangle = 0.5f;

	void Start () {
		orig_speed_backup = speed;
		windowID = Random.Range(0,100000);
		windowRect= new Rect (50, 50, 200, 150);
		this.setState(false);
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
			}
			else
			{
				gameObject.GetComponent<Renderer>().material.color = Color.red;
				gameObject.GetComponent<Collider>().material.staticFriction = ConveyorConstants.static_friction;
				gameObject.GetComponent<Collider>().material.dynamicFriction = ConveyorConstants.dynamic_friction;
			}	
		}
	}
	public void setExcelOverride(bool excel_override,bool excel_val)
	{
		this.excel_override = excel_override;
		this.excel_val = excel_val;
		setBeltColor();
	}
	
	public void setState(bool enabled){
		this.running = enabled;
		setBeltColor();
	}
	public bool getDisconnect(){
		return this.disconnectstate;
	}
	
	public bool getState(){
		return this.running;	
	}

	void OnGUI(){
		if(GUIopen==1)
		{
			windowRect = GUI.Window (windowID, windowRect, WindowFunction, "Device Name: " + gameObject.transform.parent.name);
		}
	}

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
			}
			gameObject.GetComponent<Renderer>().material.color= Color.blue;
		}else{
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

	void FixedUpdate(){
		//gameObject.renderer.material.SetTextureOffset("_MainTex",new Vector2(0,-Time.time*direction*speed/10));
	}


	void OnCollisionStay(Collision collision) {
	
		if(delete_bag)
		{
			Destroy(collision.gameObject);	
		}
		if(release_fixed_box_height)
		{
			collision.rigidbody.constraints = 0;
		
		}
		if(merge)
		{
			direction =1;
		}
		else{
			direction = -1;
		}
		
		float tempspeed = 0;
		if(((this.running||override_speed)&&excel_override==false)||(excel_override&&excel_val)){
			tempspeed = this.speed;			
		}
		
	    var rigidbody = collision.gameObject.GetComponent<Rigidbody>();
	    var relativeposition = gameObject.transform.InverseTransformPoint(rigidbody.transform.position);
	    
	    Ray ray = new Ray (rigidbody.position, new Vector3(0f,-1f,0f));
		RaycastHit hit;
		if (Physics.Raycast (ray,out hit, 1)) {
			//Debug.DrawLine (ray.origin, hit.point);
			if(hit.collider==gameObject.GetComponent<Collider>())
			{		
				
				if(set_fixed_box_height&&(rigidbody.rotation.eulerAngles.x <1f||rigidbody.rotation.eulerAngles.x >359f)&&(rigidbody.rotation.eulerAngles.z <1f||rigidbody.rotation.eulerAngles.z >359f))
				{
					// adjust the y position precisely as desired.
					rigidbody.position = new Vector3(rigidbody.position.x,fixed_box_height_value + collision.transform.localScale.y/2,rigidbody.position.z);
					rigidbody.constraints = RigidbodyConstraints.FreezePositionY;			
				}
				
				
				//get the magnitude of the velocity
			    var rigidbodyspeed = rigidbody.velocity.magnitude;
			    
			    // adjust it's direction-  :
				// 0* it's current direction
				// + 1 times where it should be going
				// 1* toward the center of the belt.
				// then normalized back to it's previous magnitude.
			    rigidbody.velocity = (0*rigidbody.velocity.normalized + direction * transform.up -transform.right*(gameObject.transform.InverseTransformPoint(rigidbody.transform.position).x+0.5f) ).normalized * rigidbodyspeed;
			  
			   // if the bag is stopped, just set it exactly in the speed and direction it should be
			   if(rigidbody.velocity.magnitude<0.01)
			   {
			   	   rigidbody.velocity = direction*transform.up*tempspeed/60;
				// if it's moving, divide the speed by the normalized dotproduct of it's direction onto where it SHOULD be going
				// what this effectively does, is increase the bags speed ABOVE what it should be, when it's being
				// corrected towards the center of the belt
				// additionally, this attempts to speed up boxes that are being diverted,
				// which bounces them off the divert walls, and effectively keeps the speed up correctly.
			   }else
			   {
			       rigidbody.velocity = direction*rigidbody.velocity.normalized/(Vector3.Dot(rigidbody.velocity.normalized,transform.up)*60/tempspeed);
			   }
				
				if(!collision.gameObject.GetComponent<Rigidbody>().freezeRotation)
				{
					collision.gameObject.transform.Rotate(new Vector3(-rigidbody.velocity.normalized.z*ConveyorConstants.beltbagrotateback,0,rigidbody.velocity.normalized.x*ConveyorConstants.beltbagrotateback)  ,Space.World)	;	
				}
				//collision.gameObject.transform.Rotate(new Vector3(uncatchangle,0f,0f)); // TODO, a command here could fix the catching boxes
		    }
	    }
		else{
		}
	}

        
}
    
