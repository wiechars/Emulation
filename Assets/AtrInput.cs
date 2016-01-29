using UnityEngine;
using System.Collections;

public class AtrInput : MonoBehaviour {
	
	public string IO_name_ATR;
	public string IO_name_photoeye_trigger;
	
	
	private string IATA_last_bag = "";
		
	// Use this for initialization
	void Start () {
		object[] obj = GameObject.FindSceneObjectsOfType(typeof (GameObject));		
		foreach (object o in obj)
		{
			GameObject g = (GameObject) o;
			if(g.GetComponent<PhotoEye>()!=null)
			{
				if(g.GetComponent<PhotoEye>().IO_name_photoeye==IO_name_photoeye_trigger)
				{
					g.GetComponent<PhotoEye>().ConnectATR(gameObject);
				}
			}
		}
	}
	
	
	public void SetLastIATA(string iata){
		this.IATA_last_bag = iata;	
		Debug.Log("Set IATA:"+iata);
	}
	
	public string GetLastIATA(){
		return this.IATA_last_bag;
	}
}
