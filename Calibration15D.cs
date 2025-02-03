using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;
using MathNet.Numerics.LinearAlgebra.Complex;
 
public class Calibration15D : MonoBehaviour
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
    private double[] openHandGesture;
    private double[] gunGesture;
    private double[] clawGesture;
    private double[] fistGesture;
    public double[] currentGesture = new double[15];
    public double[,] currentGesture2D = new double[15, 1];
    public double[,] a = new double[2, 15];
    public MathNet.Numerics.LinearAlgebra.Double.DenseMatrix aMatrix;

    // Variables to define the 2D plane for movement
    public double movementScale = 1.0f; // Scale factor for hand movements
    public Vector2 minBounds;          // Min X and Y values for 2D plane movement
    public Vector2 maxBounds;          // Max X and Y values for 2D plane movement

    // Flags to check if gestures are captured
    public bool isOpenHandCaptured = false;
    public bool isGunGestureCaptured = false;
    public bool isClawGestureCaptured = false;
    public bool isFistGestureCaptured = false;
    public bool isCalibrated = false;

    void Update()
    {
        // Get the current frame from the Leap Motion provider
        Frame frame = leapProvider.CurrentFrame;

        if (frame != null && frame.Hands.Count > 0)
        {
            // Use the first hand detected
            Hand firstHand = frame.Hands[0];

            // Get the current gesture vector (relative joint angles of the hand)
            currentGesture = GetHandGestureVector(firstHand);

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
            if (isOpenHandCaptured && isGunGestureCaptured && isClawGestureCaptured && isFistGestureCaptured && !isCalibrated)
            {
                double[,] p = new double[8, 1] {{1.0}, {1.0}, {-1.0}, {1.0}, {-1.0}, {-1.0},{1.0}, {-1.0} };
                double[,] h = new double[8, 30];
                for (int i = 0; i < 15; i++){
                    h[0 , i] = openHandGesture[i];
                    h[1 , i + 15] = openHandGesture[i];
                    h[2 , i] = gunGesture[i];
                    h[3 , i + 15] = gunGesture[i];
                    h[4 , i] = clawGesture[i];
                    h[5 , i + 15] = clawGesture[i];
                    h[6 , i] = fistGesture[i];
                    h[7 , i + 15] = fistGesture[i];
                }
                var hMatrix = MathNet.Numerics.LinearAlgebra.Double.DenseMatrix.OfArray(h);
                var hPlusMatrix = hMatrix.PseudoInverse();
                var pMatrix = MathNet.Numerics.LinearAlgebra.Double.DenseMatrix.OfArray(p);
                var flattenedAMatrix = hPlusMatrix.Multiply(pMatrix);
                for (int i = 0; i < 15; i++){
                    a[0, i] = flattenedAMatrix[i, 0];
                    a[1, i] = flattenedAMatrix[i + 15, 0];
                }
                aMatrix = MathNet.Numerics.LinearAlgebra.Double.DenseMatrix.OfArray(a);
                isCalibrated = true;
            }
            if (isCalibrated)
            {
                // Calculate the 2D cursor position by interpolating between the gesture vectors
                for (int i = 0; i < 15; i++){
                    currentGesture2D[i, 0] = currentGesture[i];
                }
                var cursorMapping = aMatrix.Multiply(MathNet.Numerics.LinearAlgebra.Double.DenseMatrix.OfArray(currentGesture2D));
                Vector2 cursorPosition = new Vector2((float)(cursorMapping[0, 0] * movementScale), (float)(cursorMapping[1, 0] * movementScale));

                // Clamp the 2D movement within the defined bounds
                cursorPosition.x = Mathf.Clamp(cursorPosition.x, minBounds.x, maxBounds.x);
                cursorPosition.y = Mathf.Clamp(cursorPosition.y, minBounds.y, maxBounds.y);

                // Move the object to the new 2D cursor position (keeping Z constant)
                movableObject.transform.position = new Vector3(cursorPosition.x, cursorPosition.y, movableObject.transform.position.z);
            }
        }
    }

    // Function to get the gesture vector (finger joint angles) of the hand
    double[] GetHandGestureVector(Hand hand)
    {
        double[] gestureVector = new double[15]; // 15-dimensional vector (3 joints per finger)
        int index = 0;

        foreach (Finger finger in hand.fingers)
        {
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

            // Calculate relative angles between each pair of bones
            gestureVector[index++] = Vector3.Angle(proximalDirection, intermediateDirection);  // Between Proximal and Intermediate
            gestureVector[index++] = Vector3.Angle(intermediateDirection, distalDirection);    // Between Intermediate and Distal
            gestureVector[index++] = Vector3.Angle(distalDirection, Vector3.forward);         // Relative to forward direction
        }

        return gestureVector;
    }

    
}
