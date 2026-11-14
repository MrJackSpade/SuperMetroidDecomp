using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The Speed Booster escape: its setup and what its lavaquake does to the lava, the earthquake and the events.</summary>
[RoomLifetime]
internal sealed class SpeedBoosterEscapeObjects
{
    /// <summary>Samus as the room reads and changes her.</summary>
    private readonly IRoomSamusAccess? _samus;
    /// <summary>The game's event flags.</summary>
    private readonly IGlobalEventFlags? _events;
    /// <summary>The room's effect overlay FX.</summary>
    private readonly IRoomFxMotion? _roomFx;
    /// <summary>The room's earthquake.</summary>
    private readonly IRoomEarthquakeWriter? _earthquake;

    /// <summary>Binds the escape to its room-level state owners.</summary>
    internal SpeedBoosterEscapeObjects(IRoomSamusAccess? samus, IGlobalEventFlags? events, IRoomFxMotion? roomFx, IRoomEarthquakeWriter? earthquake)
    {
        _samus = samus;
        _events = events;
        _roomFx = roomFx;
        _earthquake = earthquake;
    }

    /// <summary>
    /// Sets up the Speed Booster escape. Once the lavaquake has been outrun the escape is gone on later room loads;
    /// otherwise its lavaquake starts, before its first lava rise.
    /// </summary>
    internal void SetupSpeedBoosterEscapeSlot(RoomObjectSlot slot)
    {
        Ensure.NotNull(slot);
        if (_events is null || _roomFx is null || _earthquake is null)
        {
            throw new InvalidOperationException(
                "Speed Booster escape room object requires event, room-FX, and earthquake owners.");
        }

        if (_events.Has(StoryEvent.OutranSpeedBoosterLavaquake))
            slot.Delete();
        else
            slot.Become(new SpeedBoosterEscapeObject(this));
    }

    /// <summary>
    /// Starts the lavaquake when Samus has the Speed Booster and the lava has a target, and returns true; otherwise
    /// returns false, having stopped the lava and the earthquake when she lacks it.
    /// </summary>
    internal bool TryStartLavaquake()
    {
        IRoomFxMotion fx = RoomFx();
        if (!LiveSamus().CollectedItems.HasAny(SamusEquipmentFlags.SpeedBooster))
        {
            fx.RemoveLiquidTarget();
            fx.SetLiquidMotion(
                levelSpeed: SignedEightEight.Zero,
                moveDelay: 0);
            (_earthquake ?? throw new InvalidOperationException(
                "Speed Booster escape room object lost its earthquake owner.")).Stop();
            return false;
        }

        // A liquid with no target ends the lavaquake.
        if (fx.TargetSurface is null)
            return false;

        fx.SetLiquidMotion(
            levelSpeed: SpeedBoosterEscapeDefinitions.InitialLavaquakeVelocity);
        return true;
    }

    /// <summary>Starts the lava moving once Samus has run far enough left, and returns whether she has.</summary>
    internal bool TryStartLavaMoving()
    {
        IRoomFxMotion fx = RoomFx();
        if (LiveSamus().Position.X.Whole > SpeedBoosterEscapeDefinitions.StartFxMotionSamusX)
            return false;
        fx.SetLiquidMotion(moveDelay: 1);
        return true;
    }

    /// <summary>Raises the lava to <paramref name="rise"/> once Samus reaches its line, and returns whether she has.</summary>
    internal bool TryRaiseLava(SpeedBoosterEscapeStageDefinition rise)
    {
        IRoomFxMotion fx = RoomFx();
        // The rise waits while Samus remains right of its threshold.
        if (rise.TargetSamusX < LiveSamus().Position.X.Whole)
            return false;

        // A lava with no surface lies below every line, so the rise gives it its highest line.
        fx.SetLiquidMotion(
            liquidLevel: rise.MaximumFxY < AbsentLiquidSurfaceDepth.Of(fx.BaseY?.Whole)
                ? rise.MaximumFxY
                : null,
            levelSpeed: rise.YVelocity);
        return true;
    }

    /// <summary>Marks the lavaquake outrun.</summary>
    internal void MarkEscaped() =>
        (_events ?? throw new InvalidOperationException(
            "Speed Booster escape room object lost its event writer.")).Set(StoryEvent.OutranSpeedBoosterLavaquake);

    /// <summary>The room's lava.</summary>
    private IRoomFxMotion RoomFx() => _roomFx
        ?? throw new InvalidOperationException("Resident Speed Booster escape room object lost its room-FX owner.");

    /// <summary>The live Samus.</summary>
    private IRoomSamus LiveSamus() => _samus?.Current
        ?? throw new InvalidOperationException("Resident Speed Booster escape room object has no live Samus owner.");
}
