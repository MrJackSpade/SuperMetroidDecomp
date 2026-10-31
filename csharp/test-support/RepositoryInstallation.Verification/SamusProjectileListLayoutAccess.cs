using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>Verification access to <see cref="SamusProjectileListLayout"/> reads extraction does not make.</summary>
internal static class SamusProjectileListLayoutAccess
{
    /// <summary>The native instruction pointers checks compare cursors with.</summary>
    extension(SamusProjectileListLayout self)
    {
        /// <summary>Each step's instruction pointer, by program.</summary>
        internal IReadOnlyDictionary<SamusProjectileProgram, ushort[]> StepPositions => self.stepPositions;

        /// <summary>The instruction pointer of a projectile's cursor; a cleared projectile's is zero.</summary>
        internal ushort Position(AnimationProgramCursor<SamusProjectileProgram>? cursor) => cursor is { } live ? self.Position(live) : (ushort)0;

        /// <summary>The cursor instruction pointer <paramref name="place"/> stands for; zero is a cleared projectile.</summary>
        /// <exception cref="InvalidDataException">The pointer is no list position.</exception>
        internal AnimationProgramCursor<SamusProjectileProgram>? CursorAt(int place) =>
            place == 0 ? null : self.TryCursorAt(place, out AnimationProgramCursor<SamusProjectileProgram> cursor) ? cursor
                : throw new InvalidDataException($"$93:{place:X4} is not a Samus projectile list position.");
    }
}
