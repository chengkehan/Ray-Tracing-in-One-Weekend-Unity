using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chapter1 : ChapterBase
{
    protected override void Start()
    {
        ppmTexture = new PPMTexture();
        ppmTexture.Init(200, 100);

        for (int j = 0; j < ppmTexture.Height; ++j)
        {
            for (int i = 0; i < ppmTexture.Width; ++i)
            {
                float r = (float)i / ppmTexture.Width;
                float g = (float)j / ppmTexture.Height;
                float b = 0.2f;
                ppmTexture.WriteAPixel(r, g, b);
            }
        }

        ppmTexture.Complete();
    }
}
