using System;
using UdonSharp;
using UnityEngine;

namespace Scenes.Scripts
{
    public class TimeCounter : UdonSharpBehaviour
    {
        public double time;
        private bool timeStarted = false;
        private double finalTime;
        private void Update()
        {
            if (timeStarted)
            {
                time += Time.deltaTime;
            }
        }

        public void StartTime()
        {
            time = 0;
            finalTime = 0;
            timeStarted = true;
        }

        public void FinishTime()
        {
            finalTime = time;
        }

        public void StopTime()
        {
            timeStarted = false;
        }
    }
}