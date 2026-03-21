using System.Collections.Generic;
using NUnit.Framework.Interfaces;
using UnityEngine;
using UnityEngine.UI.Extensions;

public class DrawGlyph : MonoBehaviour
{
    public Camera cam;
    public float maxPointDist = 0.1f;
    public bool debugMode = false;
    public bool casting = false;

    private List<Vector2> glyphPoints = new List<Vector2>();
    private List<GlyphSO> glyphs = new List<GlyphSO>();
    public UILineRenderer lr;

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
            dollarRecognizer.SavePattern(glyph.name, glyph.points, glyph.spell);
        }
    }

    void Update()
    {
        if (Input.GetMouseButton(0) && casting)
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

        if (Input.GetMouseButtonUp(0) && casting)
        {
            DollarRecognizer.Result result = dollarRecognizer.Recognize(glyphPoints);
            glyphPoints.Clear();

            if (result.Match == null)
                return;

            Debug.Log(result.ToString());
            result.Match.Spell.Cast();
            PlayerAnimation.Instance.Spell();
        }

        if (!casting)
            glyphPoints.Clear();

        RenderLine(glyphPoints);
    }

    public void RenderLine(List<Vector2> glyphPoints)
    {
        Vector2[] points = new Vector2[glyphPoints.Count];
        for (int n = 0; n < glyphPoints.Count; n++)
        {
            Vector2 screenPoint = glyphPoints[n]; 

            points[n] = screenPoint;
        }

        lr.Points = points;
    }
}
