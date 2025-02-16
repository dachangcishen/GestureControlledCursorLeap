using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Session
{
    public string ParticipantID;
    public int numberOfTrialsPerBlock;
    public int numberOfBlocksPerSession;
    
    [SerializeField]
    public List<Block> blocks;

    public Session(string participantID, int numberOfTrials, int numberOfBlocks)
    {
        ParticipantID = participantID;
        numberOfTrialsPerBlock = numberOfTrials;
        numberOfBlocksPerSession = numberOfBlocks;
        blocks = new();

        for (int i = 0; i < numberOfBlocksPerSession; i++)
        {
            blocks.Add(new Block(numberOfTrials));
        }
    }
}

[System.Serializable]
public class Block
{
    
    public int numberOfTrialsInBlock;
    [SerializeField]
    public List<Trial> trials;
    public List<Vector3> trialPositions = new() { new Vector3(0, 0, 2),  new Vector3(1.0f, 1.0f, 2), new Vector3(0, 0, 2),new Vector3(-1.0f, -1.0f, 2), new Vector3(0, 0, 2), new Vector3(-1.0f, 1.0f, 2), new Vector3(0, 0, 2), new Vector3(1.0f, -1.0f, 2) };
    public Block(int numberOfTrials)
    {
        trials = new();
        numberOfTrialsInBlock = numberOfTrials;
        //shuffle the positions
        //trialPositions = trialPositions.OrderBy(_ => Random.value).ToList();
        for (int i = 0; i < numberOfTrialsInBlock; i++)
        {
            trials.Add(new Trial(trialPositions[i].x, trialPositions[i].y, trialPositions[i].z));   
        }
    }

}

[System.Serializable]
public class Trial
{
    public float xPosition;
    public float yPosition;
    public float zPosition;
    public int interceptedResult = -999;
    public float responseTime = -999.0f;

    public Trial(float xPositionForTrial, float yPositionForTrial, float zPositionForTrial)
    {
        xPosition = xPositionForTrial;
        yPosition = yPositionForTrial;
        zPosition = zPositionForTrial;
    }
}

public class Response
{
    public int interceptedResult = -999;
    public float responseTime = -999.0f;
}