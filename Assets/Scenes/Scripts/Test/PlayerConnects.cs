using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common;

namespace Scenes.Scripts.Test
{
    public class PlayerConnects : UdonSharpBehaviour
    {
        public GameObject targetObject;  // The object that the player should move towards
        public Rigidbody rb;
        public SpringJoint spring;
        public float playerSpeed = 5f;   // Speed of the local player moving towards the object
        public float objectSpeed = 3f;   // Speed of the object moving towards the 
        private bool shouldTp = false;

        void Update()
        {
            if (Networking.LocalPlayer == null || targetObject == null) return;
            if(Vector3.Distance(Networking.LocalPlayer.GetPosition(), spring.connectedBody.transform.position) < 5f) return;

            // Get positions of the player and the object
            Vector3 playerPosition = Networking.LocalPlayer.GetPosition();
            Vector3 objectPosition = targetObject.transform.position;

            // Calculate direction vectors towards each other
            Vector3 playerDirection = (objectPosition - playerPosition).normalized;
            // Set velocities
            Vector3 playerVelocity = playerDirection * playerSpeed;


            // Apply the velocities
            Networking.LocalPlayer.SetVelocity(playerVelocity);
            if (shouldTp)
            {
                rb.velocity = new Vector3(0,0,0);
                targetObject.transform.position = playerPosition;
            }
            shouldTp = !shouldTp;
        }
    
    }
}
