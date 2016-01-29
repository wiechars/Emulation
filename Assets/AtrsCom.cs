using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Net.Sockets;
using mysql_connection_driver;
using System.Text;
using System.Timers;

using System.Data;
using System.Data.Odbc;
using System.Linq;

public class AtrsCom : MonoBehaviour {
	
	public string IO_name_photoeye_trigger;
	public string IATA_last_bag="NOREAD";
	
	public int ATR_PORT = 9400;	
    private List<UserConnection> clients = new List<UserConnection>();
    private TcpListener listener;
    private Thread listenerThread;	
	private int max_emu_to_plc_io;
	
	
	System.Text.ASCIIEncoding encoding;
	private bool firstpass = true;
	
	BitArray emu_to_plc_bits;
	
	
	
	private int lasttrigger =0;
	private bool loggingenabled = false;
	private bool debuglogenabled = false;
	private bool ready = false;
	
	private bool getnewinputs = false;
	
	
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
		encoding = new System.Text.ASCIIEncoding();		
		
		max_emu_to_plc_io = 1600; // 200 bytes
		emu_to_plc_bits = new BitArray(max_emu_to_plc_io);
		
		
		
		
		Logger.WriteLine("[INFO]","STARTUP AtrsCom on port " + ATR_PORT + " linked to photoeye " + IO_name_photoeye_trigger,"",true);
		Debug.Log("STARTUP AtrsCom on port " + ATR_PORT + " linked to photoeye " + IO_name_photoeye_trigger+".\r\n");
		ready = true;
		
		listenerThread = new Thread(new ThreadStart(DoListen));
        listenerThread.Start();
		
		
	}
	
	void OnApplicationQuit() {
		if(listener !=null){
			listener.Server.Close();			
		}

		if(listenerThread!=null)
		{
			listenerThread.Abort();  			
		}
		 
    }
	
    // This subroutine is used a background listener thread to allow reading incoming
    // messages without lagging the user interface.
    private void DoListen()
	{
        try {
            // Listen for new connections.
            listener = new TcpListener(System.Net.IPAddress.Any, ATR_PORT);
            listener.Start();
 
			do
			{
				// Create a new user connection using TcpClient returned by
				// TcpListener.AcceptTcpClient()
				UserConnection client = new UserConnection(listener.AcceptTcpClient());
				// Create an event handler to allow the UserConnection to communicate
				// with the window.
				client.LineReceived += new LineReceive(OnLineReceived);
				//AddHandler client.LineReceived, AddressOf OnLineReceived;
				clients.Add(client);
			}while(true);
        } 
		catch(Exception ex){
			//MessageBox.Show(ex.ToString());
        }
	}	
	
	private byte[] GetBytes(string str)
	{
	    byte[] bytes = new byte[str.Length * sizeof(char)];
	    System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
	    return bytes;
	}
 
    // This is the event handler for the UserConnection when it receives a full line.
    // Parse the cammand and parameters and take appropriate action.
    private void OnLineReceived(UserConnection sender, byte[] data)
	{
		//Debug.Log("Received:\r\n"+data);
			
		byte[] iobytes = data; //encoding.GetBytes(data);
		
		int trigger = 0;
		if(iobytes.Length>=4){
			trigger = Convert.ToInt32(iobytes[160])+(Convert.ToInt32(iobytes[161])*256)+(Convert.ToInt32(iobytes[162])*65536)+(Convert.ToInt32(iobytes[163])*16777216);
		}
		
		if(trigger == lasttrigger)
		{
			if(true||loggingenabled)
			{
				Debug.Log("Ignored Trigger " +  trigger.ToString());
			}
			return;
		}
		lasttrigger = trigger;
		Debug.Log(lasttrigger);
		
		BitArray iovals = new System.Collections.BitArray(iobytes);
		
		string plc_to_emu_bit_string_bits = "";
		string plc_to_emu_bit_string = "";
		
		for(var i=0; i<iobytes.Length;i++)
		{
			plc_to_emu_bit_string += iobytes[i].ToString()+ " ";
			
		}
		
	
		
		if(loggingenabled)
		{
			Debug.Log("Received:\r\n"+plc_to_emu_bit_string);
			//Debug.Log("Received bits:\r\n"+plc_to_emu_bit_string_bits);
		}
		
		
		
		
		Loom.QueueOnMainThread(()=>{
			sendResponse(sender,IATA_last_bag,lasttrigger);
		});
		
		
    }
	
	private void Update()
	{
		if(getnewinputs)
		{
			return;
		}
		if(!ready)
		{
			return;
		}
	}
	
	
	
	
	private void sendResponse(UserConnection sender,string IATAstring,int lasttrigger){
		
		int triggerLocation = 100;
		byte[] responsebytes = new byte[200]; //BitArrayToByteArray(emu_to_plc_bits);
		
		for(var i=0; i<IATAstring.Length;i++)
		{
			responsebytes[7+i]=(byte)IATAstring[i];  //(copies the IATA characters)	
		}
		responsebytes[6] = (byte)IATAstring.Length;
		
		int triggerpointer = triggerLocation; //index of start of 40th dint;
		int trigger = lasttrigger;
		for(var i=0;i<4;i++){
			int byteval = (int)Math.Floor(trigger/Math.Pow(256,3-i));
			responsebytes[triggerpointer+3-i] = (byte)byteval;
			trigger=trigger - byteval;
		}
		
		sender.SendData(responsebytes);
		if(loggingenabled)
		{	
			
			Debug.Log("Sent IATA:"+ IATAstring +"\r\n");
		}
	}
 
	
	public void SetLastIATA(string iata){
		this.IATA_last_bag = iata;	
		//Debug.Log("Set IATA:"+iata);
	}
	
	private byte[] BitArrayToByteArray(BitArray bits)
	{
		
	    byte[] ret = new byte[bits.Length / 8];
	    bits.CopyTo(ret, 0);
	    return ret;
	}
}
