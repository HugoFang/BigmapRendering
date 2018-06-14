using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadTree : MonoBehaviour
{

    [SerializeField]
    public Camera mainCamera = null;
    [HideInInspector]
    public float tfov = 0.0f;
    [HideInInspector]
    public float cfar = 0.0f;
    [HideInInspector]
    public float aspect = 0.0f;
    [HideInInspector]
    public float cright = 0.0f;
    [HideInInspector]
    public Vector3 terrainRootPosition = Vector3.zero;

    public int maxLevel = 3;
    public int maxResolution = 1024;
    public float meter = 2.0f;
    [SerializeField]
    public int chunkedMeshResolution = 128;

    //[SerializeField]
    public List<float> distanceGates = new List<float>();
    //[SerializeField]
    [HideInInspector]
    public List<Vector2> distanceUpperLowerGates = new List<Vector2>();
    //[SerializeField]
    [HideInInspector]
    public List<float> cellSize_list = new List<float>();

    //The QuadTree Nodes save Here
    private List<QuadTreeNode> root_node_list = new List<QuadTreeNode>();
    private Dictionary<string, QuadTreeNode> all_node_dist = new Dictionary<string, QuadTreeNode>();
    [HideInInspector]
    public List<List<QuadTreeNode>> active_node_list = new List<List<QuadTreeNode>>();
    [SerializeField]
    private List<Mesh> chunked_mesh_list = new List<Mesh>();
    [SerializeField]
    private Material terrainMaterial = null;
    [SerializeField]
    private List<Material> terrainMaterial_list = new List<Material>();
    [SerializeField]
    private List<MaterialPropertyBlock> mpb_list = new List<MaterialPropertyBlock>();
    [SerializeField]
    private float detailRepeatSize = 128.0f;
    public float heightFactor = 1000.0f;

    public Texture2D heightMap = null;
    public Texture2D normalMap = null;
    [SerializeField]
    private Texture2D detialMap = null;
    [SerializeField]
    private Texture2D detailTex1 = null;
    [SerializeField]
    private Texture2D detailNormal1 = null;
    [SerializeField]
    private Texture2D detailTex2 = null;
    [SerializeField]
    private Texture2D detailNormal2 = null;
    [SerializeField]
    private Texture2D detailTex3 = null;
    [SerializeField]
    private Texture2D detailNormal3 = null;
    [SerializeField]
    private Texture2D detailTex4 = null;
    [SerializeField]
    private Texture2D detailNormal4 = null;

    public Texture2D grassMap = null;
    [SerializeField]
    private Texture2DArray grassTextures = null;
    [SerializeField]
    private Mesh grassMesh = null;
    [SerializeField]
    private List<Texture2D> grasses = new List<Texture2D>();
    [SerializeField]
    private float grassShaderTimeScale = 1.0f;
    [SerializeField]
    private float grassDistanceCutoff = 0.8f;
    [SerializeField]
    private float grassDensityFactor = 0.25f;
    [SerializeField]
    private Material grassMaterial = null;
    private MaterialPropertyBlock grass_mpb = null;

    //The Plants in the scene
    public Texture2D treeMap = null;
    public List<GameObject> trees = new List<GameObject>();

    private List<List<Matrix4x4>> chunked_trms_list = new List<List<Matrix4x4>>();
    private List<List<Vector4>> neighbourVector_list = new List<List<Vector4>>();
    private List<List<Vector4>> chunkRootPosition_list = new List<List<Vector4>>();

    private List<Matrix4x4> grass_trms_list = new List<Matrix4x4>();
    private List<Vector4> grassRootPosition_list = new List<Vector4>();


    [SerializeField]
    private bool safeCheck = false;

    //Initialize all the nodes;
    private void Awake()
    {

       
        SafeCheck();
        if (safeCheck)
        {
            UpdateTreeIm();
            UpdateCameraIm();
            UpdateLODIm();
            InitNodes();
            SetTerrainMaterialUniformProp();
            SetGrassMaterialUniformProp();
        }

    }
    //Make used of the quadtree nodes to implement 
    private void Update()
    {
        ClearActiveList();
        UpdateNodesDistance();
        UpdateNodesNeighbour();
        RenderChunkedMesh();
        RenderGrass();
        ResetNodes();
    }


    private void SafeCheck()
    {
        if (terrainMaterial==null)
        {
            Debug.Log("Terrain Material could not be null");
            safeCheck = false;
        }

        if (chunked_mesh_list.Count == 0)
        {
            Debug.Log("Chunked mesh could not be empty");
            safeCheck = false;
        }

        if (distanceGates.Count == 0)
        {
            Debug.Log("Chunked distance gates could not be empty");
            safeCheck = false;
        }
        if (maxLevel == 0)
        {
            Debug.Log("Terrain max level should not be 0");
        }

        safeCheck = true;
    }

    private void UpdateTreeIm()
    {
        terrainRootPosition = this.transform.position;
        Material tMaterial = null;
        for (int i = 0; i < maxLevel; i++)
        {
            tMaterial = new Material(terrainMaterial);
            terrainMaterial_list.Add(tMaterial);
            mpb_list.Add(new MaterialPropertyBlock());
        }
        grass_mpb = new MaterialPropertyBlock();
        for (int i = 0; i < maxLevel; i++)
        {
            active_node_list.Add(new List<QuadTreeNode>());
            chunked_trms_list.Add(new List<Matrix4x4>());
            neighbourVector_list.Add(new List<Vector4>());
            chunkRootPosition_list.Add(new List<Vector4>());
        }
    }

    private void UpdateCameraIm()
    {
        float fov = mainCamera.fieldOfView;
        tfov = Mathf.Tan(Mathf.Deg2Rad * fov / 2);
        aspect = mainCamera.aspect;
        cfar = mainCamera.farClipPlane;
        cright = cfar * tfov * aspect;
    }

    private void UpdateLODIm()
    {

        float tempCellSize = 0.0f;
        for (int i = 0; i < maxLevel; i++)
        {
            tempCellSize = meter * Mathf.Pow(2, maxLevel - i - 1);
            cellSize_list.Add(tempCellSize);
        }
        distanceUpperLowerGates.Add(new Vector2(cfar, distanceGates[0]));

        for (int i = 1; i < maxLevel; i++)
        {
            distanceUpperLowerGates.Add(new Vector2(distanceGates[i - 1], distanceGates[i]));
        }

    }

    //private void UpdateObjects()
    //{
        
    //    GameObject lodObject = null;
    //    string oName = "";
    //    MeshRenderer mr = null;
    //    MeshFilter mf = null;
        
    //    List<List<Material>> materials_list = new List<List<Material>>();
    //    List<Mesh> meshes_lsit = new List<Mesh>();
    //    foreach(GameObject tree in trees)
    //    {
    //        oName = tree.name;
    //        for(int i = 0;i<maxLevel;i++)
    //        {
    //            lodObject = tree.transform.Find(oName+"_LOD"+i).gameObject;
    //            mr = lodObject.GetComponent<MeshRenderer>();
    //            mf = lodObject.GetComponent<MeshFilter>();

    //            materials_list.Add(new List<Material>(mr.materials));
    //            meshes_lsit.Add(mf.mesh);
    //        }
    //        //treesMesh.Add(meshes_lsit);
    //        //treesMaterial.Add(materials_list);

    //        materials_list = new List<List<Material>>();
    //        meshes_lsit = new List<Mesh>();
    //    }
    //}

    private void InitNodes()
    {
        ProduceChildren(null, 0);
        AddSibling();
    }

    private void ProduceChildren(QuadTreeNode parent, int level)
    {
        if (level >= maxLevel)
            return;
        Vector3 chunkRoot = Vector3.zero;
        Vector3 cChunkRoot = chunkRoot;
        string pindex = "";
        string cindex = pindex + "";
        float cellSize = cellSize_list[level];
        float dx = 0;
        float dz = 0;

        if (level == 0)
        {
            chunkRoot = terrainRootPosition;
        }
        else
        {
            chunkRoot = parent.lbPos;
            pindex = parent.index;
        }

        for (int i = 0; i < 4; i++)
        {
            cindex = pindex + i;
            QuadTreeNode temp = new QuadTreeNode(cindex, level, parent, this);
            dx = Mathf.FloorToInt(i / 2) * chunkedMeshResolution * cellSize;
            dz = (i % 2) * chunkedMeshResolution * cellSize;
            cChunkRoot.x = chunkRoot.x + dx;
            cChunkRoot.z = chunkRoot.z + dz;
            temp.UpdateNodePositionIm(cChunkRoot, chunkedMeshResolution, cellSize);
            temp.UpdateTrmsMatrix();
            temp.UpdateTreePosition();
            if (level == 0)
            {
                root_node_list.Add(temp);
            }
            else
            {

                parent.children.Add(temp);
            }
            all_node_dist.Add(cindex, temp);
            ProduceChildren(temp, level + 1);
        }

    }
    private void AddSibling()
    {
        foreach (QuadTreeNode node in root_node_list)
        {
            AddSibling(node);
        }
    }

    private void AddSibling(QuadTreeNode node)
    {
        string index = node.index;
        QuadTreeNode sibling = null;
        for (int i = 0; i < 4; i++)
        {
            string sindex = GetSiblingNew(index, i);
            //Debug.Log(index + ":"+i+":" + sindex);
            if (all_node_dist.TryGetValue(sindex, out sibling))
            {
                node.siblings.Add(sibling);
            }
            else
            {
                node.siblings.Add(null);
            }

        }
        if (node.currentLevel < maxLevel - 1)
        {
            foreach (QuadTreeNode n in node.children)
            {
                AddSibling(n);
            }
        }
    }

    private string GetSibling(string index, int sible)
    {
        int slength = index.Length;
        string o = index;
        int f = 0;
        if (slength == 0)
            return "";
        if (sible == 0)
        {
            f = -2;
        }
        else if (sible == 1)
        {
            f = 1;
        }
        else if (sible == 2)
        {
            f = 2;
        }
        else if (sible == 3)
        { 
            f = -1;
        }
        for (int i = slength - 1; i >= 0; i--)
        {
            int num = int.Parse(o.Substring(i, 1));
            num += f;
            if (-1 < num && num < 4)
            {
                if (i == slength - 1)
                    o = o.Substring(0, i) + num;
                else
                    o = o.Substring(0, i) + num + o.Substring(i + 1, slength - i - 1);
                f = 0;
                break;
            }
            else if (num >= 4)
            {
                num -= 4;
                if (i == slength - 1)
                    o = o.Substring(0, i) + num;
                else
                    o = o.Substring(0, i) + num + o.Substring(i + 1, slength - i - 1);
            }
            else
            {
                num += 4;
                if (i == slength - 1)
                    o = o.Substring(0, i) + num;
                else
                    o = o.Substring(0, i) + num + o.Substring(i + 1, slength - i - 1);
            }
        }
        if (f == 0)
            return o;
        else
            return "";
    }
    private string GetSiblingNew(string index, int sible)
    {
        int slength = index.Length;
        string o = index;
        bool f = false;
        if (slength == 0)
            return "";

        string target = "";

        for (int i = slength - 1; i >= 0; i--)
        {
            int num = int.Parse(o.Substring(i, 1));

            if(num ==0)
            {
                if(sible == 0)
                {
                    target = "2";
                }
                else if(sible == 1)
                {
                    target = "1";
                    f = true;
                }else if(sible == 2)
                {
                    target = "2";
                    f = true;
                }else if(sible == 3)
                {
                    target = "1";
                }
                

            }else if(num==1)
            {
                if (sible == 0)
                {
                    target = "3";
                }
                else if (sible == 1)
                {
                    target = "0";
                }
                else if (sible == 2)
                {
                    target = "3";
                    f = true;
                }
                else if (sible == 3)
                {
                    target = "0";
                    f = true;
                }

            }
            else if(num==2)
            {
                if (sible == 0)
                {
                    target = "0";
                    f = true;
                }
                else if (sible == 1)
                {
                    target = "3";
                    f = true;
                }
                else if (sible == 2)
                {
                    target = "0";
                }
                else if (sible == 3)
                {
                    target = "3";
                }
            }
            else
            {
                if (sible == 0)
                {
                    target = "1";
                    f = true;
                }
                else if (sible == 1)
                {
                    target = "2";
                }
                else if (sible == 2)
                {
                    target = "1";
                }
                else if (sible == 3)
                {
                    target = "2";
                    f = true;
                }
            }

            if (i == slength - 1)
                o = o.Substring(0, i) + target;
            else
                o = o.Substring(0, i) + target + o.Substring(i + 1, slength - i - 1);

            if (f)
                break;
        }

        if (f)
            return o;
        else
            return "";
    }

    private void ClearActiveList()
    {
        foreach (List<QuadTreeNode> qt_list in active_node_list)
        {
            qt_list.Clear();
        }
    }

    private void ClearChunkedTrmsList()
    {
        foreach (List<Matrix4x4> trms_list in chunked_trms_list)
        {
            trms_list.Clear();
        }
    }

    private void ClearNeighbourVectorList()
    {
        foreach (List<Vector4> neightbourv_list in neighbourVector_list)
        {
            neightbourv_list.Clear();
        }
    }
    private void ClearChunkRootPositionlist()
    {
        foreach (List<Vector4> cps_list in chunkRootPosition_list)
        {
            cps_list.Clear();
        }
    }


    private void UpdateNodesDistance()
    {
        NodeType nodeType = NodeType.DISACTIVE;
        foreach (QuadTreeNode node in root_node_list)
        {
            nodeType = node.UpdateNodeStats();
            if (nodeType == NodeType.ACTIVE && node.SimpleTerrainViewCulling())
            {
                node.DisactiveChildNodes();
                active_node_list[node.currentLevel].Add(node);
            }
        }
    }

    private void ResetNodes()
    {
        foreach (QuadTreeNode node in root_node_list)
        {
            node.ResetNode();
        }
    }
    private void UpdateNodesNeighbour()
    {
        foreach (List<QuadTreeNode> qt_list in active_node_list)
        {
            foreach (QuadTreeNode qt in qt_list)
            {
                for (int i = 0; i < 4; i++)
                {
                    QuadTreeNode qts = qt.siblings[i];
                    if (qts == null)
                        qt.neighBourVector[i] = 1;
                    else if (qts.nodeType == NodeType.DISACTIVE)
                    {
                        qt.neighBourVector[i] = 1;
                    }
                    else
                    {
                        qt.neighBourVector[i] = 0;
                    }
                }
            }
        }
    }

    private void RenderChunkedMesh()
    {
        //Update All the Imformation needed for rendering the chunked mesh
        ClearChunkedTrmsList();
        ClearNeighbourVectorList();
        ClearChunkRootPositionlist();
        ClearGrassTrmsList();
        ClearGrassPositionList();
        ClearTrees();
        UpdateTerrainNodesRenderIm();
        RenderTrees();
        for (int i = 0; i < maxLevel; i++)
        {
            SetTerrainMaterialProp(i);
            mpb_list[i].Clear();
            if (active_node_list[i].Count != 0)
            {
                mpb_list[i].SetVectorArray("_NeighbourVector", neighbourVector_list[i]);
                mpb_list[i].SetVectorArray("_ChunkRootPosition", chunkRootPosition_list[i]);

                Graphics.DrawMeshInstanced(chunked_mesh_list[i], 0,
                    terrainMaterial_list[i], chunked_trms_list[i], mpb_list[i], UnityEngine.Rendering.ShadowCastingMode.On,
                    true);
            }
        }
    }

    private void RenderGrass()
    {
        SetGrassMaterialProp();

        grass_mpb.Clear();
        grass_mpb.SetVectorArray("_GrassRootPosition", grassRootPosition_list);
        Graphics.DrawMeshInstanced(grassMesh, 0, grassMaterial, grass_trms_list, grass_mpb, UnityEngine.Rendering.ShadowCastingMode.On,
                    true);
    }


    private void RenderTrees()
    {

        for (int i = 0; i < maxLevel; i++)
        {
            if (active_node_list[i].Count != 0)
            {
                foreach(QuadTreeNode qtn in active_node_list[i])
                {
                    qtn.RenderTree();
                }
            }
        }
    }

    private void ClearTrees()
    {
        foreach(KeyValuePair<string,QuadTreeNode> qtn in all_node_dist)
        {
            qtn.Value.HideTrees();
        }
    }

    private void ClearGrassTrmsList()
    {
        grass_trms_list.Clear();
    }
    private void ClearGrassPositionList()
    {
        grassRootPosition_list.Clear();
    }


    private void SetTerrainMaterialUniformProp()
    {
        if (terrainMaterial_list.Count == 0)
        {
            Debug.LogError("Terrain Material Should Not Be Null");
            return;
        }
        for (int i = 0; i < maxLevel; i++)
        {
            terrainMaterial_list[i].SetFloat("_Meter", meter);
            terrainMaterial_list[i].SetFloat("_TotalResolution", maxResolution);
            terrainMaterial_list[i].SetVector("_TerrainRootPosition", terrainRootPosition);
            terrainMaterial_list[i].SetFloat("_HeightFactor", heightFactor);

            terrainMaterial_list[i].SetTexture("_HeightMap", heightMap);
            terrainMaterial_list[i].SetTexture("_NormalMap", normalMap);
            terrainMaterial_list[i].SetTexture("_DetailMap", detialMap);

            terrainMaterial_list[i].SetFloat("_DetailMapRepeatSize", detailRepeatSize);
            terrainMaterial_list[i].SetTexture("_DetailTex1", detailTex1);
            terrainMaterial_list[i].SetTexture("_DetailNormal1", detailNormal1);
            terrainMaterial_list[i].SetTexture("_DetailTex2", detailTex2);
            terrainMaterial_list[i].SetTexture("_DetailNormal2", detailNormal2);
            terrainMaterial_list[i].SetTexture("_DetailTex3", detailTex3);
            terrainMaterial_list[i].SetTexture("_DetailNormal3", detailNormal3);
            terrainMaterial_list[i].SetTexture("_DetailTex4", detailTex4);
            terrainMaterial_list[i].SetTexture("_DetailNormal4", detailNormal4);
        }
    }

    private void SetTerrainMaterialProp(int level)
    {
        if (terrainMaterial_list.Count == 0)
        {
            Debug.LogError("Terrain Material Should Not Be Null");
            return;
        }

        float cellSize = cellSize_list[level];
        Vector2 bounds = distanceUpperLowerGates[level];
        float upperBound = bounds.x;
        float lowerBound = bounds.y;
        terrainMaterial_list[level].SetVector("_CameraPos", mainCamera.transform.position);
        terrainMaterial_list[level].SetFloat("_CellSize", cellSize);
        terrainMaterial_list[level].SetFloat("_UpperBound", upperBound);
        terrainMaterial_list[level].SetFloat("_LowerBound", lowerBound);
    }

    private void SetGrassMaterialUniformProp()
    {
        grassMaterial.SetFloat("_Meter", meter);
        grassMaterial.SetFloat("_MaxResolution", maxResolution);
        grassMaterial.SetVector("_TerrainRootPosition", terrainRootPosition);
        grassMaterial.SetTexture("_NormalMap", normalMap);
        grassMaterial.SetFloat("_HeightFactor", heightFactor);
        grassMaterial.SetFloat("_TimeScale", grassShaderTimeScale);

        grassMaterial.SetFloat("_LowerBound", distanceUpperLowerGates[maxLevel - 1].y);
        grassMaterial.SetFloat("_UpperBound", distanceUpperLowerGates[maxLevel - 1].x);
        grassMaterial.SetTexture("_HeightMap", heightMap);
        grassMaterial.SetTexture("_GrassMap", grassMap);
        grassMaterial.SetTexture("_GrassTextures", grassTextures);
        grassMaterial.SetFloat("_DistanceCutoff", grassDensityFactor);
    }

    private void SetGrassMaterialProp()
    {
        grassMaterial.SetVector("_CameraPos", mainCamera.transform.position);
    }

    private void UpdateTerrainNodesRenderIm()
    {
        List<QuadTreeNode> tempQuadTreeNodes = null;
        List<Matrix4x4> trms = null;
        List<Vector4> neighbours = null;
        List<Vector4> chunkRoot_list = null;
        for (int i = 0; i < maxLevel; i++)
        {
            tempQuadTreeNodes = active_node_list[i];
            trms = chunked_trms_list[i];
            neighbours = neighbourVector_list[i];
            chunkRoot_list = chunkRootPosition_list[i];
            if (i == maxLevel - 1)
            {
                foreach (QuadTreeNode qt in tempQuadTreeNodes)
                {
                    trms.Add(qt.trmsMatrix);
                    neighbours.Add(qt.neighBourVector);
                    chunkRoot_list.Add(qt.lbPos - terrainRootPosition);

                    grass_trms_list.Add(qt.trmsMatrix);
                    grass_trms_list.Add(qt.trmsMatrix01);
                    grass_trms_list.Add(qt.trmsMatrix10);
                    grass_trms_list.Add(qt.trmsMatrix11);

                    grassRootPosition_list.Add(qt.lbPos - terrainRootPosition);
                    grassRootPosition_list.Add((qt.lbPos +qt.ltPos)/2 - terrainRootPosition);
                    grassRootPosition_list.Add((qt.lbPos + qt.rbPos)/2 - terrainRootPosition);
                    grassRootPosition_list.Add(qt.centerPos- terrainRootPosition);
                }
            }
            else
            {
                foreach (QuadTreeNode qt in tempQuadTreeNodes)
                {
                    trms.Add(qt.trmsMatrix);
                    neighbours.Add(qt.neighBourVector);
                    chunkRoot_list.Add(qt.lbPos - terrainRootPosition);
                }
            }
        }

    }

}
public class QuadTreeNode
{
    public string index;
    public QuadTree root;
    public bool isTreeOn = false;

    public List<QuadTreeNode> siblings = new List<QuadTreeNode>();
    public List<QuadTreeNode> children = new List<QuadTreeNode>();
    

    public QuadTreeNode parent = null;

    public Matrix4x4 trmsMatrix = Matrix4x4.identity;
    public Matrix4x4 trmsMatrix01 = Matrix4x4.identity;
    public Matrix4x4 trmsMatrix10 = Matrix4x4.identity;
    public Matrix4x4 trmsMatrix11 = Matrix4x4.identity;


    // level would be start from 0;The higher Level the higher LOD
    public int currentLevel = -1;

    public Vector3 centerPos = Vector3.zero;
    public Vector3 lbPos = Vector3.zero;
    public Vector3 rbPos = Vector3.zero;
    public Vector3 ltPos = Vector3.zero;
    public Vector3 rtPos = Vector3.zero;
    public Vector4 neighBourVector = Vector4.zero;

    public float ccDistance = 0.0f;
    public List<GameObject> trees = null;

    public NodeType nodeType = NodeType.DISACTIVE;

    public QuadTreeNode(string index, int currentLevel, QuadTreeNode parent, QuadTree root)
    {

        this.index = index;
        this.currentLevel = currentLevel;
        this.parent = parent;
        this.root = root;

        nodeType = NodeType.DISACTIVE;

    }

    public void SetSiblings(QuadTreeNode westSibling, QuadTreeNode northSibling,
        QuadTreeNode eastSibling, QuadTreeNode southSibling)
    {
        siblings.Add(westSibling);
        siblings.Add(northSibling);
        siblings.Add(eastSibling);
        siblings.Add(southSibling);
    }

    public void UpdateNodePositionIm(Vector3 chunkRoot, int resolution, float cellSize)
    {
        lbPos = chunkRoot;

        centerPos.x = lbPos.x + resolution / 2 * cellSize;
        centerPos.z = lbPos.z + resolution / 2 * cellSize;
        centerPos.y = lbPos.y;


        rbPos.x = centerPos.x + resolution / 2 * cellSize;
        rbPos.z = lbPos.z;
        rbPos.y = lbPos.y;

        ltPos.x = lbPos.x;
        ltPos.z = centerPos.z + resolution / 2 * cellSize;
        ltPos.y = lbPos.y;

        rtPos.x = centerPos.x + resolution / 2 * cellSize;
        rtPos.z = centerPos.z + resolution / 2 * cellSize;
        rtPos.y = lbPos.y;
    }

    public NodeType UpdateNodeStats()
    {
        Vector2 Bounds = root.distanceUpperLowerGates[currentLevel];
        ccDistance = (root.mainCamera.transform.position - lbPos).magnitude;
        ccDistance = Mathf.Min(ccDistance, (root.mainCamera.transform.position - rbPos).magnitude);
        ccDistance = Mathf.Min(ccDistance, (root.mainCamera.transform.position - ltPos).magnitude);
        ccDistance = Mathf.Min(ccDistance, (root.mainCamera.transform.position - rtPos).magnitude);
        ccDistance = Mathf.Min(ccDistance, (root.mainCamera.transform.position - centerPos).magnitude);
        if (ccDistance >= Bounds.x)
        {
            //nodeType = NodeType.DISACTIVE;
            DisactiveNode();
        }
        else if (ccDistance >= Bounds.y)
        {
            nodeType = NodeType.ACTIVE;
            if(currentLevel != root.maxLevel -1)
            {
                foreach(QuadTreeNode qt in children)
                {
                    qt.DisactiveNode();
                }
                
            }
        }
        else
        {
            if (children.Count == 0)
            {
                nodeType = NodeType.ACTIVE;
                
            }
            else
            {
                NodeType n1 = children[0].UpdateNodeStats();
                NodeType n2 = children[1].UpdateNodeStats();
                NodeType n3 = children[2].UpdateNodeStats();
                NodeType n4 = children[3].UpdateNodeStats();

                if (n1 != NodeType.DISACTIVE || n2 != NodeType.DISACTIVE || n3 != NodeType.DISACTIVE || n4 != NodeType.DISACTIVE)
                {
                    nodeType = NodeType.OPEN;
                    if (children[0].SimpleTerrainViewCulling() && children[0].nodeType != NodeType.OPEN)
                    {
                        children[0].nodeType = NodeType.ACTIVE;
                        children[0].DisactiveChildNodes();
                        root.active_node_list[currentLevel + 1].Add(children[0]);
                    }
                    if (children[1].SimpleTerrainViewCulling() && children[1].nodeType != NodeType.OPEN)
                    {
                        children[1].nodeType = NodeType.ACTIVE;
                        children[1].DisactiveChildNodes();
                        root.active_node_list[currentLevel + 1].Add(children[1]);
                    }
                    if (children[2].SimpleTerrainViewCulling() && children[2].nodeType != NodeType.OPEN)
                    {
                        children[2].nodeType = NodeType.ACTIVE;
                        children[2].DisactiveChildNodes();
                        root.active_node_list[currentLevel + 1].Add(children[2]);
                    }
                    if (children[3].SimpleTerrainViewCulling() && children[3].nodeType != NodeType.OPEN)
                    {
                        children[3].nodeType = NodeType.ACTIVE;
                        children[3].DisactiveChildNodes();
                        root.active_node_list[currentLevel + 1].Add(children[3]);
                    }
                }
            }

        }
        return nodeType;
    }

    public void ResetNode()
    {
        nodeType = NodeType.DISACTIVE;
        if (currentLevel < root.maxLevel - 1)
        {
            foreach (QuadTreeNode qt in children)
            {
                qt.ResetNode();
            }
        }
    }

    public void DisactiveNode()
    {
        nodeType = NodeType.DISACTIVE;
        if(currentLevel != root.maxLevel -1)
        {
            foreach(QuadTreeNode qtn in children)
            {
                qtn.DisactiveNode();
            }
        }
    }

    public void DisactiveChildNodes()
    {
        if (currentLevel != root.maxLevel - 1)
        {
            foreach (QuadTreeNode qtn in children)
            {
                qtn.DisactiveNode();
            }
        }
    }

    public void UpdateTrmsMatrix()
    {
        trmsMatrix = Matrix4x4.Translate(this.lbPos - root.terrainRootPosition);
        trmsMatrix01 = Matrix4x4.Translate((this.lbPos + this.ltPos)/2 - root.terrainRootPosition);
        trmsMatrix10 = Matrix4x4.Translate((this.lbPos + this.rbPos)/2- root.terrainRootPosition);
        trmsMatrix11 = Matrix4x4.Translate(this.centerPos - root.terrainRootPosition);
    }

    public void UpdateTreePosition()
    {
        if(currentLevel == root.maxLevel -1)
        {
            trees = new List<GameObject>();
            //float tm = root.meter;
            Vector3 start = lbPos - root.terrainRootPosition;
            Vector3 end = rtPos - root.terrainRootPosition;
            int zstart =Mathf.FloorToInt(start.z / root.meter);
            int xstart = Mathf.FloorToInt(start.x / root.meter );
            int zend = Mathf.FloorToInt(end.z / root.meter);
            int xend = Mathf.FloorToInt(end.x / root.meter);

            int treeTypes = root.trees.Count;
            for (int z = zstart; z <= zend; z+=8)
            {
                for (int x = xstart; x < xend; x+=8)
                {
                    float ux = x / (float)root.maxResolution;
                    float uz = z / (float)root.maxResolution;
                    float ux1 = (x + 2) / (float)root.maxResolution;
                    float uz1 = (z + 2) / (float)root.maxResolution;
                    float ux0 = (x - 2) / (float)root.maxResolution;
                    float uz0 = (z - 2) / (float)root.maxResolution;


                    Color t = root.grassMap.GetPixelBilinear(ux, uz);
                    float height = root.heightMap.GetPixelBilinear(ux, uz).r;
                    float heightx1 = root.heightMap.GetPixelBilinear(ux1, uz).r;
                    float heightx0 = root.heightMap.GetPixelBilinear(ux0, uz).r;
                    float heightz1 = root.heightMap.GetPixelBilinear(ux, uz1).r;
                    float heightz0 = root.heightMap.GetPixelBilinear(ux, uz0).r;

                    Color normal = root.normalMap.GetPixelBilinear(ux, uz);
                    Vector3 n = new Vector3(normal.r * 2 - 1, normal.g * 2 - 1, normal.b * 2 - 1).normalized;
                    height = height+ (1 - n.x) * heightx0 + n.x * heightx1 + (1 - n.z) * heightz0 + n.z * heightz1;
                    height /= 3.0f;

                    if (t.g > 0.80f)
                    {
                        int ttype = Mathf.FloorToInt(t.b * treeTypes);
                        Vector3 treePosition = new Vector3(x * root.meter + root.terrainRootPosition.x,
                            height*root.heightFactor + root.terrainRootPosition.y, z * root.meter + root.terrainRootPosition.z);
                        trees.Add(GameObject.Instantiate(root.trees[ttype], treePosition, Quaternion.identity));
                    }
                }
            }
            foreach(GameObject tree in trees)
            {
                tree.SetActive(false);
            }
        }

    }

    public void RenderTree()
    {
        if(currentLevel== root.maxLevel -1)
        {
            if (!isTreeOn)
            {
                isTreeOn = true;
                foreach (GameObject t in trees)
                {
                    t.SetActive(true);
                }
            }
        }
        else
        {
            if(!isTreeOn)
            {
                foreach (QuadTreeNode
    qtn in children)
                {
                    qtn.RenderTree();
                }
            }
        }
    }

    public void HideTrees()
    {
        if (currentLevel == root.maxLevel - 1)
        {
            if (isTreeOn)
            {
                isTreeOn = false;
                foreach (GameObject t in trees)
                {
                    t.SetActive(false);
                }
            }
        }
        else
        {
            if(isTreeOn)
            {
                isTreeOn = false;
                //foreach (QuadTreeNode qtn in children)
                //{
                //    qtn.HideTrees();
                //}
            }

        }
    }
    public bool SimpleTerrainViewCulling()
    {
        float cellSize = root.cellSize_list[currentLevel];
        if (ccDistance < cellSize * root.chunkedMeshResolution / 2)
            return true;

        Vector3 cposition = root.mainCamera.transform.position;
        Vector3 clocalRotationEuler = root.mainCamera.transform.localRotation.eulerAngles;
        Quaternion yRotation = Quaternion.Euler(0, clocalRotationEuler.y, 0);

        Vector3 lbound = new Vector3(-root.cright, 0, root.cfar);
        lbound = yRotation * lbound;
        Vector3 rbound = new Vector3(root.cright, 0, root.cfar);
        rbound = yRotation * rbound;

        return
            (
            PointInTriangle(cposition, lbPos, lbound, rbound) ||
            PointInTriangle(cposition, rbPos, lbound, rbound) ||
            PointInTriangle(cposition, ltPos, lbound, rbound) ||
            PointInTriangle(cposition, rtPos, lbound, rbound)
            );
    }

    public bool PointInTriangle(Vector3 sp, Vector3 tp, Vector3 va, Vector3 vb)
    {
        Vector3 sp2 = sp + va;
        Vector3 sp3 = sp + vb;

        Vector3 va2 = sp3 - sp2;
        Vector3 vb2 = sp - sp2;

        Vector3 va3 = sp - sp3;
        Vector3 vb3 = sp2 - sp3;

        return (PointInCenter(sp, tp, va, vb)
            && PointInCenter(sp2, tp, va2, vb2)
            && PointInCenter(sp3, tp, va3, vb3));
    }

    public bool PointInCenter(Vector3 sp, Vector3 tp, Vector3 va, Vector3 vb)
    {
        Vector3 vst = tp - sp;
        float result = Vector3.Dot(Vector3.Cross(vst, va), Vector3.Cross(vb, va));

        return sgn(result);
    }

    public bool sgn(float num)
    {
        if (num > 0)
            return true;
        else
            return false;
    }

}

public enum NodeType
{
    DISACTIVE = -1,
    ACTIVE = 0,
    OPEN = 1
}
