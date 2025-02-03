using System;
using System.Collections;
using UnityEngine;

public class ExperimentManager : MonoBehaviour
{
    public string ParticipantID;
    public int numberOfBlocks = 4;
    public int numberOfTrials = 8;

    public Session thisSession;
    public SessionScriptable thisSessionScriptable;

    public bool waitingForResponse = false;

    Trial thisTrial;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        thisSession = new Session(ParticipantID, numberOfTrials, numberOfBlocks);
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

    }

    public void OnTrialEndFunction(int interceptedResultResponse)
    {
        thisTrial.interceptedResult = interceptedResultResponse;
        waitingForResponse = false;
    }

    IEnumerator ExperimentScriptableCoroutine(SessionScriptable session)
    {
        Debug.Log($"Coroutine Started with Participant ID {session.ParticipantID}");
        
        for (int currentBlock = 0; currentBlock < session.numberOfBlocksPerSession; currentBlock++)
        {
            yield return new WaitForSeconds(0.1f);

            Debug.Log($"Starting block number {currentBlock}");

            Block thisBlock = session.blocks[currentBlock];

            for (int currentTrial = 0; currentTrial < thisBlock.numberOfTrialsInBlock; currentTrial++)
            {
                thisTrial = thisBlock.trials[currentTrial];

                if (thisTrial.interceptedResult != -999)
                {
                    continue;
                }

                yield return new WaitForSeconds(0.1f);
                
                Debug.Log($"Starting trial number {currentTrial}");

                DateTime startTime = DateTime.Now;

                EventManager.BeginTrial(thisTrial);

                waitingForResponse = true;
                yield return new WaitWhile(() => waitingForResponse);

                // Save results/session to text file
                session.SaveScriptable();

                Debug.Log($"Finished trial number {currentTrial} with result {thisTrial.interceptedResult}");

                thisTrial.responseTime = (float)(DateTime.Now - startTime).TotalSeconds;
            }
        }
    }

    IEnumerator ExperimentCoroutine(Session session)
    { 
        Debug.Log($"Coroutine Started with Participant ID {session.ParticipantID}");

        for (int currentBlock = 0; currentBlock < session.numberOfBlocksPerSession; currentBlock++)
        {
            yield return new WaitForSeconds(0.1f);

            Debug.Log($"Starting block number {currentBlock}");

            Block thisBlock = session.blocks[currentBlock];

            for (int currentTrial = 0; currentTrial < thisBlock.numberOfTrialsInBlock; currentTrial++)
            {
                yield return new WaitForSeconds(0.1f);

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
