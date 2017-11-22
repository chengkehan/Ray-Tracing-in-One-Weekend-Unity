using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChapterBaseAntialiasing : MonoBehaviour, RTRenderer
{
    protected PPMTexture ppmTexture = new PPMTexture();

    protected RTCamera cam = null;

    private RenderingTasksManager renderingTasksManager = new RenderingTasksManager();

	protected bool multiThreadRendering = true;

	protected bool isScreenSize = false;

	protected int numSamples = 100;

    protected virtual void Awake()
    {
		RTMath.ThreadInitRnd();
    }

    protected virtual void Start()
    {
        cam = new RTCamera();

        ppmTexture.Init(isScreenSize ? Screen.width : 200, isScreenSize ? Screen.height : 100);

        Vector3 origin = Vector3.zero;
        Vector3 leftBottomCorner = new Vector3(-2, -1, -1);
        Vector3 horizontal = new Vector3(4, 0, 0);
        Vector3 vertical = new Vector3(0, 2, 0);

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
        }
    }

    private void Update()
    {
        if(renderingTasksManager != null && multiThreadRendering)
        {
            renderingTasksManager.Update();
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
