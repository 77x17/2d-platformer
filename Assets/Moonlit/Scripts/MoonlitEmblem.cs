using UnityEngine;
using UnityEngine.UI;

namespace Moonlit
{
    // Keep emblems on the standard uGUI Image rendering path in Game View and in players.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class MoonlitEmblem : Image
    {
        public enum Shape { Star, Heart, Spark }

        public Shape shape { get; private set; }

        public void Configure(Shape symbol)
        {
            shape = symbol;
            string resource = symbol == Shape.Star ? "EmblemStar" : symbol == Shape.Spark ? "EmblemSpark" : "HeartFull";
            sprite = Resources.Load<Sprite>(resource);
            preserveAspect = true;
            raycastTarget = false;
        }
    }
}
