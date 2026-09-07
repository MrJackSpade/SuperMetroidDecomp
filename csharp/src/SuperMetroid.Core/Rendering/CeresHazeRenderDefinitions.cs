namespace SuperMetroid.Core.Rendering;

/// <summary>Expanded scanline layout of the bank-$88 FX type $2C fixed-color gradient.</summary>
internal static class CeresHazeRenderDefinitions
{
    /// <summary>Physical line 64 starts the increasing color bands after the initial flat region.</summary>
    internal const int RampFirstLine = 64;
    /// <summary>The initial visible band supplies five-bit component one.</summary>
    internal const int InitialComponent = 1;
    /// <summary>The first increasing band supplies five-bit component two.</summary>
    internal const int RampFirstComponent = 2;
    /// <summary>Each subsequent HDMA color band spans eight physical scanlines.</summary>
    internal const int BandHeight = 8;
    /// <summary>The gradient saturates at five-bit component sixteen.</summary>
    internal const int MaximumComponent = 16;
}
