

using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;

namespace Scenes.Scripts.Obstacle.Ladder
{
    public class LadderBar : UdonSharpBehaviour
    {
        public float maxAngleLook = 45f;
        
        private bool pickedUp = false;
        private Vector3 initialPosition;
        private Quaternion initialRotation;
        private VRCPlayerApi player;
        private VRCPickup.PickupHand hand;
        private VRC.SDK3.Components.VRCPickup pickup;

        private Vector3 initialPositionPlayer;
        private Quaternion initialRotationPlayer;
        
        VRCPlayerApi.TrackingData handPlayer;
        VRCPlayerApi.TrackingData headPlayer;
        
        
        
        void Start()
        {
            initialPosition = transform.position;
            initialRotation = transform.rotation;
            player = Networking.LocalPlayer;
            pickup = (VRC.SDK3.Components.VRCPickup)GetComponent(typeof(VRC.SDK3.Components.VRCPickup));
            initialRotationPlayer = LookTowardsNegativeX(transform);
            
            
            
        }

        public override void OnPickup()
        {
            base.OnPickup();
            hand = pickup.currentHand;
            pickedUp = true;
            initialPositionPlayer = player.GetPosition();
            headPlayer = player.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
            
        }

        public override void OnDrop()
        {
            base.OnDrop();
            transform.position = initialPosition;
            transform.rotation = initialRotation;
            pickedUp = false;
        }

        private void Update()
        {
            if (pickedUp)
            {
                player.SetVelocity(Vector3.zero);
                
                Vector3 relativePosition =
                    GetRelativePosition(GetHandPosition(player, hand), player.GetPosition(), initialPosition);
                
                
                //relativePosition.x = initialPositionPlayer.x;
                //relativePosition.z = initialPositionPlayer.z;
                
                //VRCPlayerApi.TrackingData trackingData = player.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
                
                //Quaternion clampedQuaterionPlayerLookDirection = ClampRotation(initialRotationPlayer, player.GetRotation(), maxAngleLook);
                
                //player.TeleportTo(relativePosition,clampedQuaterionPlayerLookDirection);
                
                player.TeleportTo(relativePosition,headPlayer.rotation);
                
            }
        }

        private void LateUpdate()
        {
            if (pickedUp)
            {
                transform.rotation = initialRotation;
            }
        }

        private Vector3 GetHandPosition(VRCPlayerApi player, VRC.SDK3.Components.VRCPickup.PickupHand hand)
        {

            if (hand == VRC.SDK3.Components.VRCPickup.PickupHand.Left)
            {
                handPlayer = player.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand);
            }
            else if (hand == VRC.SDK3.Components.VRCPickup.PickupHand.Right)
            {
                handPlayer = player.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand);
            }
            else
            {
                // If hand is None, return the player's position as fallback
                return player.GetPosition();
            }

            return handPlayer.position;
        }
        public static Vector3 GetRelativePosition(Vector3 origin, Vector3 reference, Vector3 target)
        {
            Vector3 offset = reference - origin; // Get the relative position offset
            return target + offset; // Apply the offset to the target position
        }
        public static Quaternion ClampRotation(Quaternion initialPlayerQuaternion, Quaternion currentPlayerQuaternion, float maxAngle)
        {
            // Calculate the angle difference
            float angleDifference = Quaternion.Angle(initialPlayerQuaternion, currentPlayerQuaternion);

            // If the current rotation exceeds the allowed angle, clamp it
            if (angleDifference > maxAngle)
            {
                // Find the rotation that is exactly at the maxAngle limit
                float clampedT = maxAngle / angleDifference; // Normalize the angle
                return Quaternion.Slerp(initialPlayerQuaternion, currentPlayerQuaternion, clampedT);
            }

            // Otherwise, return the current rotation
            return currentPlayerQuaternion;
        }
        Quaternion LookTowardsNegativeX(Transform reference)
        {
            return Quaternion.LookRotation(-reference.right);
        }
        /*Vector3 GetRandomLocalZPoint(Transform reference, float minZ, float maxZ)
        {
            float randomZ = Random.Range(minZ, maxZ);
            return new Vector3(0, 0, randomZ); // Local space coordinate
        }*/
        /*(float slope, float intercept) GetLinearEquation(Vector3 p1, Vector3 p2)
        {
            float slope = (p2.z - p1.z) / (p2.x - p1.x);  // m = (z2 - z1) / (x2 - x1)
            float intercept = p1.z - slope * p1.x;        // b = z1 - m * x1
            return (slope, intercept);
        }*/


    }
}
