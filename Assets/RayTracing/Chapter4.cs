using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chapter4 : ChapterBase
{
    protected override Color GetColor(RTRay ray, int depth)
    {
        Vector3 sphereCenter = new Vector3(0, 0, -1);
        float sphereRadius = 0.5f;
        if(HitSphere(sphereCenter, sphereRadius, ray))
        {
            return Color.red;
        }

        return GetBackgroundColor(ray);
    }

    private bool HitSphere(Vector3 center, float radius, RTRay ray)
    {
        Vector3 oc = ray.origin - center;
        float a = Vector3.Dot(ray.direction, ray.direction);
        float b = 2.0f * Vector3.Dot(oc, ray.direction);
        float c = Vector3.Dot(oc, oc) - radius * radius;
        float discriminant = b * b - 4 * a * c;
        return discriminant >= 0;
    }
}
