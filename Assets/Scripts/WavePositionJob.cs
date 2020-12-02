using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

/// <summary>
/// 波のSpeedを元にして、波の変位uを更新するJob
/// </summary>
[BurstCompile]
public struct WavePositionJob : IJobParallelFor
{
    [ReadOnly] public float DeltaTime;
    [ReadOnly] public NativeArray<float> Speed; // 波の速さ (d/dt) u
    public NativeArray<float> Position; // 波の速さ (d/dt) u

    public void Execute(int index)
    {
        // 波の変位の更新
        Position[index] += Speed[index] * DeltaTime;
    }
}