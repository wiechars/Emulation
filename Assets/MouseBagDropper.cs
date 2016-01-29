using UnityEngine;
using System.Collections;

public class MouseBagDropper : MonoBehaviour {
	
	private enum security_status_type { Clear, Alarmed, Error, Pending, Unscanned }
	
	private float lastbagdrop = 0;	
	private float delay =0.1f;
	
	public float baglength = 28f;
	public float bagwidth = 16f;
	public float bagheight = 12f;
	
	public float bagalarmed = 10;
	public float bagerror = 5;
	public float bagpending = 5;
	public float bagclear = 80;
	
	private float dropheight = 0.1f;
	
	
	private float bag_static_friction = 0f;//0.1f;
	private float bag_dynamic_friction = 0f;//0.1f;
	
	private int multiply=0;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		
		if (Input.GetKey (KeyCode.B)){
			multiply = 0;
			DropBag();
	    }
		
		if (Input.GetKey (KeyCode.N)){
			multiply = 10;
			DropBag();
	    }	
	}
	
	
	void DropBag(){
		Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			
		RaycastHit hit;
		
		if (Physics.Raycast (ray, out hit, 10000)) {
				
		    //Debug.DrawLine (ray.origin, hit.point);
			
			if((Time.time - lastbagdrop) > delay)
			{
				lastbagdrop = Time.time;
				if(hit.collider.gameObject.name == "Conveyor Belt"||hit.collider.gameObject.name == "Conveyer Belt")
				{
					GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
					cube.transform.position = new Vector3(hit.point.x, hit.point.y + cube.transform.localScale.y/2 -0.001f+dropheight, hit.point.z);
					cube.transform.localScale = new Vector3 (bagwidth/12f, bagheight/12f, baglength/12f);
					cube.transform.rotation = hit.collider.gameObject.transform.rotation;
					
					cube.transform.Rotate(new Vector3(0,1,0),90f);
					cube.transform.Rotate(new Vector3(0,0,1),90f);
					
					
					
					//cube.transform.rotation = new Quaternion(1,1,1,1);
//cube.transform.localRotation = new Quaternion(-hit.collider.gameObject.transform.rotation.y,hit.collider.gameObject.transform.rotation.z,-hit.collider.gameObject.transform.rotation.x,1);

					
					//cube.transform.rotation =  hit.collider.gameObject.transform.rotation;
					//cube.transform.rota
					//cube.transform.rotation.SetFromToRotation(new Vector3(1,1,1),new Vector3(1,1,2));
					cube.AddComponent<Rigidbody>();
					
//						cube.rigidbody.constraints = RigidbodyConstraints.FreezePositionY;
					//if(constrainY)
					//{
					//	cube.rigidbody.constraints = RigidbodyConstraints.FreezePositionY;
					//}
					cube.GetComponent<Rigidbody>().angularDrag = Mathf.Infinity;
					
					//cube.rigidbody.mass = 10;
					
					
					cube.GetComponent<Collider>().material.dynamicFriction = bag_dynamic_friction;
					cube.GetComponent<Collider>().material.staticFriction = bag_static_friction;
					cube.gameObject.AddComponent<Bag>();
					cube.gameObject.AddComponent<BagDrag>();//DragRigidbody>();BagDrag
					
					// Sets the IATA tag
					if(CommunicationMaster.IATA_tags.Count>0)
					{
						cube.gameObject.GetComponent<Bag>().setIATA(CommunicationMaster.IATA_tags[0].ToString());//CommunicationMaster.IATA_tags.  .ElementAt(0);
						CommunicationMaster.IATA_tags.RemoveAt(0);
					}
					
					cube.name = cube.gameObject.GetComponent<Bag>().getIATA() + " "+Random.Range(0,10000).ToString();
					
					/*
					float chosentype = Random.Range(0,bagalarmed+bagpending+bagerror+bagclear);
					if(chosentype <bagalarmed)
					{
						cube.gameObject.GetComponent<Bag>().security_status = (int)security_status_type.Alarmed;
					}
					else if(chosentype <bagalarmed+bagpending)
					{
						cube.gameObject.GetComponent<Bag>().security_status = (int)security_status_type.Pending;
					}
					else if(chosentype <bagalarmed+bagpending+bagerror)
					{
						cube.gameObject.GetComponent<Bag>().security_status = (int)security_status_type.Error;
					}
					else
					{
						cube.gameObject.GetComponent<Bag>().security_status = (int)security_status_type.Clear;
					}
					*/
			    	Debug.Log ("Dropped bag : " + hit.collider.gameObject.name);
					
					for(int i=0;i<multiply;i++)
					{
						Instantiate(cube);
					}
				}
			}
		}
	}
}
