using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Gameplay;

/// <summary>
/// The gameplay window registers: their binding and per-frame preparation.
/// </summary>
internal sealed class GameplayWindowRegisters
{
    /// <summary>Supplies Mother Brain's BG2 visibility when preparing main-screen selection.</summary>
    private readonly RoomEnemySystem _enemies;
    /// <summary>Supplies door-transition screen-layer overrides.</summary>
    private readonly ActiveRoomState _room;
    /// <summary>Supplies waterfall blend configuration for window preparation.</summary>
    private readonly RoomLayer3FxState _roomLayer3Fx;

    /// <summary>Binds room, enemy and layer-three owners to the window-register coordinator.</summary>
    internal GameplayWindowRegisters(RoomEnemySystem enemies, ActiveRoomState room, RoomLayer3FxState roomLayer3Fx)
    {
        _enemies = enemies;
        _room = room;
        _roomLayer3Fx = roomLayer3Fx;
    }

    /// <summary>
    /// Window and screen-selection bytes uploaded by the most recent accepted NMI.
    /// A lag NMI deliberately leaves this snapshot unchanged.
    /// </summary>
    public GameplayWindowRegisterSnapshot DisplayedGameplayWindowRegisters =>
        Cache.Displayed;

    /// <summary>Gets the nonserialized hardware cache, restoring its initial screen selection on first access.</summary>
    [field: NonSerialized]
    internal GameplayWindowRegisterCache Cache
    {
        get
        {
            if (field is not null)
                return field;

            field = new GameplayWindowRegisterCache();
            field.InitializeWindowAndScreenSelection();
            return field;
        }
    }

    /// <summary>
    /// Reattaches nonserialized routing after construction or debugger-state restoration.
    /// The cache itself is presentation hardware and is republished from a clean layer-
    /// blending baseline on the next main-loop pass.
    /// </summary>
    internal void BindGameplayWindowRegisters(SamusProjectileSystem projectiles) =>
        projectiles.Chainsaw.BindGameplayWindowRegisters(Cache);

    /// <summary>
    /// Models the window/screen subset of the ordinary layer-blending initializer before
    /// projectile callbacks run. Edge and logic bytes intentionally survive; native
    /// <c>$88:8075</c> clears selection/admission and assigns TM/TS, but does not clear them.
    /// </summary>
    internal void PrepareGameplayWindowRegisters()
    {
        GameplayWindowRegisterCache registers = Cache;
        registers.InitializeWindowAndScreenSelection();

        // Door and Mother Brain presentation already translate the non-default TM owners.
        // Seed the same shared byte before a Chainsaw slot-four word store can corrupt it.
        SnesMainScreenLayers mainScreen = _room.DoorTransitionMainScreenLayers ??
            (SnesMainScreenLayers.Bg1 | SnesMainScreenLayers.Obj |
                (_enemies.DisplaySources.MotherBrain?.DeathBg2Hidden == true
                    ? SnesMainScreenLayers.None
                    : SnesMainScreenLayers.Bg2));
        registers.MainScreen = mainScreen;
        if (_room.DoorTransitionMainScreenLayers is null &&
            _roomLayer3Fx.LayerBlendConfiguration == LayerBlendingConfiguration.WaterfallSubtractive)
        {
            registers.MainScreen = WaterfallRoomDisplayRules.MainScreen;
            registers.Subscreen = WaterfallRoomDisplayRules.Subscreen;
        }
    }
}
