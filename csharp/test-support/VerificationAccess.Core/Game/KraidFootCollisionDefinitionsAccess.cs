using SuperMetroid.Core.Game;

/// <summary>Verification access to <see cref="KraidFootCollisionDefinitions"/> members production does not use.</summary>
internal static class KraidFootCollisionDefinitionsAccess
{
    /// <summary>Calculates Kraid foot collision frame roots using the compiled frame width.</summary>
    extension(KraidFootCollisionDefinitions)
    {
        /// <summary>Computes a foot collision frame root from the fixed native record stride.</summary>
        internal static ushort FramePointer(int index) =>
            checked((ushort)(KraidFootCollisionDefinitions.FirstFrame + index * KraidFootCollisionDefinitions.FrameByteCount));
    }
}
