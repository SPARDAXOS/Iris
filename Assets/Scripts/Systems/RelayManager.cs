using System;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using static MyUtility.Utility;


public class RelayManager {

    private float signInReattemptDuration = 5.0f;
    private const uint maximumAllowedClients = 2;


    private bool initialized = false;
    private static bool unityServicesInitialized = false;
    private static bool signedIn = false;

    private bool attemptingSignIn = false;
    private string currentJoinCode = string.Empty;

    private float signInReattemptTimer = 0.0f;


    private Netcode netcodeRef = null;
    private Allocation hostAllocation = null;
    private JoinAllocation clientAllocation = null;
    private RelayServerData relayServerData;


    public void Initialize(Netcode netcode) {
        if (initialized) {
            Warning("Attempted to initialize RelayManager after it was already initialized!");
            return;
        }


        netcodeRef = netcode;
        InitializeUnityServices();
        initialized = true;
    }
    public void Tick() {
        if (!initialized || !unityServicesInitialized)
            return;

        CheckSignInStatus();
    }

    private void CheckSignInStatus() {
        if (IsSignedIn())
            return;

        UpdateSignInReattemptTimer();
        if (signInReattemptTimer == 0.0f && !attemptingSignIn) {
            signInReattemptTimer = signInReattemptDuration;
            if (netcodeRef.IsDebugLogEnabled())
                Log("Attempting to sign in...");

            if (Application.internetReachability == NetworkReachability.NotReachable)
                Warning("Unable to sign in.\nNo internet connection detected!");
            else {
                attemptingSignIn = true;
                SignIn();
            }
        }
    }
    private void UpdateSignInReattemptTimer() {
        if (attemptingSignIn)
            return;

        signInReattemptTimer -= Time.deltaTime;
        if (signInReattemptTimer <= 0.0f)
            signInReattemptTimer = 0.0f;
    }


    private async void InitializeUnityServices() {
        try {
            if (netcodeRef.IsDebugLogEnabled())
                Log("Initializing UnityServices...");

            await UnityServices.InitializeAsync();

            unityServicesInitialized = true;
            if (netcodeRef.IsDebugLogEnabled())
                Log("UnityServices initialized successfully!");

            AuthenticationService.Instance.SignedIn += SignedInCallback;
        }
        catch (Exception exception) {
            Warning("Failed to initialize Unity Services\nGlobal multiplayer will be unavailable!\n" + exception);
        }
    }


    private async void SignIn() {
        try {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            attemptingSignIn = false;
        }
        catch (Exception exception) {
            Warning("Failed to sign in.\nReattempting in " + signInReattemptDuration + " seconds...");
            Warning("Exception thrown\n" + exception);
            attemptingSignIn = false;
        }
    }


    public async void CreateRelay(Action<string> codeCallback) {
        try {
            hostAllocation = await RelayService.Instance.CreateAllocationAsync((int)maximumAllowedClients - 1, null); //minus Host
            currentJoinCode = await RelayService.Instance.GetJoinCodeAsync(hostAllocation.AllocationId);

            codeCallback(currentJoinCode); //THIS...

            relayServerData = new RelayServerData(hostAllocation, "dtls");
            netcodeRef.GetUnityTransport().SetRelayServerData(relayServerData);
            netcodeRef.EnableNetworking();


        } catch(RelayServiceException exception) {
            Warning("Failed to host relay!\n" + exception.Message);
        }
    }
    public async void JoinRelay(string code) {
        if (string.IsNullOrEmpty(code)) {
            if (netcodeRef.IsDebugLogEnabled())
                Warning("Failed to join relay with code: " + code);
            return;
        }

        try {
            if (netcodeRef.IsDebugLogEnabled())
                Log("Joining relay with code " + code);

            clientAllocation =  await RelayService.Instance.JoinAllocationAsync(code);

            RelayServerData relayServerData2 = new RelayServerData(clientAllocation, "dtls");
            netcodeRef.GetUnityTransport().SetRelayServerData(relayServerData2);
            netcodeRef.EnableNetworking();
        }
        catch (RelayServiceException exception) {
            Warning("Failed to join relay!\n" + exception.Message);
        }
    }


    public static bool IsSignedIn() { return signedIn; }
    public static bool IsUnityServicesInitialized() { return unityServicesInitialized; }


    private void SignedInCallback() {
        if (netcodeRef.IsDebugLogEnabled())
            Log("Signed in using ID " + AuthenticationService.Instance.PlayerId);
        signedIn = true;
    }
}
