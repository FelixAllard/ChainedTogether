using System;
using System.Linq;
using System.Numerics;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDK3.ClientSim;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;
using VRC.Udon.Serialization.OdinSerializer.Utilities;
using Vector3 = UnityEngine.Vector3;

namespace Scenes.Scripts.Chain
{
    public class ColarHook : UdonSharpBehaviour
    {
        public RigidBodyOverider foundScript;
        public Rigidbody colarRigidBody;
        public bool activated = false;
        public VRCPlayerApi player;
        [FormerlySerializedAs("hasStabilized")] public bool seeVelocity = false; // Prevents instant force spike on activation
        public Transform otherCollar;
        public ColarHook otherCollarHook;
        public Rigidbody otherCollarRigidBody;
        public float maximumDistanceCollar;
        public float multiplierCollarVelocityStrenght = 10f;
        public float minimumMagnitude = 5f;

        private Vector3 velocityStartOfFrame;

        private Vector3 lastVelocityUpdate;
      
        public Transform[] linkTransform;
        public Rigidbody[] linkRigidBody;

        public float tresholdDeviation = 0.05f;
        public float maximumCableLenght = 6.5f;


        public Transform redBall;
        void Start()
        {
            if (colarRigidBody == null)
            {
                colarRigidBody = GetComponent<Rigidbody>();
                if (colarRigidBody == null)
                {
                    Debug.LogError("[ColarHook] No Rigidbody found!");
                }
            }
            linkRigidBody = GetRigidbodiesFromLinks(linkTransform);
            
        }
        private bool isTransferringOwnership = false;

        public override void Interact()
        {
            base.Interact();
            
            if (isTransferringOwnership)
                return;
            if (Networking.IsOwner(gameObject))
            {
                SendCustomNetworkEvent(NetworkEventTarget.All, nameof(OwnerTookControl));  
            }
            
            player = Networking.LocalPlayer;
            RequestSerialization();
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            isTransferringOwnership = true;
            
        }

        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            base.OnOwnershipTransferred(player);
            this.player = player;
            activated = true;
            BoxCollider boxCollider = colarRigidBody.gameObject.GetComponent<BoxCollider>();
            boxCollider.enabled = false;
            isTransferringOwnership = false;
        }

        public void OwnerTookControl()
        {
            player = Networking.GetOwner(this.gameObject);
            activated = true;
            BoxCollider boxCollider = colarRigidBody.gameObject.GetComponent<BoxCollider>();
            boxCollider.enabled = false;
            isTransferringOwnership = false;
        }
        

        private void FixedUpdate()
        {
            if(!activated)
                return;
            colarRigidBody.transform.position = player.GetPosition() + new Vector3(0, 1f, 0);
            
            if (!Networking.IsOwner(gameObject)) // Ensure only the owner modifies the object
                return;
            /*else
            {
                transform.LookAt(otherCollar.position);
                this.otherCollarRigidBody.velocity = Vector3.zero;
            }*/
            velocityStartOfFrame = player.GetVelocity();
            colarRigidBody.velocity = velocityStartOfFrame;
            lastVelocityUpdate = Vector3.zero;
        }

        private void LateUpdate()
        {
            if (!activated || !Networking.IsOwner(gameObject))
                return;

            bool wasGrounded = player.IsPlayerGrounded();
            Vector3 jointVelocityColar = velocityStartOfFrame - colarRigidBody.velocity;
            
            jointVelocityColar *= multiplierCollarVelocityStrenght;
    
            jointVelocityColar.x = Mathf.Clamp(jointVelocityColar.x, -12f, 12f);
            jointVelocityColar.y = Mathf.Clamp(jointVelocityColar.y, -12f, 0f); 
            jointVelocityColar.z = Mathf.Clamp(jointVelocityColar.z, -12f, 12f);

            Vector3 chainBendPoint = FindChainBendPoint(this.transform);

            if (Vector3.Distance(this.transform.position, otherCollar.position) < 2f)
            {
                return;
            }
            

            if (player.GetVelocity().magnitude - lastVelocityUpdate.magnitude > 15f)
            {
                player.SetVelocity(jointVelocityColar);
                lastVelocityUpdate = jointVelocityColar;
                //Debug.Log("Player Velocity was to high, trying to salvage");
                return;
            }
            //Save if the cable just go haywire
            if (GetLenghtCable() > maximumCableLenght + 1f)
            {
                foreach (var link in linkRigidBody)
                {
                    link.velocity = Vector3.zero;
                }
            }
    
            if (Vector3.Distance(this.transform.position, otherCollar.position) > maximumDistanceCollar)
            {
                Vector3 teleportationPoint = GetPointAtDistance(otherCollar.position, player.GetPosition(), maximumDistanceCollar-2f);
                player.TeleportTo(teleportationPoint, player.GetRotation());
                player.SetVelocity(GetVelocityTowards(this.transform.position, otherCollar.position, 10f));
                colarRigidBody.velocity = Vector3.zero;
                //Debug.Log("To far from other collar");

                return;
                 
            }
            if (GetLenghtCable() > maximumCableLenght)
            {
                
                player.SetVelocity(GetVelocityTowards(this.transform.position, chainBendPoint, 4f));
                //Vector3 teleportPosition = GetPointAtDistance(player.GetPosition(), chainBendPoint, 0.3f);
                //Debug.Log("Cable Lenght Reached");
                return;
            }
            if (jointVelocityColar.magnitude < minimumMagnitude)
            {
                if (!wasGrounded && lastVelocityUpdate != Vector3.zero)
                {
                    player.SetVelocity(player.GetVelocity() - lastVelocityUpdate);
                    lastVelocityUpdate = Vector3.zero;
                }
                //Debug.Log("Low velociy, not worth putting to use");
                return;
            }
            
            
            

            
            /*
            Vector3 velocityAddedToPlayerSinceStartOfFrame = player.GetVelocity() - velocityStartOfFrame;
            Vector3 totalVelocity = jointVelocityColar + velocityAddedToPlayerSinceStartOfFrame + velocityStartOfFrame;

            if (!wasGrounded)
            {
                player.TeleportTo(player.GetPosition() + new Vector3(0, 0.01f , 0), player.GetRotation(), default, true);
        
                Vector3 downwardVelocity = new Vector3(0, -0.01f, 0);
                totalVelocity += downwardVelocity;
                player.SetVelocity(totalVelocity - lastVelocityUpdate);
            }
            else
            {
                player.SetVelocity(totalVelocity - lastVelocityUpdate);
            }
    
            lastVelocityUpdate = -totalVelocity;*/
            
            
            
            
            
        }

        float GetLenghtCable()
        {
            float distance = 0;
            for (int i = 0; i < linkTransform.Length; i++)
            {
                if(i==0)
                    continue;
                distance += Vector3.Distance(linkTransform[i-1].position, linkTransform[i].position);
                
            }
            return distance;
        }
        Vector3 GetPointAtDistance(Vector3 start, Vector3 target, float distance)
        {
            Vector3 direction = (target - start).normalized; // Get direction
            return start + direction * distance; // Move along the direction by 'distance'
        }
        Vector3 GetVelocityTowards(Vector3 start, Vector3 target, float speed)
        {
            Vector3 direction = (target - start).normalized; // Get unit direction
            return direction * speed; // Scale by speed
        }
        bool IsVelocityTowards(Vector3 velocity, Vector3 currentPosition, Vector3 targetPosition, float maxAngle = 70f)
        {
            if (velocity.magnitude == 0) return false; // No movement

            Vector3 toTarget = (targetPosition - currentPosition).normalized; // Direction to target
            Vector3 velocityDirection = velocity.normalized; // Normalize velocity to compare direction

            float dot = Vector3.Dot(velocityDirection, toTarget); // Get dot product

            float threshold = Mathf.Cos(maxAngle * Mathf.Deg2Rad); // Convert max angle to a dot product value

            return dot >= threshold; // True if within the allowed angle
        }
        Vector3 FindChainBendPoint(Transform characterTransform)
        {
            if (linkTransform == null || linkTransform.Length < 3)
            {
                //Debug.LogError("[ColarHook] Link Found!");
                return Vector3.zero; // Not enough links to analyze bends
            }

            // Find the closest link to the character
            int closestIndex = 0;
            float minDistance = float.MaxValue;

            for (int i = 0; i < linkTransform.Length; i++)
            {
                float distance = Vector3.Distance(characterTransform.position, linkTransform[i].position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestIndex = i;
                }
            }

            // Start iterating from the closest link
            for (int i = closestIndex; i < linkTransform.Length - 2; i++)
            {
                // Get the three links to analyze
                Vector3 p1 = linkTransform[i].position;
                Vector3 p2 = linkTransform[i + 1].position;
                Vector3 p3 = linkTransform[i + 2].position;

                // Compute the average position of these three links
                Vector3 averagePosition = (p1 + p2 + p3) / 3f;

                // Compute the expected position along the straight line between p1 and p3
                Vector3 expectedPosition = p1 + (p3 - p1) * 0.5f; // Midpoint of the straight line

                // Check if the average deviates from the expected line
                float deviation = Vector3.Distance(averagePosition, expectedPosition);

                if (deviation > tresholdDeviation) // Adjust threshold as needed
                {
                    //redBall.position = p2;
                    return p2; // Return the middle link where the bend occurs
                }
            }
            //redBall.position = linkTransform[linkTransform.Length - 1].position;
            // If no bend is found, return the last link (assume chain is straight)
            return linkTransform[linkTransform.Length - 1].position;
        }
        Vector3 ModifyVelocityTowardsDirection(Vector3 currentVelocity, Vector3 targetDirection)
        {
            if (targetDirection == Vector3.zero)
            {
                //Debug.LogError("[ColarHook] No direction found!");
                return Vector3.zero; // No target direction, return zero velocity
            }

            float magnitude = currentVelocity.magnitude; // Preserve the current velocity's magnitude
            Vector3 normalizedDirection = targetDirection.normalized; // Normalize the target direction
            return normalizedDirection * magnitude; // Apply the magnitude to the new direction
        }
        Vector3 GetVelocityTowardsDirection(Vector3 targetDirection, float speedMultiplier)
        {
            if (targetDirection == Vector3.zero)
            {
                return Vector3.zero; // No movement if the direction is zero
            }

            Vector3 normalizedDirection = targetDirection.normalized; // Normalize the target direction
            return normalizedDirection * speedMultiplier; // Apply the multiplier
        }
        Rigidbody[] GetRigidbodiesFromLinks(Transform[] linkTransforms)
        {
            Rigidbody[] rigidbodies = new Rigidbody[linkTransforms.Length];

            for (int i = 0; i < linkTransforms.Length; i++)
            {
                // Get the Rigidbody attached to the current link Transform
                Rigidbody rb = linkTransforms[i].GetComponent<Rigidbody>();

                if (rb != null)
                {
                    rigidbodies[i] = rb;
                }
                else
                {
                    //Debug.LogWarning("No Rigidbody found on link transform at index " + i);
                }
            }

            return rigidbodies;
        }





    }
    
}
