using UnityEngine;
using UnityEngine.UI;
using static MyUtility.Utility;

public class LoadingScreen : Entity {

    public enum LoadingProcess {
        NONE = 0,
        LOADING_ASSETS,
        LOADING_LEVEL
    }

    private LoadingProcess currentLoadingProcess = LoadingProcess.NONE;
    private bool loadingProcessRunning = false;

    private Image progressBarRef;

    public override void Initialize(GameInstance game) {
        if (initialized)
            return;

        SetupReferences();
        gameObject.SetActive(false);
        gameInstanceRef = game;
        initialized = true;
    }
    private void SetupReferences() {

        //Progress Bar
        Transform progressBarTransform = transform.Find("ProgressBar");
        if (!Validate(progressBarTransform, "LoadingMenu failed to get reference to ProgressBar transform", ValidationLevel.ERROR, false))
            return;
        progressBarRef = progressBarTransform.GetComponent<Image>();
        if (!Validate(progressBarRef, "LoadingMenu failed to get reference to Image component", ValidationLevel.ERROR, false))
            return;

        progressBarRef.fillAmount = 0.0f;
    }

    public bool StartLoadingProcess(LoadingProcess process) {
        if (loadingProcessRunning) {
            Warning("StartLoadingProcess was called when a loading process was still active!");
            return false;
        }

        currentLoadingProcess = process;
        gameObject.SetActive(true);
        loadingProcessRunning = true;
        progressBarRef.fillAmount = 0.0f;
        return true;
    }
    public void UpdateLoadingBar(float value) {
        if (!loadingProcessRunning) {
            Warning("UpdateLoadingBar was called when no loading process was active!");
            return;
        }


        progressBarRef.fillAmount = value;
        //Stop process here once value is 100%? probably do it manually to have more control to fake out loading.
    }
    public void FinishLoadingProcess() {
        if (!loadingProcessRunning) {
            Warning("FinishLoadingProcess was called when no loading process was active!");
            return;
        }


        currentLoadingProcess = LoadingProcess.NONE;
        gameObject.SetActive(false);
        loadingProcessRunning = false;
        progressBarRef.fillAmount = 0.0f;
    }

    public bool IsLoadingProcessRunning() {  return loadingProcessRunning; }
    public LoadingProcess GetCurrentLoadingProcess() { return currentLoadingProcess; }
}
