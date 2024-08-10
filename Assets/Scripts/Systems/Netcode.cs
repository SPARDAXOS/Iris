using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using static MyUtility.Utility;

public class Netcode : Entity {


    public const uint DEFAULT_SERVER_PORT = 6312;
    public const uint HOST_ID = 0;
    public const uint CLIENT_ID = 1;

    public enum NetworkingState {
        NONE = 0,
        LOCAL_CLIENT,
        GLOBAL_CLIENT,
        LOCAL_HOST,
        GLOBAL_HOST
    }

    [SerializeField] private bool enableNetworkLog = true;


    public NetworkingState currentState = NetworkingState.NONE;



    private const uint clientsLimit = 2;
    private uint connectedClients = 0;
    public bool running = false;

    private IPAddress localIPAddress = null;
    
    private Encryptor encryptor;

    private NetworkManager networkManagerRef = null;
    private UnityTransport unityTransportRef = null;

    private RelayManager relayManager = null;


    public override void Initialize(GameInstance game) {
        if (initialized)
            return;


        networkManagerRef = GetComponent<NetworkManager>();
        unityTransportRef = networkManagerRef.GetComponent<UnityTransport>();

        relayManager = new RelayManager();
        relayManager.Initialize(this);

        //transportLayer.ConnectionData.Address = "192.0.0.1";


        encryptor = new Encryptor();
        QueuryOwnIPAddress();
        //QueryIPAddresses(); //For testing ethernet connections
        SetupCallbacks();
        gameInstanceRef = game;
        initialized = true;
    }
    public override void Tick() {
        if (!initialized) {
            //Error("Attempted to tick Netcode while it was not initialized!");
            return;
        }

        //NOTE: 
        //Check somehow in case of disconnections and         gameInstanceRef.InterruptGame();

        relayManager.Tick();
    }
    private void SetupCallbacks() {
        networkManagerRef.OnClientConnectedCallback += OnClientConnectedCallback;
        networkManagerRef.OnClientDisconnectCallback += OnClientDisconnectCallback;
        networkManagerRef.ConnectionApprovalCallback += ConnectionApprovalCallback;
    }
    private void QueuryOwnIPAddress() {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var address in host.AddressList) {
            if (address.AddressFamily == AddressFamily.InterNetwork) {
                localIPAddress = address;
                if (enableNetworkLog)
                    Log("LocalHost: " + address);
                return;
            }
        }
    }
    private void QueryIPAddresses() {

        localIPAddress = null;
        foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces()) {
            NetworkInterfaceType wifi = NetworkInterfaceType.Wireless80211;
            NetworkInterfaceType ethernet = NetworkInterfaceType.Ethernet;

            if (item.NetworkInterfaceType == wifi && item.OperationalStatus == OperationalStatus.Up) {
                Log("WI_FI Connections detected of type " + item.NetworkInterfaceType + " name: " + item.Id);
                foreach (var ip in item.GetIPProperties().UnicastAddresses) {
                    if (ip.Address.AddressFamily == AddressFamily.InterNetwork) {
                        Log("IPV4 Address : " + ip.Address.ToString());
                        localIPAddress = ip.Address;
                    }
                    //else if (ip.Address.AddressFamily == AddressFamily.InterNetworkV6)
                    //    Log("IPV6 Address : " + ip.Address.ToString() + "\nName: " + ip.Address.);
                    
                }

            }



            if (item.NetworkInterfaceType == ethernet && item.OperationalStatus == OperationalStatus.Up) {
                Log("Ethernet Connections detected of type " + item.NetworkInterfaceType + " name: " + item.Id);
                foreach (var ip in item.GetIPProperties().UnicastAddresses) {
                    if (ip.Address.AddressFamily == AddressFamily.InterNetwork) {
                        Log("IPV4 Address : " + ip.Address.ToString());
                        localIPAddress = ip.Address;
                    }
                    //else if (ip.Address.AddressFamily == AddressFamily.InterNetworkV6)
                    //    Log("IPV6 Address : " + ip.Address.ToString() + "\nName: " + ip.Address.);

                }
            }





        }

        Log("IP ADDRESS IS " + localIPAddress);
    }


    public string GetEncryptedLocalHost() {
        return encryptor.Encrypt(localIPAddress.GetAddressBytes());
    }
    public string DecryptConnectionCode(string targetCode) {
        return encryptor.Decrypt(targetCode);
    }





    public IPAddress GetLocalIPAddress() {
        return localIPAddress;
    }


    private void DisconnectAllClients() {
        foreach(var client in networkManagerRef.ConnectedClients) {
            if (client.Value.ClientId != (ulong)GetClientID())
                networkManagerRef.DisconnectClient(client.Value.ClientId, "Server Shutdown");
        }
    }
    public void StopNetworking() {
        if (!running)
            return;

        networkManagerRef.Shutdown();
        if (gameInstanceRef.IsDebuggingEnabled())
            Log("Networking has stopped!");

        connectedClients = 0; //This kinda does it. 
        currentState = NetworkingState.NONE;
        if (IsHost())
            DisconnectAllClients();
    }
    public bool EnableNetworking() {

        running = false;
        if (currentState == NetworkingState.LOCAL_CLIENT || currentState == NetworkingState.GLOBAL_CLIENT)
            running = networkManagerRef.StartClient();
        else if (currentState == NetworkingState.LOCAL_HOST || currentState == NetworkingState.GLOBAL_HOST)
            running = networkManagerRef.StartHost();

        return running;
    }




    public bool StartLocalClient(string address) {
        if (enableNetworkLog)
            Log("Attempting to connect to..." + address);

        unityTransportRef.SetConnectionData(address, (ushort)DEFAULT_SERVER_PORT);
        currentState = NetworkingState.LOCAL_CLIENT;
        unityTransportRef.ConnectionData.Address = address;
        return EnableNetworking();
    }
    public bool StartGlobalClient(string targetAddress) {
        if (!RelayManager.IsUnityServicesInitialized()) {
            Error("Unable to start global client!\nUnity Services are not initialized.");
            return false;
        }

        currentState = NetworkingState.GLOBAL_CLIENT;
        relayManager.JoinRelay(targetAddress);
        return true; //Start as client on code being received! callable by connection menu
    }



    public bool StartLocalHost() {
        currentState = NetworkingState.LOCAL_HOST;
        unityTransportRef.SetConnectionData(localIPAddress.ToString(), (ushort)DEFAULT_SERVER_PORT);
        return EnableNetworking();
    }
    public bool StartGlobalHost(Action<string> codeCallback) {
        if (!RelayManager.IsUnityServicesInitialized()) {
            Error("Unable to start global host!\nUnity Services are not initialized.");
            return false;
        }

        currentState = NetworkingState.GLOBAL_HOST;
        relayManager.CreateRelay(codeCallback);
        return true;
    }





    public bool IsDebugLogEnabled() { return enableNetworkLog; }
    public bool IsHost() {
        return networkManagerRef.IsHost;
    }
    public bool IsClient() {
        return networkManagerRef.IsClient;
    }
    public uint GetConnectedClientsCount() {
        return connectedClients;
    }
    public static ulong GetClientID() {
        return NetworkManager.Singleton.LocalClientId;
    }

    public ulong GetOtherClient(ulong id) {
        foreach(var client in networkManagerRef.ConnectedClientsIds) {
            if(id != client)
                return client;
        }
        return id;
    }


    public UnityTransport GetUnityTransport() { return unityTransportRef; }
    public RelayManager GetRelayManager() { return relayManager; }

    private void OnConnectedToServer() {
        Log("I have connected to a server! " + GetClientID());

    }

    //Break it into host code and client code!½
    //Callbacks
    private void OnClientConnectedCallback(ulong ID) {
        if (enableNetworkLog)
            Log("Client " + ID + " has connected!");




        Log("Client ID is " + ID);
        connectedClients++; //Disconnecting doesnt trigger this on relay for some reason

        //Need to do stuff with the client ID - Server Auth

        if (IsHost()) {
            if (GetConnectedClientsCount() == 2)
                gameInstanceRef.GetRPCManagement().ConfirmConnectionServerRpc();
        }
    }
    private void OnClientDisconnectCallback(ulong ID) {
        if (enableNetworkLog)
            Log("Disconnection request received from " + ID + "\nReason: " + networkManagerRef.DisconnectReason);

        connectedClients--;

        //HMMMM gotta start thinking about authority
        if (connectedClients != 2) //Technically any disconnection should interrupt.
            gameInstanceRef.InterruptGame(); //Works well for when both connected and the game started but not while hosting or doing connection menu stuff
    }
    private void ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response) {
        if (enableNetworkLog)
            Log("Connection request received from " + request.ClientNetworkId);

        
        response.CreatePlayerObject = false;
        response.Approved = true;
        if (enableNetworkLog)
            Log("Connection request was accepted!");
    }
}
