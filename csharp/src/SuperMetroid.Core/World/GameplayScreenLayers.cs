using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.World;

/// <summary>
/// The layers gameplay shows on the main screen (#1407): chosen on each main-loop pass, shown from the accepted frame
/// that latches them. A lag frame leaves the shown layers in place.
/// </summary>
internal sealed class GameplayScreenLayers
{
    /// <summary>The layers an ordinary room shows: both backgrounds and the sprites.</summary>
    private const SnesMainScreenLayers OrdinaryLayers = SnesMainScreenLayers.Bg1 | SnesMainScreenLayers.Bg2 | SnesMainScreenLayers.Obj;

    /// <summary>Supplies door-transition screen-layer overrides.</summary>
    private readonly ActiveRoomState _room;

    /// <summary>Binds the room whose door transitions override the layers.</summary>
    internal GameplayScreenLayers(ActiveRoomState room)
    {
        _room = room;
    }

    /// <summary>The layers the main loop chose for the next accepted frame.</summary>
    [field: NonSerialized]
    internal SnesMainScreenLayers Chosen { get; private set; } = OrdinaryLayers;

    /// <summary>The layers the most recent accepted frame shows.</summary>
    [field: NonSerialized]
    public SnesMainScreenLayers Displayed { get; private set; }

    /// <summary>
    /// Chooses the layers before projectile callbacks run: a door transition's own layers, BG1 and the sprites alone
    /// while Mother Brain's death hides BG2, and the waterfall rooms' layers when their blending is subtractive.
    /// </summary>
    internal void Choose(RoomScope room)
    {
        Chosen = _room.DoorTransitionMainScreenLayers ??
            (SnesMainScreenLayers.Bg1 | SnesMainScreenLayers.Obj |
                (room.Enemies.DisplaySources.MotherBrain?.DeathBg2Hidden == true
                    ? SnesMainScreenLayers.None
                    : SnesMainScreenLayers.Bg2));
        if (_room.DoorTransitionMainScreenLayers is null &&
            room.Layer3Fx.LayerBlendConfiguration == LayerBlendingConfiguration.WaterfallSubtractive)
        {
            Chosen = WaterfallRoomDisplayRules.MainScreen;
        }
    }

    /// <summary>The accepted frame shows the chosen layers.</summary>
    internal void Latch() => Displayed = Chosen;
}
