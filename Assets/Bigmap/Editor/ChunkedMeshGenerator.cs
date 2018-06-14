using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ChunkedMeshGenerator
{

    [MenuItem("Assets/Bigmap/Create/Chunked Mesh L0D0")]
    public static void CreateChunkedMeshLOD0()
    {

        int resolution = 128;
        int cellSize = 8;

        string savePath = "/Bigmap/BigmapMesh/ChunkedMeshs";
        int saveFile = 0;

        Mesh chunkedMesh = new Mesh();
        List<Vector3> normal_lsit = new List<Vector3>();
        List<Vector3> vertex_list = new List<Vector3>();
        List<int> triangle_list = new List<int>();
        List<Vector4> texcoord2_list = new List<Vector4>();
        List<Vector4> texcoord3_list = new List<Vector4>();       

        for(int z = 0;z<resolution+1;z++)
        {
            for(int x = 0; x<resolution+1; x++)
            {
                Vector3 vertex = 
                    new Vector3(x * cellSize, 0, z * cellSize);

                Vector4 texcoord2 = 
                    new Vector4(x == 0 || z == 0 || x == resolution || z == resolution ? 1 : 0,
                    0, 
                    0, 0);
                if(x==0)
                {
                    texcoord2.y = 0;
                    texcoord2.z = 0;
                }else if (z==resolution)
                {
                    texcoord2.y = 0;
                    texcoord2.z = 1;
                }else if(x==resolution)
                {
                    texcoord2.y = 1;
                    texcoord2.z = 0;
                }else if(z==0)
                {
                    texcoord2.y = 1;
                    texcoord2.z = 1;
                }

                Vector4 texcoord3 = new Vector4(x % 2 == 0 ? 0 : 1,
                    z % 2 == 0 ? 0 : 1,
                    0, 0);

                vertex_list.Add(vertex);
                texcoord2_list.Add(texcoord2);
                texcoord3_list.Add(texcoord3);

                normal_lsit.Add(new Vector3(0, 1, 0));
            }
        }

        for(int z=0; z<resolution;z++)
        {
            for(int x=0;x<resolution;x++)
            {
                //Triangle Fan one
                triangle_list.Add(x + z * (resolution +1));
                triangle_list.Add(x + (z + 1) * (resolution+1));
                triangle_list.Add(x + z * (resolution+1) + 1);
                

                //Triangle Fan Two
                triangle_list.Add(x + z * (resolution + 1) + 1);
                triangle_list.Add(x + (z + 1) * (resolution + 1));
                triangle_list.Add(x + (z + 1) * (resolution + 1) + 1);
            }

        }
        chunkedMesh.SetVertices(vertex_list);
        chunkedMesh.SetUVs(2, texcoord2_list);
        chunkedMesh.SetUVs(3, texcoord3_list);
        chunkedMesh.SetTriangles(triangle_list, 0);
        chunkedMesh.SetNormals(normal_lsit);

        while (File.Exists(Application.dataPath + 
            savePath+resolution+"_"+ cellSize +"_"+
            saveFile + ".asset"))
        {
            ++saveFile;
        }

        AssetDatabase.CreateAsset(chunkedMesh, "Assets" +
            savePath + resolution + "_" + cellSize + "_" +
            saveFile + ".asset");

    }

    [MenuItem("Assets/Bigmap/Create/Chunked Mesh L0D1")]
    public static void CreateChunkedMeshLOD1()
    {

        int resolution = 128;
        int cellSize = 4;

        string savePath = "/Bigmap/BigmapMesh/ChunkedMeshs";
        int saveFile = 0;

        Mesh chunkedMesh = new Mesh();
        List<Vector3> normal_lsit = new List<Vector3>();
        List<Vector3> vertex_list = new List<Vector3>();
        List<int> triangle_list = new List<int>();
        List<Vector4> texcoord2_list = new List<Vector4>();
        List<Vector4> texcoord3_list = new List<Vector4>();

        for (int z = 0; z < resolution + 1; z++)
        {
            for (int x = 0; x < resolution + 1; x++)
            {
                Vector3 vertex =
                    new Vector3(x * cellSize, 0, z * cellSize);

                Vector4 texcoord2 =
                    new Vector4(x == 0 || z == 0 || x == resolution || z == resolution ? 1 : 0,
                    0,
                    0, 0);
                if (x == 0)
                {
                    texcoord2.y = 0;
                    texcoord2.z = 0;
                }
                else if (z == resolution)
                {
                    texcoord2.y = 0;
                    texcoord2.z = 1;
                }
                else if (x == resolution)
                {
                    texcoord2.y = 1;
                    texcoord2.z = 0;
                }
                else if (z == 0)
                {
                    texcoord2.y = 1;
                    texcoord2.z = 1;
                }

                Vector4 texcoord3 = new Vector4(x % 2 == 0 ? 0 : 1,
                    z % 2 == 0 ? 0 : 1,
                    0, 0);

                vertex_list.Add(vertex);
                texcoord2_list.Add(texcoord2);
                texcoord3_list.Add(texcoord3);

                normal_lsit.Add(new Vector3(0, 1, 0));
            }
        }

        for (int z = 0; z < resolution; z++)
        {
            for (int x = 0; x < resolution; x++)
            {
                //Triangle Fan one
                triangle_list.Add(x + z * (resolution + 1));
                triangle_list.Add(x + (z + 1) * (resolution + 1));
                triangle_list.Add(x + z * (resolution + 1) + 1);


                //Triangle Fan Two
                triangle_list.Add(x + z * (resolution + 1) + 1);
                triangle_list.Add(x + (z + 1) * (resolution + 1));
                triangle_list.Add(x + (z + 1) * (resolution + 1) + 1);
            }

        }
        chunkedMesh.SetVertices(vertex_list);
        chunkedMesh.SetUVs(2, texcoord2_list);
        chunkedMesh.SetUVs(3, texcoord3_list);
        chunkedMesh.SetTriangles(triangle_list, 0);
        chunkedMesh.SetNormals(normal_lsit);

        while (File.Exists(Application.dataPath +
            savePath + resolution + "_" + cellSize + "_" +
            saveFile + ".asset"))
        {
            ++saveFile;
        }

        AssetDatabase.CreateAsset(chunkedMesh, "Assets" +
            savePath + resolution + "_" + cellSize + "_" +
            saveFile + ".asset");

    }


    [MenuItem("Assets/Bigmap/Create/Chunked Mesh L0D2")]
    public static void CreateChunkedMeshLOD2()
    {

        int resolution = 128;
        int cellSize = 2;

        string savePath = "/Bigmap/BigmapMesh/ChunkedMeshs";
        int saveFile = 0;

        Mesh chunkedMesh = new Mesh();
        List<Vector3> normal_lsit = new List<Vector3>();
        List<Vector3> vertex_list = new List<Vector3>();
        List<int> triangle_list = new List<int>();
        List<Vector4> texcoord2_list = new List<Vector4>();
        List<Vector4> texcoord3_list = new List<Vector4>();

        for (int z = 0; z < resolution + 1; z++)
        {
            for (int x = 0; x < resolution + 1; x++)
            {
                Vector3 vertex =
                    new Vector3(x * cellSize, 0, z * cellSize);

                Vector4 texcoord2 =
                    new Vector4(x == 0 || z == 0 || x == resolution || z == resolution ? 1 : 0,
                    0,
                    0, 0);
                if (x == 0)
                {
                    texcoord2.y = 0;
                    texcoord2.z = 0;
                }
                else if (z == resolution)
                {
                    texcoord2.y = 0;
                    texcoord2.z = 1;
                }
                else if (x == resolution)
                {
                    texcoord2.y = 1;
                    texcoord2.z = 0;
                }
                else if (z == 0)
                {
                    texcoord2.y = 1;
                    texcoord2.z = 1;
                }

                Vector4 texcoord3 = new Vector4(x % 2 == 0 ? 0 : 1,
                    z % 2 == 0 ? 0 : 1,
                    0, 0);

                vertex_list.Add(vertex);
                texcoord2_list.Add(texcoord2);
                texcoord3_list.Add(texcoord3);

                normal_lsit.Add(new Vector3(0, 1, 0));
            }
        }

        for (int z = 0; z < resolution; z++)
        {
            for (int x = 0; x < resolution; x++)
            {
                //Triangle Fan one
                triangle_list.Add(x + z * (resolution + 1));
                triangle_list.Add(x + (z + 1) * (resolution + 1));
                triangle_list.Add(x + z * (resolution + 1) + 1);


                //Triangle Fan Two
                triangle_list.Add(x + z * (resolution + 1) + 1);
                triangle_list.Add(x + (z + 1) * (resolution + 1));
                triangle_list.Add(x + (z + 1) * (resolution + 1) + 1);
            }

        }
        chunkedMesh.SetVertices(vertex_list);
        chunkedMesh.SetUVs(2, texcoord2_list);
        chunkedMesh.SetUVs(3, texcoord3_list);
        chunkedMesh.SetTriangles(triangle_list, 0);
        chunkedMesh.SetNormals(normal_lsit);

        while (File.Exists(Application.dataPath +
            savePath + resolution + "_" + cellSize + "_" +
            saveFile + ".asset"))
        {
            ++saveFile;
        }

        AssetDatabase.CreateAsset(chunkedMesh, "Assets" +
            savePath + resolution + "_" + cellSize + "_" +
            saveFile + ".asset");

    }
}
