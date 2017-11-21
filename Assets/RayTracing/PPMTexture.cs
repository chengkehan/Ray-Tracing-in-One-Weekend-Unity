using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PPMTexture
{
    private Texture2D texture = null;
    public Texture2D Texture
    {
        get
        {
            return texture;
        }
    }

    public int Width
    {
        get
        {
            return texture == null ? 0 : texture.width;
        }
    }

    public int Height
    {
        get
        {
            return texture == null ? 0 : texture.height;
        }
    }

    public bool sRGB = false;

    private Color[] pixels = null;

    private int pIndex = 0;

    public void Init(int width, int height)
    {
        texture = new Texture2D(width, height, TextureFormat.RGB24, false, true);
        texture.wrapMode = TextureWrapMode.Clamp;
        pixels = texture.GetPixels();
        pIndex = 0;
    }

    public void WriteAPixel(float r, float g, float b)
    {
        pixels[pIndex++] = new Color(ToSRGB(r), ToSRGB(g), ToSRGB(b));
    }

    public void WriteAPixel(float r, float g, float b, int pixelIndex)
    {
        pixels[pixelIndex] = new Color(ToSRGB(r), ToSRGB(g), ToSRGB(b));
    }

    public void WriteAPixel(Color c, int pixelIndex)
    {
        c.r = ToSRGB(c.r);
        c.g = ToSRGB(c.g);
        c.b = ToSRGB(c.b);

        pixels[pixelIndex] = c;
    }

    public void WriteAPixel(Color c)
    {
        c.r = ToSRGB(c.r);
        c.g = ToSRGB(c.g);
        c.b = ToSRGB(c.b);

        pixels[pIndex++] = c;
    }

    public void Complete()
    {
        texture.SetPixels(pixels);
        texture.Apply();
    }

    private float ToSRGB(float v)
    {
        if (sRGB)
        {
            return Mathf.Pow(v, 1 / 2.2f);
        }
        else
        {
            return v;
        }
    }
}
