using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class WaveJobSystem : MonoBehaviour
{
    [SerializeField] private WaveParameter waveParameter;
    [SerializeField] private Material material = null;
    [SerializeField] private int simluationSubStep = 1; // 波の速さVを大きく設定するなどして、波が発散する場合は、この値を増やしてください
    private WaveMesh2D_Job waveMesh = null;
    private NativeArray<float> accelArray;
    private NativeArray<float> speedArray;
    private NativeArray<float> waveArray;
    
    /// <summary>
    /// 開始時に実行
    /// </summary>
    void Start()
    {
        // メッシュの追加
        waveMesh = gameObject.AddComponent<WaveMesh2D_Job>();
        
        // Native Arrayのメモリ割り当て
        int arrayLength = waveParameter.NumX * waveParameter.NumY;
        accelArray = new NativeArray<float>(arrayLength, Allocator.Persistent);
        speedArray = new NativeArray<float>(arrayLength, Allocator.Persistent);
        waveArray = new NativeArray<float>(arrayLength, Allocator.Persistent);
        
        // パラメータ設定
        waveParameter.DeltaX = waveParameter.MeshSize.x / waveParameter.NumX;
        waveParameter.DeltaY = waveParameter.MeshSize.y / waveParameter.NumY;
        waveParameter.V /= simluationSubStep;
        
        // 波の初期状態の設定
        InitializeWave();
        
        // Mesh作成
        waveMesh.Initialize(waveParameter, waveArray, material);
    }
    
    
    private void FixedUpdate()
    {
        for (int step = 0; step < simluationSubStep; step++)
        {
            RunJob();
        }
        waveMesh.UpdateMesh();
    }

    /// <summary>
    /// ジョブの実行
    /// </summary>
    private void RunJob()
    {
        float deltaTime = Time.fixedDeltaTime;
        var speedJob = new WaveSpeedJob
        {
            Accel = accelArray,
            Speed = speedArray,
            WaveArray = waveArray,
            Parameter = waveParameter,
            DeltaTime = deltaTime,
        };

        var speedHandle = speedJob.Schedule(speedArray.Length, 1);
        var positionJob = new WavePositionJob
        {
            Speed = speedArray,
            Position = waveArray,
            DeltaTime = deltaTime,
        };

        var positionHandle = positionJob.Schedule(speedArray.Length, 1, speedHandle);
        positionHandle.Complete();
    }

    /// <summary>
    /// 波の初期化
    /// </summary>
    private void InitializeWave()
    {
        Vector2 center = new Vector2(1f, 1f);
        int i = 0;
        for (int yi = 0; yi < waveParameter.NumY; yi++)
        {
            for (int xi = 0; xi < waveParameter.NumX; xi++)
            {
                var p = GetVertexPosition(xi, yi);
                float r = (new Vector2(p.x, p.z) - center).magnitude;
                float h = Mathf.Exp(-r * 8.0f) * 2.0f;
                h = Mathf.Clamp01(h);
                waveArray[i++] = h;
            }
        }
    }
    
    /// <summary>
    /// 頂点座標取得
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private Vector3 GetVertexPosition(int x, int y)
    {
        return new Vector3(
            (float) x / waveParameter.NumX * waveParameter.MeshSize.x - waveParameter.MeshSize.x / 2f,
            GetWave(x, y),
            (float) y / waveParameter.NumY * waveParameter.MeshSize.y - waveParameter.MeshSize.y / 2f
        );
    }

    /// <summary>
    /// 波の取得
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private float GetWave(int x, int y)
    {
        return waveArray[x + y * waveParameter.NumX];
    }
    
    /// <summary>
    /// NativeArrayの解放 (確保したNativeArrayは自分で開放する必要がある)
    /// </summary>
    private void OnDestroy()
    {
        waveArray.Dispose();
        speedArray.Dispose();
        accelArray.Dispose();
    }
}