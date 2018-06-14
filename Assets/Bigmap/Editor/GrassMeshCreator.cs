using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;



public class GrassMeshCreator {

    
    [MenuItem("Assets/Bigmap/Create/Grass Mesh")]
    public static void CreateGrassMesh()
    {
        string savePath = "/Bigmap/BigmapMesh/GrassMesh";
        int saveFile = 0;
        Mesh grassMesh = new Mesh();

        List<Vector3> vertexList = new List<Vector3>();
        List<Vector2> uvsList = new List<Vector2>();
        List<int> indexList = new List<int>();

        vertexList.Add(new Vector3(-0.5f, 0.0f, 0.0f));
        vertexList.Add(new Vector3(0.5f, 0.0f, 0.0f));
        vertexList.Add(new Vector3(-0.5f,1.0f,0.0f));
        vertexList.Add(new Vector3(0.5f, 1.0f, 0.0f));

        uvsList.Add(new Vector2(0.0f, 0.0f));
        uvsList.Add(new Vector2(1.0f, 0.0f));
        uvsList.Add(new Vector2(0.0f, 1.0f));
        uvsList.Add(new Vector2(1.0f, 1.0f));

        //0,1,3
        //0,3,2
        indexList.Add(0);indexList.Add(1);indexList.Add(3);
        indexList.Add(0);indexList.Add(3);indexList.Add(2);

        grassMesh.SetVertices(vertexList);
        grassMesh.SetTriangles(indexList,0);
        grassMesh.SetUVs(0, uvsList);

        //Log the test path;
        Debug.Log(Application.dataPath + savePath+saveFile + ".asset");

        while(File.Exists(Application.dataPath + savePath + saveFile + ".asset"))
        {
            ++saveFile;
        }
        

        AssetDatabase.CreateAsset(grassMesh, "Assets/BigMap/BigmapMesh/GrassMesh"+saveFile+".asset");
    }

    [MenuItem("Assets/Bigmap/Create/New Grass Mesh")]
    public static void CreateInstancingWavingGrass()
    {
        string savePath = "/Bigmap/Resources/Textures/Masks/GrassPerlin";
        string meshSavePath = "/Bigmap/BigmapMesh/NGrassMesh";
        int saveFile = 0;

        int width = 128;
        float grassLenth = 1.0f;
        float zOrg = 0.5f;
        float xOrg = 0.3f;
        float scale = 4.0f;
        int x = 0;
        int z = 0;
        Texture2D perlinNoise = new Texture2D(width, width, TextureFormat.ARGB32, false);
        Color[] pix = new Color[width * width];
        for(z=0;z<width;z++)
        {
            for(x=0;x<width;x++)
            {
                float xCoord = xOrg + x /(float) width * scale;
                float zCoord = zOrg + z /(float) width * scale;

                float sample = Mathf.PerlinNoise(xCoord, zCoord);
                pix[z * width + x] = new Color(sample, sample, sample);
            }
        }
        perlinNoise.SetPixels(pix);
        perlinNoise.Apply();

        byte[] pixbytes = perlinNoise.EncodeToPNG();

        while (File.Exists(Application.dataPath + savePath + saveFile + ".png"))
        {
            ++saveFile;
        }
        File.WriteAllBytes(Application.dataPath + savePath + saveFile + ".png", pixbytes);


        //Create Grass Mesh

        Mesh grassMesh = new Mesh();

        List<Vector3> vertexList = new List<Vector3>();
        List<Vector4> uvs0List = new List<Vector4>();

        //uvs2 is used to represent the grass fit
        //uvs2.xyz is mean tha the grass fit what color
        //uvs.w is the density need to draw the grass
        List<Vector4> uvs2List = new List<Vector4>();
        //uvs3 means what kind of the grass is the grass
        List<Vector4> uvs3List = new List<Vector4>();
        //List<Vector3> uvs4List = new List<Vector3>();

        List<int> indexList = new List<int>();

        float zpos = 0.0f;
        float xpos = 0.0f;

        int vertexCount = 0;

        for (z = 0; z < width; z++)
        {
            for (x = 0; x < width; x++)
            {
                float pix_num = pix[z * width + x].r;
                if(pix_num<0.5f)
                {
                    continue;
                }

                zpos = grassLenth * z;
                xpos = grassLenth * x;

                Vector3 centerPos = 
                    new Vector3(xpos +Random.Range(-0.25f,0.25f), 0, zpos + Random.Range(-0.25f, 0.25f));
                Quaternion rq = 
                    Quaternion.Euler(0, Random.Range(0, 180), 0);

                //Add all vertex
                vertexList.Add(centerPos + rq * new Vector3(-0.5f, 0.0f, 0.0f));
                vertexList.Add(centerPos + rq * new Vector3(0.5f, 0.0f, 0.0f));
                vertexList.Add(centerPos + rq * new Vector3(-0.5f, 1.0f, 0.0f));
                vertexList.Add(centerPos + rq * new Vector3(0.5f, 1.0f, 0.0f));

                //uvs4List.Add(centerPos);
                //uvs4List.Add(centerPos);
                //uvs4List.Add(centerPos);
                //uvs4List.Add(centerPos);

                //Add all uvs0;
                uvs0List.Add(new Vector4(0.0f, 0.0f,centerPos.x,centerPos.z));
                uvs0List.Add(new Vector4(1.0f, 0.0f, centerPos.x, centerPos.z));
                uvs0List.Add(new Vector4(0.0f, 1.0f, centerPos.x, centerPos.z));
                uvs0List.Add(new Vector4(1.0f, 1.0f, centerPos.x, centerPos.z));

                //Add all uvs3;
                int pick = pick3();
                Vector4 texcoord2 = Vector4.zero;
                for(int i =0;i<=2;i++)
                {
                    if (i == pick)
                    {
                        texcoord2[i] = 0;
                    }
                    else
                        texcoord2[i] = 1;
                }
                texcoord2.w = pix_num;
                for (int i = 0; i < 4; i++)
                {
                    uvs2List.Add(texcoord2);
                }

                //Add all uvs2;
                Vector4 texcoord3 = Vector4.zero;
                for(int i=0;i<3;i++)
                {
                    texcoord3[i] = Random.Range(0.0f, 1.0f);
                }

                //Debug.Log(texcoord3);
                for (int i = 0; i < 4; i++)
                {
                    uvs3List.Add(texcoord3);
                }
                vertexCount += 4;
                int c = vertexCount - 4;
                indexList.Add(c); indexList.Add(c+1); indexList.Add(c+3);
                indexList.Add(c); indexList.Add(c+3); indexList.Add(c+2);

            }
        }

        //Add all 
        grassMesh.SetVertices(vertexList);
        grassMesh.SetUVs(0, uvs0List);
        grassMesh.SetUVs(2, uvs2List);
        grassMesh.SetUVs(3, uvs3List);
        //grassMesh.SetUVs(1, uvs4List);
        grassMesh.SetTriangles(indexList, 0);

        AssetDatabase.CreateAsset(grassMesh, "Assets/BigMap/BigmapMesh/GrassMesh" + saveFile + ".asset");

    }

    public static int pick3()
    {

        float rand = Random.Range(0, 3);
        return Mathf.FloorToInt(rand);
    }
}
