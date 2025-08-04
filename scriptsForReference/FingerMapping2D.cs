using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;
//using Leap.Unity;

public class FingerMapping2D : MonoBehaviour
{
    // Reference to Leap Motion Hand Provider
    public LeapProvider leapProvider;

    // Reference to the GameObject you want to move (e.g., Cube or Sphere)
    public GameObject movableObject;

    // Variables to define the 2D plane for movement
    public float movementScale = 1.0f; // Scale factor for hand movements
    public Vector2 minBounds;          // Min X and Y values for 2D plane movement
    public Vector2 maxBounds;          // Max X and Y values for 2D plane movement

    // Matrix to map the high-dimensional finger joint angles to 2D coordinates
    private float[,] mappingMatrix;

    // Start is called before the first frame update
    void Start()
    {
        // Initialize a simple mapping matrix (19 dimensions to 2)
        // For simplicity, you can adjust this matrix based on calibration data
        mappingMatrix = new float[,]
        {
            { 0.1f, 0.2f, 0.1f, 0.2f, 0.1f, 0.2f, 0.1f, 0.2f, 0.1f, 0.2f,
              0.1f, 0.2f, 0.1f, 0.2f, 0.1f, 0.2f, 0.1f, 0.2f, 0.1f }, // X Mapping
            { 0.1f, 0.1f, 0.2f, 0.2f, 0.1f, 0.1f, 0.2f, 0.2f, 0.1f, 0.1f,
              0.2f, 0.2f, 0.1f, 0.1f, 0.2f, 0.2f, 0.1f, 0.1f, 0.2f }  // Y Mapping
        };
    }

    void Update()
    {
        // Get the current frame from the Leap Motion provider
        Frame frame = leapProvider.CurrentFrame;

        if (frame != null && frame.Hands.Count > 0)
        {
            // Use the first hand detected (you can expand this to handle both hands if needed)
            Hand firstHand = frame.Hands[0];

            // Get all finger joints data into a 19-dimensional vector (as in the study)
            float[] fingerAngles = new float[19];
            //int index = 0;

            //foreach (Finger finger in firstHand.Fingers)
            //{
            //    // Get the direction of each bone
            //    Vector3 proximalDirection = new Vector3(
            //        finger.Bone(Bone.BoneType.TYPE_PROXIMAL).Direction.x,
            //        finger.Bone(Bone.BoneType.TYPE_PROXIMAL).Direction.y,
            //        finger.Bone(Bone.BoneType.TYPE_PROXIMAL).Direction.z);

            //    Vector3 intermediateDirection = new Vector3(
            //        finger.Bone(Bone.BoneType.TYPE_INTERMEDIATE).Direction.x,
            //        finger.Bone(Bone.BoneType.TYPE_INTERMEDIATE).Direction.y,
            //        finger.Bone(Bone.BoneType.TYPE_INTERMEDIATE).Direction.z);

            //    Vector3 distalDirection = new Vector3(
            //        finger.Bone(Bone.BoneType.TYPE_DISTAL).Direction.x,
            //        finger.Bone(Bone.BoneType.TYPE_DISTAL).Direction.y,
            //        finger.Bone(Bone.BoneType.TYPE_DISTAL).Direction.z);

            //    Vector3 metacarpalDirection = new Vector3(
            //        finger.Bone(Bone.BoneType.TYPE_METACARPAL).Direction.x,
            //        finger.Bone(Bone.BoneType.TYPE_METACARPAL).Direction.y,
            //        finger.Bone(Bone.BoneType.TYPE_METACARPAL).Direction.z);

            //    // Calculate relative angles between each pair of bones
            //    fingerAngles[index++] = Vector3.Angle(metacarpalDirection, proximalDirection);      // Between Metacarpal and Proximal
            //    fingerAngles[index++] = Vector3.Angle(proximalDirection, intermediateDirection);    // Between Proximal and Intermediate
            //    fingerAngles[index++] = Vector3.Angle(intermediateDirection, distalDirection);      // Between Intermediate and Distal
            //}

            // Add wrist flexion/extension (for simplicity, we�ll use palm normal)
            Vector3 palmNormal = new Vector3(
                firstHand.PalmNormal.x,
                firstHand.PalmNormal.y,
                firstHand.PalmNormal.z);

            fingerAngles[15] = Vector3.Angle(palmNormal, Vector3.up);    // Flexion/Extension
            fingerAngles[16] = Vector3.Angle(palmNormal, Vector3.right); // Abduction/Adduction
            fingerAngles[17] = Vector3.Angle(palmNormal, Vector3.forward); // Another degree of freedom
            fingerAngles[18] = new Vector3(firstHand.PalmPosition.x, firstHand.PalmPosition.y, firstHand.PalmPosition.z).magnitude; // Palm's magnitude

            // Map the 19-dimensional finger angle vector to 2D space (x, y) using the mapping matrix
            float xPosition = 0.0f;
            float yPosition = 0.0f;

            for (int i = 0; i < 19; i++)
            {
                xPosition += fingerAngles[i] * mappingMatrix[0, i];
                yPosition += fingerAngles[i] * mappingMatrix[1, i];
            }

            // Apply scaling factor
            xPosition *= movementScale;
            yPosition *= movementScale;

            // Clamp the 2D movement within the defined bounds
            xPosition = Mathf.Clamp(xPosition, minBounds.x, maxBounds.x);
            yPosition = Mathf.Clamp(yPosition, minBounds.y, maxBounds.y);

            // Apply the 2D movement to the GameObject (keeping Z constant)
            movableObject.transform.position = new Vector3(xPosition, yPosition, movableObject.transform.position.z);
        }
    }
}
