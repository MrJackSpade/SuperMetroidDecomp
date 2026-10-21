using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Drawing;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>Verification's native view of <see cref="SpacePirateCollisionDefinitions"/>: frames by bank-$B2 address.</summary>
internal static class SpacePirateCollisionDefinitionsAccess
{
    /// <summary>Native views of the catalog.</summary>
    extension(SpacePirateCollisionDefinitions)
    {
        /// <summary>Number of compiled Space Pirate collision component frames.</summary>
        internal static int FrameCount => SpacePirateCollisionDefinitions.Frames.Count();

        /// <summary>Number of compiled Space Pirate hitbox lists.</summary>
        internal static int ListCount => SpacePirateCollisionDefinitions.HitboxLists.Length;

        /// <summary>The <paramref name="index"/>th compiled frame, in native order, with its native address.</summary>
        internal static SpacePirateCollisionFrame Frame(int index)
        {
            KeyValuePair<EnemyFrameId, SpacePirateCollisionComponent[]> frame = SpacePirateCollisionDefinitions.Frames.ElementAt(index);
            return new(EnemyFrameCartridgeLookupTooling.Word(frame.Key), frame.Value);
        }

        /// <summary>The components of the frame at native address <paramref name="pointer"/>.</summary>
        internal static ReadOnlySpan<SpacePirateCollisionComponent> ComponentsAt(ushort pointer) =>
            pointer == CommonEnemyEmptyExtendedFrameToolingDefinitionsConstants.Frame || RoomEnemySlotFrameAccess.Shows(SpacePiratesBank, pointer)
                ? SpacePirateCollisionDefinitions.Frames.Of(RoomEnemySlotFrameAccess.FrameAt(SpacePiratesBank, pointer))
                : throw new InvalidDataException($"Space Pirate collision frame $B2:{pointer:X4} is not compiled.");
    }

    /// <summary>$B2, the bank holding the Pirates' frames.</summary>
    private const byte SpacePiratesBank = 0xb2;
}
