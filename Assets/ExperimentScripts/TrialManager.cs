using UnityEngine;

public class TrialManager : MonoBehaviour
{
    public GameObject enemyPrefab;

    public bool trialStarted = false;
    public Trial currentTrial;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        EventManager.OnTrialBegin += OnBeginTrialFunction;
        EventManager.OnTrialEnd += OnTrialEndFunction;
    }

    // Update is called once per frame
    void Update()
    {
        /*
        if (trialStarted)
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                EventManager.EndTrial(1);
                trialStarted = false;
            }
            if (Input.GetKeyDown(KeyCode.Y))
            {
                EventManager.EndTrial(0);
                trialStarted = false;
            }
        }
        */
    }

    void OnTrialEndFunction(int interceptedResultResponse)
    {
        trialStarted = false;
    }

    public void OnBeginTrialFunction(Trial trial)
    {
        currentTrial = trial;
        Debug.Log($"TrialManager: BeginTrial event received with condition {trial.xPosition}, {trial.yPosition}, {trial.zPosition}");
        SpawnTarget();
        trialStarted = true;
    }

    void SpawnTarget()
    {
        GameObject newTarget = Instantiate(enemyPrefab);
        Vector3 newPosition = newTarget.transform.position;
        newPosition.x = currentTrial.xPosition;
        newPosition.y = currentTrial.yPosition;
        newPosition.z = currentTrial.zPosition;
        newTarget.transform.position = newPosition;
    }
}
