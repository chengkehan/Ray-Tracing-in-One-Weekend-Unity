using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chapter6 : ChapterBaseAntialiasing
{
    private HitableList scene = null;

    protected override void Awake()
    {
        scene = new HitableList();

        scene.list.Add(new RTSphere().Set(new Vector3(0, 0, -1), 0.5f));
        scene.list.Add(new RTSphere().Set(new Vector3(0, -100.5f, -1), 100));
    }

    public override Color GetColor(RTRay ray, int depth)
    {
        HitRecord hit;
        if(scene.Hit(ray, 0, 1000, out hit))
        {
            Vector3 n = hit.n;
            return new Color((n.x + 1) * 0.5f, (n.y + 1) * 0.5f, (n.z + 1) * 0.5f);
        }

        return GetEnvironmentColor(ray);
    }
}
