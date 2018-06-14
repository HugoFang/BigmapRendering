using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkedMeshController : MonoBehaviour {


    public Camera mainCamera = null;

    //The default is  600 300
    [SerializeField]
    private Vector3 terrainRootPosition = Vector3.zero;
    [SerializeField]
    private List<float> distanceGates = new List<float>();
    [SerializeField]
    private float resolution = 128;
    [SerializeField]
    private Vector4 testNeighbourVector = new Vector4(0, 0, 0, 0);
    [SerializeField]
    private MeshRenderer meshRenderer;
    [SerializeField]
    private MeshFilter meshFilter;

    [SerializeField]
    private float maxResolution = 1024.0f;
    [SerializeField]
    private float meter = 2.0f;


    [SerializeField]
    private Mesh chunkedMesh = null;
    [SerializeField]
    private Material terrainMaterial = null;
    private MaterialPropertyBlock mpb = null;
    [SerializeField]
    private Texture2D heightMap = null;
    [SerializeField]
    private float heightFactor = 1000.0f;
    [SerializeField]
    private float averageHeight = 0.0f;

    [SerializeField]
    private Texture2D detailMap = null;

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

    [SerializeField]
    private Vector3 centerPos = Vector3.zero;
    [SerializeField]
    private Vector3 lbPos = Vector3.zero;
    [SerializeField]
    private Vector3 rbPos = Vector3.zero;
    [SerializeField]
    private Vector3 ltPos = Vector3.zero;
    [SerializeField]
    private Vector3 rtPos = Vector3.zero;


    [SerializeField]
    private float cellSize = 8.0f;
    [SerializeField]
    private int currentLevel = 0;
    [SerializeField]
    private float detailRepeatSize = 128.0f;
    
    [SerializeField]
    private float tfov = 0.0f;
    [SerializeField]
    private float cfar = 0.0f;
    [SerializeField]
    private float aspect = 0.0f;
    [SerializeField]
    private float cright = 0.0f;

    [SerializeField]
    private float ccDistance = 0.0f;
    [SerializeField]
    private float upperGate = 1200.0f;
    [SerializeField]
    private float lowerGate = 400.0f;

    


    private void Awake()
    {
        if(mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        meshRenderer = this.GetComponent<MeshRenderer>();
        meshFilter = this.GetComponent<MeshFilter>();

        if(meshRenderer.material == null)
        {
            meshRenderer.material = terrainMaterial;
        }
        else
        {
            terrainMaterial = meshRenderer.material;
        }
        if (meshFilter.mesh == null)
        {
            meshFilter.mesh = chunkedMesh;
        }
        else
        {
            chunkedMesh = meshFilter.mesh;
        }
        //Update the position;
        mpb = new MaterialPropertyBlock();
        UpdateNodePositionIm();
        UpdateNodeCameraIm();
        UpdateDistanceGates();
        UpdateDistance();
        SetUniformMaterialParameter();
        UpdateMaterialParameter();
    }


    private void Update()
    {

        UpdateDistance();

        if (SimpleTerrainViewCulling())
        {
            UpdateMaterialParameter();

            Graphics.DrawMesh(chunkedMesh, 
                Matrix4x4.identity, terrainMaterial,LayerMask.NameToLayer("Terrain"),
                mainCamera,0,mpb,true,true);
        }

    }

    public void UpdateNodeCameraIm()
    {
        float fov = mainCamera.fieldOfView;
        tfov = Mathf.Tan(Mathf.Deg2Rad * fov / 2);
        aspect = mainCamera.aspect;
        cfar = mainCamera.farClipPlane;
        cright = cfar * tfov * aspect;
    }

    public void UpdateNodePositionIm()
    {
        centerPos.x = this.transform.position.x + resolution / 2 * cellSize;
        centerPos.z = this.transform.position.z + resolution / 2 * cellSize;
        centerPos.y = this.transform.position.y;

        lbPos = this.transform.position;
        rbPos.x = centerPos.x + resolution / 2 * cellSize;
        rbPos.z = this.transform.position.z;
        rbPos.y = this.transform.position.y;

        ltPos.x = this.transform.position.x;
        ltPos.z = centerPos.z + resolution / 2 * cellSize;
        ltPos.y = this.transform.position.y;

        rtPos.x = centerPos.x + resolution / 2 * cellSize;
        rtPos.z = centerPos.z + resolution / 2 * cellSize;
        rtPos.y = this.transform.position.y;
    }

    public float UpdateDistanceFactor()
    { 
        if(currentLevel ==0)
        {
            return Mathf.Max((cfar - ccDistance)/(cfar-distanceGates[0]), 0.0f);
        }
        else
        {
            return Mathf.Max(
                (distanceGates[currentLevel - 1] - ccDistance) / 
                (distanceGates[currentLevel -1] -distanceGates[currentLevel]),0.0f);
        }
    }

    public void UpdateDistanceGates()
    {
        if(currentLevel == 0)
        {
            upperGate = cfar;
            lowerGate = distanceGates[0];
        }
        else
        {
            upperGate = distanceGates[currentLevel - 1];
            lowerGate = distanceGates[currentLevel];
        }
    }

    public void UpdateNeighbourVector()
    {
        testNeighbourVector = new Vector4(0, 0, 1, 0);
    }

    public void UpdateDistance()
    {
        ccDistance = Mathf.Abs(centerPos.x - mainCamera.transform.position.x) +
           Mathf.Abs(centerPos.z - mainCamera.transform.position.z);
    }
    public void UpdateAverageHeight()
    {
        if(heightMap == null)
        {
            Debug.LogError("Heigh map of the Terrain Should not be empty");
            return;
        }
        Vector3 delta = (this.transform.position - terrainRootPosition);
        float bx = delta.x / maxResolution;
        float bz = delta.z / maxResolution;
        float totalSize = maxResolution * meter;
        float height = 0.0f;
        for(int x =0;x<resolution;x++)
        {
            for(int z =0;z<resolution;z++)
            {
                float dx = x * cellSize / totalSize + bx;
                float dz = z * cellSize / totalSize + bz;
                height += heightMap.GetPixel(Mathf.FloorToInt(dx * maxResolution), 
                    Mathf.FloorToInt(dz * maxResolution)).r * heightFactor;
            }
        }
        averageHeight = height / (resolution * resolution);
    }


    public void SetUniformMaterialParameter()
    {
        if(terrainMaterial == null)
        {
            Debug.LogError("Terrain Material Should Not Be Null");
            return;
        }

        terrainMaterial.SetFloat("_Meter", meter);
        terrainMaterial.SetFloat("_TotalResolution", maxResolution);
        terrainMaterial.SetVector("_TerrainRootPosition", terrainRootPosition);
        terrainMaterial.SetFloat("_HeightFactor", heightFactor);
        terrainMaterial.SetFloat("_UpperBound", upperGate);
        terrainMaterial.SetFloat("_LowerBound", lowerGate);

        terrainMaterial.SetTexture("_HeightMap", heightMap);
        terrainMaterial.SetTexture("_DetailMap", detailMap);

        terrainMaterial.SetFloat("_DetailMapRepeatSize", detailRepeatSize);
        terrainMaterial.SetFloat("_CellSize", cellSize);
        terrainMaterial.SetTexture("_DetailTex1", detailTex1);
        terrainMaterial.SetTexture("_DetailNormal1", detailNormal1);
        terrainMaterial.SetTexture("_DetailTex2", detailTex2);
        terrainMaterial.SetTexture("_DetailNormal2", detailNormal2);
        terrainMaterial.SetTexture("_DetailTex3", detailTex3);
        terrainMaterial.SetTexture("_DetailNormal3", detailNormal3);
        terrainMaterial.SetTexture("_DetailTex4", detailTex4);
        terrainMaterial.SetTexture("_DetailNormal4", detailNormal4);
    }

    public void UpdateMaterialParameter()
    {
        if(terrainMaterial ==null)
        {
            Debug.LogError("Terrain Material Should Not Be Null");
            return;
        }

        terrainMaterial.SetVector("_NeighbourVector",testNeighbourVector);
        terrainMaterial.SetVector("_ChunkRootPosition", this.transform.position);
        terrainMaterial.SetVector("_CameraPos", mainCamera.transform.position);
    }

    public void UpdateCellSize()
    {
        cellSize = meter *
            Mathf.Pow(2,(distanceGates.Count - currentLevel + 1));
    }

    public bool TerrainViewCulling()
    {
        Matrix4x4 viewToWorldMatrix = 
            mainCamera.cameraToWorldMatrix;

        float fov = mainCamera.fieldOfView;
        float tfov = Mathf.Tan(Mathf.Deg2Rad*fov/2);
        float aspect = mainCamera.aspect;
        float far = mainCamera.farClipPlane;
        float top = far * tfov;
        float right = top * aspect;

        Vector4 cFarLeftTop = new Vector4(-right, top, far,1);
        Vector4 cFarRightTop = new Vector4(right, top, far, 1);
        Vector4 cFarLeftBottom = new Vector4(-right, -top, far, 1);
        Vector4 cFarRightBottom = new Vector4(right, -top, far, 1);

        cFarLeftTop = viewToWorldMatrix * cFarLeftTop;
        cFarRightTop = viewToWorldMatrix * cFarRightTop;
        cFarLeftBottom = viewToWorldMatrix * cFarLeftBottom;
        cFarRightBottom = viewToWorldMatrix * cFarRightBottom;
        
        return false;
    }

    public bool SimpleTerrainViewCulling()
    {
        if (ccDistance < cellSize * resolution / 2)
            return true;

        Vector3 cposition = mainCamera.transform.position;
        Vector3 clocalRotationEuler = mainCamera.transform.localRotation.eulerAngles;
        Quaternion yRotation = Quaternion.Euler(0, clocalRotationEuler.y, 0);

        Vector3 lbound = new Vector3(-cright, 0, cfar);
        lbound = yRotation * lbound;
        Vector3 rbound = new Vector3(cright, 0, cfar);
        rbound = yRotation * rbound;

        return 
            (
            PointInTriangle(cposition,lbPos,lbound,rbound)||
            PointInTriangle(cposition,rbPos,lbound,rbound)||
            PointInTriangle(cposition,ltPos,lbound,rbound)||
            PointInTriangle(cposition,rtPos,lbound,rbound)
            );
    }

    public bool PointInTriangle(Vector3 sp,Vector3 tp, Vector3 va,Vector3 vb)
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

    public bool PointInCenter(Vector3 sp,Vector3 tp,Vector3 va,Vector3 vb)
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
