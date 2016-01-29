using UnityEngine;
using System.Collections;

public class DiverterReleaseBagHeight : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnCollisionStay(Collision collision) {
		
		//if(release_fixed_box_height)
		//{
			collision.rigidbody.constraints = 0;		
		//}
	}
}
