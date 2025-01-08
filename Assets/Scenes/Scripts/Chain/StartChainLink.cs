using UdonSharp;
using UnityEngine;

namespace Scenes.Scripts.Chain
{
    public class StartChainLink : UdonSharpBehaviour
    {
        public Rigidbody nextLink; // Next link in the chain
        public Rigidbody previousLink; // Previous link in the chain (for both ends)

        public float linearLimitValue = 0.01f; // Tighter constraint to prevent separation
        public float springStrength = 5000f; // Stronger spring force to reduce stretching
        public float damperStrength = 250f; // Increased damping for more stability
        public float angularLimit = 5f; // Restricts rotation to prevent unnatural separation

        private ConfigurableJoint joint;

        void Start()
        {
            // Ensure Rigidbody is configured properly
            Rigidbody rb = GetComponent<Rigidbody>();
            rb.mass = 2f;
            rb.drag = 0.05f;
            rb.angularDrag = 0.02f;
            rb.interpolation = RigidbodyInterpolation.Interpolate; // Smooth physics movement
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous; // Prevents tunneling
            rb.constraints = RigidbodyConstraints.FreezePositionY; // Keeps chain stable

            // Get ConfigurableJoint
            joint = GetComponent<ConfigurableJoint>();
            if (joint == null)
            {
                Debug.LogError("ConfigurableJoint is missing on the chain link!");
                return;
            }

            // Connect to previous or next link
            if (previousLink != null)
            {
                joint.connectedBody = previousLink;
            }
            else if (nextLink != null)
            {
                joint.connectedBody = nextLink;
            }

            // Set tight anchor positions for minimal separation
            /*joint.anchor = Vector3.zero;
            joint.connectedAnchor = Vector3.zero;*/

            // Limit linear motion
            joint.xMotion = ConfigurableJointMotion.Limited;
            joint.yMotion = ConfigurableJointMotion.Limited;
            joint.zMotion = ConfigurableJointMotion.Limited;

            // Limit angular motion
            joint.angularXMotion = ConfigurableJointMotion.Limited;
            joint.angularYMotion = ConfigurableJointMotion.Limited;
            joint.angularZMotion = ConfigurableJointMotion.Limited;

            // Set linear limit
            SoftJointLimit linearLimit = new SoftJointLimit();
            linearLimit.limit = linearLimitValue;
            joint.linearLimit = linearLimit;

            // Set linear limit spring force
            SoftJointLimitSpring spring = new SoftJointLimitSpring();
            spring.spring = springStrength;
            spring.damper = damperStrength;
            joint.linearLimitSpring = spring;

            // Set angular limits
            SoftJointLimit angularX = new SoftJointLimit();
            angularX.limit = angularLimit;
            joint.highAngularXLimit = angularX;

            SoftJointLimit angularY = new SoftJointLimit();
            angularY.limit = angularLimit;
            joint.angularYLimit = angularY;

            SoftJointLimit angularZ = new SoftJointLimit();
            angularZ.limit = angularLimit;
            joint.angularZLimit = angularZ;

            // Disable collision between connected links
            if (nextLink != null)
            {
                Physics.IgnoreCollision(GetComponent<Collider>(), nextLink.GetComponent<Collider>(), true);
            }
            if (previousLink != null)
            {
                Physics.IgnoreCollision(GetComponent<Collider>(), previousLink.GetComponent<Collider>(), true);
            }

            // Prevent accidental breaking
            joint.breakForce = Mathf.Infinity;
            joint.breakTorque = Mathf.Infinity;
        }
    }
}
