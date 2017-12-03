using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chapter12 : ChapterBaseAntialiasing
{
    private HitableList scene = null;

    protected override void Awake()
    {
		base.Awake();

		numSamples = 800;
		isScreenSize = true;
        ppmTexture.sRGB = true;

        scene = new HitableList();

		scene.list.Add(new RTSphere().Set(new Vector3(0, -1000, 0), 1000).SetMaterial<RTSphere>(new Lambertian().SetAlbedo<Lambertian>(new Color(0.5f, 0.5f, 0.5f))));

		for(int a = -11; a < 11; ++a)
		{
			for(int b = -11; b < 11; ++b)
			{
				float rnd = RTMath.Rnd01();
				Vector3 center = new Vector3(a + 0.9f * rnd, 0.2f, b + 0.9f * rnd);
				if((center - new Vector3(4, 0.2f, 0)).magnitude > 0.9f)
				{
					if(rnd < 0.6f)
					{
						scene.list.Add(new RTSphere().Set(center, 0.2f).SetMaterial<RTSphere>(new Lambertian().SetAlbedo<Lambertian>(new Color(RTMath.Rnd01(), RTMath.Rnd01(), RTMath.Rnd01()))));
					}
					else if(rnd < 0.9f)
					{
						scene.list.Add(new RTSphere().Set(center, 0.2f).SetMaterial<RTSphere>(new Metal().SetAlbedo<Metal>(new Color(0.5f * (1 + RTMath.Rnd01()), 0.5f * (1 + RTMath.Rnd01()), 0.5f * (1 + RTMath.Rnd01()))).SetFuzz(0.5f * RTMath.Rnd01())));
					}
					else
					{
						scene.list.Add(new RTSphere().Set(center, 0.2f).SetMaterial<RTSphere>(new Dielectric().SetRefractiveIndex(1.5f)));
					}
				}
			}
		}

		scene.list.Add(new RTSphere().Set(new Vector3(0, 1, 0), 1).SetMaterial<RTSphere>(new Dielectric().SetRefractiveIndex(1.5f)));
		scene.list.Add(new RTSphere().Set(new Vector3(-4, 1, 0), 1).SetMaterial<RTSphere>(new Lambertian().SetAlbedo<Lambertian>(new Color(0.4f, 0.2f, 0.1f))));
		scene.list.Add(new RTSphere().Set(new Vector3(4, 1, 0), 1).SetMaterial<RTSphere>(new Metal().SetAlbedo<Metal>(new Color(0.7f, 0.6f, 0.5f)).SetFuzz(0.0f)));
    }

    public override Color GetColor(RTRay ray, int depth)
    {
        HitRecord hit;
		if(scene.Hit(ray, 0.001f, 1000, out hit))
        {
            RTRay scattered;
            Color attenuation;
            if(depth < 50 && hit.material.Scatter(ray, hit, out attenuation, out scattered))
            {
                return attenuation * GetColor(scattered, depth + 1);
            }
            else
            {
                return Color.black;
            }
        }

		return RTCanvas.GetEnvironmentColor(ray);
    }

	public override IRTCamera CreateCamera (int canvasWidth, int canvasHeight)
	{
		Vector3 lookFrom = new Vector3(13, 2, 3);
		Vector3 lookAt = new Vector3(0, 0, 0);
		float focus_dist = (lookAt - lookFrom).magnitude;

		return new RTCameraD(lookFrom, lookAt, Vector3.up, 20, (float)canvasWidth / canvasHeight, 0.1f, focus_dist);
	}
}
