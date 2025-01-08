using UdonSharp;
using VRC.SDK3.Components;
using VRC.SDKBase;

namespace Scenes.Scripts.TextMeshProScripts.House
{
    public class ButtonResponseHOUSE : UdonSharpBehaviour
    {
        void Start()
        {
        
        }

        public void GithubClick()
        {
            VRCUrl url = new VRCUrl("github.com");
        }
    }
}
