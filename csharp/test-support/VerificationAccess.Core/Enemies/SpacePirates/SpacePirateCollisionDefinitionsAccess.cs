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
        /// <summary>Number of compiled Space Pirate collision frames.</summary>
        internal static int FrameCount => SpacePirateCollisionDefinitions.Frames.Count();

        /// <summary>The <paramref name="index"/>th compiled frame, in native order, with its native address.</summary>
        internal static SpacePirateCollisionFrame Frame(int index)
        {
            KeyValuePair<EnemyFrameId, EnemyCollisionHitbox[]> frame = SpacePirateCollisionDefinitions.Frames.ElementAt(index);
            return new(EnemyFrameCartridgeLookupTooling.FrameHitboxWord(frame.Key), frame.Value);
        }

        /// <summary>The hitboxes of the frame at native address <paramref name="pointer"/>.</summary>
        internal static ReadOnlySpan<EnemyCollisionHitbox> HitboxesAt(ushort pointer) =>
            pointer == CommonEnemyEmptyFrameToolingDefinitionsConstants.Frame || RoomEnemyPlaceFrameAccess.Shows(SpacePiratesBank, pointer)
                ? SpacePirateCollisionDefinitions.Frames.Of(RoomEnemyPlaceFrameAccess.FrameAt(SpacePiratesBank, pointer))
                : throw new InvalidDataException($"Space Pirate collision frame $B2:{pointer:X4} is not compiled.");
    }

    /// <summary>$B2, the bank holding the Pirates' frames.</summary>
    private const byte SpacePiratesBank = 0xb2;
}
