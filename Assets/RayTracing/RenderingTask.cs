using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class RenderingTasksManager
{
    public delegate void TaskCompleteCB(RenderingTask task);

    private const int CONCURRENCY = 4;

    private List<Item> items = new List<Item>();

	private Thread daemon = null;
	private volatile bool daemonWorking = false;

	public void Start()
	{
		if(daemon == null)
		{
			daemon = new Thread(DaemonWorking);
			daemonWorking = true;
			daemon.Start();
		}
	}

	public bool IsAllTasksComplete()
	{
		int numItems = items.Count;
		for(int i = 0; i < numItems; ++i)
		{
			Item item = items[i];
			if(!item.task.IsDestroied)
			{
				return false;
			}
		}
		return true;
	}

    public void AddTask(RenderingTask task, TaskCompleteCB taskCompleteCB)
    {
        if(task == null || taskCompleteCB == null)
        {
            return;
        }

        items.Add(new Item() { task=task, taskCompeteCB = taskCompleteCB });
    }

    public void Update()
    {
        int numItems = items.Count;

        for(int i = 0; i < numItems; ++i)
        {
            Item item = items[i];
            if(item.task.IsComplete)
            {
                item.taskCompeteCB(item.task);
                item.task.Destroy();
            }
        }
    }

    public void Destroy()
    {
        if (items != null)
        {
            int numItems = items.Count;
            for (int i = 0; i < numItems; ++i)
            {
                items[i].task.Destroy();
            }
            items.Clear();
        }

		daemonWorking = false;
    }

	private void DaemonWorking()
	{
		while(daemonWorking)
		{
			int numItems = items.Count;

			int numWorkingItems = 0;
			for(int i = 0; i < numItems; ++i)
			{
				Item item = items[i];
				if(item.task.IsWorking)
				{
					++numWorkingItems;
				}
			}

			for(int i = 0; i < numItems; ++i)
			{
				Item item = items[i];
				if (item.task.IsReady && numWorkingItems + 1 <= CONCURRENCY)
				{
					item.task.Start();
					++numWorkingItems;
				}
			}
		}
	}

    private class Item
    {
        public RenderingTask task = null;

        public TaskCompleteCB taskCompeteCB = null;
    }
}

public class RenderingTask
{
	public bool IsReady
	{
		get
		{
			return status == STATUS_READY;
		}
	}

    public bool IsWorking
    {
        get
        {
			return status == STATUS_WORKING;
        }
    }

    public bool IsComplete
    {
        get
        {
			return status == STATUS_COMPLETE;
        }
    }

    public bool IsDestroied
    {
        get
        {
			return status == STATUS_DESTROIED;
        }
    }

    public const int SIZE = 200;

    private Thread thread = null;

    private int pIndex = 0;

    private RTRenderer renderer = null;

	private IRTCamera cam = null;

    private int canvasWidth = 0;

    private int canvasHeight = 0;

    private int numSamples = 0;

    private Item[] items = new Item[SIZE];

	private const int STATUS_UNDEFINED = 0;
	private const int STATUS_READY = 1;
	private const int STATUS_WORKING = 2;
	private const int STATUS_COMPLETE = 3;
	private const int STATUS_DESTROIED = 4;
	private volatile int status = STATUS_UNDEFINED;

    public void Destroy()
    {
		status = STATUS_DESTROIED;
    }

    public int NumItems()
    {
        return pIndex;
    }

    public bool GetFinalData(int index, out int pixelIndex, out Color color)
    {
        if (!IsComplete || index >= pIndex)
        {
            pixelIndex = 0;
            color = Color.black;
            return false;
        }
        else
        {
            pixelIndex = items[index].pixelIndex;
            color = items[index].finalColor;
            return true;
        }
    }

	public void Init(RTRenderer renderer, IRTCamera cam, int canvasWidth, int canvasHeight, int numSamples)
    {
		if(status != STATUS_UNDEFINED)
        {
            return;
        }

        this.renderer = renderer;
        this.cam = cam;
        this.canvasWidth = canvasWidth;
        this.canvasHeight = canvasHeight;
        this.numSamples = numSamples;
		status = STATUS_READY;
    }

    public bool IsFull()
    {
        return pIndex >= SIZE;
    }

    public void AddItem(int i, int j, int pixelIndex)
    {
        if(IsFull() || IsWorking)
        {
            return;
        }

        items[pIndex] = new Item() { i=i, j=j, pixelIndex=pixelIndex };
        ++pIndex;
    }

    public void Start()
    {
		if(status != STATUS_READY)
		{
			return;
		}

		status = STATUS_WORKING;
        thread = new Thread(Working);
        thread.Start();
    }

    private void Working()
    {
		RTMath.ThreadInitRnd();

        for (int i = 0; i < pIndex; ++i)
        {
            if(IsDestroied)
            {
                return;
            }

            Item item = items[i];
            Color color = Color.black;
            for (int s = 0; s < numSamples; ++s)
            {
                float u = (item.i + RTMath.Rnd01()) / canvasWidth;
                float v = (item.j + RTMath.Rnd01()) / canvasHeight;
                RTRay ray = cam.GetRay(u, v);
                color += renderer.GetColor(ray, 0);

            }
            color /= numSamples;
            item.finalColor = color;
            items[i] = item;
        }

		status = STATUS_COMPLETE;
    }

    private struct Item
    {
        public int i;
        public int j;
        public int pixelIndex;
        public Color finalColor;
    }
}
