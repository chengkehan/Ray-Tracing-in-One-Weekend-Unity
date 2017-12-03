using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chapter11 : ChapterBaseAntialiasing
{
    private HitableList scene = null;

    protected override void Awake()
    {
		base.Awake();

        ppmTexture.sRGB = true;

        scene = new HitableList();

        scene.list.Add(
            new RTSphere().
                Set(new Vector3(0, 0, -1), 0.5f).
                SetMaterial<RTSphere>(
                    new Lambertian().
                        SetAlbedo<Lambertian>(new Color(0.8f, 0.3f, 0.3f))
                )
        );
        scene.list.Add(
            new RTSphere().
                Set(new Vector3(0, -100.5f, -1), 100).
                SetMaterial<RTSphere>(
                    new Lambertian().
                        SetAlbedo<Lambertian>(new Color(0.8f, 0.8f, 0.0f))
                )
        );

        scene.list.Add(
            new RTSphere().
                Set(new Vector3(1, 0, -1), 0.5f).
                SetMaterial<RTSphere>(
                    new Metal().
                        SetAlbedo<Metal>(new Color(0.8f, 0.6f, 0.2f)).
						SetFuzz(0.5f)
                )
        );

        scene.list.Add(
            new RTSphere().
                Set(new Vector3(-1, 0, -1), 0.5f).
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
		Vector3 lookFrom = new Vector3(3, 3, 2);
		Vector3 lookAt = new Vector3(0, 0, -1);
		float focus_dist = (lookAt - lookFrom).magnitude;

		return new RTCameraD(lookFrom, lookAt, Vector3.up, 20, (float)canvasWidth / canvasHeight, 2.0f, focus_dist);
	}
}
