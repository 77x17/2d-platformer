using UnityEngine;

namespace Moonlit
{
    // Fixed overhead attachment, moving lower attachment. Ropes never float with the lift.
    public sealed class LiftSuspension : MonoBehaviour
    {
        readonly LineRenderer[] ropes = new LineRenderer[2];
        Vector2 anchor;
        float halfWidth;
        public void Initialize(Vector2 fixedAnchor, float width)
        {
            anchor = fixedAnchor; halfWidth = width * .40f;
            for (int i = 0; i < 2; i++)
            {
                var go = new GameObject("Fixed banyan suspension " + i); go.transform.SetParent(transform);
                var line = go.AddComponent<LineRenderer>(); line.useWorldSpace = true; line.positionCount = 2;
                line.sharedMaterial = Visuals.Material; line.sortingOrder = 4;
                line.startWidth = line.endWidth = .065f;
                line.startColor = line.endColor = new Color(.32f, .35f, .20f);
                ropes[i] = line;
            }
        }
        void LateUpdate()
        {
            for (int i = 0; i < 2; i++)
            {
                float side = i == 0 ? -halfWidth : halfWidth;
                ropes[i].SetPosition(0, anchor + Vector2.right * side);
                ropes[i].SetPosition(1, transform.position + new Vector3(side, .30f, 0));
            }
        }
    }
}
