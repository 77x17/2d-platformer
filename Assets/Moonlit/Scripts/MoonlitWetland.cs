using UnityEngine;

namespace Moonlit
{
    public sealed partial class MoonlitLevel
    {
        static readonly Sprite[] marshPlants = new Sprite[6];

        static Sprite MarshPlant(int index)
        {
            if (marshPlants[index]) return marshPlants[index];
            var atlas = Resources.Load<Texture2D>("WetlandPlants");
            const int cell = 128;
            var rect = new Rect(index % 3 * cell, (1 - index / 3) * cell, cell, cell);
            marshPlants[index] = Sprite.Create(atlas, rect, new Vector2(.5f, 2f / cell), cell, 0, SpriteMeshType.FullRect);
            return marshPlants[index];
        }

        bool OpenWater(float x, float waterY, float clearance)
        {
            foreach (var ground in land)
            {
                var position = ground.transform.position;
                if (position.y < waterY - .24f) continue;
                if (Mathf.Abs(position.x - x) < ground.size.x * .5f + clearance) return false;
            }
            return true;
        }

        void PlantWaterMargins(float start, float end, float waterY)
        {
            var random = new System.Random(Mathf.RoundToInt((start + 47) * 21) + stage * 337);
            // Reeds can take root on low banks, including both ends of an actual gap.
            foreach (var ground in land)
            {
                var center = ground.transform.position;
                if (center.y < waterY - .15f || center.y > waterY + 2.55f) continue;
                for (int side = -1; side <= 1; side += 2)
                {
                    float edge = center.x + side * ground.size.x * .5f;
                    if (edge < start || edge > end || !OpenWater(edge + side * .35f, waterY, .1f)) continue;
                    if (random.NextDouble() < (stage == 3 ? .49 : .18)) continue;
                    float inset = .46f + (float)random.NextDouble() * .42f;
                    var foot = new Vector2(edge - side * inset, center.y - .045f);
                    var reed = Visuals.Sprite("Cỏ bờ nước", MarshPlant(random.Next(3)), foot, decoration, 1,
                        1.03f + .69f * (float)random.NextDouble());
                    reed.color = new Color(.76f,.84f,.83f); reed.flipX = side < 0;
                    contacts.Add(new GroundContact { art = reed, localY = 0 });
                    var breeze = reed.gameObject.AddComponent<AmbientMotion>();
                    breeze.sway = 2; breeze.speed = .88f; breeze.phase = edge; breeze.windAffected = true;
                }
            }
            // Leave stretches of open current between small floating colonies.
            for (float x = start + 2.3f + (float)random.NextDouble() * 3; x < end - 1.5f;
                x += 5.5f + (float)random.NextDouble() * 7)
            {
                if (!OpenWater(x, waterY, .95f)) continue;
                var pads = Visuals.Sprite("Lá nổi trên dòng", MarshPlant(3 + random.Next(3)),
                    new Vector2(x, waterY - .08f), decoration, -3, 1.05f + .62f * (float)random.NextDouble());
                pads.color = new Color(.77f,.84f,.85f); pads.flipX = random.Next(2) == 1;
                var drift = pads.gameObject.AddComponent<AmbientMotion>();
                drift.sway = .9f; drift.bob = .028f; drift.speed = .91f; drift.phase = x;
            }
        }
    }
}
