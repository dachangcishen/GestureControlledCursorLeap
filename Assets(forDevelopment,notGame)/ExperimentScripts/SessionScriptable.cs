using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "SessionScriptable", menuName = "SessionScriptables/Session", order = 1)]
public class SessionScriptable : ScriptableObject
{
    public string ParticipantID;
    public int numberOfTrialsPerBlock;
    public int numberOfBlocksPerSession;

    public bool hasBeenInitialised = false;

    [SerializeField]
    public List<Block> blocks;

    public void InitialiseSession(string participantID, int numberOfTrials, int numberOfBlocks)
    {
        ParticipantID = participantID;
        numberOfTrialsPerBlock = numberOfTrials;
        numberOfBlocksPerSession = numberOfBlocks;
        blocks = new();

        for (int i = 0; i < numberOfBlocksPerSession; i++)
        {
            blocks.Add(new Block(numberOfTrials));
        }

        hasBeenInitialised = true;
    }

    public void SaveScriptable()
    {
        #if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
        #endif
    }
}
