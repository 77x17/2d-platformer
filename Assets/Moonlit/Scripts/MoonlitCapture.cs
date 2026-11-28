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
        public static void Save(string path, int width = 1600, int height = 900)
        {
            var camera = MoonlitGame.Instance.ViewCamera;
            var canvas = MoonlitGame.Instance.hud.GetComponent<Canvas>();
            var previousMode = canvas.renderMode; var previousCamera = canvas.worldCamera;
            float previousDistance = canvas.planeDistance;
            var uiLayers = canvas.GetComponentsInChildren<Canvas>(true);
            var orders = new int[uiLayers.Length];
            int lift = 1000 - canvas.sortingOrder;
            for (int i = 0; i < uiLayers.Length; i++) orders[i] = uiLayers[i].sortingOrder;
            var oldTarget = camera.targetTexture; var oldActive = RenderTexture.active;
            var texture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            Texture2D image = null;
            try
            {
                texture.Create(); camera.targetTexture = texture;
                canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 1;
                for (int i = 0; i < uiLayers.Length; i++)
                    if (uiLayers[i] == canvas || uiLayers[i].overrideSorting) uiLayers[i].sortingOrder = orders[i] + lift;
                Canvas.ForceUpdateCanvases();
                var request = new UniversalRenderPipeline.SingleCameraRequest { destination = texture };
                RenderPipeline.SubmitRenderRequest(camera, request);
                RenderTexture.active = texture;
                image = new Texture2D(width, height, TextureFormat.RGB24, false);
                image.ReadPixels(new Rect(0, 0, width, height), 0, 0); image.Apply();
                File.WriteAllBytes(path, image.EncodeToPNG());
            }
            finally
            {
                canvas.renderMode = previousMode; canvas.worldCamera = previousCamera; canvas.planeDistance = previousDistance;
                for (int i = 0; i < uiLayers.Length; i++)
                    if (uiLayers[i] == canvas || uiLayers[i].overrideSorting) uiLayers[i].sortingOrder = orders[i];
                camera.targetTexture = oldTarget; RenderTexture.active = oldActive;
                texture.Release(); Object.Destroy(texture); if (image) Object.Destroy(image);
            }
        }
    }
}
