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
    internal ushort RequireRandomNumber() =>
        (_readRandomNumber ?? throw MissingService("read the current random-number word"))();

    internal bool RequireAreaBossDefeated() =>
        (_isAreaBossDefeated ?? throw MissingService("read the area-boss flag"))();

    internal void RequireSetAreaBossDefeated() =>
        (_setAreaBossDefeated ?? throw MissingService("set the area-boss flag"))();

    internal bool RequireAreaMiniBossDefeated() =>
        (_isAreaMiniBossDefeated ?? throw MissingService("read the area-miniboss flag"))();

    internal void RequireSetAreaMiniBossDefeated() =>
        (_setAreaMiniBossDefeated ?? throw MissingService("set the area-miniboss flag"))();

    internal bool RequireEvent(int eventNumber) =>
        (_hasEvent ?? throw MissingService("read global event bits"))(eventNumber);

    internal void RequireSetEvent(int eventNumber) =>
        (_setEvent ?? throw MissingService("set a global event bit"))(eventNumber);

    internal void RequireClearEvent(int eventNumber) =>
        (_clearEvent ?? throw MissingService("clear a global event bit"))(eventNumber);

    internal bool RequireAreaTorizoDefeated() =>
        (_isAreaTorizoDefeated ?? throw MissingService("read the area-Torizo flag"))();

    internal void RequireSetAreaTorizoDefeated() =>
        (_setAreaTorizoDefeated ?? throw MissingService("set the area-Torizo flag"))();

    internal bool RequireRoomPlmPresent(ushort headerPointer) =>
        (_isRoomPlmPresent ?? throw MissingService("query the active room PLM population"))(
            headerPointer);

    internal void RequireSetRoomScrollByte(int index, byte value) =>
        (_setRoomScrollByte ?? throw MissingService("write a room scroll byte"))(index, value);

    internal byte RequireReadRoomScrollByte(int index) =>
        (_readMotherBrainRoomScrollByte ?? throw MissingService("read a room scroll byte"))(
            index);

    internal void RequireSetSamusControlsEnabled(bool enabled) =>
        (_setSamusControlsEnabled ?? throw MissingService("change Samus control enablement"))(
            enabled);

    internal void RequireSetLayerBlendingDefaultConfig(ushort value) =>
        (_setMotherBrainLayerBlendingDefaultConfig ??
            throw MissingService("write Mother Brain's layer-blending configuration"))(value);

    internal void RequireSetMotherBrainBg2Scroll(ushort x, ushort y) =>
        (_setMotherBrainBg2Scroll ??
            throw MissingService("write Mother Brain's BG2 scroll registers"))(x, y);

    private static InvalidOperationException MissingService(string operation) => new(
        $"Room enemy AI attempted to {operation}, but RoomEnemySystem.Load did not install " +
        "the required cartridge-state service.");
}
