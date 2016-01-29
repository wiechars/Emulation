using UnityEngine;
using System.Collections;

public class BagDrag : MonoBehaviour {
	
	private float downTime;
	private bool isHandled;
	private float lastClick = 0f;
	private float waitTime = 0f;
	private float start_y;
	private Vector3 start_rotation;
	
	void OnMouseDown () {
		start_y = gameObject.GetComponent<Rigidbody>().position.y; // record the initial height of the box
		start_rotation = gameObject.transform.localEulerAngles;
	    //start recording the time when a key is pressed and held.
	    downTime = Time.time;
	    isHandled = false;
	 
	 
	    lastClick = Time.time;
	}
 
	void OnMouseDrag(){
	    //open a menu when the key has been held for long enough.
	    if((Time.time > downTime + waitTime) && !isHandled){
	        
			Camera mainCamera = Camera.main;
			
			Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
			
			Plane hPlane = new Plane(Vector3.up,new Vector3(0f,gameObject.GetComponent<Rigidbody>().position.y,0f));//start_y,0f));
			float distance = 0;
			if(hPlane.Raycast(ray,out distance))
			{
				//gameObject.rigidbody.position.
				gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;//.FreezePositionY;
				gameObject.GetComponent<Rigidbody>().isKinematic = false;
				gameObject.GetComponent<Rigidbody>().position = ray.GetPoint(distance);
				//gameObject.transform.localEulerAngles = new Vector3(start_rotation.x,gameObject.gameObject.transform.localEulerAngles.y,start_rotation.z);
			}
	    }
	}
	
	void OnMouseUp(){
		if(!gameObject.GetComponent<Bag>().one_click)
		{
			UnityEngine.GameObject new_bag = (GameObject)Instantiate(gameObject,gameObject.transform.position,Quaternion.Euler(start_rotation.x,gameObject.gameObject.transform.localEulerAngles.y,start_rotation.z));
			new_bag.GetComponent<Rigidbody>().isKinematic = false;
			//gameObject.transform.localEulerAngles = new Vector3(;
		
			Destroy(gameObject);
		}
		isHandled = true;// reset the timer for the next button press
	}
}
	
		