using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GrassTexturePacker  {

    
    [MenuItem("Assets/Bigmap/Pack The Grass Texture")]
    public static void PackGrassTexture()
    {
        List<Texture2D> grassMap_list = new List<Texture2D>();
        string[] filename = Directory.GetFiles("Assets/Bigmap/Resources/Textures/Grass/"); 
        foreach(string f in filename)
        {
            string temp = f.Substring(f.IndexOf("Assets"));
            if(!f.Contains("meta"))
            {
                Debug.Log(temp);
                Texture2D t = AssetDatabase.LoadAssetAtPath<Texture2D>(temp);
                grassMap_list.Add(t);
            }

        }
        //Texture2D t = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Bigmap/Resources/Textures/Grass/Fern.psd");
       
        //foreach(Object t in textures)
        //{
        //    grassMap_list.Add(t as Texture2D);
        //}
        //grassMap_list.Add(t);

        int width = grassMap_list[0].width;
        TextureFormat tf = grassMap_list[0].format;
        Texture2DArray grassTexture2d = new Texture2DArray(width, width, grassMap_list.Count, tf, false);

        int depth = 0;
        foreach(Texture2D  texture in grassMap_list)
        {
            Graphics.CopyTexture(texture, 0, grassTexture2d, depth);
            depth++;
        }

        AssetDatabase.CreateAsset(grassTexture2d, "Assets/Bigmap/Resources/Textures/Grass/grass_map_2d.asset");
    }

}
