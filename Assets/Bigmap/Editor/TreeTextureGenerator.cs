using System.IO;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;


public class TreeTextureGenerator
{

    [MenuItem("Assets/Bigmap/Create/Tree Map(Perlin)")]
    public static void CreatePerlinTreeMasks()
    {
        string savePath = "/Bigmap/Resources/Textures/Masks/TreeMap";
        string heightMapPath = "Assets/Bigmap/Resources/Textures/Masks/TerrainHeightMap.png";
        int saveFile = 0;

        Texture2D heightMap = AssetDatabase.LoadAssetAtPath<Texture2D>(heightMapPath);
        int width = heightMap.width;

        float TreeLength = 2.0f;
        float zOrg = 0.3f;
        float xOrg = 0.3f;
        float scale = 5.0f;

        int x = 0;
        int z = 0;

        Texture2D perlinNoise = new Texture2D(width, width, TextureFormat.ARGB32, false);
        Color[] pix = new Color[width * width];

        for(z=0;z<width;z++)
        {
            for(x=0;x<width;x++)
            {
                float xCoord = xOrg + x / (float)width * scale;
                float zCoord = zOrg + z / (float)width * scale;

                float sample = Mathf.PerlinNoise(xCoord, zCoord);
                float height = heightMap.GetPixel(x, z).r;
                float type = GetTreeType(2);
                pix[z * width + x] = new Color(sample, height, type);
            }

        }
        perlinNoise.SetPixels(pix);
        perlinNoise.Apply();

        byte[] pixbytes = perlinNoise.EncodeToPNG();

        while (File.Exists(Application.dataPath + savePath+saveFile +".png"))
        {
            ++saveFile;
        }
        File.WriteAllBytes(Application.dataPath + savePath + saveFile + ".png", pixbytes);
    }

    public static float GetTreeType(int num)
    {
        float type = Random.Range(1, num) / (float)num;
        return type;
    }
}
