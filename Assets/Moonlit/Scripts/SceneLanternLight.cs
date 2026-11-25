using System.Collections.Generic;
using UnityEngine;

namespace Moonlit
{
    public sealed class SceneLanternLight : MonoBehaviour
    {
        static readonly List<SceneLanternLight> Active = new List<SceneLanternLight>();
        public SpriteRenderer Source { get; set; }
        void OnEnable() { Active.Add(this); }
        void OnDisable() { Active.Remove(this); }

        // Only four light vectors cross the CPU/GPU boundary, regardless of chapter length.
        public static void NearCamera(Vector2 center, Vector4[] output)
        {
            for (int i = 0; i < output.Length; i++) output[i] = Vector4.zero;
            foreach (var light in Active)
            {
                if (!light.Source || light.Source.color.a < .15f) continue;
                Vector2 at = light.transform.position;
                float distance = (center-at).sqrMagnitude;
                if (distance > 26*26) continue;
                var candidate = new Vector4(at.x,at.y,Mathf.Max(2.2f,light.Source.bounds.size.x*.95f),light.Source.color.a);
                for (int i = 0; i < output.Length; i++)
                {
                    if (output[i].w != 0 && distance >= (center - (Vector2)output[i]).sqrMagnitude) continue;
                    for (int j = output.Length-1; j > i; j--) output[j] = output[j-1];
                    output[i] = candidate;
                    break;
                }
            }
        }
    }
}
