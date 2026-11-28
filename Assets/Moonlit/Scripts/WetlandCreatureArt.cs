using System;
using UnityEngine;

namespace Moonlit
{
    internal sealed class WetlandCreatureArt
    {
        static Sprite[] carpFrames, crowFrames;
        static readonly Color ShallowWater = new Color(.62f, .83f, .93f, .86f);
        static readonly Color Spearmint = new Color(.65f, .99f, .91f);
        static readonly Color RippleColor = new Color(.71f, .95f, .93f, .76f);
        static readonly Color WarningGold = new Color(1f, .75f, .36f);
        readonly bool carp;
        readonly Vector2 anchor;
        readonly Sprite[] frames;
        readonly SpriteRenderer body;
        readonly SpriteRenderer[] sparks = new SpriteRenderer[3], cue = new SpriteRenderer[3];
        readonly LineRenderer[] rings;

        public WetlandCreatureArt(WetlandSpecies kind, Transform parent)
        {
            carp = kind == WetlandSpecies.Fish;
            anchor = parent.position;
            frames = carp ? (carpFrames ??= Frames("BronzeCarp")) : (crowFrames ??= Frames("MidnightCrow"));
            body = Visuals.Sprite(carp ? "Bronze Carp Scales" : "Indigo Crow Wings", frames[0], anchor, parent, carp ? -3 : 12);
            for (int i = 0; i < sparks.Length; i++)
            {
                sparks[i] = Visuals.Sprite("Lantern Spark", Visuals.MoonMote, anchor, parent, 18, .18f);
                sparks[i].enabled = false;
                if (carp)
                {
                    cue[i] = Visuals.Box("Water Bubble", anchor, new Vector2(.06f, .09f), RippleColor, parent, 17);
                    cue[i].transform.rotation = Quaternion.Euler(0, 0, 37);
                }
                else
                {
                    cue[i] = Visuals.Box("Air Streak", anchor, new Vector2(.06f, .23f), WarningGold, parent, 17);
                    cue[i].transform.rotation = Quaternion.Euler(0, 0, (i - 1) * -27);
                }
                cue[i].enabled = false;
            }
            rings = new LineRenderer[carp ? 2 : 1];
            for (int i = 0; i < rings.Length; i++)
            {
                var ring = new GameObject(carp ? "Surface Ripple" : "Dive Warning");
                ring.transform.SetParent(parent, false);
                var line = ring.AddComponent<LineRenderer>();
                line.useWorldSpace = false; line.loop = true; line.positionCount = 24;
                line.widthMultiplier = carp ? .031f : .045f;
                line.sharedMaterial = Visuals.Material; line.sortingOrder = 16;
                for (int j = 0; j < line.positionCount; j++)
                {
                    float t = 2 * Mathf.PI * j / line.positionCount;
                    line.SetPosition(j, new Vector3(Mathf.Cos(t), Mathf.Sin(t) * .17f));
                }
                line.enabled = false;
                rings[i] = line;
            }
        }

        static Sprite[] Frames(string resource)
        {
            var atlas = Resources.LoadAll<Sprite>(resource);
            Array.Sort(atlas, (a, b) => string.CompareOrdinal(a.name, b.name));
            if (atlas.Length != 4) throw new InvalidOperationException(resource + " requires four imported sprite frames.");
            return atlas;
        }

        public void Pose(Vector2 center, Vector2 target, WetlandPhase phase, int frame, float facing, float time, float progress)
        {
            bool warning = phase == WetlandPhase.Warning, stunned = phase == WetlandPhase.Stunned;
            body.sprite = frames[frame];
            body.transform.position = center;
            body.flipX = facing < 0;
            float tilt = stunned ? Mathf.Cos(time * 4) * 5 : 0;
            body.transform.localRotation = Quaternion.Euler(0, 0, tilt);
            body.transform.localScale = Vector3.one;
            body.color = stunned ? Spearmint : carp && center.y < anchor.y ? ShallowWater : Color.white;
            for (int i = 0; i < sparks.Length; i++)
            {
                sparks[i].enabled = stunned;
                cue[i].enabled = warning;
                if (stunned)
                {
                    float t = time * 4 + i * 2.0944f;
                    sparks[i].transform.position = center + new Vector2(.36f * Mathf.Cos(t), .57f + .13f * Mathf.Sin(t));
                }
                if (warning && carp)
                {
                    float rise = Mathf.Repeat(time * 1.9f + .333f * i, 1);
                    cue[i].transform.position = anchor + new Vector2(.26f * Mathf.Sin(i * 4 + rise), .07f + .67f * rise);
                    cue[i].color = Color.Lerp(RippleColor, Color.clear, rise * rise);
                }
                else if (warning)
                {
                    cue[i].transform.position = center + new Vector2(facing * .29f + (i - 1) * .17f, .78f + .09f * Mathf.Cos(i - 1));
                }
            }
            for (int i = 0; i < rings.Length; i++)
            {
                var ring = rings[i];
                bool quietRipple = carp && i == 0 && phase == WetlandPhase.Resting;
                ring.enabled = warning || quietRipple;
                if (!ring.enabled) continue;
                float t = Mathf.Repeat(time * (warning ? 1.45f : .62f) + i * .50f, 1);
                float size = carp ? Mathf.Lerp(.25f, warning ? 1.16f : .62f, t) : Mathf.Lerp(.85f, .49f, progress);
                ring.transform.position = carp ? anchor + Vector2.up * .04f : target + Vector2.down * .70f;
                ring.transform.localScale = new Vector3(size, size, 1);
                Color tint = carp ? RippleColor : WarningGold;
                tint.a = carp ? (1 - t) * (warning ? .94f : .43f) : .68f + .24f * Mathf.Sin(time * 12);
                ring.startColor = ring.endColor = tint;
            }
        }

        public void Splash(Vector2 at, MoonlitGame game)
        {
            game.effects.Burst(at + Vector2.up * .12f, RippleColor, 12, 2.2f);
        }
    }
}
