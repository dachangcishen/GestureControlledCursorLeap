using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Collections.Generic;
public class ExperimentManager : MonoBehaviour
{
    public string ParticipantID;
    public int numberOfBlocks = 10;
    public int numberOfTrials = 8;

    public Session thisSession;
    public SessionScriptable thisSessionScriptable;
    public TMP_Text LeftTop;
    public TMP_Text RightTop;
    public TMP_Text LeftBottom;
    public TMP_Text RightBottom;
    public TMP_Text Middle;
    public bool waitingForResponse = false;
    public bool probeEnabled = false;
    public bool probeAdopted = false;
    private GameObject target;
    Block thisBlock;
    Trial thisTrial;
    DateTime startTime;
    Vector3 startPosition;
    private GameObject player;
    public bool waitingForReaction = false;
    public bool reachedWrongPosition = false;
    List<Color> colors = new List<Color>() { Color.green, Color.blue, new Color(0.5f, 0f, 0.5f, 1f), Color.yellow };
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        thisSession = new Session(ParticipantID, numberOfTrials, numberOfBlocks);
        player = GameObject.FindWithTag("Player");
        // thisSessionScriptable = ScriptableObject.CreateInstance<SessionScriptable>();
        if (thisSessionScriptable.hasBeenInitialised == false)
        {
            thisSessionScriptable.InitialiseSession(ParticipantID, numberOfTrials, numberOfBlocks);
            thisSessionScriptable.SaveScriptable();
        }
        
        EventManager.OnTrialEnd += OnTrialEndFunction;

        //StartCoroutine(ExperimentCoroutine(thisSession));
        StartCoroutine(ExperimentScriptableCoroutine(thisSessionScriptable));
    }

    // Update is called once per frame
    void Update()
    {
        if(waitingForReaction)
        {
            // Check if the player has moved significantly from the start position
            // If so, record the reaction time and stop waiting for reaction
            if(Mathf.Abs(player.transform.position.x - startPosition.x) + Mathf.Abs(player.transform.position.y - startPosition.y) > 0.15f)
            {
                thisTrial.reactionTime = (float)(DateTime.Now - startTime).TotalSeconds;
                waitingForReaction = false;
            }
        }
        if(!waitingForReaction && !reachedWrongPosition && target != null){
            if(Mathf.Abs(player.transform.position.x) + Mathf.Abs(player.transform.position.y) > 1.85f)
            {
                if(Mathf.Abs(player.transform.position.x - target.transform.position.x) + Mathf.Abs(player.transform.position.y - target.transform.position.y) > 0.15f)
                {
                    reachedWrongPosition = true;
                }
            }
        }
    }

    public void OnTrialEndFunction(int interceptedResultResponse)
    {
        thisTrial.interceptedResult = interceptedResultResponse;
        waitingForResponse = false;
    }

    IEnumerator ExperimentScriptableCoroutine(SessionScriptable session)
    {
        Debug.Log($"Coroutine Started with Participant ID {session.ParticipantID}");
        System.Random rnd = new System.Random();
        LeftTop.text = "";
        RightTop.text = "";
        LeftBottom.text = "";
        RightBottom.text = "";
        Middle.text = "";
        string filePath = Application.dataPath + "/" + ParticipantID + "_" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
        for (int currentBlock = 0; currentBlock < session.numberOfBlocksPerSession; currentBlock++)
        {
            yield return new WaitForSeconds(1.0f);

            Debug.Log($"Starting block number {currentBlock}");

            thisBlock = session.blocks[currentBlock];

            for (int i = 1; i <= 4; i++)
            {
                if (thisBlock.trialPositions[i - 1].Equals(new Vector3(-1.0f, 1.0f, 2)))
                {
                    LeftTop.text = i.ToString();
                }
                else if (thisBlock.trialPositions[i - 1].Equals(new Vector3(1.0f, 1.0f, 2)))
                {
                    RightTop.text = i.ToString();
                }
                else if (thisBlock.trialPositions[i - 1].Equals(new Vector3(-1.0f, -1.0f, 2)))
                {
                    LeftBottom.text = i.ToString();
                }
                else if (thisBlock.trialPositions[i - 1].Equals(new Vector3(1.0f, -1.0f, 2)))
                {
                    RightBottom.text = i.ToString();
                }
            }
            yield return new WaitForSeconds(2.0f);
            Middle.text = "Plan";

            LeftTop.text = "";
            RightTop.text = "";
            LeftBottom.text = "";
            RightBottom.text = "";

            yield return new WaitForSeconds(1.0f);
            Middle.text = "";
            probeAdopted = false;
            
            if (probeEnabled) {
                int randomInt = rnd.Next(5);
                if (randomInt == 0)
                {
                    probeAdopted = true;
                }
            }
            if (probeAdopted){                
                int currentTrial = rnd.Next(4);
                thisTrial = thisBlock.trials[currentTrial];
                //yield return new WaitForSeconds(1.0f);
                startTime = DateTime.Now;
                EventManager.BeginTrial(thisTrial);
                startPosition = player.transform.position;
                waitingForReaction = true;
                waitingForResponse = true;
                reachedWrongPosition = false;
                target = GameObject.FindWithTag("Respawn");
                target.GetComponent<Renderer>().material.color = Color.red;
                yield return new WaitWhile(() => waitingForResponse);
                if(reachedWrongPosition) thisTrial.interceptedResult = 0;

                session.SaveScriptable();                
                Debug.Log($"Finished trial number {currentTrial} with result {thisTrial.interceptedResult}");
                thisTrial.moveTime = (float)(DateTime.Now - startTime).TotalSeconds;

                using (StreamWriter file = new StreamWriter(filePath, true))
                {
                    file.WriteLine($"{currentBlock} {currentTrial} {probeEnabled} {probeAdopted} {thisTrial.interceptedResult} {thisTrial.reactionTime} {thisTrial.moveTime} {thisTrial.xPosition} {thisTrial.yPosition} {thisTrial.zPosition}");
                }
                continue;
            }

            for (int currentTrial = 0; currentTrial < thisBlock.numberOfTrialsInBlock / 2; currentTrial++)
            {
                thisTrial = thisBlock.trials[currentTrial];
                
                /*
                if (thisTrial.interceptedResult != -999)
                {
                    continue;
                }
                */

                //yield return new WaitForSeconds(1.0f);
                
                Debug.Log($"Starting trial number {currentTrial}");
                startTime = DateTime.Now;
                EventManager.BeginTrial(thisTrial);
                startPosition = player.transform.position;
                if (currentTrial > 0) startPosition = thisBlock.trialPositions[currentTrial - 1];
                waitingForReaction = true;
                waitingForResponse = true;
                reachedWrongPosition = false;
                target = GameObject.FindWithTag("Respawn");
                yield return new WaitWhile(() => waitingForResponse);
                if(reachedWrongPosition) thisTrial.interceptedResult = 0;
                
                // Save results/session to text file
                session.SaveScriptable();
                Debug.Log($"Finished trial number {currentTrial} with result {thisTrial.interceptedResult}");
                thisTrial.moveTime = (float)(DateTime.Now - startTime).TotalSeconds;

                using (StreamWriter file = new StreamWriter(filePath, true))
                {
                    file.WriteLine($"{currentBlock} {currentTrial} {probeEnabled} {probeAdopted} {thisTrial.interceptedResult} {thisTrial.reactionTime} {thisTrial.moveTime} {thisTrial.xPosition} {thisTrial.yPosition} {thisTrial.zPosition}");
                }
            }

            
            LeftTop.color = Color.white;
            RightTop.color = Color.white;
            LeftBottom.color = Color.white;
            RightBottom.color = Color.white;
        }
    }

    IEnumerator ExperimentCoroutine(Session session)
    { 
        Debug.Log($"Coroutine Started with Participant ID {session.ParticipantID}");

        for (int currentBlock = 0; currentBlock < session.numberOfBlocksPerSession; currentBlock++)
        {
            yield return new WaitForSeconds(2.0f);
            Debug.Log($"Starting block number {currentBlock}");
            Block thisBlock = session.blocks[currentBlock];
            for (int currentTrial = 0; currentTrial < thisBlock.numberOfTrialsInBlock; currentTrial++)
            {
                yield return new WaitForSeconds(2.0f);
                Debug.Log($"Starting trial number {currentTrial}");
                thisTrial = thisBlock.trials[currentTrial];
                EventManager.BeginTrial(thisTrial);
                waitingForResponse = true;
                yield return new WaitWhile(() => waitingForResponse);

                // Save results/session to text file


                Debug.Log($"Finished trial number {currentTrial} with result {thisTrial.interceptedResult}");
            }
        }

        yield return null;
    }
}
