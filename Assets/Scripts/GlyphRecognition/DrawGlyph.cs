using System.Collections.Generic;
using UnityEngine;

public class DrawGlyph : MonoBehaviour
{
    public Camera cam;
    public float maxPointDist = 0.1f;
    public bool debugMode = false;

    private List<Vector2> glyphPoints = new List<Vector2>();
    private List<GlyphSO> glyphs = new List<GlyphSO>();
    public LineRenderer lr;

    public DollarRecognizer dollarRecognizer;

    public static DrawGlyph Instance;

    void Awake()
    {
        Instance = this;
        dollarRecognizer = new DollarRecognizer();

        GlyphSO[] loadedGlyphs = Resources.LoadAll<GlyphSO>("Glyphs/");
        Debug.Log("Loaded " + loadedGlyphs.Length + " glyphs");
        
        foreach(GlyphSO glyph in loadedGlyphs)
        {
            dollarRecognizer.SavePattern(glyph.name, glyph.points);
        }
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            if (glyphPoints.Count < 1)
            {
                glyphPoints.Add((Vector2)Input.mousePosition);
            }
            else
            {
                if (Vector2.Distance(glyphPoints[glyphPoints.Count - 1], Input.mousePosition) > maxPointDist)
                {
                    glyphPoints.Add((Vector2)Input.mousePosition);
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            Debug.Log(dollarRecognizer.Recognize(glyphPoints).ToString());

            glyphPoints.Clear();
        }

        RenderLine(glyphPoints);
    }

    public void RenderLine(List<Vector2> glyphPoints)
    {
        lr.positionCount = glyphPoints.Count;
        for (int n = 0; n < glyphPoints.Count; n++)
        {
            Vector3 worldP = cam.ScreenToWorldPoint(glyphPoints[n]);
            worldP.z = 0;
            lr.SetPosition(n, worldP);
        }
    }
}
