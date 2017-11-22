using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChapterBase : MonoBehaviour, RTRenderer
{
    protected PPMTexture ppmTexture = new PPMTexture();

    protected virtual void Awake()
    {
		RTMath.ThreadInitRnd();
    }

    protected virtual void Start()
    {
        ppmTexture.Init(200, 100);

        RTRay ray = new RTRay();
        Vector3 origin = Vector3.zero;
        Vector3 leftBottomCorner = new Vector3(-2, -1, -1);
        Vector3 horizontal = new Vector3(4, 0, 0);
        Vector3 vertical = new Vector3(0, 2, 0);
        for (int j = 0; j < ppmTexture.Height; ++j)
        {
            for (int i = 0; i < ppmTexture.Width; ++i)
            {
                float u = (float)i / ppmTexture.Width;
                float v = (float)j / ppmTexture.Height;
                ray.Set(origin, leftBottomCorner + horizontal * u + vertical * v);
                Color color = GetColor(ray, 0);
                ppmTexture.WriteAPixel(color);
            }
        }

        ppmTexture.Complete();
    }

	public virtual Color GetColor(RTRay ray, int depth)
    {
        return Color.white;
    }

    protected Color GetBackgroundColor(RTRay ray)
    {
        Vector3 unit_dir = ray.direction.normalized;
        float blend = (unit_dir.y + 1) * 0.5f;
        return Color.white * (1 - blend) + new Color(0.5f, 0.7f, 1) * blend;
    }

    private void OnGUI()
    {
        if(ppmTexture != null)
        {
            GUI.DrawTexture(new Rect(0, 0, 400, 400), ppmTexture.Texture, ScaleMode.ScaleToFit, false);
        }
    }
}
