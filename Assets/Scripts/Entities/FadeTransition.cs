using System;
using UnityEngine;
using static MyUtility.Utility;

public class FadeTransition : Entity {


    private Action onFadeCallback = null;
    private Action onFinishedCallback = null;
    private Animation animationComp = null;


    public override void Initialize(GameInstance game) {
        if (initialized)
            return;

        SetupReferences();
        gameObject.SetActive(false);
        initialized = true;
    }
    private void SetupReferences() {
        animationComp = GetComponent<Animation>();
    }

    public void StartTransition(Action onFade, Action onFinished = null) {
        if (!animationComp) {
            Error("Failed to start fade transition\nAnimation component is invalid!");
            return;
        }
        if (onFade == null && onFinished == null) {
            Error("Failed to start fade transition\nCallbacks are invalid!");
            return;
        }


        onFadeCallback = onFade;
        onFinishedCallback = onFinished;
        animationComp.Play("Fade");
        gameObject.SetActive(true);
    } 
    public bool isTransitionActive() {
        return animationComp.isPlaying;
    }
    public void StopTransition() {
        if (!animationComp.isPlaying) {
            Warning("Failed to stop fade transition\nTransition was not active!");
            return;
        }

        animationComp.Stop();
        gameObject.SetActive(false);
        onFadeCallback = null;
        onFinishedCallback= null;
    }


    public void InvokeFadeCallback() {
        if (onFadeCallback != null)
            onFadeCallback.Invoke();
    }
    public void InvokeFadeFinishedCallback() {
        if (onFinishedCallback != null)
            onFinishedCallback.Invoke();

        onFadeCallback = null;
        onFinishedCallback = null;
        gameObject.SetActive(false);
    }
}
