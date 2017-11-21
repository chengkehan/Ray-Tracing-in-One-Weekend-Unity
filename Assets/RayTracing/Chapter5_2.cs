using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chapter5_2 : ChapterBase
{
    private RTSphere sphere = null;

    protected override void Awake()
    {
        sphere = new RTSphere();
        sphere.Set(new Vector3(0, 0, -1), 0.5f);
    }

    protected override Color GetColor(RTRay ray, int depth)
    {
        HitRecord hit;
        if(sphere.Hit(ray, 0, 1000, out hit))
        {
            Vector3 n = hit.n;
            return new Color((n.x + 1) * 0.5f, (n.y + 1) * 0.5f, (n.z + 1) * 0.5f);
        }

        return GetBackgroundColor(ray);
    }
}
