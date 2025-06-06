/******************************************************************************
 * Copyright (C) Ultraleap, Inc. 2011-2024.                                   *
 *                                                                            *
 * Use subject to the terms of the Apache License 2.0 available at            *
 * http://www.apache.org/licenses/LICENSE-2.0, or another agreement           *
 * between Ultraleap and you, your company or other organization.             *
 ******************************************************************************/

namespace LeapInternal
{
    using Leap;
    using UnityEngine;
    public static class CopyFromLeapCExtensions
    {
        public static readonly float MM_TO_M = 1e-3f;

        /**
         * Provides a static LeapTransform that converts from Leap units and coordinates to Unity
         */
        public static LeapTransform LeapToUnityTransform
        {
            get
            {
                LeapTransform leapToUnityTransform = new LeapTransform(Vector3.zero, Quaternion.identity, new Vector3(MM_TO_M, MM_TO_M, MM_TO_M));
                leapToUnityTransform.MirrorZ();

                return leapToUnityTransform;
            }
        }

        public static void TransformToUnityUnits(this Hand hand)
        {
            hand.Transform(LeapToUnityTransform);
        }


        /**
         * Copies the data from an internal tracking message into a frame.
         *
         * @param trackingMsg The internal tracking message with the data to be copied into this frame.
         */
        public static Frame CopyFrom(this Frame frame, ref LEAP_TRACKING_EVENT trackingMsg)
        {
            frame.Id = (long)trackingMsg.info.frame_id;
            frame.Timestamp = (long)trackingMsg.info.timestamp;
            frame.CurrentFramesPerSecond = trackingMsg.framerate;

            frame.ResizeHandList((int)trackingMsg.nHands);

            for (int i = frame.Hands.Count; i-- != 0;)
            {
                LEAP_HAND hand;
                StructMarshal<LEAP_HAND>.ArrayElementToStruct(trackingMsg.pHands, i, out hand);
                frame.Hands[i].CopyFrom(ref hand, frame.Id);
            }

            return frame;
        }

        /**
         * Copies the data from an internal hand definition into a hand.
         *
         * @param leapHand The internal hand definition to be copied into this hand.
         * @param frameId The frame id of the frame this hand belongs to.
         */
        private static Hand CopyFrom(this Hand hand, ref LEAP_HAND leapHand, long frameId)
        {
            hand.FrameId = frameId;
            hand.Id = (int)leapHand.id;

            hand.Arm.CopyFrom(leapHand.arm, Bone.BoneType.UNKNOWN);

            hand.Confidence = leapHand.confidence;
            hand.GrabStrength = leapHand.grab_strength;
            hand.PinchStrength = leapHand.pinch_strength;
            hand.PinchDistance = leapHand.pinch_distance * MM_TO_M; // This is not converted to M when scaling the hand, so we should convert it here
            hand.PalmWidth = leapHand.palm.width;
            hand.IsLeft = leapHand.type == eLeapHandType.eLeapHandType_Left;
            hand.TimeVisible = (float)(leapHand.visible_time * 1e-6);
            hand.PalmPosition = leapHand.palm.position.ToVector3();
            hand.StabilizedPalmPosition = leapHand.palm.stabilized_position.ToVector3();
            hand.PalmVelocity = leapHand.palm.velocity.ToVector3();
            hand.PalmNormal = leapHand.palm.normal.ToVector3();
            hand.Rotation = leapHand.palm.orientation.ToQuaternion();
            hand.Direction = leapHand.palm.direction.ToVector3();
            hand.WristPosition = hand.Arm.NextJoint;

            hand.fingers[0].CopyFrom(leapHand.thumb, Leap.Finger.FingerType.THUMB, hand.Id, hand.TimeVisible);
            hand.fingers[1].CopyFrom(leapHand.index, Leap.Finger.FingerType.INDEX, hand.Id, hand.TimeVisible);
            hand.fingers[2].CopyFrom(leapHand.middle, Leap.Finger.FingerType.MIDDLE, hand.Id, hand.TimeVisible);
            hand.fingers[3].CopyFrom(leapHand.ring, Leap.Finger.FingerType.RING, hand.Id, hand.TimeVisible);
            hand.fingers[4].CopyFrom(leapHand.pinky, Leap.Finger.FingerType.PINKY, hand.Id, hand.TimeVisible);

            hand.TransformToUnityUnits();

            return hand;
        }

        /**
         * Copies the data from an internal finger definition into a finger.
         *
         * @param leapBone The internal finger definition to be copied into this finger.
         * @param type The finger type of this finger.
         * @param frameId The frame id of the frame this finger belongs to.
         * @param handId The hand id of the hand this finger belongs to.
         * @param timeVisible The time in seconds that this finger has been visible.
         */
        private static Finger CopyFrom(this Finger finger, LEAP_DIGIT leapBone, Finger.FingerType type, int handId, float timeVisible)
        {
            finger.Id = (handId * 10) + leapBone.finger_id;
            finger.HandId = handId;
            finger.TimeVisible = timeVisible;

            Bone metacarpal = finger.bones[0];
            Bone proximal = finger.bones[1];
            Bone intermediate = finger.bones[2];
            Bone distal = finger.bones[3];

            metacarpal.CopyFrom(leapBone.metacarpal, Leap.Bone.BoneType.METACARPAL);
            proximal.CopyFrom(leapBone.proximal, Leap.Bone.BoneType.PROXIMAL);
            intermediate.CopyFrom(leapBone.intermediate, Leap.Bone.BoneType.INTERMEDIATE);
            distal.CopyFrom(leapBone.distal, Leap.Bone.BoneType.DISTAL);

            finger.TipPosition = distal.NextJoint;
            finger.Direction = intermediate.Direction;
            finger.Width = intermediate.Width;
            finger.Length = (leapBone.finger_id == 0 ? 0.0f : 0.5f * proximal.Length) + intermediate.Length + 0.77f * distal.Length; //The values 0.5 for proximal and 0.77 for distal are used in platform code for this calculation
            finger.IsExtended = leapBone.is_extended != 0;
            finger.Type = type;

            return finger;
        }

        /**
         * Copies the data from an internal bone definition into a bone.
         *
         * @param leapBone The internal bone definition to be copied into this bone.
         * @param type The bone type of this bone.
         */
        private static Bone CopyFrom(this Bone bone, LEAP_BONE leapBone, Bone.BoneType type)
        {
            bone.Type = type;
            bone.PrevJoint = leapBone.prev_joint.ToVector3();
            bone.NextJoint = leapBone.next_joint.ToVector3();
            bone.Direction = (bone.NextJoint - bone.PrevJoint);
            bone.Length = bone.Direction.magnitude;

            if (bone.Length < float.Epsilon)
            {
                bone.Direction = Vector3.zero;
            }
            else
            {
                bone.Direction /= bone.Length;
            }

            bone.Center = (bone.PrevJoint + bone.NextJoint) / 2.0f;
            bone.Rotation = leapBone.rotation.ToQuaternion();
            bone.Width = leapBone.width;

            return bone;
        }
    }
}