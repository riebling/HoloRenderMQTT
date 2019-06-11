using UnityEngine;

public struct LineDrawer
{
    private LineRenderer lineRenderer;
    private float lineSize;

    public LineDrawer(float lineSize = 0.2f)
    {
        GameObject lineObj = new GameObject("LineObj");
        lineRenderer = lineObj.AddComponent<LineRenderer>();
        //Particles/Additive
        lineRenderer.material = new Material(Shader.Find("Hidden/Internal-Colored"));
        lineRenderer.startWidth = lineSize;
        lineRenderer.endWidth = lineSize;
        lineRenderer.startColor = Color.black;

        this.lineSize = lineSize;
    }

    private void init(float lineSize = 0.2f)
    {
        if (lineRenderer == null)
        {
            GameObject lineObj = new GameObject("LineObj");
            lineRenderer = lineObj.AddComponent<LineRenderer>();
            lineRenderer.material = new Material(Shader.Find("Hidden/Internal-Colored"));
            this.lineSize = lineSize;
        }
    }

    public void AddLine(Vector3 start, Vector3 end, Color color)
    {
        if (lineRenderer == null)
        {
            init(0.2f);
        }

        //Set color
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;

        //Set width
        lineRenderer.startWidth = lineSize;
        lineRenderer.endWidth = lineSize;

        //Increment vertex count
        int count = lineRenderer.positionCount; // initially 2
        lineRenderer.positionCount = count + 1; // increment

        // There's already a default (black) line from vertex 0 to 1
        // so our first line will be from vertex 1 to 2

        //Set the postion of both two lines
        lineRenderer.SetPosition(count-1, start);
        lineRenderer.SetPosition(count, end);
    }

    public void Destroy()
    {
        if (lineRenderer != null)
        {
            UnityEngine.Object.Destroy(lineRenderer.gameObject);
        }
    }
}
