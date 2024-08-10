using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreSystem : Entity {

    //Taking the more dangerous route gives more score.

    [SerializeField] private int crashScorePenality = 100;
    [SerializeField] private int chaosScoreBonus = 10;


    private int currentScore = 0;

    public override void Initialize(GameInstance game) {
        if (initialized)
            return;


        gameInstanceRef = game;
        initialized = true;
    }
    public override void Tick() {
        if (!initialized)
            return;



    }
    public void SetupStartState() {
        currentScore = 0;
    }


    public void RegisterCrashScorePenality() {
        currentScore -= crashScorePenality;
    }
    public void RegisterChaosScoreBonus() {
        currentScore += chaosScoreBonus;
    }



}
