using System;
using UnityEngine;

/// <summary>
/// 波のパラメータ
/// </summary>
[Serializable]
public struct WaveParameter
{
    public int NumX; // グリッドの数(X)
    public int NumY; // グリッドの数(Y)
    public float V; // 波が伝わる速さ
    public Vector2 MeshSize; // メッシュの大きさ
    [System.NonSerialized] public float DeltaX; // グリッド間の距離(X方向)
    [System.NonSerialized] public float DeltaY; // グリッド間の距離(Y方向)
}