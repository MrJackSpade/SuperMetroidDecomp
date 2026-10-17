namespace SuperMetroid.Core.Enemies.SpacePirates;
using SuperMetroid.Core.Rooms.LevelData;

/// <summary>Verification access to <see cref="SpacePirateCollisionDefinitions"/> members production does not use.</summary>
internal static class SpacePirateCollisionDefinitionsAccess
{
    /// <summary>Indexes Space Pirate collision frames with their native pointers, and counts the hitbox lists.</summary>
    extension(SpacePirateCollisionDefinitions)
    {
        /// <summary>Number of compiled Space Pirate collision component frames.</summary>
        internal static int FrameCount => SpacePirateCollisionDefinitions.Frames.Count;

        /// <summary>Number of compiled Space Pirate hitbox lists.</summary>
        internal static int ListCount => SpacePirateCollisionDefinitions.HitboxLists.Length;

        /// <summary>Pairs a native frame pointer with its compiled component record.</summary>
        internal static SpacePirateCollisionFrame Frame(int index) =>
            new(SpacePirateCollisionDefinitions.Frames.PointerAt(index), SpacePirateCollisionDefinitions.Frames.RecordAt(index));
    }
}
