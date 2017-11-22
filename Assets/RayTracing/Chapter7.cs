using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chapter7 : ChapterBaseAntialiasing
{
    private HitableList scene = null;

    protected override void Awake()
    {
		base.Awake();

        scene = new HitableList();

        scene.list.Add(new RTSphere().Set(new Vector3(0, 0, -1), 0.5f));
        scene.list.Add(new RTSphere().Set(new Vector3(0, -100.5f, -1), 100));
    }

    public override Color GetColor(RTRay ray, int depth)
    {
        HitRecord hit;
		if(scene.Hit(ray, 0.001f, 1000, out hit))
        {
            Vector3 target = hit.p + hit.n + RTMath.RndInUnitSphere();
            float absorptivity = 0.5f;
            return absorptivity * GetColor(new RTRay().Set(hit.p, target - hit.p), depth + 1);
        }

		return RTCanvas.GetEnvironmentColor(ray);
    }
}
