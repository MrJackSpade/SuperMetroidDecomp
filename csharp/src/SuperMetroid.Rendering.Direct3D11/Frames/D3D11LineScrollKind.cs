namespace SuperMetroid.Rendering.Direct3D11.Frames;

/// <summary>The described shape a line-scroll slot holds (#1468; Tiles.hlsl's LineScroll constants).</summary>
internal enum D3D11LineScrollKind : uint
{
    /// <summary>One scroll on every line, or a list of bands of scroll down the screen (<c>ScrollBands</c>).</summary>
    Bands = 0,
    /// <summary>A sine wave moves each line of its band (<c>ScrollWave</c>).</summary>
    Wave = 1,
    /// <summary>A stretch sags each line of its band, and the lines above keep their own scroll (<c>ScrollStretch</c>).</summary>
    Stretch = 2,
    /// <summary>A waveform of samples moves each line of its band (<c>SampledScrollWave</c>).</summary>
    Sampled = 3,
}
