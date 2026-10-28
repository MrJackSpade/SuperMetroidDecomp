using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Core.Hardware;

/// <summary>Verification access to <see cref="SnesCgram"/> members production does not use.</summary>
internal static class SnesCgramAccess
{
    /// <summary>Provides a fixture reset for all entries in CGRAM.</summary>
    extension(SnesCgram self)
    {
        /// <summary>Clears all 256 colors to black.</summary>
        internal void Clear()
        {
            for (int index = 0; index < SnesCgram.ColorCount; index++)
                self.SetColor(index, default);
        }
    }
}
