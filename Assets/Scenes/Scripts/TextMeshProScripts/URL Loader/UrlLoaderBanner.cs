using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Components;
using VRC.SDK3.Image;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

namespace Scenes.Scripts.TextMeshProScripts.URL_Loader
{
    public class UrlLoaderBanner : UdonSharpBehaviour
    {
        public VRCUrl imageUrl;
        public RawImage targetImage;

        private VRCImageDownloader imageDownloader;


        void Start()
        {
            imageDownloader = new VRCImageDownloader();
            DownloadImage();
        }

        public void DownloadImage()
        {
            if (targetImage == null || imageDownloader == null) return;

            TextureInfo textureInfo = new TextureInfo();
            textureInfo.GenerateMipMaps = false;
            textureInfo.FilterMode = FilterMode.Trilinear;

            imageDownloader.DownloadImage(imageUrl, null, (IUdonEventReceiver)this, textureInfo);
        }

        public override void OnImageLoadSuccess(IVRCImageDownload imageDownload)
        {
            if (targetImage != null)
            {
                targetImage.texture = imageDownload.Result;
            }
        }
        
    }
}