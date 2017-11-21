using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chapter3 : ChapterBase
{
    public override Color GetColor(RTRay ray, int depth)
    {
        return GetBackgroundColor(ray);
    }
}
