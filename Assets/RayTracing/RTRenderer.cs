using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface RTRenderer
{
	void StartRendering();

    Color GetColor(RTRay ray, int depth);

	IRTCamera CreateCamera(int canvasWidth, int canvasHeight);

	void RenderingComplete();
}
