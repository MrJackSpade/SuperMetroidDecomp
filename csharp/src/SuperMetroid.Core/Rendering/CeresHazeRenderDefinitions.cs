namespace SuperMetroid.Core.Rendering;

/// <summary>Expanded scanline layout of the bank-$88 FX type $2C fixed-color gradient.</summary>
internal static class CeresHazeRenderDefinitions
{
    /// <summary>Physical line 64 starts the increasing color bands after the initial flat region.</summary>
    internal const int RampFirstLine = 64;
    /// <summary>$88:DF03 begins with 64 scanlines reading table byte zero: component zero when fully faded in.</summary>
    internal const int InitialComponent = 0;
    /// <summary>The next eight scanlines read table byte one: component one when fully faded in.</summary>
    internal const int RampFirstComponent = 1;
    /// <summary>Each subsequent HDMA color band spans eight physical scanlines.</summary>
    internal const int BandHeight = 8;
    /// <summary>$88:DE2D's final table write uses counter fifteen; the following call only changes pre-instruction.</summary>
    internal const int MaximumComponent = 15;
}
