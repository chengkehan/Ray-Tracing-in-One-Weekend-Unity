using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chapter5 : ChapterBase
{
    protected override Color GetColor(RTRay ray, int depth)
    {
        Vector3 sphereCenter = new Vector3(0, 0, -1);
        float sphereRadius = 0.5f;
        float hit = HitSphere(sphereCenter, sphereRadius, ray);
        if(hit >= 0)
        {
            Vector3 n = (ray.PointAt(hit) - sphereCenter).normalized;
            return new Color((n.x + 1) * 0.5f, (n.y + 1) * 0.5f, (n.z + 1) * 0.5f);
        }

        return GetBackgroundColor(ray);
    }

    private float HitSphere(Vector3 center, float radius, RTRay ray)
    {
        Vector3 oc = ray.origin - center;
        float a = Vector3.Dot(ray.direction, ray.direction);
        float b = 2.0f * Vector3.Dot(oc, ray.direction);
        float c = Vector3.Dot(oc, oc) - radius * radius;
        float discriminant = b * b - 4 * a * c;
        if(discriminant < 0)
        {
            return -1.0f;
        }
        else
        {
            return (-b - Mathf.Sqrt(discriminant)) / (2 * a);
        }
    }
}
