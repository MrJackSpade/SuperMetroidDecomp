namespace SuperMetroid.Core.Enemies.SpacePirates;
using SuperMetroid.Core.Rooms.LevelData;

/// <summary>Verification access to <see cref="SpacePirateCollisionDefinitions"/> members production does not use.</summary>
internal static class SpacePirateCollisionDefinitionsAccess
{
    /// <summary>Indexes Space Pirate collision frames and hitbox lists with their native pointers.</summary>
    extension(SpacePirateCollisionDefinitions)
    {
        /// <summary>Number of compiled Space Pirate collision component frames.</summary>
        internal static int FrameCount => SpacePirateCollisionDefinitions.Frames.Count;

        /// <summary>Number of compiled Space Pirate hitbox lists.</summary>
        internal static int ListCount => SpacePirateCollisionDefinitions.Lists.Count;

        /// <summary>Pairs a native frame pointer with its compiled component record.</summary>
        internal static SpacePirateCollisionFrame Frame(int index) =>
            new(SpacePirateCollisionDefinitions.Frames.PointerAt(index), SpacePirateCollisionDefinitions.Frames.RecordAt(index));

        /// <summary>Pairs a native list pointer with its compiled hitbox record.</summary>
        internal static SpacePirateCollisionList List(int index) =>
            new(SpacePirateCollisionDefinitions.Lists.PointerAt(index), SpacePirateCollisionDefinitions.Lists.RecordAt(index));
    }
}
