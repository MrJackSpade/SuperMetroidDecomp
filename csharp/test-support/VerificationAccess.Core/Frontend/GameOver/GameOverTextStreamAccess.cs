using SuperMetroid.AssetExtraction.Frontend;

namespace SuperMetroid.AssetExtraction.Frontend.GameOver;

/// <summary>Verification's view of the import pipeline's game-over text pointers (<see cref="GameOverTextSources"/>).</summary>
internal static class GameOverTextStreamAccess
{
    /// <summary>A stream's bank-$81 source pointer.</summary>
    extension(GameOverTextStream self)
    {
        /// <summary>The bank-$81 pointer of this stream's tile words and text commands.</summary>
        internal ushort SourcePointer => GameOverTextSources.Pointer(self.Element);
    }
}
