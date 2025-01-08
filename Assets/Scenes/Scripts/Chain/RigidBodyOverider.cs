using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Scenes.Scripts.Chain
{
    public class RigidBodyOverider : UdonSharpBehaviour
    {
        private VRCPlayerApi localPlayer;
        public Rigidbody rigidBody;
        private bool isForLocalPlayer = false;
        private Vector3 previousVelocity;
        private Vector3 playerVelocity;
        public ConfigurableJoint joint; // The joint that connects the secondary object to the player

        void Start()
        {
            localPlayer = Networking.LocalPlayer;
            isForLocalPlayer = localPlayer.IsOwner(this.gameObject);
            previousVelocity = Vector3.zero;

            // Initialize Rigidbody and make sure gravity is off
            if (rigidBody != null)
            {
                rigidBody.useGravity = false;
                rigidBody.isKinematic = false; // Make sure it can be affected by joints
            }
        }

        void FixedUpdate()
        {
            /*if (isForLocalPlayer)
            {
                rigidBody.transform.position = localPlayer.GetPosition();
                rigidBody.transform.rotation = localPlayer.GetRotation();
                // Capture the player's current velocity
                playerVelocity = localPlayer.GetVelocity();

                // Adjust the player's velocity based on the velocity of the secondary rigid body
                Vector3 velocityDifference = rigidBody.velocity - previousVelocity;

                // Apply the velocity difference to the player's velocity
                if (velocityDifference.sqrMagnitude > 0.01f)
                {
                    // You can tweak the multiplier to adjust the strength of the effect
                    Vector3 appliedForce = velocityDifference * 0.5f;  
                    localPlayer.SetVelocity(playerVelocity + appliedForce);
                }

                // Track the previous velocity for next frame calculations
                previousVelocity = rigidBody.velocity;
            }*/
        }
        
    }
}