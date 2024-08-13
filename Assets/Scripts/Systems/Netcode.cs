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

    public enum NetworkingState {
        NONE = 0,
        LOCAL_CLIENT,
        GLOBAL_CLIENT,
        LOCAL_HOST,
        GLOBAL_HOST
    }

    [SerializeField] private bool enableNetworkLog = true;
    private NetworkingState currentState = NetworkingState.NONE;

    private const uint clientsLimit = 2;

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

        encryptor = new Encryptor();
        QueuryOwnIPAddress();
        //QueryIPAddresses(); //For testing ethernet connections
        SetupCallbacks();
        gameInstanceRef = game;
        initialized = true;
    }
    public override void Tick() {
        if (!initialized)
            return;

        relayManager.Tick();
    }
    private void SetupCallbacks() {
        networkManagerRef.OnClientConnectedCallback += OnClientConnectedCallback;
        networkManagerRef.OnClientDisconnectCallback += OnClientDisconnectCallback;
        networkManagerRef.ConnectionApprovalCallback += ConnectionApprovalCallback;
    }


    //Query
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
                    else if (ip.Address.AddressFamily == AddressFamily.InterNetworkV6)
                        Log("IPV6 Address : " + ip.Address.ToString() + "\nName: " + ip.Address.ToString());
                }
            }
            if (item.NetworkInterfaceType == ethernet && item.OperationalStatus == OperationalStatus.Up) {
                Log("Ethernet Connections detected of type " + item.NetworkInterfaceType + " name: " + item.Id);
                foreach (var ip in item.GetIPProperties().UnicastAddresses) {
                    if (ip.Address.AddressFamily == AddressFamily.InterNetwork) {
                        Log("IPV4 Address : " + ip.Address.ToString());
                        localIPAddress = ip.Address;
                    }
                    else if (ip.Address.AddressFamily == AddressFamily.InterNetworkV6)
                        Log("IPV6 Address : " + ip.Address.ToString() + "\nName: " + ip.Address.ToString());

                }
            }
        }

        Log("IP ADDRESS IS " + localIPAddress);
    }


    //Encryption/Decryption
    public string GetEncryptedLocalHost() {
        return encryptor.Encrypt(localIPAddress.GetAddressBytes());
    }
    public string DecryptConnectionCode(string targetCode) {
        return encryptor.Decrypt(targetCode);
    }
    public IPAddress GetLocalIPAddress() {
        return localIPAddress;
    }


    //Connection
    public bool EnableNetworking() {
        if (currentState == NetworkingState.LOCAL_CLIENT || currentState == NetworkingState.GLOBAL_CLIENT)
            return networkManagerRef.StartClient();
        else if (currentState == NetworkingState.LOCAL_HOST || currentState == NetworkingState.GLOBAL_HOST)
            return networkManagerRef.StartHost();

        return false;
    }
    public void StopNetworking() {

        networkManagerRef.Shutdown();
        if (gameInstanceRef.IsDebuggingEnabled())
            Log("Networking has stopped!");

        currentState = NetworkingState.NONE;
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


    //Getters
    public UnityTransport GetUnityTransport() { return unityTransportRef; }
    public RelayManager GetRelayManager() { return relayManager; }
    public int GetConnectedClientsCount() { return networkManagerRef.ConnectedClients.Count; }
    public static ulong GetClientID() { return NetworkManager.Singleton.LocalClientId; }
    public ulong GetOtherClient(ulong id) {
        foreach(var client in networkManagerRef.ConnectedClientsIds) {
            if (id != client)
                return client;
        }
        return id;
    }
    public bool IsDebugLogEnabled() { return enableNetworkLog; }
    public bool IsHost() { return networkManagerRef.IsHost; }
    public bool IsClient() { return networkManagerRef.IsClient; }
    public bool IsRunning() { return networkManagerRef.IsListening; }


    //Callbacks
    private void OnClientConnectedCallback(ulong ID) {
        if (enableNetworkLog)
            Log("Client " + ID + " has connected!");

        if (IsHost()) {
            if (GetConnectedClientsCount() == 1)
                gameInstanceRef.CreatePlayer1(ID);
            else if (GetConnectedClientsCount() == 2) {
                gameInstanceRef.CreatePlayer2(ID);
                gameInstanceRef.GetRPCManagement().ConfirmConnectionServerRpc();
            }
        }
    }
    private void OnClientDisconnectCallback(ulong ID) {
        if (enableNetworkLog)
            Log("Disconnection request received from " + ID + "\nReason: " + networkManagerRef.DisconnectReason);

        StopNetworking();
        gameInstanceRef.InterruptGame();
    }
    private void ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response) {
        if (enableNetworkLog)
            Log("Connection request received from " + request.ClientNetworkId);

        if (clientsLimit == GetConnectedClientsCount()) {
            if (enableNetworkLog)
                Log("Connection request denied!\nReason: Clients limit reached.");

            response.CreatePlayerObject = false; //? not sure.
            response.Approved = false;
        }
        else {
            if (enableNetworkLog)
                Log("Connection request was accepted!");

            response.CreatePlayerObject = false;
            response.Approved = true;
        }
    }
}
