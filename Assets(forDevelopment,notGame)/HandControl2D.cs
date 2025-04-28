using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;
//using Leap.Unity;

public class HandControl2D : MonoBehaviour
{
    // Reference to Leap Motion Hand Provider
    public LeapProvider leapProvider;

    // Reference to the GameObject you want to move (e.g., Cube or Sphere)
    public GameObject movableObject;

    // Variables to define the 2D plane for movement
    public float movementScale = 1.0f;  // Scale factor for hand movements
    public Vector2 minBounds;           // Min X and Y values for 2D plane movement
    public Vector2 maxBounds;           // Max X and Y values for 2D plane movement

    void Update()
    {
        // Get the current frame from the Leap Motion provider
        Frame frame = leapProvider.CurrentFrame;
        
        if (frame != null && frame.Hands.Count > 0)
        {
            // Use the first hand detected (can be extended to multiple hands if needed)
            Hand firstHand = frame.Hands[0];

            // Get palm position and map it to 2D coordinates
            Vector3 palmPosition = firstHand.PalmPosition;

            // Convert Leap Motion Vector to Unity Vector3
            Vector3 handPosition = new Vector3(palmPosition.x, palmPosition.y, palmPosition.z);

            // Normalize hand position into the defined 2D movement plane
            Vector2 movement2D = new Vector2(handPosition.x, handPosition.y) * movementScale;

            // Clamp the 2D movement within the bounds
            movement2D.x = Mathf.Clamp(movement2D.x, minBounds.x, maxBounds.x);
            movement2D.y = Mathf.Clamp(movement2D.y, minBounds.y, maxBounds.y);

            // Apply the 2D movement to the GameObject (keeping Z constant)
            movableObject.transform.position = new Vector3(movement2D.x, movement2D.y, movableObject.transform.position.z);
        }
    }
}
