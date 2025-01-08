using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Scenes.Scripts.OverallScript
{
    public class VrHandler : UdonSharpBehaviour
    {
        private VRCPlayerApi player;

        void Start()
        {
            player = Networking.LocalPlayer;
            
            // Check if the player is in VR
            if (player.IsUserInVR())
            {
                // Do something specific for VR users
                PerformVRActions();
            }
            else
            {
                // Do something specific for non-VR users
                PerformNonVRActions();
            }
        }

        void PerformVRActions()
        {
            player.SetWalkSpeed(player.GetRunSpeed());
            // Additional VR-specific logic here
            Debug.Log("Player is in VR! Performing VR-specific actions.");
        }

        void PerformNonVRActions()
        {
            

            // Additional non-VR-specific logic here
            Debug.Log("Player is not in VR. Performing non-VR-specific actions.");
        }
    }
}
