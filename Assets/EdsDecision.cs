using UnityEngine;
using System.Collections;

public class EdsDecision : MonoBehaviour {
	private enum security_status_type { Clear, Alarmed, Error, Pending, Unscanned }
	
	/*
	void OnTriggerEnter (Collider collision) {
		if(collision.gameObject.GetComponent<Bag>().security_status == (int)security_status_type.Clear)
		{
			collision.gameObject.renderer.material.color = Color.green;
		}
		else if(collision.gameObject.GetComponent<Bag>().security_status == (int)security_status_type.Error)
		{
			collision.gameObject.renderer.material.color = Color.red;
		}
		else if(collision.gameObject.GetComponent<Bag>().security_status == (int)security_status_type.Alarmed)
		{
			collision.gameObject.renderer.material.color = Color.red;
		}
		else if(collision.gameObject.GetComponent<Bag>().security_status == (int)security_status_type.Pending)
		{
			collision.gameObject.renderer.material.color = Color.yellow;
		}
	
	
	}*/
}
