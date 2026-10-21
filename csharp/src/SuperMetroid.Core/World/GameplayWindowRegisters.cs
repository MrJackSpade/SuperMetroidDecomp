using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.World;

/// <summary>
/// The gameplay window registers: their binding and per-frame preparation.
/// </summary>
internal sealed class GameplayWindowRegisters
{
    /// <summary>Supplies door-transition screen-layer overrides.</summary>
    private readonly ActiveRoomState _room;

    /// <summary>Binds room and enemy owners to the window-register coordinator.</summary>
    internal GameplayWindowRegisters(ActiveRoomState room)
    {
        _room = room;
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
    internal void PrepareGameplayWindowRegisters(RoomScope room)
    {
        GameplayWindowRegisterCache registers = Cache;
        registers.InitializeWindowAndScreenSelection();

        // Door and Mother Brain presentation already translate the non-default TM owners.
        // Seed the same shared byte before a Chainsaw slot-four word store can corrupt it.
        SnesMainScreenLayers mainScreen = _room.DoorTransitionMainScreenLayers ??
            (SnesMainScreenLayers.Bg1 | SnesMainScreenLayers.Obj |
                (room.Enemies.DisplaySources.MotherBrain?.DeathBg2Hidden == true
                    ? SnesMainScreenLayers.None
                    : SnesMainScreenLayers.Bg2));
        registers.MainScreen = mainScreen;
        if (_room.DoorTransitionMainScreenLayers is null &&
            room.Layer3Fx.LayerBlendConfiguration == LayerBlendingConfiguration.WaterfallSubtractive)
        {
            registers.MainScreen = WaterfallRoomDisplayRules.MainScreen;
            registers.Subscreen = WaterfallRoomDisplayRules.Subscreen;
        }
    }
}
