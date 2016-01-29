using UnityEngine;
using System.Collections;

public class ConveyorTurn : MonoBehaviour {
	
	public float speed = 220;
	private float orig_speed_backup;
	public bool turn_right = true;
	public float turn_angle_degrees = 90;
	public bool release_fixed_box_height = false;
	//public float setting_Y_constraint = 0;
	private int GUIopen = 0;
	private Rect windowRect;
	private int windowID=1;
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
	
	void OnGUI(){
		if(GUIopen==1)
		{
			windowRect = GUI.Window (windowID, windowRect, WindowFunction, "Device Name: " + gameObject.transform.parent.name);
		}
	}
	public bool getDisconnect(){
		return this.disconnectstate;
	}
	
	public bool getState(){
		return this.running;	
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
	
	void WindowFunction (int windowID) {
	    if (GUI.Button (new Rect(170, 0,30, 30), "X")) {
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
	
	void OnCollisionStay(Collision collision) {
	
		if(release_fixed_box_height)
		{
			collision.rigidbody.constraints = 0;
		
		}
		
		float tempspeed=0;
		if(((this.running||override_speed)&&excel_override==false)||(excel_override&&excel_val)){
			tempspeed = this.speed;			
		}
		
		var rigidbody = collision.gameObject.GetComponent<Rigidbody>();
		var relativeposition = gameObject.transform.InverseTransformPoint(rigidbody.transform.position);
	
	    if(relativeposition.y>0&&relativeposition.x<0){
	    	float tanfloat = -relativeposition.y/relativeposition.x;
	    	//print(tanfloat.ToString());
	    	float tanresult = Mathf.Rad2Deg*Mathf.Atan(tanfloat);    	
	    	//print(tanresult.ToString());    	
		    if(tanresult>(90f-turn_angle_degrees))	    
		    {
				// Generate a ray from the box straight down- used to see if the center of the box is above the belt or not.
				// This next if condition makes sure of this.
			    Ray ray = new Ray (rigidbody.position, new Vector3(0,-1,0));
				RaycastHit hit;
				if (Physics.Raycast (ray, out hit, 1)) {
					
					//if the ray from the box that is hitting something, is actually hitting US (the conveyer)
					if(hit.collider==gameObject.GetComponent<Collider>())
					{
					
					    float rigidbodyspeed = rigidbody.velocity.magnitude;
					    
					    
					    Vector3 distancetocenter = (rigidbody.transform.position - gameObject.transform.position);
					    
					    Vector3 xvec= new Vector3(0,0,0);
					    Vector3 yvec= new Vector3(0,0,0);
					    
						
					    //rigidbody.transform.position.
					    if(turn_right){
						    xvec = new Vector3(distancetocenter.z,0,0);
						    yvec = new Vector3(0,0, -distancetocenter.x);
						    rigidbody.transform.rotation = Quaternion.AngleAxis(rigidbody.transform.rotation.eulerAngles.y + Time.deltaTime*tempspeed/5,new Vector3(0,1,0));    
					    }else{
						    xvec = new Vector3(-distancetocenter.z,0,0);
					    	yvec = new Vector3(0,0, distancetocenter.x);
					    	rigidbody.transform.rotation = Quaternion.AngleAxis(rigidbody.transform.rotation.eulerAngles.y - Time.deltaTime*tempspeed/5,new Vector3(0,1,0));	
						}
					    // unit vector i want to be going.
					    Vector3 directionitshouldbe = ( xvec+ yvec).normalized;
					      
					    //rigidbody.velocity = directionitshouldbe*tempspeed/60;
						
						
					    // adjusts the velocity direction to what it should be.
//					    rigidbody.velocity = (1*rigidbody.velocity.normalized + directionitshouldbe).normalized *rigidbodyspeed;
					    //rigidbody.transform.Rotate(Vector3(0,1,0)*.02);
					    			
					    
					    // If the velocity, period, is too slow, speed it up a bit.
//					    if(rigidbody.velocity.magnitude*60< tempspeed){
//					   		rigidbody.velocity = rigidbody.velocity + directionitshouldbe/5; //Accelleration Rate!          //3*conveyorVelocity * 
//					    }
//					    else if(rigidbody.velocity.magnitude*60> tempspeed){
					   		rigidbody.velocity = directionitshouldbe*tempspeed/60;//rigidbody.velocity - directionitshouldbe/5; //Deccelleration Rate!          //3*conveyorVelocity * 
//					    }
						
						//collision.gameObject.transform.Rotate(new Vector3(uncatchangle,0f,0f)); // TODO, a command here could fix the catching boxes
						if(!collision.gameObject.GetComponent<Rigidbody>().freezeRotation)
						{
							collision.gameObject.transform.Rotate(new Vector3(-rigidbody.velocity.normalized.z*ConveyorConstants.beltbagrotateback,0,rigidbody.velocity.normalized.x*ConveyorConstants.beltbagrotateback)  ,Space.World)	;	
						}
					}
				}
			}
			else{
				
				
				
			}
		}
	}
}


