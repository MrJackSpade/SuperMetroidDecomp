namespace SuperMetroid.Core.Compatibility;

/// <summary>The game's own timing: a room loads completely within the first door-scroll update.</summary>
/// <remarks>
/// Consulted during each door-scroll update of a transition. Evidence: the original door loader initializes the room's
/// enemies while the door keeps scrolling only for as long as loading takes on the original hardware, and the scroll runs
/// on while it does; the game eliminates that wait, so every enemy is initialized from the first scroll update, which the
/// door alignment, elevator and first-entry checks step every transition with, and a replay supplies the progress its
/// recording shows through its own <see cref="IRoomLoadProgress"/>.
/// </remarks>
[CompatibilityService("a room loads completely within the first door-scroll update")]
public sealed class InstantRoomLoad : IRoomLoadProgress
{
    /// <summary>The shared, stateless instance every session uses.</summary>
    public static InstantRoomLoad Instance { get; } = new();

    /// <summary>Restricts the policy to its shared instance.</summary>
    private InstantRoomLoad() { }

    /// <summary>Reports every enemy initialized from the first scroll update on.</summary>
    /// <param name="slot">The enemy's slot; every slot is initialized at once.</param>
    /// <returns>Always true.</returns>
    public bool HasInitializedEnemySlot(int slot) => true;
}
