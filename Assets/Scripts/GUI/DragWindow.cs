using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static MyUtility.Utility;

public class DragWindow : MonoBehaviour {
    public enum DragWindowType {
        LEFT,
        RIGHT,
        TOP,
        BOTTOM
    }


    [SerializeField] private DragWindowType dragWindowType = DragWindowType.BOTTOM;
    [SerializeField] private float SnapOpenThreshold = 0.5f;



    private Image DragWindowHandle;
    private Image DropWindowBody;


    private Rect windowBodyRect;
    private Vector3 DragWindowClosePosition;

    private Vector3 lastDraggingMousePosition;
    private bool dragging = false;


    private void Start() {

        DragWindowHandle = GetComponent<Image>();
        DropWindowBody = transform.Find("DragWindowBody").GetComponent<Image>();
        DragWindowClosePosition = DragWindowHandle.rectTransform.localPosition; //Could be any other axis. 
        windowBodyRect = DropWindowBody.rectTransform.rect;
    }
    private void Update() {
        UpdateWindow();

        //Log(DragWindowHandle.rectTransform.localPosition.y);

    }
    private void UpdateWindow() {
        if (!dragging)
            return;

        Vector3 currentHandlePosition = DragWindowHandle.rectTransform.localPosition;

        switch (dragWindowType) {
            case DragWindowType.LEFT: {



                }
                break;
            case DragWindowType.RIGHT: {

                }
                break;
            case DragWindowType.TOP: {
                    var currentPosition = DragWindowHandle.rectTransform.localPosition;
                    float delta = Input.mousePosition.y - lastDraggingMousePosition.y;
                    DragWindowHandle.rectTransform.localPosition = new Vector3(currentPosition.x, currentPosition.y - delta, currentPosition.z);

                }
                break;
            case DragWindowType.BOTTOM: {
                    float delta = Input.mousePosition.y - lastDraggingMousePosition.y;
                    if (currentHandlePosition.y + delta > DragWindowClosePosition.y + windowBodyRect.height)
                        DragWindowHandle.rectTransform.localPosition = new Vector3(currentHandlePosition.x, DragWindowClosePosition.y + windowBodyRect.height, currentHandlePosition.z);
                    else if (currentHandlePosition.y + delta < DragWindowClosePosition.y)
                        DragWindowHandle.rectTransform.localPosition = DragWindowClosePosition;
                    else
                        DragWindowHandle.rectTransform.localPosition = new Vector3(currentHandlePosition.x, currentHandlePosition.y + delta, currentHandlePosition.z);

                }
                break;
        }


        lastDraggingMousePosition = Input.mousePosition;
    }
    private void DecideWindowState() {

        Vector3 handlePosition = DragWindowHandle.rectTransform.localPosition;

        switch (dragWindowType) {
            case DragWindowType.LEFT: {

                }
                break;
            case DragWindowType.RIGHT: {

                }
                break;
            case DragWindowType.TOP: {


                }
                break;
            case DragWindowType.BOTTOM: { //Note: Keep in mind the sign.
                    if (handlePosition.y >= DragWindowClosePosition.y + DropWindowBody.rectTransform.rect.height * SnapOpenThreshold) {
                        Log("Open");
                        DragWindowHandle.rectTransform.localPosition 
                            = new Vector3(handlePosition.x, DragWindowClosePosition.y + DropWindowBody.rectTransform.rect.height, handlePosition.z);
                        //Opened!
                    }
                    else {
                        DragWindowHandle.rectTransform.localPosition = DragWindowClosePosition;
                        Log("Close");
                    }
                }
                break;
        }
    }




    public void DragWindowBegin() {
        Log("Begin!");

        lastDraggingMousePosition = Input.mousePosition;
        dragging = true;
    }
    public void DragWindowEnd() {
        Log("End!");

        DecideWindowState();
        dragging = false;
    }
}
