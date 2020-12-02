using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

/// <summary>
/// 波の加速度・速度を計算するJob
/// </summary>
[BurstCompile]
public struct WaveSpeedJob : IJobParallelFor
{
    [ReadOnly] public WaveParameter Parameter;
    [ReadOnly] public NativeArray<float> WaveArray; // 波の変位u
    public NativeArray<float> Accel; // 波の加速度　(d/dt)^2 u
    public NativeArray<float> Speed; // 波の速さ (d/dt) u
    public float DeltaTime;
    
    public void Execute(int index)
    {
        int xi = index % Parameter.NumX;
        int yi = index / Parameter.NumX;
        
        // 端点の場合は何もしない
        if (xi == 0 || xi == Parameter.NumX - 1) return;
        if (yi == 0 || yi == Parameter.NumY - 1) return;

        float wave = GetWave(xi, yi);
        float waveX1 = GetWave(xi - 1, yi);
        float waveX2 = GetWave(xi + 1, yi);
        float waveY1 = GetWave(xi, yi - 1);
        float waveY2 = GetWave(xi, yi + 1);
        
        float d2ux = (waveX1 + waveX2 - 2f * wave);
        float d2uy = (waveY1 + waveY2 - 2f * wave);

        float dvdx = (Parameter.V / Parameter.DeltaX);
        Accel[index] = dvdx * dvdx * (d2ux + d2uy) * DeltaTime;
        Speed[index] += Accel[index] * DeltaTime;
    }

    float GetWave(int xi, int yi)
    {
        return WaveArray[xi + yi * Parameter.NumX];
    }
}