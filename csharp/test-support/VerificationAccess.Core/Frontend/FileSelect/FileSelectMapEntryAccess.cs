using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>Verification access to <see cref="FileSelectMapEntry"/> members production does not use.</summary>
internal static class FileSelectMapEntryAccess
{
    /// <summary>Adds the verifier's empty subscreen to a file-select map render.</summary>
    extension(FileSelectMapEntry self)
    {
        /// <summary>BG1/OBJ/subscreen are visible inside window one; the cleared BG2 covers its exterior.</summary>
        internal Rgba32[] Render(ReadOnlySpan<Rgba32> areaScene) => self.Render(areaScene, new Rgba32[areaScene.Length]);
    }
}
