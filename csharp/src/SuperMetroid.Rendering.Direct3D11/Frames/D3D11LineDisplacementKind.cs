namespace SuperMetroid.Rendering.Direct3D11.Frames;

/// <summary>The described shape a layer pass displaces its lines by (#1468; Tiles.hlsl's Displacement constants).</summary>
internal enum D3D11LineDisplacementKind : uint
{
    /// <summary>No shape: every line keeps its scroll.</summary>
    None = 0,
    /// <summary>A sine wave pushes each line in its band sideways.</summary>
    Wave = 1,
    /// <summary>A stretch sags each line in its band downward, and the lines above keep their own scroll.</summary>
    Stretch = 2,
}
