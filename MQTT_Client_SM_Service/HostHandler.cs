using NDC8.ACINET.ACI;
using Serilog;


namespace MQTT_Client_SM_Service
{
    public class HostHandler
    {
        public string IPaddress;
        public int Port;

        //Check if connecting
        public bool connecting = false;

        //Keep track if disconnected by button or communication is down.
        public bool disconnectedByUser = false;


        public HostHandler()
        {

        }

        

        public void Connect(string ipaddress, int port)
        {
            IPaddress = ipaddress;
            Port = port;

            //Log.Information("Connecting to SM... IP: " + ipaddress + " Port: " + port);

            //If false and not trying to connect it should connect.
            if (!VCP9412.ACI_Instance.IsConnected && !connecting)
            {
                ConnectSM();
            }
            else
            {
                DisconnectSM();
            }
        }




        public void ConnectSM()
        {
            VCP9412.ACI_Instance.Open(IPaddress, Port);
            VCP9412.ACI_Instance.Connected += Instance_Connected;
            VCP9412.ACI_Instance.Disconnected += Instance_Disconnected;
            VCP9412.ACI_Instance.ReciveData += Instance_ReciveData;

            connecting = true;
            Log.Information("SM: CONNECTING... IP: " + IPaddress + " Port: " + Port);
        }

        public void DisconnectSM()
        {
            Log.Information("SM: DISCONNECTING...");
            //Set disconnected by user
            disconnectedByUser = true;
            connecting = false;
            VCP9412.ACI_Instance.Close();
            VCP9412.ACI_Instance.Connected -= Instance_Connected;
            VCP9412.ACI_Instance.Disconnected -= Instance_Disconnected;
            VCP9412.ACI_Instance.ReciveData -= Instance_ReciveData;
            VCP9412.ACI_Instance.Dispose();
        }

        private void Instance_Connected(string host, int port)
        {

            string text = "SM: CONNECTED WITH SERVER " + IPaddress + ", " + Port;

            Log.Information(text);
            connecting = false;
            disconnectedByUser = false;
        }

        public void Instance_Disconnected(string host, int port)
        {

            string text;
            //Check if dosconnected by user or if COM to system went down.
            if (disconnectedByUser)
            {
                text = "SM: DISCONNECTED";
                Log.Information(text);
            }
            else
            {
                text = "SM: COMMUNICATION LOST, RECONNECTING....";
                Log.Information(text);

                connecting = true;
            }

        }

        public void Instance_ReciveData(IACIMessage msg)
        {

                string text = "SM: MSG: " + msg;
                Log.Information(text);
            


        }
    }
}
