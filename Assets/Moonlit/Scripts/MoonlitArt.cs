using UnityEngine;

namespace Moonlit
{
    [CreateAssetMenu(menuName = "Moonlit/Art Library")]
    public sealed class MoonlitArt : ScriptableObject
    {
        [Tooltip("Rows: idle, run, jump, lantern. Four frames per row.")]
        public Sprite[] cuoi;
        [Tooltip("Rows: idle, walk, pounce, stunned. Four frames per row.")]
        public Sprite[] dog, cat;
        public Sprite[] environment;
        [Tooltip("Four flat walking surfaces. Pivots sit on the solid moss line, not the transparent image top.")]
        public Sprite[] mossPlatforms;
        public Sprite moonToken;
        [Tooltip("16 extra props: log, bamboo, stone, roots, crate, plate, leaf, branch, rope, stump, steps, planks, leaves, fern, vine.")]
        public Sprite[] forestExtras;
        public Sprite forest, moon;
        public Material spriteMaterial;
        public Font uiFont;
        public Sprite Hero(int row, int frame) => cuoi[row * 4 + frame % 4];
        public Sprite Creature(bool isCat, int row, int frame) => (isCat ? cat : dog)[row * 4 + frame % 4];
    }
}
