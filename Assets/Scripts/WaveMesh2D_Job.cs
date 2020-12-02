using System.Linq;
using Unity.Collections;
using UnityEngine;

/// <summary>
/// 波のメッシュを管理するクラス(C# JobSystemから動かす)
/// </summary>
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class WaveMesh2D_Job : MonoBehaviour
{
    [SerializeField] private WaveParameter parameter;
    private WaveJobSystem jobSystem;
    private Vector2Int resolution; // メッシュ解像度
    private Vector3[] vertices = null; // メッシュ頂点
    private Vector3[] normals = null; // メッシュ法線
    private Mesh mesh = null;
    private NativeArray<float> waveArray;

    /// <summary>
    /// 初期化
    /// </summary>
    public void Initialize(WaveParameter parameter, NativeArray<float> waveArray, Material material)
    {
        this.parameter = parameter;
        this.waveArray = waveArray;
        resolution = new Vector2Int(parameter.NumX, parameter.NumY);
        CreateMesh(material);
    }

    /// <summary>
    /// メッシュ作成
    /// </summary>
    private void CreateMesh(Material material)
    {
        mesh = new Mesh();

        // 頂点の作成
        int vertexCount = resolution.x * resolution.y;

        // 頂点・法線・UV作成
        vertices = new Vector3[vertexCount];
        normals = new Vector3[vertexCount].Select(x => new Vector3(0, 1, 0)).ToArray();
        
        var uv = new Vector2[vertexCount];
        int vi = 0;
        for (int yi = 0; yi < resolution.y; yi++)
        {
            for (int xi = 0; xi < resolution.x; xi++)
            {
                vertices[vi] = CalcVertexPosition(xi, yi);
                uv[vi] = new Vector2((float)xi / (resolution.x - 1), (float)yi / (resolution.y - 1));
                vi++;
            }
        }

        // 頂点インデックス作成
        int triangleCount = (resolution.x - 1) * (resolution.y - 1) * 6;
        int[] triangles = new int[triangleCount];
        int offset = 0;
        int ti = 0;
        for (int yi = 0; yi < resolution.y - 1; yi++)
        {
            for (int xi = 0; xi < resolution.x - 1; xi++)
            {
                triangles[ti++] = offset;
                triangles[ti++] = offset + resolution.x;
                triangles[ti++] = offset + 1;
                triangles[ti++] = offset + resolution.x;
                triangles[ti++] = offset + resolution.x + 1;
                triangles[ti++] = offset + 1;

                offset += 1;
            }

            offset += 1;
        }

        mesh.SetVertices(vertices);
        mesh.uv = uv;
        mesh.SetTriangles(triangles, 0);
        
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().material = material;
    }

    /// <summary>
    /// メッシュ更新
    /// </summary>
    public void UpdateMesh()
    {
        float dx = parameter.MeshSize.x / resolution.x;
        float dy = parameter.MeshSize.y / resolution.y;
        int vi = 0;
        for (int yi = 0; yi < resolution.y; yi++)
        {
            for (int xi = 0; xi < resolution.x; xi++)
            {
                vertices[vi] = CalcVertexPosition(xi, yi);
                vi++;
            }
        }

        for (int yi = 1; yi < resolution.y - 1; yi++)
        {
            for (int xi = 1; xi < resolution.x - 1; xi++)
            {
                // 法線の計算
                float dudx = (GetWaveHeight(xi + 1, yi) - GetWaveHeight(xi - 1, yi)) / parameter.DeltaX;
                float dudy = (GetWaveHeight(xi, yi) - GetWaveHeight(xi - 1, yi)) / parameter.DeltaY;
                normals[xi + yi * resolution.x] = new Vector3(-dudx, 1.0f, -dudy).normalized;
            }
        }

        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
    }

    /// <summary>
    /// 頂点座標の計算
    /// </summary>
    private Vector3 CalcVertexPosition(int x, int y)
    {
        return new Vector3(
            (float) x / resolution.x * parameter.MeshSize.x - parameter.MeshSize.x / 2f,
            GetWaveHeight(x, y),
            (float) y / resolution.y * parameter.MeshSize.y - parameter.MeshSize.y / 2f
        );
    }
    
    /// <summary>
    /// 波の高さ 取得
    /// </summary>
    private float GetWaveHeight(int x, int y)
    {
        return waveArray[x + y * resolution.x];
    }
}