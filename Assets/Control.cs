using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;


public class Control : MonoBehaviour {
	
	const int
		kPort = 42209,
		kHostConnectionBacklog = 10;
	
	static Control instance;
	
	Socket socket;
	IPAddress ip;
	
	static Control Instance
	{
		get
		{
			if(instance == null)
			{
				instance = (Control)FindObjectOfType(typeof(Control));
			}
			return instance;
		}
		
	}
	
	public static Socket Socket
	{
		get
		{
			return Instance.socket;
		}
	}
	
	// Use this for initialization
	void Start () {
		Host(kPort);
		
	}
	
	public bool Host(int port)
	{
		socket = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
		
		try
		{
			socket.Bind(new IPEndPoint(IP,kPort));
			socket.Listen(kHostConnectionBacklog);
			socket.BeginAccept(new System.AsyncCallback(OnClientConnect),socket);
		}
		catch(System.Exception e)
		{
			
			socket = null;
			return false;
		}
		return true;
		
	}
	
	void OnClientConnect(System.IAsyncResult result)
	{
		try
		{
			GameObject.Find("M_TC1_01").GetComponent<ConveyorForward>().setState(true);
		}
		catch(System.Exception e)
		{
			Debug.LogError("Exception - incoming connection"+e);	
		}
		
		try
		{
			socket.BeginAccept(new System.AsyncCallback(OnClientConnect),socket);	
		}
		catch(System.Exception e)
		{
			Debug.LogError("Exception - incoming connection"+e);
		}
		
	}
	
	void OnEndHostComplete(System.IAsyncResult result)
	{
		socket = null;	
	}
	
	public IPAddress IP
	{
		get
		{
			if(ip == null)
			{
				ip = (
					from entry in Dns.GetHostEntry (Dns.GetHostName()).AddressList
						where entry.AddressFamily == AddressFamily.InterNetwork
							select entry
					).FirstOrDefault();
			}
			return ip;
		}
	}
	// Update is called once per frame
	void Update () {
	
	}
}
