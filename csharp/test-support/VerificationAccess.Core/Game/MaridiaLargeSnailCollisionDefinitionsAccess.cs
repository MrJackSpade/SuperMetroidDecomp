using SuperMetroid.Core.Game;

/// <summary>Verification access to <see cref="MaridiaLargeSnailCollisionDefinitions"/> members production does not use.</summary>
internal static class MaridiaLargeSnailCollisionDefinitionsAccess
{
    /// <summary>Enumerates the native roots of Maridia large-snail hitbox lists.</summary>
    extension(MaridiaLargeSnailCollisionDefinitions)
    {
        /// <summary>Native roots for the Oum collision hitbox lists.</summary>
        internal static IEnumerable<ushort> HitboxPointers => MaridiaLargeSnailCollisionDefinitions.Lists.Pointers;
    }
}
