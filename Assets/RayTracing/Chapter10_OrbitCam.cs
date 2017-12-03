using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chapter10_OrbitCam : ChapterBaseAntialiasing
{
    private HitableList scene = null;

    protected override void Awake()
    {
		base.Awake();

		isScreenSize = true;
		numSamples = 800;
        ppmTexture.sRGB = true;

        scene = new HitableList();

        scene.list.Add(
            new RTSphere().
                Set(new Vector3(0, 0, 0), 0.5f).
                SetMaterial<RTSphere>(
                    new Lambertian().
                        SetAlbedo<Lambertian>(new Color(0.8f, 0.3f, 0.3f))
                )
        );
        scene.list.Add(
            new RTSphere().
                Set(new Vector3(0, -100.5f, 0), 100).
                SetMaterial<RTSphere>(
                    new Lambertian().
                        SetAlbedo<Lambertian>(new Color(0.8f, 0.8f, 0.0f))
                )
        );

        scene.list.Add(
            new RTSphere().
                Set(new Vector3(1, 0, 0), 0.5f).
                SetMaterial<RTSphere>(
                    new Metal().
                        SetAlbedo<Metal>(new Color(0.8f, 0.6f, 0.2f)).
						SetFuzz(0.25f)
                )
        );

        scene.list.Add(
            new RTSphere().
                Set(new Vector3(-1, 0, 0), 0.5f).
                SetMaterial<RTSphere>(
//                    new Metal().
//                        SetAlbedo<Metal>(new Color(0.8f, 0.8f, 0.8f)).
//                        SetFuzz(0.5f)
						new Dielectric().SetRefractiveIndex(1.5f)
                )
        );
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
		float x = Mathf.Cos(degree * Mathf.Deg2Rad) * radius;
		float z = Mathf.Sin(degree * Mathf.Deg2Rad) * radius;
		degree += degreeInc;

		return new RTCameraC(new Vector3(x, height, z), new Vector3(0, 0, 0), Vector3.up, 70, (float)canvasWidth / canvasHeight);
	}

	private float degree = 0;

	private float radius = 2;

	private float height = 0.75f;

	private float degreeInc = 5;

	public override void RenderingComplete ()
	{
		base.RenderingComplete ();
		System.IO.File.WriteAllBytes("/Users/jimCheng/Desktop/untitled folder 2/a" + (degree-degreeInc).ToString("00000") + ".jpg", ppmTexture.Texture.EncodeToJPG(100));

		if(degree < 360)
		{
			StartRendering();
		}
	}
}
