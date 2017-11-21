using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface RTRenderer
{
    Color GetColor(RTRay ray, int depth);
}
