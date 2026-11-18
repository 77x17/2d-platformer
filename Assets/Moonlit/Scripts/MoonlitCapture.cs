using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Moonlit
{
    public static class MoonlitCapture
    {
        // Render the actual game camera + UI into an offscreen target for build QA.
        // Does not depend on a visible desktop, and does not alter source art.
        public static void Save(string path)
        {
            var camera = MoonlitGame.Instance.cameraRig.GetComponent<Camera>();
            var canvas = MoonlitGame.Instance.hud.GetComponent<Canvas>();
            var previousMode = canvas.renderMode; var previousCamera = canvas.worldCamera;
            float previousDistance = canvas.planeDistance;
            var oldTarget = camera.targetTexture; var oldActive = RenderTexture.active;
            var texture = new RenderTexture(1600, 900, 24, RenderTextureFormat.ARGB32);
            Texture2D image = null;
            try
            {
                texture.Create(); camera.targetTexture = texture;
                canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 1;
                canvas.sortingOrder = 1000; Canvas.ForceUpdateCanvases();
                var request = new UniversalRenderPipeline.SingleCameraRequest { destination = texture };
                RenderPipeline.SubmitRenderRequest(camera, request);
                RenderTexture.active = texture;
                image = new Texture2D(1600, 900, TextureFormat.RGB24, false);
                image.ReadPixels(new Rect(0, 0, 1600, 900), 0, 0); image.Apply();
                File.WriteAllBytes(path, image.EncodeToPNG());
            }
            finally
            {
                canvas.renderMode = previousMode; canvas.worldCamera = previousCamera; canvas.planeDistance = previousDistance;
                camera.targetTexture = oldTarget; RenderTexture.active = oldActive;
                texture.Release(); Object.Destroy(texture); if (image) Object.Destroy(image);
            }
        }
    }
}
