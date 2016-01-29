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

public class CommunicationMaster : MonoBehaviour {
	
//	const int[] debug_id = new int[1]{8};
	public TextAsset dbConfig;
	
	public static List<string> IATA_tags = new List<string>();
	
	
	const int PORT_NUM = 9319;	
    private Hashtable clients = new Hashtable();
    private TcpListener listener;
    private Thread listenerThread;	
	private Db db;
	
	
	const int EXCEL_PORT_NUM = 2000;
	private Hashtable excelClients = new Hashtable();
	private TcpListener excelListener;
	private Thread excelListenerThread;
	
	
	
	
	
	System.Text.ASCIIEncoding encoding;
	private bool firstpass = true;
	
	

	Dictionary<string,int> plc_to_emu;
	ConveyorForward[] conveyer;
	bool[] conveyerstate;
	bool[] conveyerlaststate;
	int[] conveyerio;
	bool[] conveyer_excel_override;
	bool[] conveyer_excel_val;
	
	ConveyorTurn[] conveyer_turn_run;
	bool[] conveyer_turn_run_state;
	bool[] conveyer_turn_run_laststate;
	int[] conveyer_turn_run_io;
	bool[] conveyer_turn_run_excel_override;
	bool[] conveyer_turn_run_excel_val;
	
	MergeConveyor[] conveyer_merge_run;
	bool[] conveyer_merge_run_state;
	bool[] conveyer_merge_run_laststate;
	int[] conveyer_merge_run_io;
	bool[] conveyer_merge_run_excel_override;
	bool[] conveyer_merge_run_excel_val;
	
	ConveyorForward[] conveyer_disconnect;
	bool[] conveyer_disconnect_state;
	bool[] conveyer_disconnect_laststate;
	int[] conveyer_disconnect_io;
	
	ConveyorTurn[] conveyer_turn_disconnect;
	bool[] conveyer_turn_disconnect_state;
	bool[] conveyer_turn_disconnect_laststate;
	int[] conveyer_turn_disconnect_io;
		
	MergeConveyor[] conveyer_merge_disconnect;
	bool[] conveyer_merge_disconnect_state;
	bool[] conveyer_merge_disconnect_laststate;
	int[] conveyer_merge_disconnect_io;
	
	DiverterLeftC[] hsd_cycle;
	bool[] hsd_cycle_state;
	bool[] hsd_cycle_laststate;
	bool[] hsd_last_cycle_io;
	int[] hsd_cycle_io;
	
	DiverterLeftC[] hsd_extended_prox;
	bool[] hsd_extended_prox_state;
	bool[] hsd_extended_prox_laststate;
	int[] hsd_extended_prox_io;
	
	DiverterLeftC[] hsd_retracted_prox;
	bool[] hsd_retracted_prox_state;
	bool[] hsd_retracted_prox_laststate;
	int[] hsd_retracted_prox_io;
	
	
	VerticalDiverter[] vsu_cycle;
	bool[] vsu_cycle_state;
	bool[] vsu_cycle_laststate;
	bool[] vsu_last_cycle_io;
	int[] vsu_cycle_io;
	
	VerticalDiverter[] vsu_up_prox;
	bool[] vsu_up_prox_state;
	bool[] vsu_up_prox_laststate;
	int[] vsu_up_prox_io;
	
	VerticalDiverter[] vsu_down_prox;
	bool[] vsu_down_prox_state;
	bool[] vsu_down_prox_laststate;
	int[] vsu_down_prox_io;
	
	ControlStationButton[] cs_button_output;//lighting
	bool[] cs_button_output_state;
	bool[] cs_button_output_laststate; // allows for not updating unnecessarily
	int[] cs_button_output_io;
	
	ControlStationButton[] cs_button_input;//button pressing
	int[] cs_button_input_io;
	
	PhotoEye[] pe_assign_security_clear; //photoeyeassigns
	bool[] pe_assign_security_clear_state;
	bool[] pe_assign_security_clear_laststate;
	int[] pe_assign_security_clear_io;
 	PhotoEye[] pe_assign_security_alarmed;
	bool[] pe_assign_security_alarmed_state;
	bool[] pe_assign_security_alarmed_laststate;
	int[] pe_assign_security_alarmed_io;
	PhotoEye[] pe_assign_security_pending;
	bool[] pe_assign_security_pending_state;
	bool[] pe_assign_security_pending_laststate;
	int[] pe_assign_security_pending_io;
	
	Dictionary<string,int> emu_to_plc;
	int max_emu_to_plc_io;
	PhotoEye[] photoeye;
	bool[] photoeyestate;
	int[] photoeyeIO;
	bool[] photoeye_excel_override;
	bool[] photoeye_excel_val;
	
	
	BmaInput[] bmas;
	int[] bma_OK_io;
	int[] bma_OOG_io;
	
	
	BitArray emu_to_plc_bits;
	private int lasttrigger =0;
	private bool loggingenabled = false;
	private bool debuglogenabled = false;
	private bool ready = false;
	
	private bool getnewinputs = false;
	
	
	
	
	// Use this for initialization
	void Start () {
		Logger.WriteLine("[INFO]","STARTUP","",true);
		
		IataTags.initIATATags();
				
		
		
		object[] obj = GameObject.FindSceneObjectsOfType(typeof (GameObject));
		encoding = new System.Text.ASCIIEncoding();		
		MySql_connection db = new MySql_connection();	
		db.setConnectionString(GetConnectionString());
		try{
			plc_to_emu = db.getPLCtoEMU();		
			emu_to_plc = db.getEMUtoPLC();
		}catch(Exception e){
			Logger.WriteLine("[ERROR]","Initialization","Unable to get IO data from mysqlconnection.dll. Check Database connectivity and dbConfig.txt ConnectionString.",true);
		}
		
		if(emu_to_plc == null)
		{
			Debug.Log("Unable to get IO data from mysqlconnection.dll. Check Database connectivity.");
			Logger.WriteLine("[ERROR]","Initialization","Unable to get IO data from mysqlconnection.dll. Check Database connectivity.",true);
			return;
		}
		else{
			Logger.WriteLine("[INFO]","Initialization","Successfully retrieved config values from the database. Lengths: "+plc_to_emu.Count+", "+emu_to_plc.Count,true);
			Debug.Log("Successfully retrieved config values from the database. Lengths: "+plc_to_emu.Count+", "+emu_to_plc.Count);
		}
		
		/////////////   EMU to PLC
		
		max_emu_to_plc_io = 0;		
		int photoeyecount = 0;
		int conveyer_disconnect_count =0;
		int conveyer_merge_disconnect_count =0;
		int conveyer_turn_disconnect_count = 0;
		int hsd_extended_prox_count = 0;
		int hsd_retracted_prox_count = 0;
		int vsu_up_prox_count = 0;
		int vsu_down_prox_count =0;
		int cs_button_input_count = 0; //pressed or not
		int bma_count = 0;
		foreach (object o in obj)
		{
			GameObject g = (GameObject) o;
			if(g.GetComponent<PhotoEye>()!=null)
			{
				if(emu_to_plc.ContainsKey(g.GetComponent<PhotoEye>().IO_name_photoeye))
				{
					if(emu_to_plc[g.GetComponent<PhotoEye>().IO_name_photoeye]>max_emu_to_plc_io){
						max_emu_to_plc_io = emu_to_plc[g.GetComponent<PhotoEye>().IO_name_photoeye];
					}
					photoeyecount++;
				}
			}
			else if(g.GetComponent<ConveyorForward>()!=null){
				if(emu_to_plc.ContainsKey(g.GetComponent<ConveyorForward>().IO_name_disconnect))
				{
					conveyer_disconnect_count ++;
				}
			}
			else if(g.GetComponent<ConveyorTurn>()!=null){
				if(emu_to_plc.ContainsKey(g.GetComponent<ConveyorTurn>().IO_name_disconnect))
				{
					conveyer_turn_disconnect_count ++;
				}
			}
			else if(g.GetComponent<MergeConveyor>()!=null){
				if(emu_to_plc.ContainsKey(g.GetComponent<MergeConveyor>().IO_name_disconnect))
				{
					conveyer_merge_disconnect_count ++;
				}
			}else if(g.GetComponent<DiverterLeftC>()!=null){
				if(emu_to_plc.ContainsKey(g.GetComponent<DiverterLeftC>().IO_name_extended_prox))
				{
					hsd_extended_prox_count ++;
				}
				if(emu_to_plc.ContainsKey(g.GetComponent<DiverterLeftC>().IO_name_retracted_prox))
				{
					hsd_retracted_prox_count ++;
				}
			}else if(g.GetComponent<VerticalDiverter>()!=null){
				if(emu_to_plc.ContainsKey(g.GetComponent<VerticalDiverter>().IO_name_up_prox))
				{
					vsu_up_prox_count ++;
				}
				if(emu_to_plc.ContainsKey(g.GetComponent<VerticalDiverter>().IO_name_dn_prox))
				{
					vsu_down_prox_count ++;
				}
			}else if(g.GetComponent<ControlStationButton>()!=null){
				if(emu_to_plc.ContainsKey(g.GetComponent<ControlStationButton>().IO_input))
				{
					cs_button_input_count ++;
				}
			}else if(g.GetComponent<BmaInput>()!=null){
				if(emu_to_plc.ContainsKey(g.GetComponent<BmaInput>().IO_name_BMA_OK))
				{
					bma_count ++;
				}
			}
		}
		
		
		
		
		
		
		
		
		
		if(max_emu_to_plc_io%8!=0)
		{
			max_emu_to_plc_io = (int)(Math.Floor((double)max_emu_to_plc_io/8)+1)*8;
		}
	
		max_emu_to_plc_io = 1312; // make the response larger, fixed size. (temporary ? )
		emu_to_plc_bits = new BitArray(max_emu_to_plc_io);
		
		
		photoeye = new PhotoEye[photoeyecount];
		photoeyestate = new bool[photoeyecount];
		photoeyeIO = new int[photoeyecount];
		photoeye_excel_override = new bool[photoeyecount];
		photoeye_excel_val = new bool[photoeyecount];
		photoeyecount = 0;
		
		
		conveyer_disconnect = new ConveyorForward[conveyer_disconnect_count];
		conveyer_disconnect_state = new bool[conveyer_disconnect_count];
		conveyer_disconnect_laststate= new bool[conveyer_disconnect_count];
		conveyer_disconnect_io = new int[conveyer_disconnect_count];		
		conveyer_disconnect_count =0;
		
		conveyer_turn_disconnect = new ConveyorTurn[conveyer_turn_disconnect_count];
		conveyer_turn_disconnect_state = new bool[conveyer_turn_disconnect_count];
		conveyer_turn_disconnect_laststate= new bool[conveyer_turn_disconnect_count];
		conveyer_turn_disconnect_io = new int[conveyer_turn_disconnect_count];		
		conveyer_turn_disconnect_count =0;
		
		conveyer_merge_disconnect = new MergeConveyor[conveyer_merge_disconnect_count];
		conveyer_merge_disconnect_state = new bool[conveyer_merge_disconnect_count];
		conveyer_merge_disconnect_laststate= new bool[conveyer_merge_disconnect_count];
		conveyer_merge_disconnect_io = new int[conveyer_merge_disconnect_count];		
		conveyer_merge_disconnect_count =0;
		
		
		hsd_extended_prox = new DiverterLeftC[hsd_extended_prox_count];
		hsd_extended_prox_state = new bool[hsd_extended_prox_count];
		hsd_extended_prox_laststate= new bool[hsd_extended_prox_count];
		hsd_extended_prox_io = new int[hsd_extended_prox_count];		
		hsd_extended_prox_count =0;
		
		hsd_retracted_prox = new DiverterLeftC[hsd_retracted_prox_count];
		hsd_retracted_prox_state = new bool[hsd_retracted_prox_count];
		hsd_retracted_prox_laststate= new bool[hsd_retracted_prox_count];
		hsd_retracted_prox_io = new int[hsd_retracted_prox_count];		
		hsd_retracted_prox_count =0;
		
		
		vsu_up_prox = new VerticalDiverter[vsu_up_prox_count];
		vsu_up_prox_state = new bool[vsu_up_prox_count];
		vsu_up_prox_laststate= new bool[vsu_up_prox_count];
		vsu_up_prox_io = new int[vsu_up_prox_count];		
		vsu_up_prox_count =0;
		
		vsu_down_prox = new VerticalDiverter[vsu_down_prox_count];
		vsu_down_prox_state = new bool[vsu_down_prox_count];
		vsu_down_prox_laststate= new bool[vsu_down_prox_count];
		vsu_down_prox_io = new int[vsu_down_prox_count];		
		vsu_down_prox_count =0;
		
		cs_button_input = new ControlStationButton[cs_button_input_count];
		cs_button_input_io = new int[cs_button_input_count];
		cs_button_input_count = 0;
		
		
		
		
		bmas = new BmaInput[bma_count];
		bma_OK_io = new int[bma_count];
		bma_OOG_io = new int[bma_count];
		bma_count =0;
		
		
		foreach (object o in obj)
		{
			GameObject g = (GameObject) o;
			if(g.GetComponent<PhotoEye>()!=null)
			{
				if(emu_to_plc.ContainsKey(g.GetComponent<PhotoEye>().IO_name_photoeye))
				{
					photoeye[photoeyecount] = g.GetComponent<PhotoEye>();
					photoeyestate[photoeyecount] = photoeye[photoeyecount].getState();
					photoeyeIO[photoeyecount] = emu_to_plc[photoeye[photoeyecount].IO_name_photoeye];
					photoeyecount ++;
				}
			}
			else if(g.GetComponent<ConveyorForward>()!=null)
			{
				if(emu_to_plc.ContainsKey(g.GetComponent<ConveyorForward>().IO_name_disconnect))
				{
					conveyer_disconnect[conveyer_disconnect_count] = g.GetComponent<ConveyorForward>();
					conveyer_disconnect_state[conveyer_disconnect_count] = conveyer_disconnect[conveyer_disconnect_count].getState();
					conveyer_disconnect_io[conveyer_disconnect_count] = emu_to_plc[conveyer_disconnect[conveyer_disconnect_count].IO_name_disconnect];
					conveyer_disconnect_count ++;
				}
			}
			else if(g.GetComponent<ConveyorTurn>()!=null)
			{
				if(emu_to_plc.ContainsKey(g.GetComponent<ConveyorTurn>().IO_name_disconnect))
				{
					conveyer_turn_disconnect[conveyer_turn_disconnect_count] = g.GetComponent<ConveyorTurn>();
					conveyer_turn_disconnect_state[conveyer_turn_disconnect_count] = conveyer_turn_disconnect[conveyer_turn_disconnect_count].getState();
					conveyer_turn_disconnect_io[conveyer_turn_disconnect_count] = emu_to_plc[conveyer_turn_disconnect[conveyer_turn_disconnect_count].IO_name_disconnect];
					conveyer_turn_disconnect_count ++;
				}
			}
			else if(g.GetComponent<MergeConveyor>()!=null)
			{
				if(emu_to_plc.ContainsKey(g.GetComponent<MergeConveyor>().IO_name_disconnect))
				{
					conveyer_merge_disconnect[conveyer_merge_disconnect_count] = g.GetComponent<MergeConveyor>();
					conveyer_merge_disconnect_state[conveyer_merge_disconnect_count] = conveyer_merge_disconnect[conveyer_merge_disconnect_count].getState();
					conveyer_merge_disconnect_io[conveyer_merge_disconnect_count] = emu_to_plc[conveyer_merge_disconnect[conveyer_merge_disconnect_count].IO_name_disconnect];
					conveyer_merge_disconnect_count ++;
				}
			}
			else if(g.GetComponent<DiverterLeftC>()!=null)
			{
				if(emu_to_plc.ContainsKey(g.GetComponent<DiverterLeftC>().IO_name_extended_prox))
				{
					hsd_extended_prox[hsd_extended_prox_count] = g.GetComponent<DiverterLeftC>();
					hsd_extended_prox_state[hsd_extended_prox_count] = hsd_extended_prox[hsd_extended_prox_count].getState();
					hsd_extended_prox_io[hsd_extended_prox_count] = emu_to_plc[hsd_extended_prox[hsd_extended_prox_count].IO_name_extended_prox];
					hsd_extended_prox_count ++;
				}
				if(emu_to_plc.ContainsKey(g.GetComponent<DiverterLeftC>().IO_name_retracted_prox))
				{
					hsd_retracted_prox[hsd_retracted_prox_count] = g.GetComponent<DiverterLeftC>();
					hsd_retracted_prox_state[hsd_retracted_prox_count] = hsd_retracted_prox[hsd_retracted_prox_count].getState();
					hsd_retracted_prox_io[hsd_retracted_prox_count] = emu_to_plc[hsd_retracted_prox[hsd_retracted_prox_count].IO_name_retracted_prox];
					hsd_retracted_prox_count ++;
				}
			}else if(g.GetComponent<VerticalDiverter>()!=null)
			{
				if(emu_to_plc.ContainsKey(g.GetComponent<VerticalDiverter>().IO_name_up_prox))
				{
					vsu_up_prox[vsu_up_prox_count] = g.GetComponent<VerticalDiverter>();
					vsu_up_prox_state[vsu_up_prox_count] = vsu_up_prox[vsu_up_prox_count].getState();
					vsu_up_prox_io[vsu_up_prox_count] = emu_to_plc[vsu_up_prox[vsu_up_prox_count].IO_name_up_prox];
					vsu_up_prox_count ++;
				}
				if(emu_to_plc.ContainsKey(g.GetComponent<VerticalDiverter>().IO_name_dn_prox))
				{
					vsu_down_prox[vsu_down_prox_count] = g.GetComponent<VerticalDiverter>();
					vsu_down_prox_state[vsu_down_prox_count] = vsu_down_prox[vsu_down_prox_count].getState();
					vsu_down_prox_io[vsu_down_prox_count] = emu_to_plc[vsu_down_prox[vsu_down_prox_count].IO_name_dn_prox];
					vsu_down_prox_count ++;
				}
			}		
			else if(g.GetComponent<ControlStationButton>()!=null)
			{
				if(emu_to_plc.ContainsKey(g.GetComponent<ControlStationButton>().IO_input))
				{
					cs_button_input[cs_button_input_count] = g.GetComponent<ControlStationButton>();
					cs_button_input_io[cs_button_input_count] = emu_to_plc[cs_button_input[cs_button_input_count].IO_input];
					cs_button_input_count ++;
				}
			}else if(g.GetComponent<BmaInput>()!=null){
				if(emu_to_plc.ContainsKey(g.GetComponent<BmaInput>().IO_name_BMA_OK))
				{
					bmas[bma_count] = g.GetComponent<BmaInput>();
					bma_OK_io[bma_count] = emu_to_plc[bmas[bma_count].IO_name_BMA_OK];
					bma_OOG_io[bma_count] = emu_to_plc[bmas[bma_count].IO_name_BMA_OOG];
					bma_count ++;
				}
			}
		}
		
		
		
		
		/////////////  PLC to EMU
		
		
		/// Builds up the arrays for the different devices
		int conveyercount =0;
		int conveyer_turn_run_count = 0;
		int conveyer_merge_run_count = 0;
		int hsd_cycle_count=0;
		int vsu_cycle_count = 0;
		int cs_button_output_count = 0;
		int pe_assign_security_clear_count= 0;
		int pe_assign_security_alarmed_count= 0;
		int pe_assign_security_pending_count= 0;
		
			
			
			
			
		foreach (object o in obj)
		{
			GameObject g = (GameObject) o;
			if(g.GetComponent<ConveyorForward>()!=null)
			{
				if(plc_to_emu.ContainsKey(g.GetComponent<ConveyorForward>().IO_name_motor_run))
				{
					conveyercount++;
				}
			}
			else if(g.GetComponent<ConveyorTurn>()!=null)
			{
				if(plc_to_emu.ContainsKey(g.GetComponent<ConveyorTurn>().IO_name_motor_run))
				{
					conveyer_turn_run_count++;
				}
			}
			else if(g.GetComponent<MergeConveyor>()!=null)
			{
				if(plc_to_emu.ContainsKey(g.GetComponent<MergeConveyor>().IO_name_motor_run))
				{
					conveyer_merge_run_count++;
				}
			}
			else if(g.GetComponent<DiverterLeftC>()!=null)
			{
				if(plc_to_emu.ContainsKey(g.GetComponent<DiverterLeftC>().IO_name_cycle_command))
				{
					hsd_cycle_count++;
				}
			}
			else if(g.GetComponent<VerticalDiverter>()!=null)
			{
				if(plc_to_emu.ContainsKey(g.GetComponent<VerticalDiverter>().IO_name_cycle_cmd))
				{
					vsu_cycle_count++;
				}
			}else if(g.GetComponent<ControlStationButton>()!=null)
			{
				if(plc_to_emu.ContainsKey(g.GetComponent<ControlStationButton>().IO_output))
				{
					cs_button_output_count++;
				}
			}
			else if(g.GetComponent<PhotoEye>()!=null)
			{
				if(plc_to_emu.ContainsKey(g.GetComponent<PhotoEye>().IO_assign_bag_clear))
				{
					pe_assign_security_clear_count++;
				}
				if(plc_to_emu.ContainsKey(g.GetComponent<PhotoEye>().IO_assign_bag_alarmed))
				{
					pe_assign_security_alarmed_count++;
				}
				if(plc_to_emu.ContainsKey(g.GetComponent<PhotoEye>().IO_assign_bag_pending))
				{
					pe_assign_security_pending_count++;
				}
			}
		}
		
		
		
		
		
		
		
		
		
		conveyer = new ConveyorForward[conveyercount];		
		conveyerstate = new bool[conveyercount];
		conveyerlaststate = new bool[conveyercount];
		conveyerio = new int[conveyercount];
		conveyer_excel_override = new bool[conveyercount];
		conveyer_excel_val = new bool[conveyercount];
		conveyercount =0;		
		
		conveyer_turn_run = new ConveyorTurn[conveyer_turn_run_count];		
		conveyer_turn_run_state = new bool[conveyer_turn_run_count];
		conveyer_turn_run_laststate = new bool[conveyer_turn_run_count];
		conveyer_turn_run_io = new int[conveyer_turn_run_count];
		conveyer_turn_run_excel_override = new bool[conveyer_turn_run_count];
		conveyer_turn_run_excel_val = new bool[conveyer_turn_run_count];		
		conveyer_turn_run_count =0;	
		
		conveyer_merge_run = new MergeConveyor[conveyer_merge_run_count];		
		conveyer_merge_run_state = new bool[conveyer_merge_run_count];
		conveyer_merge_run_laststate = new bool[conveyer_merge_run_count];
		conveyer_merge_run_io = new int[conveyer_merge_run_count];	
		conveyer_merge_run_excel_override = new bool[conveyer_merge_run_count];
		conveyer_merge_run_excel_val = new bool[conveyer_merge_run_count];	
		conveyer_merge_run_count =0;
		
		hsd_cycle = new DiverterLeftC[hsd_cycle_count];
		hsd_cycle_state = new bool[hsd_cycle_count];
		hsd_cycle_laststate = new bool[hsd_cycle_count];
		hsd_last_cycle_io = new bool[hsd_cycle_count];
		hsd_cycle_io = new int[hsd_cycle_count];		
		hsd_cycle_count =0;
		
		vsu_cycle = new VerticalDiverter[vsu_cycle_count];
		vsu_cycle_state = new bool[vsu_cycle_count];
		vsu_cycle_laststate = new bool[vsu_cycle_count];
		vsu_last_cycle_io = new bool[vsu_cycle_count];
		vsu_cycle_io = new int[vsu_cycle_count];		
		vsu_cycle_count =0;
		
		
		cs_button_output = new ControlStationButton[cs_button_output_count];
		cs_button_output_io = new int[cs_button_output_count];
		cs_button_output_state = new bool[cs_button_output_count];
		cs_button_output_laststate = new bool[cs_button_output_count];		
		cs_button_output_count=0;
		
		
		pe_assign_security_clear = new PhotoEye[pe_assign_security_clear_count]; //photoeyeassigns
		pe_assign_security_clear_state = new bool[pe_assign_security_clear_count];
		pe_assign_security_clear_laststate = new bool[pe_assign_security_clear_count];		
		pe_assign_security_clear_io = new int[pe_assign_security_clear_count];
		pe_assign_security_clear_count = 0;
 		pe_assign_security_alarmed = new PhotoEye[pe_assign_security_alarmed_count];
		pe_assign_security_alarmed_state = new bool[pe_assign_security_alarmed_count];
		pe_assign_security_alarmed_laststate = new bool[pe_assign_security_alarmed_count];
		pe_assign_security_alarmed_io = new int[pe_assign_security_alarmed_count];
		pe_assign_security_alarmed_count = 0;
		pe_assign_security_pending = new PhotoEye[pe_assign_security_pending_count];
		pe_assign_security_pending_state = new bool[pe_assign_security_pending_count];
		pe_assign_security_pending_laststate = new bool[pe_assign_security_pending_count];
		pe_assign_security_pending_io = new int[pe_assign_security_pending_count];
		pe_assign_security_pending_count = 0;
		
		foreach (object o in obj)
		{
			GameObject g = (GameObject) o;
			if(g.GetComponent<ConveyorForward>()!=null)
			{
				if(plc_to_emu.ContainsKey(g.GetComponent<ConveyorForward>().IO_name_motor_run))
				{
					conveyer[conveyercount] = g.GetComponent<ConveyorForward>();
					conveyerstate[conveyercount] = conveyer[conveyercount].getState();
					conveyerio[conveyercount] = plc_to_emu[conveyer[conveyercount].IO_name_motor_run];
					conveyercount ++;
				}
			}
			else if(g.GetComponent<ConveyorTurn>()!=null)
			{
				if(plc_to_emu.ContainsKey(g.GetComponent<ConveyorTurn>().IO_name_motor_run))
				{
					conveyer_turn_run[conveyer_turn_run_count] = g.GetComponent<ConveyorTurn>();
					conveyer_turn_run_state[conveyer_turn_run_count] = conveyer_turn_run[conveyer_turn_run_count].getState();
					conveyer_turn_run_io[conveyer_turn_run_count] = plc_to_emu[conveyer_turn_run[conveyer_turn_run_count].IO_name_motor_run];
					conveyer_turn_run_count ++;
				}
			}
			else if(g.GetComponent<MergeConveyor>()!=null)
			{
				if(plc_to_emu.ContainsKey(g.GetComponent<MergeConveyor>().IO_name_motor_run))
				{
					conveyer_merge_run[conveyer_merge_run_count] = g.GetComponent<MergeConveyor>();
					conveyer_merge_run_state[conveyer_merge_run_count] = conveyer_merge_run[conveyer_merge_run_count].getState();
					conveyer_merge_run_io[conveyer_merge_run_count] = plc_to_emu[conveyer_merge_run[conveyer_merge_run_count].IO_name_motor_run];
					conveyer_merge_run_count ++;
				}
			}
			else if(g.GetComponent<DiverterLeftC>()!=null)
			{
				if(plc_to_emu.ContainsKey(g.GetComponent<DiverterLeftC>().IO_name_cycle_command))
				{
					hsd_cycle[hsd_cycle_count] = g.GetComponent<DiverterLeftC>();
					hsd_cycle_state[hsd_cycle_count] = hsd_cycle[hsd_cycle_count].getState();  //   <<== prolly not needed
					hsd_cycle_io[hsd_cycle_count] = plc_to_emu[hsd_cycle[hsd_cycle_count].IO_name_cycle_command];
					hsd_cycle_count ++;
				}
			}
			else if(g.GetComponent<VerticalDiverter>()!=null)
			{
				if(plc_to_emu.ContainsKey(g.GetComponent<VerticalDiverter>().IO_name_cycle_cmd))
				{
					vsu_cycle[vsu_cycle_count] = g.GetComponent<VerticalDiverter>();
					vsu_cycle_state[vsu_cycle_count] = vsu_cycle[vsu_cycle_count].getState();  //   <<== prolly not needed
					vsu_cycle_io[vsu_cycle_count] = plc_to_emu[vsu_cycle[vsu_cycle_count].IO_name_cycle_cmd];
					vsu_cycle_count ++;
				}
			}else if(g.GetComponent<ControlStationButton>()!=null)
			{
				if(plc_to_emu.ContainsKey(g.GetComponent<ControlStationButton>().IO_output))
				{
					cs_button_output[cs_button_output_count] = g.GetComponent<ControlStationButton>();
					cs_button_output_state[cs_button_output_count] = cs_button_output[cs_button_output_count].getState();  //   <<== prolly not needed
					cs_button_output_io[cs_button_output_count] = plc_to_emu[cs_button_output[cs_button_output_count].IO_output];
					cs_button_output_count ++;
				}
			}else if(g.GetComponent<PhotoEye>()!=null) // Assumes all states are false to start.
			{
				if(plc_to_emu.ContainsKey(g.GetComponent<PhotoEye>().IO_assign_bag_clear))
				{
					pe_assign_security_clear[pe_assign_security_clear_count] = g.GetComponent<PhotoEye>();
					pe_assign_security_clear_io[pe_assign_security_clear_count] = plc_to_emu[pe_assign_security_clear[pe_assign_security_clear_count].IO_assign_bag_clear];
					pe_assign_security_clear_count++;
				}
				if(plc_to_emu.ContainsKey(g.GetComponent<PhotoEye>().IO_assign_bag_alarmed))
				{
					pe_assign_security_alarmed[pe_assign_security_alarmed_count] = g.GetComponent<PhotoEye>();
					pe_assign_security_alarmed_io[pe_assign_security_alarmed_count] = plc_to_emu[pe_assign_security_clear[pe_assign_security_alarmed_count].IO_assign_bag_alarmed];
					pe_assign_security_alarmed_count++;
				}
				if(plc_to_emu.ContainsKey(g.GetComponent<PhotoEye>().IO_assign_bag_pending))
				{
					pe_assign_security_pending[pe_assign_security_pending_count] = g.GetComponent<PhotoEye>();
					pe_assign_security_pending_io[pe_assign_security_pending_count] = plc_to_emu[pe_assign_security_pending[pe_assign_security_pending_count].IO_assign_bag_pending];
					pe_assign_security_pending_count++;
				}
			}
		}
		
			
		
		
		Debug.Log("IO points from database initialized successfully.\r\n");
		Logger.WriteLine("[INFO]","IO initialized","IO points from database initialized successfully.\r\n",true);
		ready = true;
		
		listenerThread = new Thread(new ThreadStart(DoListen));
        listenerThread.Start();
		
		
		excelListenerThread = new Thread(new ThreadStart(DoExcelListen));
		excelListenerThread.Start();
		
		
		
		//responderThread = new Thread(new ThreadStart(DoResponder));
		//responderThread.Start();
		
	}
	
	void OnApplicationQuit() {
		if(listener !=null){
			listener.Server.Close();			
		}

		if(listenerThread!=null)
		{
			listenerThread.Abort();  			
		}
		if(excelListener !=null){
			excelListener.Server.Close();			
		}

		if(excelListenerThread!=null)
		{
			excelListenerThread.Abort();  			
		} 
		 
    }
	
	
	// This subroutine sends a message to all attached clients
    private void Broadcast(string strMessage)
	{
		UserConnection client;
        // All entries in the clients Hashtable are UserConnection so it is possible
        // to assign it safely.
        foreach(DictionaryEntry entry in clients)
		{
            client = (UserConnection) entry.Value;
            //client.SendData(strMessage);
        }
    }
	
	// This subroutine checks to see if username already exists in the clients 
    // Hashtable.  if it does, send a REFUSE message, otherwise confirm with a JOIN.
    private void ConnectUser(string userName, UserConnection sender)
	{;
		if (clients.Contains(userName))
		{
			ReplyToSender("REFUSE", sender);
		}
		else 
		{
			sender.Name = userName;
//			UpdateStatus(userName + " has joined the chat.");
			clients.Add(userName, sender);
			//lstPlayers.Items.Add(sender.Name);
			// Send a JOIN to sender, and notify all other clients that sender joined
			ReplyToSender("JOIN", sender);
//			SendToClients("CHAT|" + sender.Name + " has joined the chat.", sender);
		}
    }
 
    // This subroutine notifies other clients that sender left the chat, and removes
    // the name from the clients Hashtable
    private void DisconnectUser(UserConnection sender)
	{
//        UpdateStatus(sender.Name + " has left the chat.");
//        SendToClients("CHAT|" + sender.Name + " has left the chat.", sender);
        clients.Remove(sender.Name);
		//lstPlayers.Items.Remove(sender.Name);
    }
 
    // This subroutine is used a background listener thread to allow reading incoming
    // messages without lagging the user interface.
    private void DoListen()
	{
        try {
            // Listen for new connections.
            listener = new TcpListener(System.Net.IPAddress.Any, PORT_NUM);
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
//				UpdateStatus("new connection found: waiting for log-in");
			}while(true);
        } 
		catch(Exception ex){
			//MessageBox.Show(ex.ToString());
        }
	}	
		
	
	private void DoExcelListen()
	{
			// Excel listener
		try {
            // Listen for new connections.
            excelListener = new TcpListener(System.Net.IPAddress.Any, EXCEL_PORT_NUM);
            excelListener.Start();
 
			do
			{
				// Create a new user connection using TcpClient returned by
				// TcpListener.AcceptTcpClient()
				UserConnection excelClient = new UserConnection(excelListener.AcceptTcpClient());
				// Create an event handler to allow the UserConnection to communicate
				// with the window.
				excelClient.LineReceived += new LineReceive(ExcelOnLineReceived);
				//AddHandler client.LineReceived, AddressOf OnLineReceived;
//				UpdateStatus("new connection found: waiting for log-in");
			}while(true);
        } 
		catch(Exception ex){
			//MessageBox.Show(ex.ToString());
        }
		
    }
	
	
	/// This is the Excel event handler
	/// 
	/// 
	
	private void ExcelOnLineReceived(UserConnection sender, byte[] data)
	{
		
		byte[] responsebytes = GetBytes("s");
		
		byte[] iobytes = new byte[640];//data;
		
		for(int i=0;i<1280;i++)
		{
			if(i-2*(i/2)==1)
			{
				iobytes[i/2] += (byte)(data[i]*16);
			}else{
				iobytes[i/2] = (byte)(data[i]);
			}
		}
		
		string excel_to_emu_bit_string = "";
		
		for(var i=0; i<iobytes.Length;i++)
		{
			if(i==320)
			{
				excel_to_emu_bit_string += "\r\n Inputs:";
			}
			excel_to_emu_bit_string += iobytes[i].ToString()+ " ";
			
		}
		Debug.Log("excel override: Outputs:" + excel_to_emu_bit_string + "\r\n Length:"+iobytes.Length.ToString()+"\r\n");
		
		//sender.SendData(responsebytes);
		//return;
		//byte[] iobytes = data;
		
		byte[] byte_io_enabled = new byte[320];
		byte[] byte_iovals = new byte[320];
		
		for(var i=0;i<160;i++)  // lower motor output bits
		{
			byte_iovals[i] = iobytes[i];
		}
		for(var i=0;i<160;i++)
		{
			byte_io_enabled[i] = iobytes[i+160];
		}
		for(var i=0;i<160;i++) // higher photoEye INPUT bits
		{
			byte_iovals[i+160] = iobytes[i+320];
		}
		for(var i=0;i<160;i++)
		{
			byte_io_enabled[i+160] = iobytes[i+320+160];
		}
		
		excel_to_emu_bit_string = "";
		
		for(var i=0; i<byte_io_enabled.Length;i++)
		{
			excel_to_emu_bit_string += byte_io_enabled[i].ToString()+ " ";			
		}
		//Debug.Log("byte_io_enabled:" + excel_to_emu_bit_string + "\r\n Length:"+byte_io_enabled.Length.ToString()+"\r\n");
		
		
		BitArray io_enabled = new System.Collections.BitArray(byte_io_enabled);
		BitArray iovals = new System.Collections.BitArray(byte_iovals);
		
				
		
		
		for(var i=0;i<conveyer.Length;i++)
		{
			conveyer_excel_override[i] = (bool)io_enabled[conveyerio[i]];
			conveyer_excel_val[i] = (bool)iovals[conveyerio[i]];			
		}
		/*for(var i=0;i<conveyer_turn_run.Length;i++)
		{
			Debug.Log(i.ToString()+" "+conveyer_turn_run_excel_override.Length.ToString()+" "+conveyer_turn_run_excel_val.Length.ToString()+" "+iovals.Length.ToString()+" "+io_enabled.Length.ToString()+" "+conveyer_turn_run_io[i].ToString()+"\r\n");
		}*/
		
		for(var i=0;i<conveyer_turn_run.Length;i++)
		{
			conveyer_turn_run_excel_override[i] = (bool)io_enabled[conveyer_turn_run_io[i]];
			conveyer_turn_run_excel_val[i] = (bool)iovals[conveyer_turn_run_io[i]];			
		}
		for(var i=0;i<conveyer_merge_run.Length;i++)
		{
			conveyer_merge_run_excel_override[i] = (bool)io_enabled[conveyer_merge_run_io[i]];
			conveyer_merge_run_excel_val[i] = (bool)iovals[conveyer_merge_run_io[i]];
		}
		for(var i=0;i<photoeye.Length;i++)
		{
			photoeye_excel_override[i] = (bool)io_enabled[photoeyeIO[i]+(160*8)]; // gets it from shifted input
			photoeye_excel_val[i] = (bool)iovals[photoeyeIO[i]+(160*8)]; // gets it from shifted input
		}
		
		
		
		Loom.QueueOnMainThread(()=>{
			for(var i=0;i<conveyer.Length;i++)
			{
				conveyer[i].setExcelOverride(conveyer_excel_override[i],conveyer_excel_val[i]);	
			}
			for(var i=0;i<conveyer_turn_run.Length;i++)
			{
				conveyer_turn_run[i].setExcelOverride(conveyer_turn_run_excel_override[i],conveyer_turn_run_excel_val[i]);
			}
			for(var i=0;i<conveyer_merge_run.Length;i++)
			{
				conveyer_merge_run[i].setExcelOverride(conveyer_merge_run_excel_override[i],conveyer_merge_run_excel_val[i]);	
			}
			
			for(var i=0;i<photoeye.Length;i++)
			{
				photoeye[i].setExcelOverride(photoeye_excel_override[i],photoeye_excel_val[i]);
			}
		});
		
//		byte[] responsebytes = GetBytes("success");
		
		sender.SendData(responsebytes);
		
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
			if(loggingenabled)
			{
				Debug.Log("Ignored Trigger " +  trigger.ToString());
			}
			return;
		}
		lasttrigger = trigger;
		
		
		BitArray iovals = new System.Collections.BitArray(iobytes);
		
		string plc_to_emu_bit_string_bits = "";
		string plc_to_emu_bit_string = "";
		
		for(var i=0; i<iobytes.Length;i++)
		{
			plc_to_emu_bit_string += iobytes[i].ToString()+ " ";
			
		}
		
		/*
		for(var i=0;i<iovals.Length;i++)
		{	
			if(i%8==0)
			{
				plc_to_emu_bit_string_bits+=" |";
			}
			if(i%4==0)
			{
				plc_to_emu_bit_string_bits+=" ";
			}
			
			if(iovals[i]==true){
				plc_to_emu_bit_string_bits +=1;
			}
			else
			{
				plc_to_emu_bit_string_bits +=0;
			}
			
		}*/
		/*
		string plc_to_emu_bit_string_bits = "";
		string plc_to_emu_bit_string = "";
		int tbyte = 0;
		int tbytecount = 7;
		for(var i=0;i<iovals.Length;i++)
		{		
			if(i%8==0)
			{
				tbyte = 0;
				tbytecount =7;	
			}
			
			//iovals[i]
			
			
			
			if(iovals[i]==true){
				tbyte += (int) Math.Pow(2,tbytecount);
				//plc_to_emu_bit_string += "1";
				plc_to_emu_bit_string_bits+="1";
			}else{
				//tbyte += (int)Math.Pow(2,tbytecount);
				//plc_to_emu_bit_string +="0";
				plc_to_emu_bit_string_bits +="0";
			}		
			if(tbytecount==0)
			{
				plc_to_emu_bit_string += tbyte.ToString()+" ";
			}
			tbytecount--;
			
		}*/
		
		if(loggingenabled)
		{
			Debug.Log("Received:\r\n"+plc_to_emu_bit_string);
			//Debug.Log("Received bits:\r\n"+plc_to_emu_bit_string_bits);
		}
		
		
		for(var i=0;i<conveyer.Length;i++)
		{
			if((bool)iovals[conveyerio[i]] != conveyerstate[i]) // The STATE HAS CHANGED!!
			{
				conveyerstate[i]=(bool)iovals[conveyerio[i]];
			}
		}
		for(var i=0;i<conveyer_turn_run.Length;i++)
		{
			if((bool)iovals[conveyer_turn_run_io[i]] != conveyer_turn_run_state[i]) // The STATE HAS CHANGED!!
			{
				conveyer_turn_run_state[i]=(bool)iovals[conveyer_turn_run_io[i]];
			}
		}
		for(var i=0;i<conveyer_merge_run.Length;i++)
		{
			if((bool)iovals[conveyer_merge_run_io[i]] != conveyer_merge_run_state[i]) // The STATE HAS CHANGED!!
			{
				conveyer_merge_run_state[i]=(bool)iovals[conveyer_merge_run_io[i]];
			}
		}
		for(var i=0;i<hsd_cycle.Length;i++)
		{
			if((bool)iovals[hsd_cycle_io[i]] != hsd_last_cycle_io[i]) // The STATE HAS CHANGED!!
			{
				if(debuglogenabled)
				{
					Debug.Log("HSD "+ hsd_cycle_io[i]+ " cycle input changed to "+ ((bool)iovals[hsd_cycle_io[i]]).ToString());	
				}
				if((bool)iovals[hsd_cycle_io[i]]) // if rising edge.
				{
					if(debuglogenabled)
				{
					Debug.Log("HSD "+ hsd_cycle_io[i]+ " toggled to "+ (!hsd_cycle_state[i]).ToString());	
				}
					hsd_cycle_state[i]=!hsd_cycle_state[i];  // then toggle it.					
				}
				hsd_last_cycle_io[i]=(bool)iovals[hsd_cycle_io[i]]; 				
			}
		}
		for(var i=0;i<vsu_cycle.Length;i++)
		{
			if((bool)iovals[vsu_cycle_io[i]] != vsu_last_cycle_io[i]) // The STATE HAS CHANGED!!
			{
				if((bool)iovals[vsu_cycle_io[i]]) // if rising edge.
				{
					vsu_cycle_state[i]=!vsu_cycle_state[i];  // then toggle it.					
				}
				vsu_last_cycle_io[i]=(bool)iovals[vsu_cycle_io[i]]; 				
			}
		}
		for(var i=0;i<cs_button_output.Length;i++)
		{
			if((bool)iovals[cs_button_output_io[i]] != cs_button_output_state[i]) // The STATE HAS CHANGED!!
			{
				cs_button_output_state[i]=(bool)iovals[cs_button_output_io[i]];
			}
		}
		for(var i=0;i<pe_assign_security_clear.Length;i++)
		{
			if((bool)iovals[pe_assign_security_clear_io[i]] != pe_assign_security_clear_state[i]) // The STATE HAS CHANGED!!
			{
				//Debug.Log ("changed Clear val");
				pe_assign_security_clear_state[i]=(bool)iovals[pe_assign_security_clear_io[i]];
			}
		}
		for(var i=0;i<pe_assign_security_alarmed.Length;i++)
		{
			if((bool)iovals[pe_assign_security_alarmed_io[i]] != pe_assign_security_alarmed_state[i]) // The STATE HAS CHANGED!!
			{
				pe_assign_security_alarmed_state[i]=(bool)iovals[pe_assign_security_alarmed_io[i]];
			}
		}
		for(var i=0;i<pe_assign_security_pending.Length;i++)
		{
			if((bool)iovals[pe_assign_security_pending_io[i]] != pe_assign_security_pending_state[i]) // The STATE HAS CHANGED!!
			{
				pe_assign_security_pending_state[i]=(bool)iovals[pe_assign_security_pending_io[i]];
			}
		}
		
		
		int heartlog = Logger.counter++;
		if(heartlog==100)
		{
			Logger.WriteLine("[INFO]","COM_SRV","Heartbeat",true);
			Logger.counter = 0;
		}
		
		// check if different.
				
		Loom.QueueOnMainThread(()=>{
			for(var i=0;i<conveyer.Length;i++)
			{
				if(conveyerstate[i] !=conveyerlaststate[i])
				{
					if(conveyer[i].GetType()!=System.Type.GetType("ConveyorForward")){
						
					}
					else if(conveyer[i]==null){
						Logger.WriteLine("[FATAL]","COM_SRV","NULL FOUND!!!!  Somehow this reference has been lost!",true);
						
						//Debug.Log(						
					}
					conveyer[i].setState(conveyerstate[i]);
					conveyerlaststate[i] = conveyerstate[i];
				}		
			}
			for(var i=0;i<conveyer_turn_run.Length;i++)
			{
				if(conveyer_turn_run_state[i] !=conveyer_turn_run_laststate[i])
				{
					conveyer_turn_run[i].setState(conveyer_turn_run_state[i]);
					conveyer_turn_run_laststate[i] = conveyer_turn_run_state[i];
				}		
			}
			for(var i=0;i<conveyer_merge_run.Length;i++)
			{
				if(conveyer_merge_run_state[i] !=conveyer_merge_run_laststate[i])
				{
					conveyer_merge_run[i].setState(conveyer_merge_run_state[i]);
					conveyer_merge_run_laststate[i] = conveyer_merge_run_state[i];
				}		
			}
			for(var i=0;i<hsd_cycle.Length;i++)
			{
				if(hsd_cycle_state[i] !=hsd_cycle_laststate[i])
				{
					hsd_cycle[i].setState(hsd_cycle_state[i]);
					hsd_cycle_laststate[i] = hsd_cycle_state[i];
				}		
			}
			for(var i=0;i<vsu_cycle.Length;i++)
			{
				if(vsu_cycle_state[i] !=vsu_cycle_laststate[i])
				{
					vsu_cycle[i].setState(vsu_cycle_state[i]);
					vsu_cycle_laststate[i] = vsu_cycle_state[i];
				}		
			}
			for(var i=0;i<cs_button_output.Length;i++)
			{
				if(cs_button_output_state[i] !=cs_button_output_laststate[i])
				{
					cs_button_output[i].setState(cs_button_output_state[i]);
					cs_button_output_laststate[i] = cs_button_output_state[i];
				}		
			}
			for(var i=0;i<pe_assign_security_clear.Length;i++)
			{
				if(pe_assign_security_clear_state[i] !=pe_assign_security_clear_laststate[i])
				{
					pe_assign_security_clear[i].setAssignClear(pe_assign_security_clear_state[i]);
					pe_assign_security_clear_laststate[i] = pe_assign_security_clear_state[i];
				}		
			}
			for(var i=0;i<pe_assign_security_alarmed.Length;i++)
			{
				if(pe_assign_security_alarmed_state[i] !=pe_assign_security_alarmed_laststate[i])
				{
					pe_assign_security_alarmed[i].setAssignAlarmed(pe_assign_security_alarmed_state[i]);
					pe_assign_security_alarmed_laststate[i] = pe_assign_security_alarmed_state[i];
				}		
			}
			for(var i=0;i<pe_assign_security_pending.Length;i++)
			{
				if(pe_assign_security_pending_state[i] !=pe_assign_security_pending_laststate[i])
				{
					pe_assign_security_pending[i].setAssignPending(pe_assign_security_pending_state[i]);
					pe_assign_security_pending_laststate[i] = pe_assign_security_pending_state[i];
				}		
			}
			
			
			
		});
		
		// copy over last state
		
		
		//Loom.QueueOnMainThread(()=>{

		//	GameObject.Find("M_TC1_01").GetComponent<ConveyorForward>().setState(true);
		//});
		
		
		//sender.SendData("Beltname:"+conveyern + " iopoint:" + iopointst + " previousstate:"+previousstate+" newstate:"+newstate + " input byte length:"+iovals.Length.ToString());
		
		
		
		//return;
		
		/*
		byte[] overbyte = new byte[164];		
		for(var i=0;i<overbyte.Length;i++)
		{
			if(i%4==0)
			{
				overbyte[i] = (byte)(i/4+1);				
			}	
		}
		
		emu_to_plc_bits = new System.Collections.BitArray(overbyte);
		*/
		
		string triggerstring = "";
		
		int triggerpointer = max_emu_to_plc_io-32; //index of start of 40th dint;
		int temptrigger = trigger;
		for(var i=0;i<32;i++){
			int bit = (int)Math.Floor(trigger/Math.Pow(2,31-i));
			triggerstring += trigger.ToString() + " ";
			if(bit ==1)
			{
				trigger = trigger - (int)Math.Pow (2,31-i);
				emu_to_plc_bits[triggerpointer+31-i] = true; 
			}
			else
			{				
				emu_to_plc_bits[triggerpointer+31-i] = false; //false; 
			}
			 
		}
		/*for(var i=0;i<emu_to_plc_bits.Length;i++)
		{
			emu_to_plc_bits[i]=i;
		}*/
		
		if(!getnewinputs)
		{
			sendResponse(sender,emu_to_plc_bits,temptrigger.ToString());		
			return;
		}
		
		if(firstpass)
		{
			//firstpass = false;
			Loom.QueueOnMainThread(()=>{
				for(var i=0;i<photoeye.Length;i++)
				{
					emu_to_plc_bits[photoeyeIO[i]] = photoeye[i].getState();
				}
				for(var i=0;i<conveyer_disconnect.Length;i++)
				{
					emu_to_plc_bits[conveyer_disconnect_io[i]] = conveyer_disconnect[i].getDisconnect();
				}
				for(var i=0;i<conveyer_turn_disconnect.Length;i++)
				{
					emu_to_plc_bits[conveyer_turn_disconnect_io[i]] = conveyer_turn_disconnect[i].getDisconnect();
				}
				for(var i=0;i<conveyer_merge_disconnect.Length;i++)
				{
					emu_to_plc_bits[conveyer_merge_disconnect_io[i]] = conveyer_merge_disconnect[i].getDisconnect();
				}
				for(var i=0;i<hsd_extended_prox.Length;i++)
				{
					emu_to_plc_bits[hsd_extended_prox_io[i]] = hsd_extended_prox[i].GetExtended();
				}
				for(var i=0;i<hsd_retracted_prox.Length;i++)
				{
					emu_to_plc_bits[hsd_retracted_prox_io[i]] = hsd_retracted_prox[i].GetRetracted();
				}
				for(var i=0;i<vsu_up_prox.Length;i++)
				{
					emu_to_plc_bits[vsu_up_prox_io[i]] = vsu_up_prox[i].GetExtended();
				}
				for(var i=0;i<vsu_down_prox.Length;i++)
				{
					emu_to_plc_bits[vsu_down_prox_io[i]] = vsu_down_prox[i].GetRetracted();
				}
				for(var i=0;i<cs_button_input.Length;i++)
				{
					emu_to_plc_bits[cs_button_input_io[i]] = cs_button_input[i].getState();
				}
				for(var i=0;i<bmas.Length;i++)
				{
					emu_to_plc_bits[bma_OK_io[i]] = bmas[i].getBMA_OK();
					emu_to_plc_bits[bma_OOG_io[i]] = bmas[i].getBMA_OOG();
				}
				sendResponse(sender,emu_to_plc_bits,temptrigger.ToString());
				
			});
			
		}
		else{
			Loom.QueueOnMainThread(()=>{
				for(var i=0;i<photoeye.Length;i++)
				{
					emu_to_plc_bits[photoeyeIO[i]] = photoeye[i].getState();
				}
				for(var i=0;i<conveyer_disconnect.Length;i++)
				{
					emu_to_plc_bits[conveyer_disconnect_io[i]] = conveyer_disconnect[i].getDisconnect();
				}
				for(var i=0;i<conveyer_turn_disconnect.Length;i++)
				{
					emu_to_plc_bits[conveyer_turn_disconnect_io[i]] = conveyer_turn_disconnect[i].getDisconnect();
				}
				for(var i=0;i<conveyer_merge_disconnect.Length;i++)
				{
					emu_to_plc_bits[conveyer_merge_disconnect_io[i]] = conveyer_merge_disconnect[i].getDisconnect();
				}	
				for(var i=0;i<hsd_extended_prox.Length;i++)
				{
					emu_to_plc_bits[hsd_extended_prox_io[i]] = hsd_extended_prox[i].GetExtended();
				}
				for(var i=0;i<hsd_retracted_prox.Length;i++)
				{
					emu_to_plc_bits[hsd_retracted_prox_io[i]] = hsd_retracted_prox[i].GetRetracted();
				}
				for(var i=0;i<vsu_up_prox.Length;i++)
				{
					emu_to_plc_bits[vsu_up_prox_io[i]] = vsu_up_prox[i].GetExtended();
				}
				for(var i=0;i<vsu_down_prox.Length;i++)
				{
					emu_to_plc_bits[vsu_down_prox_io[i]] = vsu_down_prox[i].GetRetracted();
				}
				for(var i=0;i<cs_button_input.Length;i++)
				{
					emu_to_plc_bits[cs_button_input_io[i]] = cs_button_input[i].getState();
				}
				for(var i=0;i<bmas.Length;i++)
				{
					emu_to_plc_bits[bma_OK_io[i]] = bmas[i].getBMA_OK();
					emu_to_plc_bits[bma_OOG_io[i]] = bmas[i].getBMA_OOG();
				}
			});			
			sendResponse(sender,emu_to_plc_bits,temptrigger.ToString());
		}
		
		
			
		
		
		//sender.SendData(Encoding.UTF8.GetString(BitArrayToByteArray(emu_to_plc_bits)));
		
		
		
        string[] dataArray;
        // Message parts are divided by "|"  Break the string into an array accordingly.
		// Basically what happens here is that it is possible to get a flood of data during
		// the lock where we have combined commands and overflow
		// to simplify this proble, all I do is split the response by char 13 and then look
		// at the command, if the command is unknown, I consider it a junk message
		// and dump it, otherwise I act on it
		/*
		dataArray = data.Split((char) 13);
        dataArray = dataArray[0].Split((char) 124);
 
        // dataArray(0) is the command.
        switch( dataArray[0])
		{
            case "CONNECT":
                ConnectUser(dataArray[1], sender);
				break;
            case "CHAT":
                SendChat(dataArray[1], sender);
				break;
            case "DISCONNECT":
                DisconnectUser(sender);
				break;
            default: 
                // Message is junk do nothing with it.
				break;
        }*/
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
		/// Go through and update all the output bits!! on EVERY frame!
		for(var i=0;i<photoeye.Length;i++)
		{
			emu_to_plc_bits[photoeyeIO[i]] = photoeye[i].getState();
		}
		for(var i=0;i<conveyer_disconnect.Length;i++)
		{
			emu_to_plc_bits[conveyer_disconnect_io[i]] = conveyer_disconnect[i].getDisconnect();
		}
		for(var i=0;i<conveyer_turn_disconnect.Length;i++)
		{
			emu_to_plc_bits[conveyer_turn_disconnect_io[i]] = conveyer_turn_disconnect[i].getDisconnect();
		}
		for(var i=0;i<conveyer_merge_disconnect.Length;i++)
		{
			emu_to_plc_bits[conveyer_merge_disconnect_io[i]] = conveyer_merge_disconnect[i].getDisconnect();
		}
		for(var i=0;i<hsd_extended_prox.Length;i++)
		{
			emu_to_plc_bits[hsd_extended_prox_io[i]] = hsd_extended_prox[i].GetExtended();
		}
		for(var i=0;i<hsd_retracted_prox.Length;i++)
		{
			emu_to_plc_bits[hsd_retracted_prox_io[i]] = hsd_retracted_prox[i].GetRetracted();
		}
		for(var i=0;i<vsu_up_prox.Length;i++)
		{
			emu_to_plc_bits[vsu_up_prox_io[i]] = vsu_up_prox[i].GetExtended();
		}
		for(var i=0;i<vsu_down_prox.Length;i++)
		{
			emu_to_plc_bits[vsu_down_prox_io[i]] = vsu_down_prox[i].GetRetracted();
		}
		// copy this here
		for(var i=0;i<cs_button_input.Length;i++)
		{
			emu_to_plc_bits[cs_button_input_io[i]] = cs_button_input[i].getState();
		}
		for(var i=0;i<bmas.Length;i++)
		{
			emu_to_plc_bits[bma_OK_io[i]] = bmas[i].getBMA_OK();
			emu_to_plc_bits[bma_OOG_io[i]] = bmas[i].getBMA_OOG();
		}
		
	}
	
	
	
	
	private void sendResponse(UserConnection sender,BitArray emu_to_plc_bits,string temptrigger){
		
		
		
		string bitstring = "";
		/*
		//sender.SendData(emu_to_plc_bits.Length.ToString());	
		for(var i=0;i<emu_to_plc_bits.Length;i++)
		{				
			if(i%8==0)
			{
				bitstring+=" |";
			}
			if(i%4==0)
			{
				bitstring+=" ";
			}
			if(emu_to_plc_bits[i]==true){
				bitstring += "1";
			}else{
				bitstring +="0";
			}					
		}*/
		
		byte[] responsebytes = BitArrayToByteArray(emu_to_plc_bits);
		
		for(var i=0; i<responsebytes.Length;i++)
		{
			bitstring += responsebytes[i].ToString()+ " ";			
		}
		
		
		sender.SendData(responsebytes);
		if(loggingenabled)
		{	
			
			Debug.Log("Sent (trigger: "+ temptrigger +"):\r\n" + bitstring);
		}
		//sender.SendData("trigger:"+temptrigger + " output:" + bitstring);//Encoding.UTF8.GetString(BitArrayToByteArray(emu_to_plc_bits)));
		
	}
 
    // This subroutine sends a response to the sender.
    private void ReplyToSender(string strMessage, UserConnection sender)
	{
//        sender.SendData(strMessage);
    }
 
    // Send a chat message to all clients except sender.
    private void SendChat(string message, UserConnection sender)
	{
//        UpdateStatus(sender.Name + ": " + message);
//        SendToClients("CHAT|" + sender.Name + ": " + message, sender);
    }
	
	
	
	
	
	// Update is called once per frame
	//void Update () {
	//}
	
		// Use this for initialization
	/*void Awake()
    {
       DontDestroyOnLoad(this);
		
		GameObject M_TC1_01 = GameObject.Find("M_TC1_01");
		GameObject M_TC1_2 = GameObject.Find("M_TC1_01");
		
    }*/
	
	private byte[] BitArrayToByteArray(BitArray bits)
	{
		
	    byte[] ret = new byte[bits.Length / 8];
	    bits.CopyTo(ret, 0);
	    return ret;
	}
	
	
	
	
	public string GetConnectionString () {
		String id = "connectionString";
        ArrayList lines = new ArrayList();
        string line;
        System.IO.StringReader textStream = new System.IO.StringReader(dbConfig.text);
        string lineID = "[" + id + "]";
        bool match = false;
        while((line = textStream.ReadLine()) != null) {
            if (match) {
                if (line.StartsWith("[")) {
                    break;
                }
                if (line.Length > 0) {
                    lines.Add(line);
                }
            }
            else if (line.StartsWith(lineID)) {
                match = true;
            }
        }
        textStream.Close();
        if (lines.Count > 0) {
            return (string)(((string[])lines.ToArray(typeof(string)))[0]);
        }
        return null;
    }
}
