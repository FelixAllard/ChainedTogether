
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Scenes.Scripts.Joints.PlayerJoints
{
    public class ChainScript : UdonSharpBehaviour
    {
        [UdonSynced] public int playerIdA;  // Player ID of bodyA
        [UdonSynced] public int playerIdB;  // Player ID of bodyB
        public float maxDistance = 2f;  // Distance before force is applied
        public float elasticity = 5f;  // Strength of the elastic force
        public float damping = 0.5f;  // Damping to smooth out movement

        private VRCPlayerApi bodyA;
        private VRCPlayerApi bodyB;

        private Vector3 velocityA;  // Store velocity for bodyA
        private Vector3 velocityB;  // Store velocity for bodyB

        public void Awake()
        {
            this.enabled = false;
        }

        public void Init(VRCPlayerApi rb, VRCPlayerApi rb2)
        {
            Debug.LogWarning("Init actually ran!");
            if (rb == null)
            {
                Debug.LogWarning("VRCPlayerApi is null");
            }
            bodyA = rb;
            bodyB = rb2;
            playerIdA = bodyA.playerId;
            playerIdB = bodyB.playerId;
            Debug.Log(playerIdA + ", " + playerIdB);
            this.enabled = true;
        }

        void FixedUpdate()
        {
            // Retrieve the player objects based on player IDs
            if (bodyA == null || bodyB == null)
            {
                bodyA = VRCPlayerApi.GetPlayerById(playerIdA);
                bodyB = VRCPlayerApi.GetPlayerById(playerIdB);
            }

            if (bodyA == null || bodyB == null) return;

            Vector3 direction = bodyB.GetPosition() - bodyA.GetPosition();
            float currentDistance = direction.magnitude;

            if (currentDistance > maxDistance)
            {
                direction.Normalize();
                float stretch = currentDistance - maxDistance;

                // Hooke's Law: F = k * x
                Vector3 elasticForce = elasticity * stretch * direction;

                // Damping force to stabilize movement
                Vector3 relativeVelocity = bodyB.GetVelocity() - bodyA.GetVelocity();
                Vector3 dampingForce = damping * relativeVelocity;

                // Get the current velocity of each player
                velocityA = bodyA.GetVelocity();
                velocityB = bodyB.GetVelocity();

                // Modify the velocities based on elastic and damping forces
                Vector3 newVelocityA = velocityA + (elasticForce - dampingForce);
                Vector3 newVelocityB = velocityB - (elasticForce - dampingForce);

                // Synchronize velocities across the network
                if (Networking.IsOwner(gameObject))
                {
                    // Only update the velocities if the current player is the owner of the object
                    bodyA.SetVelocity(newVelocityA);
                    bodyB.SetVelocity(newVelocityB);

                    // Send custom network event to synchronize this change
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "SyncVelocity");
                }
            }
        }

        // Custom network event for synchronizing velocity
        public void SyncVelocity()
        {
            // Apply the synchronized velocity changes
            bodyA.SetVelocity(velocityA);
            bodyB.SetVelocity(velocityB);
        }
    }
}

