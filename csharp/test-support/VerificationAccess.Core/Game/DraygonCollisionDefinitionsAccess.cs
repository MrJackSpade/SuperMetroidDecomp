using SuperMetroid.Core.Game;

/// <summary>Verification access to <see cref="DraygonCollisionDefinitions"/> members production does not use.</summary>
internal static class DraygonCollisionDefinitionsAccess
{
    /// <summary>Counts the empty-OAM frames across Draygon's native collision animation groups.</summary>
    extension(DraygonCollisionDefinitions)
    {
        /// <summary>Empty OAM frames across both mirrored Draygon body sequences.</summary>
        internal static int EmptyOamFrameCount => 2 * (6 + 4 + 7 + 7);
    }
}
