using System;
using UdonSharp;
using UnityEngine;

namespace Scenes.Scripts.Obstacle.RotatingObstacle
{
    public class RotatingObject : UdonSharpBehaviour
    {
        public float rotationMultiplier = 10f;
        void Start()
        {
        
        }

        private void Update()
        {
            this.transform.Rotate(Vector3.up, Time.deltaTime * rotationMultiplier);
        }
    }
}
