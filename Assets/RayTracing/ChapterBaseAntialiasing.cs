using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChapterBaseAntialiasing : MonoBehaviour, RTRenderer
{
	protected PPMTexture ppmTexture = null;

	protected IRTCamera cam = null;

	private RenderingTasksManager renderingTasksManager = null;

	protected bool multiThreadRendering = true;

	protected bool isScreenSize = false;

	protected int numSamples = 100;

    protected virtual void Awake()
    {
		RTMath.ThreadInitRnd();

		ppmTexture = new PPMTexture();
    }

    protected virtual void Start()
    {
		int canvasWidth = isScreenSize ? Screen.width : 200;
		int canvasHeight = isScreenSize ? Screen.height : 100;

		cam = CreateCamera(canvasWidth, canvasHeight);
		ppmTexture.Init(canvasWidth, canvasHeight);

		renderingTasksManager = new RenderingTasksManager();

        int pixelIndex = 0;
        RenderingTask task = null;
        int itemsCount = 0;

        for (int j = 0; j < ppmTexture.Height; ++j)
        {
            for (int i = 0; i < ppmTexture.Width; ++i)
            {
                if (multiThreadRendering)
                {
                    if (itemsCount == 0)
                    {
                        task = new RenderingTask();
                        task.Init(this, cam, ppmTexture.Width, ppmTexture.Height, numSamples);
                        renderingTasksManager.AddTask(task, RenderingTaskCompleteCB);
                    }
                    task.AddItem(i, j, pixelIndex);
                    ++itemsCount;
                    if (itemsCount == RenderingTask.SIZE)
                    {
                        itemsCount = 0;
                    }
                    ++pixelIndex;
                }
                else
                {
                    Color color = Color.black;
                    for (int s = 0; s < numSamples; ++s)
                    {
                        float u = (i + RTMath.Rnd01()) / ppmTexture.Width;
                        float v = (j + RTMath.Rnd01()) / ppmTexture.Height;

                        RTRay ray = cam.GetRay(u, v);
                        color += GetColor(ray, 0);
                    }
                    color /= numSamples;
                    ppmTexture.WriteAPixel(color);
                }
            }
        }

        if (!multiThreadRendering)
        {
            ppmTexture.Complete();
			RenderingComplete();
        }
		else
		{
			renderingTasksManager.Start();
		}
    }

	public void StartRendering()
	{
		Awake();
		Start();
	}

	public virtual void RenderingComplete()
	{
		// Do nothing
	}

    private void Update()
    {
        if(renderingTasksManager != null && multiThreadRendering)
        {
            renderingTasksManager.Update();

			if(renderingTasksManager.IsAllTasksComplete())
			{
				renderingTasksManager.Destroy();
				renderingTasksManager = null;
				System.GC.Collect();

				RenderingComplete();
			}
        }
    }

    private void OnDestroy()
    {
        if(renderingTasksManager != null)
        {
            renderingTasksManager.Destroy();
            renderingTasksManager = null;
        }
    }

	public virtual IRTCamera CreateCamera(int canvasWidth, int canvasHeight)
	{
		return new RTCameraA();
	}

    public virtual Color GetColor(RTRay ray, int depth)
    {
        return Color.white;
    }

    private void RenderingTaskCompleteCB(RenderingTask task)
    {
        for (int j = 0; j < task.NumItems(); ++j)
        {
            int pixelIndex;
            Color finalColor;
            task.GetFinalData(j, out pixelIndex, out finalColor);
            ppmTexture.WriteAPixel(finalColor, pixelIndex);
        }

        ppmTexture.Complete();
    }

    private void OnGUI()
    {
        if(ppmTexture != null)
        {
            GUI.DrawTexture(new Rect(0, 0, isScreenSize ? Screen.width : 400, isScreenSize ? Screen.height : 400), ppmTexture.Texture, ScaleMode.ScaleToFit, false);
        }
    }
}
