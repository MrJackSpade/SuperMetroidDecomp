using SuperMetroid.Core.Rooms;
namespace SuperMetroid.Core.Game;

/// <summary>
/// The host services translated enemy AI uses, read from the <see cref="IRoomEnemyServices"/> that
/// <see cref="RoomEnemySystem.Load"/> installed. A host without a service supplies one that fails at
/// the exact dependency (standalone fixtures install only the services they model).
/// </summary>
public sealed partial class RoomEnemySystem
{
    internal ushort RequireRandomNumber() => EnemyRandom.Read();

    internal bool RequireAreaBossDefeated() => Services.Bosses.IsDefeated(BossBits.AreaBoss);

    internal void RequireSetAreaBossDefeated() => Services.Bosses.SetDefeated(BossBits.AreaBoss);

    internal bool RequireAreaMiniBossDefeated() => Services.Bosses.IsDefeated(BossBits.AreaMiniBoss);

    internal void RequireSetAreaMiniBossDefeated() => Services.Bosses.SetDefeated(BossBits.AreaMiniBoss);

    internal bool RequireEvent(EventNumber eventNumber) => Services.Events.Has(eventNumber);

    internal void RequireSetEvent(EventNumber eventNumber) => Services.Events.Set(eventNumber);

    internal void RequireClearEvent(EventNumber eventNumber) => Services.Events.Clear(eventNumber);

    internal bool RequireAreaTorizoDefeated() => Services.Bosses.IsDefeated(BossBits.AreaTorizo);

    internal void RequireSetAreaTorizoDefeated() => Services.Bosses.SetDefeated(BossBits.AreaTorizo);

    internal bool RequireRoomPlmPresent(PlmHeaderId headerPointer) => Services.Plms.HasActiveHeader(headerPointer);

    internal void RequireSetRoomScrollState(int index, RoomScrollState state) => Services.RoomScroll.Write(index, state);

    internal RoomScrollState RequireReadRoomScrollState(int index) => Services.RoomScroll.Read(index);

    internal void RequireSetSamusControlsEnabled(bool enabled) => Services.SamusControls.SetControlsEnabled(enabled);

    internal void RequireSetLayerBlendingDefaultConfig(LayerBlendingConfiguration value) =>
        Services.MotherBrain.SetLayerBlendingDefault(value);

    internal void RequireSetMotherBrainBg2Scroll(Position16<Bg2Pixels> scroll) => Services.MotherBrain.SetBg2Scroll(scroll);
}
