using UnityEngine;
using System.Collections;

public class MouseWasd : MonoBehaviour {
	
	private float mainSpeed = 50.0f; //regular speed
	private float shiftAdd = 150.0f; //multiplied by how long shift is held.  Basically running
	private float maxShift = 600.0f; //Maximum speed when holdin gshift 
	private float camSens = 0.25f; //How sensitive it with mouse
	private Vector3 lastMouse = new Vector3(255f, 255f, 255f); //kind of in the middle of the screen, rather than at the top (play)
	private float totalRun  = 1.0f; 
	
	
    private int zoomRate = 500;
	//private int panSpeed = 100;
		
		
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void LateUpdate () {
		lastMouse = Input.mousePosition - lastMouse ; 
	    lastMouse = new Vector3(-lastMouse.y * camSens, lastMouse.x * camSens, 0 ); 
	    lastMouse = new Vector3(transform.eulerAngles.x + lastMouse.x , transform.eulerAngles.y + lastMouse.y, 0); 
	    
	    if(Input.GetButton("Fire2"))
	    {
	    	transform.eulerAngles = lastMouse;
	    }
	    lastMouse =  Input.mousePosition;
	
	    //Mouse & camera angle done.  
	
	    
	
	    //Keyboard commands
	
	    float f = 0.0f;	
	    Vector3 p = GetBaseInput(); 	
	    if (Input.GetKey (KeyCode.LeftShift)){ 	
	        totalRun += Time.deltaTime; 	
	        p  = p * totalRun * shiftAdd; 	
	        p.x = Mathf.Clamp(p.x, -maxShift, maxShift); 	
	        p.y = Mathf.Clamp(p.y, -maxShift, maxShift);	
	        p.z = Mathf.Clamp(p.z, -maxShift, maxShift);	
	    }
	
	    else{	
	        totalRun = Mathf.Clamp(totalRun * 0.5f, 1f, 1000f); 	
	        p = p * mainSpeed;	
	    }
		
		//if (Input.GetMouseButton(2))
        //{
		
		//Debug.Log(tempwheel*Time.deltaTime*zoomRate);
		if (Input.GetKey(KeyCode.LeftShift)){
			p = p+ new Vector3(0f, 0f , 1f)*Input.GetAxis("Mouse ScrollWheel")*Time.deltaTime*zoomRate*shiftAdd;  // new Vector3(1f,1f,1f);// p.normalized*tempwheel * Time.deltaTime * zoomRate;
		}
		else{
			p = p+ new Vector3(0f, 0f , 1f)*Input.GetAxis("Mouse ScrollWheel")*Time.deltaTime*zoomRate*mainSpeed;
			
		}
		
		
		
            //desiredDistance -= Input.GetAxis("Mouse Y") * Time.deltaTime * zoomRate*0.125f * Mathf.Abs(desiredDistance);
       // }
	
	    
	
	    p = p * Time.deltaTime;	
	    if (Input.GetKey(KeyCode.Space)){ //If player wants to move on X and Z axis only	
	        f = transform.position.y; 	
	        transform.Translate(p); 	
	        transform.position = new Vector3(transform.position.x, f,transform.position.z); 	
	    }	
	    else{	
	        transform.Translate( p); 	
	    }
		
		if (Input.GetMouseButton(2))
        {
			if (Input.GetKey(KeyCode.LeftShift)){
				p = new Vector3(-Input.GetAxis("Mouse X") * shiftAdd, 0f , 0f) + new Vector3(0f, 0f,-Input.GetAxis("Mouse Y") * shiftAdd);
			}
			else
			{
				p = new Vector3(-Input.GetAxis("Mouse X") * mainSpeed, 0f , 0f) + new Vector3(0f, 0f,-Input.GetAxis("Mouse Y") * mainSpeed);
			}
			p = p*Time.deltaTime;
			f = transform.position.y; 
			transform.Translate(p);
			transform.position = new Vector3(transform.position.x, f,transform.position.z); 
			
			//p = p + transform.rotation.eulerAngles
            //grab the rotation of the camera so we can move in a psuedo local XY space
            //target.rotation = transform.rotation;
			//p = p+ new Vector3(-Input.GetAxis("Mouse X") * panSpeed, 0f , 0f);
			//p = p+ ;
			//  transform.right *( );
			//p = p+ transform.up *( -Input.GetAxis("Mouse Y")) * panSpeed;
        }
		
	}
	
	
	private Vector3 GetBaseInput(){ //returns the basic values, if it's 0 than it's not active.

	    Vector3 p_Velocity = new Vector3(0f,0f,0f);	
	    if (Input.GetKey (KeyCode.W)){	
	        p_Velocity += new Vector3(0f, 0f , 1f);	
	    }	
	    if (Input.GetKey (KeyCode.S)){	
	        p_Velocity += new Vector3(0f, 0f , -1f);	
	    }	
	    if (Input.GetKey (KeyCode.A)){	
	        p_Velocity += new Vector3(-1f, 0f , 0f);	
	    }	
	    if (Input.GetKey (KeyCode.D)){	
	        p_Velocity += new Vector3(1f, 0f , 0f);	
	    }
	    if (Input.GetKey (KeyCode.Q)){	
	        p_Velocity += new Vector3(0f, -1f , 0f);	
	    }
	    if (Input.GetKey (KeyCode.E)){	
	        p_Velocity += new Vector3(0f, 1f , 0f);	
	    }	
	    return p_Velocity;	
	}
}
