using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class RTCanvas
{
	public static Color GetEnvironmentColor(RTRay ray)
	{
		Vector3 unit_dir = ray.direction.normalized;
		float blend = (unit_dir.y + 1) * 0.5f;
		return Color.white * (1 - blend) + new Color(0.5f, 0.7f, 1) * blend;
	}
}

public class RTMath
{
    private static System.Random rnd = new System.Random();

    // [0-1)
    public static float Rnd01()
    {
        float value;
        lock(rnd)
        {
            value = rnd.Next(0, 10000) / 10000.0f;
        }
        return value;
    }

    public static Vector3 RndInUnitSphere()
    {
        Vector3 p;
        do
        {
            p = 2.0f * new Vector3(Rnd01(), Rnd01(), Rnd01()) - new Vector3(1, 1, 1);
        }
        while (Vector3.Dot(p, p) >= 1.0f);
        return p;
    }

    public static Vector3 Reflect(Vector3 v, Vector3 n)
    {
        return v - 2 * Vector3.Dot(v, n) * n;
    }

    public static bool Refract(Vector3 v, Vector3 n, float ni_over_nt, out Vector3 refracted)
    {
        // n * sin(theta) = n' * sin(theta')
        // n^2 * sin^2(theta) = n'^2 * sin^2(theta')
        // cos^2(theta') = 1 - sin^2(theta') = 1 - sin^2(theta) * (n^2 / n'^2)
        // cos^2(theta') = 1 - sin^2(theta') = 1 - (n / n')^2 * sin^2(theta)
        // cos^2(theta') = 1 - sin^2(theta') = 1 - (n / n')^2 * (1 - cos^2(theta))
        // discriminant = cos^2(theta')

        Vector3 uv = v.normalized;
        float dt = Vector3.Dot(uv, n);
        /*float dtt = Vector3.Dot(v, n);*/
        float discriminant = 1.0f - ni_over_nt * ni_over_nt * (1 - dt * dt);
        if(discriminant > 0)
        {
            refracted = ni_over_nt * (uv - n * dt/*dtt*/) - n * Mathf.Sqrt(discriminant);
            return true;
        }
        else
        {
            refracted = Vector3.zero;
            return false;
        }
    }

    public static float schlick(float cosine, float refractiveIndex)
    {
        float r0 = (1 - refractiveIndex) / (1 + refractiveIndex);
        r0 = r0 * r0;
        return r0 + (1 - r0) * Mathf.Pow(1 - cosine, 5);
    }
}

public struct RTRay
{
    public Vector3 origin;

    public Vector3 direction;

    public RTRay Set(Vector3 origin, Vector3 direction)
    {
        this.origin = origin;
        this.direction = direction;
        return this;
    }

    public Vector3 PointAt(float distance)
    {
        return origin + direction * distance;
    }
}

public abstract class RTMaterial
{
    public Color albedo;
    public T SetAlbedo<T>(Color albedo) where T : RTMaterial
    {
        this.albedo = albedo;
        return (T)this;
    }

    public abstract bool Scatter(RTRay ray, HitRecord hit, out Color attenuation, out RTRay scattered);
}

public class Lambertian : RTMaterial
{
    public override bool Scatter(RTRay ray, HitRecord hit, out Color attenuation, out RTRay scattered)
    {
        Vector3 target = hit.p + hit.n + RTMath.RndInUnitSphere();
        scattered = new RTRay().Set(hit.p, target - hit.p);
        attenuation = albedo;
        return true;
    }
}

public class Metal : RTMaterial
{
    public float fuzz = 0.0f;
    public Metal SetFuzz(float fuzz)
    {
        this.fuzz = fuzz;
        return this;
    }

    public override bool Scatter(RTRay ray, HitRecord hit, out Color attenuation, out RTRay scattered)
    {
        Vector3 reflected = RTMath.Reflect(ray.direction.normalized, hit.n);
        scattered = new RTRay().Set(hit.p, reflected + fuzz * RTMath.RndInUnitSphere());
        attenuation = albedo;
        return Vector3.Dot(scattered.direction, hit.n) > 0;
    }
}

public class Dielectric : RTMaterial
{
    public float refractiveIndex = 1.0f;
    public Dielectric SetRefractiveIndex(float refractiveIndex)
    {
        this.refractiveIndex = refractiveIndex;
        return this;
    }

    public override bool Scatter(RTRay ray, HitRecord hit, out Color attenuation, out RTRay scattered)
    {
        attenuation = new Color(1,1,1);

        Vector3 outwardN;
        float ni_over_nt;
        Vector3 refracted;
        float reflect_prob;
        float cosine;

        if(Vector3.Dot(ray.direction, hit.n) > 0)
        {
            outwardN = -hit.n;
            ni_over_nt = refractiveIndex;
            cosine = refractiveIndex * Vector3.Dot(ray.direction, hit.n) / ray.direction.magnitude;
        }
        else
        {
            outwardN = hit.n;
            ni_over_nt = 1.0f / refractiveIndex;
            cosine = -Vector3.Dot(ray.direction, hit.n) / ray.direction.magnitude;
        }

        if(RTMath.Refract(ray.direction, outwardN, ni_over_nt, out refracted))
        {
            reflect_prob = RTMath.schlick(cosine, refractiveIndex);
        }
        else
        {
            reflect_prob = 1.0f;
        }

        if(RTMath.Rnd01() < reflect_prob)
        {
            scattered = new RTRay().Set(hit.p, RTMath.Reflect(ray.direction, hit.n));
        }
        else
        {
            scattered = new RTRay().Set(hit.p, refracted);
        }

        return true;
    }
}

public struct HitRecord
{
    public float t;

    public Vector3 p;

    public Vector3 n;

    public RTMaterial material;
}

public abstract class Hitable
{
    public RTMaterial material;
    public T SetMaterial<T>(RTMaterial material) where T:Hitable
    {
        this.material = material;
        return (T)this;
    }

    public abstract bool Hit(RTRay ray, float t_min, float t_max, out HitRecord rec);
}

public class RTSphere : Hitable
{
    public Vector3 center;

    public float radius;

    public RTSphere Set(Vector3 center, float radius)
    {
        this.center = center;
        this.radius = radius;
        return this;
    }

    public override bool Hit(RTRay ray, float t_min, float t_max, out HitRecord rec)
    {
        rec = new HitRecord();

        Vector3 oc = ray.origin - center;
        float a = Vector3.Dot(ray.direction, ray.direction);
        float b = 2.0f * Vector3.Dot(oc, ray.direction);
        float c = Vector3.Dot(oc, oc) - radius * radius;
        float discriminant = b * b - 4 * a * c;
        if (discriminant < 0)
        {
            return false;
        }
        else
        {
            float t = (-b - Mathf.Sqrt(discriminant)) / (2 * a);
            if (t < t_max && t > t_min)
            {
                rec.t = t;
                rec.p = ray.PointAt(t);
                rec.n = (rec.p - center) / radius;
                rec.material = material;
                return true;
            }
            t = (-b + Mathf.Sqrt(discriminant)) / (2 * a);
            if (t < t_max && t > t_min)
            {
                rec.t = t;
                rec.p = ray.PointAt(t);
                rec.n = (rec.p - center) / radius;
                rec.material = material;
                return true;
            }

            return false;
        }
    }
}

public class HitableList : Hitable
{
    public List<Hitable> list = new List<Hitable>();

    public override bool Hit(RTRay ray, float t_min, float t_max, out HitRecord rec)
    {
        rec = new HitRecord();

        HitRecord hit;
        bool hitAnything = false;
        float t_closest = t_max;
        int numItems = list == null ? 0 : list.Count;
        for(int i = 0; i < numItems; ++i)
        {
            Hitable hitable = list[i];
            if(hitable != null && hitable.Hit(ray, t_min, t_closest, out hit))
            {
                hitAnything = true;
                t_closest = hit.t;
                rec = hit;
            }
        }
        return hitAnything;
    }
}

public class RTCamera
{
    private Vector3 origin = Vector3.zero;
    private Vector3 leftBottomCorner = new Vector3(-2, -1, -1);
    private Vector3 horizontal = new Vector3(4, 0, 0);
    private Vector3 vertical = new Vector3(0, 2, 0);

    public RTRay GetRay(float u, float v)
    {
        RTRay ray = new RTRay();
        ray.Set(origin, leftBottomCorner + horizontal * u + vertical * v);
        return ray;
    }
}
