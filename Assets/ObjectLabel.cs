
using UnityEngine;
using System.Collections;
 
[RequireComponent (typeof (GUIText))]
public class ObjectLabel : MonoBehaviour {
 
public Transform target;  // Object that this label should follow
public Vector3 offset = Vector3.up;    // Units in world space to offset; 1 unit above object by default
public bool clampToScreen = true;  // If true, label will be visible even if object is off screen
public float clampBorderSize = 0.05f;  // How much viewport space to leave at the borders when a label is being clamped
public bool useMainCamera = true;   // Use the camera tagged MainCamera
public Camera cameraToUse ;   // Only use this if useMainCamera is false
Camera cam ;
Transform thisTransform;
Transform camTransform;
 
	void Start () 
    {
	    thisTransform = transform;
    if (useMainCamera)
        cam = Camera.main;
    else
        cam = cameraToUse;
    camTransform = cam.transform;
	}
 
 
    void Update()
    {
 

		
		if (Input.GetKey (KeyCode.I)){
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			
			RaycastHit hit;
			
			if (Physics.Raycast (ray, out hit, 10000)) {
					
			    //Debug.DrawLine (ray.origin, hit.point);
				
				//if((Time.time - lastbagdrop) > delay){
					//lastbagdrop = Time.time;
					if(hit.collider.gameObject.GetComponent<Bag>()!=null){
						//Debug.Log("caught it");
						this.target = hit.collider.gameObject.transform;
					
					}
				//}
			}
		}
		
		
		if(target==null){
			return;
		}
		
        if (clampToScreen)
        {
            Vector3 relativePosition = camTransform.InverseTransformPoint(target.position);
            relativePosition.z =  Mathf.Max(relativePosition.z, 1.0f);
            thisTransform.position = cam.WorldToViewportPoint(camTransform.TransformPoint(relativePosition + offset));
            thisTransform.position = new Vector3(Mathf.Clamp(thisTransform.position.x, clampBorderSize, 1.0f - clampBorderSize),
                                             Mathf.Clamp(thisTransform.position.y, clampBorderSize, 1.0f - clampBorderSize),
                                             thisTransform.position.z);
 
        }
        else
        {
            thisTransform.position = cam.WorldToViewportPoint(target.position + offset);
        }
    }
}