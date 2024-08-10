using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static MyUtility.Utility;

[Serializable]
public struct LevelOption {
    public int index;
    public Image progressBar;
    public Image previewImage;
    public Button button;
}


public class LevelSelectMenu : Entity {


    [SerializeField] private GameObject buttonPrefab;


    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform contentRectTransform;
    [SerializeField] private TextMeshProUGUI levelNameText;
    [SerializeField] private float xCenterOffset = 0.0f;
    [SerializeField] private Vector2 defaultGuiElementSize = new Vector2(200, 400);

    //Ref
    public LevelOption[] levelOptions;

    private LevelsBundle levelsBundle;
    private ScrollRect scrollRectComp;
    private HorizontalLayoutGroup layoutGroupComp;

    private Slider timeBar;

    //Ready checks
    public bool player1Ready = false;
    public bool player2Ready = false;

    //Level votes
    public string player1LevelVote = "";
    public string player2LevelVote = "";

    //Scrolling
    private RectTransform canvasRectTransform;
    private RectTransform[] guiElements;
    private float canvasCenterX = 0.0f;
    private float guiElementWidth = 0.0f;
    private float spacing = 0.0f;
    private bool isSnapped = false;

    //Holding button
    private bool isButtonHeld = false;
    private float timer = 0.0f;
    private float timeToHold = 1.0f;

    private int middleElementIndex = 0;







    public override void Initialize(GameInstance game) {
        if (initialized)
            return;


        gameInstanceRef = game;
        SetupReferences();
        //SetupGUIElements();

        initialized = true;
    }
    public override void Tick()
    {
        if (!initialized) {
            Error("");
            return;
        }


        if (Input.GetKeyDown(KeyCode.W)) {
            scrollRectComp.content.localPosition = Vector2.zero;
        }

        //ButtonTimer();
        //UpdateTimerBar();
        //StopScrollAtEdges();
        //var currentItem = CalculateCurrentItem();
        //middleElementIndex = currentItem;
        //Snap();
        //UpdateLevelName();
    }
    public void SetupMenuStartingState() {
        player1Ready = false;
        player2Ready = false;
        //Menu gets reconstructed on each opening of the menu
        levelsBundle = gameInstanceRef.GetLevelManagement().GetLevelsBundle();
        SetupGUIElements();
    }

    private void SetupReferences() {
        //Time bar
        //var timerBarTransform = transform.Find("TimerBar");
        //Validate(timerBarTransform, "Failed to find TimerBar transform", ValidationLevel.ERROR, true);
        //timeBar = timerBarTransform.GetComponent<Slider>();
        //Validate(timeBar, "Failed to find TimerBar component", ValidationLevel.ERROR, true);
        //
        //timeBar.gameObject.SetActive(false);



        Transform scrollViewTransform = transform.Find("ScrollView");
        scrollRectComp = scrollViewTransform.GetComponent<ScrollRect>();
        layoutGroupComp = scrollRectComp.content.gameObject.GetComponent<HorizontalLayoutGroup>();



        //Canvas
        canvasRectTransform = GetComponent<RectTransform>();
        Validate(canvasRectTransform, "Failed to find Canvas RectTransform", ValidationLevel.ERROR, true);
        canvasCenterX = canvasRectTransform.rect.width / 2f;
    }

    private void SetupGUIElements() {
        if (!levelsBundle) {
            Warning("Invalid levels bundle!\nUnable to construct GUI elements.");
            return;
        }

        levelOptions = new LevelOption[levelsBundle.Entries.Length];
        for (int i = 0; i < levelsBundle.Entries.Length + 1; i++) {

            GameObject newOption = Instantiate(buttonPrefab, scrollRectComp.content);
            Transform levelButton = newOption.transform.Find("LevelButton");
            if (i == levelsBundle.Entries.Length) {
                newOption.GetComponent<Image>().enabled = false;
                levelButton.GetComponent<Image>().enabled = false;
                newOption.name = "EmptyOption";
                continue;
            }



            LevelOption levelOption = new LevelOption();
            levelOption.progressBar = newOption.GetComponent<Image>();
            levelOption.previewImage = levelButton.GetComponent<Image>();
            levelOption.button = levelButton.GetComponent<Button>();

            levelOption.progressBar.fillAmount = 0.0f;
            levelOption.previewImage.sprite = levelsBundle.Entries[i].preview;
            levelOption.index = i;

            EventTrigger eventTrigger = levelButton.GetComponent<EventTrigger>();

            EventTrigger.Entry newEvent = new EventTrigger.Entry();
            newEvent.eventID = EventTriggerType.PointerExit;
            newEvent.callback.AddListener((eventData) => { OnStartClicking(levelOption.index); });
            
            eventTrigger.triggers.Add(newEvent); 
            
            levelOptions[i] = levelOption;
        }
    }

    //private void SetupGUIElements() {
    //    guiElements = new RectTransform[levelsBundle.Entries.Length];
    //    var index = 0;
    //    foreach (var level in levelsBundle.Entries) {
    //        var levelUIObject = new GameObject();
    //        
    //        var levelUIRectTransform = levelUIObject.AddComponent<RectTransform>();
    //        levelUIRectTransform.SetParent(contentRectTransform);
    //        levelUIRectTransform.gameObject.name =
    //            "Button" + index; // Just for testing since all have the same name //level.name;
    //        levelUIRectTransform.sizeDelta = defaultGuiElementSize;
    //        guiElements[index] = levelUIRectTransform;
    //        index++;
    //
    //        var levelUIButton = levelUIObject.AddComponent<Button>();
    //        var levelUIImage = levelUIObject.AddComponent<Image>();
    //        levelUIImage.sprite = level.preview;
    //    }
    //
    //    guiElementWidth = guiElements[0].sizeDelta.x;
    //    spacing = contentRectTransform.GetComponent<HorizontalLayoutGroup>().spacing;
    //    contentRectTransform.GetComponent<ContentSizeFitter>().enabled = false;
    //}

    //Button holding
    private void ButtonTimer() {
        if (!isButtonHeld)
            return;

        timer += Time.deltaTime;

        if (!(timer >= timeToHold))
            return;

        TimerOver();
        timer = 0.0f;
        isButtonHeld = false;
    }

    private void TimerOver() {
        FinalizeVote();
    }

    private void FinalizeVote() {
        var levelData = levelsBundle.Entries[middleElementIndex];
        var levelKey = levelData.key;
        Log(levelKey);
        //If not selectable return
    }

    public void OnPointerExit() {
        isButtonHeld = false;
        timer = 0.0f;
        timeBar.gameObject.SetActive(false);
    }

    public void OnPointerEnter() {
        isButtonHeld = true;
        timeBar.gameObject.SetActive(true);
    }

    //Scrollling
    private void StopScrollAtEdges() {
        if (guiElements[0].position.x >= canvasCenterX)
            scrollRect.velocity = Vector2.zero;
        if (guiElements[^1].position.x <= canvasCenterX)
            scrollRect.velocity = Vector2.zero;
    }

    private int CalculateCurrentItem() {
        var currentItem = Mathf.RoundToInt(-(contentRectTransform.localPosition.x / (guiElementWidth + spacing)));
        if (currentItem < 0)
            currentItem = 0;
        if (currentItem > guiElements.Length - 1)
            currentItem = guiElements.Length - 1;
        return currentItem;
    }

    private void Snap() {
        if (!isSnapped && scrollRect.velocity.magnitude < 30f) {
            var localPosition = contentRectTransform.localPosition;
            var x = middleElementIndex * (guiElementWidth + spacing) + xCenterOffset;
            var updatedPosition = new Vector3(-x, localPosition.y, localPosition.z);

            localPosition = Vector3.Lerp(localPosition, updatedPosition, 10f * Time.deltaTime);
            contentRectTransform.localPosition = localPosition;

            isSnapped = Math.Abs(localPosition.x - -x) < 0.1f;
        }
        else {
            isSnapped = false;
        }
    }

    //
    private void UpdateTimerBar() {
        if (!isButtonHeld)
            return;
        timeBar.value = timer / timeToHold;
    }

    private void UpdateLevelName() {
        levelNameText.text = guiElements[middleElementIndex].name;
    }

    public void DebugLevelButton() {
        gameInstanceRef.StartGame("DebugLevel");
    }



    public void OnStartClicking(int index) {

        LevelOption option = levelOptions[index];


        float spacingTotal = layoutGroupComp.spacing * index + 1;
        float totalWidth = option.progressBar.rectTransform.rect.width * index + 1;
        float anchorPoint = -scrollRectComp.content.rect.width;

        Vector3 scrollRect = scrollRectComp.content.localPosition;
        Vector3 resultRect = new Vector3(anchorPoint + (spacingTotal + totalWidth), scrollRect.y, scrollRect.z);
        scrollRectComp.content.transform.localPosition = resultRect;
        //Log();


        levelNameText.text = levelsBundle.Entries[index].name;

        // bool positive = false;
        // Vector3 scrollRect = scrollRectComp.content.localPosition;
        // Vector3 optionRect = option.progressBar.rectTransform.localPosition;
        // float distanceToOrigin = optionRect.x;
        //
        //
        // Vector3 resultRect = new Vector3(scrollRect.x - distanceToOrigin, scrollRect.y, scrollRect.z);
        // scrollRectComp.content.transform.localPosition = resultRect;
        // scrollRectComp.velocity = Vector2.zero;


        Log("I received event with index " + index);
    }
}