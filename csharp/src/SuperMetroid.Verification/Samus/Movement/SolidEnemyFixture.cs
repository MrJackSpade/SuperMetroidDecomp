using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Verification.Samus.Movement;

/// <summary>
/// A stand-in enemy for Samus's solid-enemy collision fixtures (#1697): a place in the room's enemy table, which Samus's
/// contact records name. Only a room's enemy pass shakes an enemy, and the fixtures that check the shake use a room's
/// own enemy places, so shaking a stand-in is a fixture error.
/// </summary>
/// <param name="slotIndex">The place in the enemy table the enemy stands in for.</param>
internal sealed class SolidEnemyFixture(int slotIndex) : ISolidEnemy
{
    /// <inheritdoc/>
    public int ContactRecordPlace { get; } = slotIndex;

    /// <inheritdoc/>
    public void ShakeFromWallJumpLaunch() =>
        throw new InvalidOperationException($"Stand-in solid enemy {ContactRecordPlace} was shaken; a shake check needs a room's own enemy place.");
}
