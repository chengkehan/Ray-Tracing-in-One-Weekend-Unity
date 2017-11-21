using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class RenderingTasksManager
{
    public delegate void TaskCompleteCB(RenderingTask task);

    private const int CONCURRENCY = 4;

    private List<Item> items = new List<Item>();

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
            if (!item.task.IsWorking && !item.task.IsComplete && !item.task.IsDestroied && numWorkingItems + 1 <= CONCURRENCY)
            {
                item.task.Start();
                ++numWorkingItems;
            }
        }

        for(int i = 0; i < numItems; ++i)
        {
            Item item = items[i];
            if(item.task.IsComplete && !item.task.IsDestroied)
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
    }

    private class Item
    {
        public RenderingTask task = null;

        public TaskCompleteCB taskCompeteCB = null;
    }
}

public class RenderingTask
{
    private volatile bool _isWorking = false;
    public bool IsWorking
    {
        get
        {
            return _isWorking;
        }
        private set
        {
            _isWorking = value;
        }
    }

    private volatile bool _isComplete = false;
    public bool IsComplete
    {
        get
        {
            return _isComplete;
        }
        private set
        {
            _isComplete = value;
        }
    }

    private volatile bool _isDestroied = false;
    public bool IsDestroied
    {
        get
        {
            return _isDestroied;
        }
        private set
        {
            _isDestroied = value;
        }
    }

    public const int SIZE = 200;

    private Thread thread = null;

    private int pIndex = 0;

    private RTRenderer renderer = null;

    private RTCamera cam = null;

    private int canvasWidth = 0;

    private int canvasHeight = 0;

    private int numSamples = 0;

    private Item[] items = new Item[SIZE];

    public void Destroy()
    {
        IsDestroied = true;
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

    public void Init(RTRenderer renderer, RTCamera cam, int canvasWidth, int canvasHeight, int numSamples)
    {
        if(IsInitialized())
        {
            return;
        }

        this.renderer = renderer;
        this.cam = cam;
        this.canvasWidth = canvasWidth;
        this.canvasHeight = canvasHeight;
        this.numSamples = numSamples;
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
        if(IsWorking || IsComplete || !IsInitialized() || IsDestroied)
        {
            return;
        }

        IsWorking = true;
        thread = new Thread(Working);
        thread.Start();
    }

    private void Working()
    {
        for (int i = 0; i < pIndex; ++i)
        {
            if(IsDestroied)
            {
                IsWorking = false;
                IsComplete = false;
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

        IsWorking = false;
        IsComplete = true;
    }

    private bool IsInitialized()
    {
        return renderer != null && cam != null;
    }

    private struct Item
    {
        public int i;
        public int j;
        public int pixelIndex;
        public Color finalColor;
    }
}
