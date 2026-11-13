using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Verification.Samus.Movement;

/// <summary>
/// A stand-in enemy for Samus's solid-enemy collision fixtures (#1697): an enemy her contact records hold by reference
/// (#1702). Only a room's enemy pass shakes an enemy, and the fixtures that check the shake use a room's own enemy places,
/// so shaking a stand-in is a fixture error.
/// </summary>
internal sealed class SolidEnemyFixture : ISolidEnemy
{
    /// <inheritdoc/>
    public void ShakeFromWallJumpLaunch() =>
        throw new InvalidOperationException("A stand-in solid enemy was shaken; a shake check needs a room's own enemy place.");
}
