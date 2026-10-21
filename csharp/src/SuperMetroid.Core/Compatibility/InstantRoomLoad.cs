namespace SuperMetroid.Core.Compatibility;

/// <summary>The game's own timing: a room loads completely within the first door-scroll update.</summary>
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
