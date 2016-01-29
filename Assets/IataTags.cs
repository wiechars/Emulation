using UnityEngine;
using System.Collections.Generic;
using System;
using System.Data;
using System.Data.Odbc;

public static class IataTags {
	
	private static bool IATAEnabled = true;
	public static Queue<string> IATA_tags = new Queue<string>();
	
	private static System.Data.DataTable GetModuleData()
    {
		Debug.Log("Attempting to read IATA tags from : "+Application.dataPath+"/ATR_IATA_tags.xls");
		Logger.WriteLine("[INFO]","IATA","Attempting to read IATA tags from : "+Application.dataPath+"/ATR_IATA_tags.xls",true);
        var connString =  "Driver={Microsoft Excel Driver (*.xls)};DriverId=790;Dbq="+Application.dataPath+"/ATR_IATA_tags.xls;Extended Properties='IMEX=1'";   //string.Format(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\\GDrive\\Google Drive\\Development\\Unity\\simulationconfig.xlsm;Extended Properties=""Excel 12.0 Macro;HDR=YES"";");
        string SelectSQL = "select * from [IO$]";
        OdbcCommand dbCommand = null;
        System.Data.DataTable dTable = new DataTable("YourData");;
        OdbcConnection  conn = null;

        using(conn = new OdbcConnection (connString)){
	        conn.Open();
	        using(dbCommand = new OdbcCommand(SelectSQL.ToString(), conn))
			{
				OdbcDataReader rData = dbCommand.ExecuteReader();
				dTable.Load(rData);
			}
		}
        return dTable;
	}
	
	// Use this for initialization
	public static void initIATATags() {
		
		if(IATAEnabled)
		{
            try
            {
                System.Data.DataTable dTable = GetModuleData();
                if (dTable.Rows.Count > 0)
                {
                    for (int i = 0; i < dTable.Rows.Count; i++)
                    {
                        IATA_tags.Enqueue(dTable.Rows[i]["IATA_tag"].ToString());
                    }
					Logger.WriteLine("[INFO]", "IATA_Tags", "Read IATA Tags. Count : " + dTable.Rows.Count,true);
					Debug.Log("Read IATA Tags. Count : " + dTable.Rows.Count);
                }
            }
            catch (Exception e)
            {
                Logger.WriteLine("[ERROR]", "IATA_Tags", "Error Reading IATA Tags. " + e.Message,true);
				Debug.Log("Error Reading IATA Tags : " + e.Message);
            }
		}
	}
	
	public static string popIATA(){
		
		if(IATA_tags.Count>0)
		{
			return IATA_tags.Dequeue();
		}
		return "RAN OUT OF IATA TAGS";
	}
	
}
