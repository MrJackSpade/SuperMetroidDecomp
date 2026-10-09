namespace SuperMetroid.Core.Game;

/// <summary>
/// Required host services used by translated enemy AI.
/// </summary>
/// <remarks>
/// <see cref="RoomEnemySystem.Load"/> keeps these delegates optional because focused enemy
/// fixtures do not need every global cartridge subsystem. Once an AI routine actually uses
/// one, however, absence is a wiring failure—not a false event bit, a zero RNG sample, or a
/// harmless no-op. Every translated consumer goes through these guards so a standalone test
/// and the desktop runtime fail at the exact missing dependency.
/// </remarks>
public sealed partial class RoomEnemySystem
{
    /// <summary>Gets the current cartridge random-number word through the host service.</summary>
    /// <returns>The word supplied by the installed random-number service.</returns>
    /// <exception cref="InvalidOperationException">No random-number service was installed by <see cref="Load"/>.</exception>
    internal ushort RequireRandomNumber() =>
        (_readRandomNumber ?? throw MissingService("read the current random-number word"))();

    /// <summary>Reads the current area-boss defeated flag.</summary>
    /// <returns><see langword="true"/> when the host reports that the area boss is defeated.</returns>
    /// <exception cref="InvalidOperationException">No area-boss read service was installed by <see cref="Load"/>.</exception>
    internal bool RequireAreaBossDefeated() =>
        (_isAreaBossDefeated ?? throw MissingService("read the area-boss flag"))();

    /// <summary>Sets the current area's boss-defeated flag through the host service.</summary>
    /// <exception cref="InvalidOperationException">No area-boss write service was installed by <see cref="Load"/>.</exception>
    internal void RequireSetAreaBossDefeated() =>
        (_setAreaBossDefeated ?? throw MissingService("set the area-boss flag"))();

    /// <summary>Reads the current area miniboss defeated flag.</summary>
    /// <returns><see langword="true"/> when the host reports that the area miniboss is defeated.</returns>
    /// <exception cref="InvalidOperationException">No miniboss read service was installed by <see cref="Load"/>.</exception>
    internal bool RequireAreaMiniBossDefeated() =>
        (_isAreaMiniBossDefeated ?? throw MissingService("read the area-miniboss flag"))();

    /// <summary>Sets the current area's miniboss-defeated flag through the host service.</summary>
    /// <exception cref="InvalidOperationException">No miniboss write service was installed by <see cref="Load"/>.</exception>
    internal void RequireSetAreaMiniBossDefeated() =>
        (_setAreaMiniBossDefeated ?? throw MissingService("set the area-miniboss flag"))();

    /// <summary>Queries a global event bit used by translated enemy behavior.</summary>
    /// <param name="eventNumber">Cartridge event identifier to query.</param>
    /// <returns><see langword="true"/> when the event bit is set.</returns>
    /// <exception cref="InvalidOperationException">No event-query service was installed by <see cref="Load"/>.</exception>
    internal bool RequireEvent(EventNumber eventNumber) =>
        (_hasEvent ?? throw MissingService("read global event bits"))(eventNumber);

    /// <summary>Sets a global event bit through the host service.</summary>
    /// <param name="eventNumber">Cartridge event identifier to set.</param>
    /// <exception cref="InvalidOperationException">No event-write service was installed by <see cref="Load"/>.</exception>
    internal void RequireSetEvent(EventNumber eventNumber) =>
        (_setEvent ?? throw MissingService("set a global event bit"))(eventNumber);

    /// <summary>Clears a global event bit through the host service.</summary>
    /// <param name="eventNumber">Cartridge event identifier to clear.</param>
    /// <exception cref="InvalidOperationException">No event-clear service was installed by <see cref="Load"/>.</exception>
    internal void RequireClearEvent(EventNumber eventNumber) =>
        (_clearEvent ?? throw MissingService("clear a global event bit"))(eventNumber);

    /// <summary>Reads the current area Torizo defeated flag.</summary>
    /// <returns><see langword="true"/> when the host reports that Torizo is defeated.</returns>
    /// <exception cref="InvalidOperationException">No Torizo read service was installed by <see cref="Load"/>.</exception>
    internal bool RequireAreaTorizoDefeated() =>
        (_isAreaTorizoDefeated ?? throw MissingService("read the area-Torizo flag"))();

    /// <summary>Sets the current area's Torizo-defeated flag through the host service.</summary>
    /// <exception cref="InvalidOperationException">No Torizo write service was installed by <see cref="Load"/>.</exception>
    internal void RequireSetAreaTorizoDefeated() =>
        (_setAreaTorizoDefeated ?? throw MissingService("set the area-Torizo flag"))();

    /// <summary>Checks whether a room PLM header is present in the active room population.</summary>
    /// <param name="headerPointer">Room PLM header pointer to look up.</param>
    /// <returns><see langword="true"/> when the host finds the requested header.</returns>
    /// <exception cref="InvalidOperationException">No active-room PLM query service was installed by <see cref="Load"/>.</exception>
    internal bool RequireRoomPlmPresent(ushort headerPointer) =>
        (_isRoomPlmPresent ?? throw MissingService("query the active room PLM population"))(
            headerPointer);

    /// <summary>Writes a room's scroll state through the installed host service.</summary>
    /// <param name="index">Room scroll-state slot to update.</param>
    /// <param name="state">Scroll behavior to store in that slot.</param>
    /// <exception cref="InvalidOperationException">No room-scroll write service was installed by <see cref="Load"/>.</exception>
    internal void RequireSetRoomScrollState(int index, RoomScrollState state) =>
        (_setRoomScrollState ?? throw MissingService("write a room scroll state"))(index, state);

    /// <summary>Reads a room scroll state through the Mother Brain host service.</summary>
    /// <param name="index">Room scroll-state slot to read.</param>
    /// <returns>The state stored for that room slot.</returns>
    /// <exception cref="InvalidOperationException">No room-scroll read service was installed by <see cref="Load"/>.</exception>
    internal RoomScrollState RequireReadRoomScrollState(int index) =>
        (_readMotherBrainRoomScrollState ?? throw MissingService("read a room scroll state"))(
            index);

    /// <summary>Changes whether player input controls Samus through the host service.</summary>
    /// <param name="enabled"><see langword="true"/> to enable controls; otherwise, disable them.</param>
    /// <exception cref="InvalidOperationException">No Samus-control service was installed by <see cref="Load"/>.</exception>
    internal void RequireSetSamusControlsEnabled(bool enabled) =>
        (_setSamusControlsEnabled ?? throw MissingService("change Samus control enablement"))(
            enabled);

    /// <summary>Sets Mother Brain's default layer-blending configuration.</summary>
    /// <param name="value">Configuration to install for subsequent room rendering.</param>
    /// <exception cref="InvalidOperationException">No layer-blending service was installed by <see cref="Load"/>.</exception>
    internal void RequireSetLayerBlendingDefaultConfig(LayerBlendingConfiguration value) =>
        (_setMotherBrainLayerBlendingDefaultConfig ??
            throw MissingService("write Mother Brain's layer-blending configuration"))(value);

    /// <summary>Writes Mother Brain's BG2 horizontal and vertical scroll positions.</summary>
    /// <param name="x">Horizontal BG2 scroll value.</param>
    /// <param name="y">Vertical BG2 scroll value.</param>
    /// <exception cref="InvalidOperationException">No BG2 scroll service was installed by <see cref="Load"/>.</exception>
    internal void RequireSetMotherBrainBg2Scroll(ushort x, ushort y) =>
        (_setMotherBrainBg2Scroll ??
            throw MissingService("write Mother Brain's BG2 scroll registers"))(x, y);

    /// <summary>Creates the wiring error raised when translated AI requests a host service that was not installed.</summary>
    /// <param name="operation">Description of the attempted cartridge-state operation.</param>
    /// <returns>An exception identifying the missing <see cref="Load"/> service.</returns>
    private static InvalidOperationException MissingService(string operation) => new(
        $"Room enemy AI attempted to {operation}, but RoomEnemySystem.Load did not install " +
        "the required cartridge-state service.");
}
