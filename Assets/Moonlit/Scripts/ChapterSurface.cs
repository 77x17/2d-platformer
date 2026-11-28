using UnityEngine;

namespace Moonlit
{
    public enum ChapterSurfaceKind { Bank, BambooDeck, StoneLedge, WeatheredTimber, CanopyPerch, ChimePlatform, ExplorationDeck, ExplorationShelf }

    // Move the object to change the walking height, or edit its Box Collider to resize it.
    [DisallowMultipleComponent, RequireComponent(typeof(BoxCollider2D))]
    public sealed class ChapterSurface : MonoBehaviour
    {
        public ChapterSurfaceKind kind;
        public Vector2 Size => GetComponent<BoxCollider2D>().size * (Vector2)transform.lossyScale;
        public Vector2 Top
        {
            get
            {
                var shape = GetComponent<BoxCollider2D>();
                return transform.TransformPoint(shape.offset + Vector2.up * (shape.size.y * .5f));
            }
        }

        // The authored shape supplies the geometry for each attempt's live collider.
        // Disable its editor representation before admitting bodies to the physics scene.
        void Awake() => GetComponent<BoxCollider2D>().enabled = false;
        public Vector2 travel;

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (Application.isPlaying) return;
            bool ground = kind == ChapterSurfaceKind.Bank;
            float thickness = ground ? Mathf.Min(2.5f,Size.y) : Size.y;
            Vector3 at = (Vector3)Top + Vector3.down * (thickness * .5f);
            Vector3 size = new Vector3(Size.x, thickness, .05f);
            Color tint = ground ? new Color(.24f,.64f,.49f) : new Color(.95f,.70f,.34f);
            Gizmos.color = new Color(tint.r,tint.g,tint.b,.45f);
            Gizmos.DrawCube(at,size);
            Gizmos.color = tint; Gizmos.DrawWireCube(at,size);
            if (travel != Vector2.zero)
                Gizmos.DrawLine(transform.position - (Vector3)travel, transform.position + (Vector3)travel);
        }
#endif
    }
}
