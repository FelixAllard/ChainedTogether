using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using System.Collections;

namespace Scenes.Scripts
{
    public class GameScript : UdonSharpBehaviour
    {
        public TimeCounter timeCounter;
        public float maxDistance = 4f;  // Distance before force is applied
        public float elasticity = 1f;  // Strength of the elastic force
        public float damping = 0.5f;  // Damping to smooth out movement
        public float maxVelocity = 10f;  // Max velocity to prevent runaway speed

        public VRCPlayerApi[] players;  // Public array of players
        private VRCPlayerApi[][] playerPairs;  // Private 2D array to store player pairs

        public GameObject chainPrefab;
        private GameObject currentChain;

        // Call this method when a player interacts
        public override void Interact()
        {
            base.Interact();
            var playerList = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
            players = VRCPlayerApi.GetPlayers(playerList);

            // Create a new array of size based on player count / 2
            int pairsCount = players.Length / 2;
            playerPairs = new VRCPlayerApi[pairsCount][];

            VRCPlayerApi previousPlayer = null;
            int index = 0;

            // Loop through the players list to create pairs
            for (int i = 0; i < players.Length; i++)
            {
                if (previousPlayer == null)
                {
                    previousPlayer = players[i];
                    continue;  // Skip the first iteration since we don't have a pair yet
                }

                if (players[i] != null)
                {
                    // Add the player pair (two-player array) to the playerPairs array
                    playerPairs[index] = new VRCPlayerApi[] { previousPlayer, players[i] };
                    index++;
                }

                // Update the previous player reference
                previousPlayer = players[i];
            }

            foreach (var chainPair in playerPairs)
            {
            }
        }

        
        
        
        
        
        
        

        /*
        // FixedUpdate is called every fixed frame-rate frame, we will update the velocities for all player pairs
        void FixedUpdate()
        {
            //if(!Networking.IsMaster) return;
            if (playerPairs == null) return;

            foreach (var pair in playerPairs)
            {
                UpdatePlayerChain(pair[0], pair[1]);  // Apply the chain behavior for each player pair
            }
        }

        // Function to update the elastic and damping forces between two players
        void UpdatePlayerChain(VRCPlayerApi bodyA, VRCPlayerApi bodyB)
        {
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
                Vector3 velocityA = bodyA.GetVelocity();
                Vector3 velocityB = bodyB.GetVelocity();

                // Modify the velocities based on elastic and damping forces
                Vector3 newVelocityA = velocityA + (elasticForce - dampingForce);
                Vector3 newVelocityB = velocityB - (elasticForce - dampingForce);

                // Clamp the velocities to avoid runaway speeds
                newVelocityA = Vector3.ClampMagnitude(newVelocityA, maxVelocity);
                newVelocityB = Vector3.ClampMagnitude(newVelocityB, maxVelocity);

                // Apply the new velocities to the players
                bodyA.SetVelocity(newVelocityA);
                bodyB.SetVelocity(newVelocityB);
                
            }
        }*/
    }
}
