using UnityEngine;
using UnityEngine.UI;

public class CircleDrawer : MaskableGraphic
{
    public float thickness = 5f;
    public int segments = 100;
    public float radius = 50f;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        float outerRadius = radius;
        float innerRadius = radius - thickness;

        float angleIncrement = 360f / segments;
        Vector2 prevOuter = new Vector2(outerRadius, 0);
        Vector2 prevInner = new Vector2(innerRadius, 0);

        for (int i = 1; i <= segments; i++)
        {
            float angle = angleIncrement * i * Mathf.Deg2Rad;
            Vector2 outer = new Vector2(Mathf.Cos(angle) * outerRadius, Mathf.Sin(angle) * outerRadius);
            Vector2 inner = new Vector2(Mathf.Cos(angle) * innerRadius, Mathf.Sin(angle) * innerRadius);

            vh.AddVert(prevOuter, color, Vector2.zero);
            vh.AddVert(outer, color, Vector2.zero);
            vh.AddVert(inner, color, Vector2.zero);
            vh.AddVert(prevInner, color, Vector2.zero);

            int idx = (i - 1) * 4;
            vh.AddTriangle(idx, idx + 1, idx + 2);
            vh.AddTriangle(idx + 2, idx + 3, idx);

            prevOuter = outer;
            prevInner = inner;
        }
    }
}



