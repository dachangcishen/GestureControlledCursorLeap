using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;
//using Leap.Unity;
using TMPro;
using System;

public class Gesture19DMapping2D : MonoBehaviour
{
    // Reference to Leap Motion Hand Provider
    public LeapProvider leapProvider;

    // Reference to the GameObject you want to move (e.g., Cube or Sphere)
    public GameObject movableObject;

    // Capture buttons for each gesture
    public KeyCode captureOpenHandKey = KeyCode.UpArrow;
    public KeyCode captureGunGestureKey = KeyCode.DownArrow;
    public KeyCode captureClawGestureKey = KeyCode.LeftArrow;
    public KeyCode captureFistGestureKey = KeyCode.RightArrow;

    // Variables for storing the four gesture vectors (open hand, gun, claw, fist)
    private float[] openHandGesture;
    private float[] gunGesture;
    private float[] clawGesture;
    private float[] fistGesture;

    // Variables to define the 2D plane for movement
    public float movementScale = 1.0f; // Scale factor for hand movements
    public Vector2 minBounds;          // Min X and Y values for 2D plane movement
    public Vector2 maxBounds;          // Max X and Y values for 2D plane movement

    // Flags to check if gestures are captured
    private bool isOpenHandCaptured = false;
    private bool isGunGestureCaptured = false;
    private bool isClawGestureCaptured = false;
    private bool isFistGestureCaptured = false;

    // Toggle for using wrist movements (flexion/extension and abduction/adduction)
    public bool includeWristMovements = true;

    //public TMPro.Text infoText; 

    
    void Update()
    {
        // Get the current frame from the Leap Motion provider
        Frame frame = leapProvider.CurrentFrame;

        if (frame != null && frame.Hands.Count > 0)
        {
            // Use the first hand detected
            Hand firstHand = frame.Hands[0];

            // Get the current gesture vector (relative joint angles of the hand)
            float[] currentGesture = GetHandGestureVector(firstHand);
            // Debug.Log("Current Gesture[0]: " + currentGesture[0]);
            // Capture gestures
            if (Input.GetKeyDown(captureOpenHandKey))
            {
                openHandGesture = currentGesture;
                isOpenHandCaptured = true;
                Debug.Log("Open hand gesture captured.");
            }
            else if (Input.GetKeyDown(captureGunGestureKey))
            {
                gunGesture = currentGesture;
                isGunGestureCaptured = true;
                Debug.Log("Gun gesture captured.");
            }
            else if (Input.GetKeyDown(captureClawGestureKey))
            {
                clawGesture = currentGesture;
                isClawGestureCaptured = true;
                Debug.Log("Claw gesture captured.");
            }
            else if (Input.GetKeyDown(captureFistGestureKey))
            {
                fistGesture = currentGesture;
                isFistGestureCaptured = true;
                Debug.Log("Fist gesture captured.");
            }

            // Only move the cursor if all gestures are captured
            if (isOpenHandCaptured && isGunGestureCaptured && isClawGestureCaptured && isFistGestureCaptured)
            {
                // Calculate the 2D cursor position by interpolating between the gesture vectors
                Vector2 cursorPosition = InterpolateGestureToCursorPosition(currentGesture);

                // Apply scaling factor
                cursorPosition *= movementScale;

                // Clamp the 2D movement within the defined bounds
                cursorPosition.x = Mathf.Clamp(cursorPosition.x, minBounds.x, maxBounds.x);
                cursorPosition.y = Mathf.Clamp(cursorPosition.y, minBounds.y, maxBounds.y);

                // Move the object to the new 2D cursor position (keeping Z constant)
                movableObject.transform.position = new Vector3(cursorPosition.x, cursorPosition.y, movableObject.transform.position.z);
            }
        }
    }

    // Function to get the gesture vector (finger joint angles) of the hand, including 19 dimensions if wrist movements are enabled
    float[] GetHandGestureVector(Hand hand)
    {
        float[] gestureVector = new float[21]; // 19-dimensional vector (3 joints per finger + abduction/adduction + wrist movement)
        int index = 0;
        // Debug.Log("Hand: " + hand);
        foreach (Finger finger in hand.fingers)
        {
            // Convert Leap.Vector to Unity.Vector3 manually
            Vector3 proximalDirection = new Vector3(
                finger.bones[1].Direction.x,
                finger.bones[1].Direction.y,
                finger.bones[1].Direction.z);
            
            Vector3 intermediateDirection = new Vector3(
                finger.bones[2].Direction.x,
                finger.bones[2].Direction.y,
                finger.bones[2].Direction.z);
            
            Vector3 distalDirection = new Vector3(
                finger.bones[3].Direction.x,
                finger.bones[3].Direction.y,
                finger.bones[3].Direction.z);

            // Calculate relative angles between each pair of bones (proximal, intermediate, distal)
            gestureVector[index++] = Vector3.Angle(proximalDirection, intermediateDirection);  // Proximal to Intermediate
            gestureVector[index++] = Vector3.Angle(intermediateDirection, distalDirection);    // Intermediate to Distal
            gestureVector[index++] = Vector3.Angle(distalDirection, Vector3.forward);         // Relative to forward direction
        }

        // Add finger abduction/adduction between fingers (index-middle, middle-ring, ring-pinky)
        gestureVector[index++] = Vector3.Angle(
            new Vector3(hand.fingers[1].bones[1].Direction.x,
                        hand.fingers[1].bones[1].Direction.y,
                        hand.fingers[1].bones[1].Direction.z),
            new Vector3(hand.fingers[2].bones[1].Direction.x,
                        hand.fingers[2].bones[1].Direction.y,
                        hand.fingers[2].bones[1].Direction.z));  // Index to Middle
        
        gestureVector[index++] = Vector3.Angle(
            new Vector3(hand.fingers[2].bones[1].Direction.x,
                        hand.fingers[2].bones[1].Direction.y,
                        hand.fingers[2].bones[1].Direction.z),
            new Vector3(hand.fingers[3].bones[1].Direction.x,
                        hand.fingers[3].bones[1].Direction.y,
                        hand.fingers[3].bones[1].Direction.z));  // Middle to Ring

        gestureVector[index++] = Vector3.Angle(
            new Vector3(hand.fingers[3].bones[1].Direction.x,
                        hand.fingers[3].bones[1].Direction.y,
                        hand.fingers[3].bones[1].Direction.z),
            new Vector3(hand.fingers[4].bones[1].Direction.x,
                        hand.fingers[4].bones[1].Direction.y,
                        hand.fingers[4].bones[1].Direction.z));  // Ring to Pinky

        // Include wrist movements if enabled
        if (includeWristMovements)
        {
            // Wrist flexion/extension (relative to up/down movement)
            gestureVector[index++] = Vector3.Angle(
                new Vector3(hand.PalmNormal.x, hand.PalmNormal.y, hand.PalmNormal.z),
                Vector3.up);    // Flexion/Extension

            // Wrist abduction/adduction (side-to-side movement)
            gestureVector[index++] = Vector3.Angle(
                new Vector3(hand.PalmNormal.x, hand.PalmNormal.y, hand.PalmNormal.z),
                Vector3.right); // Abduction/Adduction
        }
        else
        {
            // If wrist movements are disabled, fill in with zeroes
            gestureVector[index++] = 0f; // Flexion/Extension
            gestureVector[index++] = 0f; // Abduction/Adduction
        }
    
        return gestureVector;
    }


    // Function to interpolate between the four gesture vectors to map to a 2D cursor position
    Vector2 InterpolateGestureToCursorPosition(float[] currentGesture)
    {
        // Normalize distances between the current gesture and each predefined gesture
        float distanceToOpenHand = CalculateGestureDistance(currentGesture, openHandGesture);
        float distanceToGunGesture = CalculateGestureDistance(currentGesture, gunGesture);
        float distanceToClawGesture = CalculateGestureDistance(currentGesture, clawGesture);
        float distanceToFistGesture = CalculateGestureDistance(currentGesture, fistGesture);

        // Calculate total distance to all gestures
        float totalDistance = distanceToOpenHand + distanceToGunGesture + distanceToClawGesture + distanceToFistGesture;

        // Calculate the interpolation weights for each gesture
        float weightOpen = 1.0f - (distanceToOpenHand / totalDistance);
        float weightGun = 1.0f - (distanceToGunGesture / totalDistance);
        float weightClaw = 1.0f - (distanceToClawGesture / totalDistance);
        float weightFist = 1.0f - (distanceToFistGesture / totalDistance);

        // Linearly interpolate the 2D cursor position based on the weights
        Vector2 cursorPosition = Vector2.zero;
        cursorPosition += weightOpen * new Vector2(minBounds.x, maxBounds.y);    // Top-left corner (Open Hand)
        cursorPosition += weightGun * new Vector2(maxBounds.x, maxBounds.y);     // Top-right corner (Gun Gesture)
        cursorPosition += weightClaw * new Vector2(minBounds.x, minBounds.y);    // Bottom-left corner (Claw Gesture)
        cursorPosition += weightFist * new Vector2(maxBounds.x, minBounds.y);    // Bottom-right corner (Fist)

        return cursorPosition;
    }

    // Function to calculate the Euclidean distance between two gesture vectors
    float CalculateGestureDistance(float[] gesture1, float[] gesture2)
    {
        float distance = 0.0f;
        for (int i = 0; i < gesture1.Length; i++)
        {
            distance += Mathf.Pow(gesture1[i] - gesture2[i], 2);
        }
        return Mathf.Sqrt(distance);
    }
}
