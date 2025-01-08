using UdonSharp;
using UnityEngine;
using VRC.Udon.Common.Interfaces;

namespace Scenes.Scripts.Obstacle.Elevator
{
    public class Elevator : UdonSharpBehaviour
    {
        public Transform objectToMove;

        private Vector3 initialPosition;
        public Transform endPositionTransform;
        private Vector3 endPosition;
        public float speed = 5f;
        public float timeAwait = 2f;

        private float journeyTime;
        private float elapsedTime = 0f;
        private bool moving = false;
        private bool movingForward = true;
        private bool waiting = false;
        private float waitTime = 0f;

        void Start()
        {
            initialPosition = objectToMove.position;
            endPosition = endPositionTransform.position;
            journeyTime = Vector3.Distance(initialPosition, endPosition) / speed;
        }

        public override void Interact()
        {
            base.Interact();
            if (!moving)
            {
                SendCustomNetworkEvent(NetworkEventTarget.All, nameof(StartElevator));
            }
        }

        public void StartElevator()
        {
            moving = true;
            elapsedTime = 0f;
            waitTime = 0f;
            waiting = false;
            movingForward = true;
        }

        void Update()
        {
            if (!moving) return;

            if (waiting)
            {
                waitTime += Time.deltaTime;
                if (waitTime >= timeAwait)
                {
                    waiting = false;
                    waitTime = 0f;
                    elapsedTime = 0f;
                    movingForward = !movingForward;
                }
                return;
            }

            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / journeyTime);
            objectToMove.position = movingForward
                ? Vector3.Lerp(initialPosition, endPosition, t)
                : Vector3.Lerp(endPosition, initialPosition, t);

            if (t >= 1f)
            {
                if (!movingForward)
                {
                    moving = false; // Allow reactivation
                }
                else
                {
                    waiting = true;
                }
            }
        }
    }

}
