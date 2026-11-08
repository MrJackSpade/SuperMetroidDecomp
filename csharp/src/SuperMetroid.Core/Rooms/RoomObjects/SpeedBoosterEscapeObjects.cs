using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The Speed Booster escape PLMs: rising lava/acid FX and the earthquake the escape triggers.</summary>
[RoomLifetime]
internal sealed class SpeedBoosterEscapeObjects : IRoomObjectFamilySource
{
    /// <summary>Registers the SpeedBoosterEscape family, which has no step of its own: its slot takes the shared path.</summary>
    public void AddObjectFamilies(RoomObjectFamilyTable table)
    {
        Ensure.NotNull(table);
        table.Add(RoomObjectFamily.SpeedBoosterEscape, static (_, _) => RoomObjectStepOutcome.Shared);
    }

    /// <summary>Samus as the room reads and changes her.</summary>
    private readonly IRoomSamusAccess? _samus;
    /// <summary>The game's event flags.</summary>
    private readonly IGlobalEventFlags? _events;
    /// <summary>The room's effect overlay FX.</summary>
    private readonly IRoomFxMotion? _roomFx;
    /// <summary>The room's earthquake.</summary>
    private readonly IRoomEarthquakeWriter? _earthquake;

    /// <summary>Binds the escape PLM behavior to its room-level state owners.</summary>
    internal SpeedBoosterEscapeObjects(IRoomSamusAccess? samus, IGlobalEventFlags? events, IRoomFxMotion? roomFx, IRoomEarthquakeWriter? earthquake)
    {
        _samus = samus;
        _events = events;
        _roomFx = roomFx;
        _earthquake = earthquake;
    }

    /// <summary>
    /// Sets up the Speed Booster escape PLM. Event 21 suppresses this actor on subsequent room loads; an
    /// unset event starts the escape's program, before its first lava rise.
    /// </summary>
    internal void SetupSpeedBoosterEscapeSlot(RoomObjectSlot slot)
    {
        if (_events is null || _roomFx is null || _earthquake is null)
        {
            throw new InvalidOperationException(
                "Speed Booster escape PLM requires event, room-FX, and earthquake owners.");
        }

        if (_events.Has(EventNumber.OutranSpeedBoosterLavaquake))
            slot.Clear();
        else
        {
            slot.StartAnimation(new SpeedBoosterEscapeObjectAnimation());
            slot.Family = new SpeedBoosterEscapeObjectState();
        }
    }

    /// <summary>Dispatches the exact callback pointer installed by the original game list.</summary>
    internal void RunSpeedBoosterEscapeFrameBehavior(RoomObjectSlot slot, SpeedBoosterEscapeFrameBehavior frameBehavior)
    {
        IRoomFxMotion fx = _roomFx
            ?? throw new InvalidOperationException(
                "Resident Speed Booster escape PLM lost its room-FX owner.");
        IRoomSamus samus = _samus?.Current
            ?? throw new InvalidOperationException(
                "Resident Speed Booster escape PLM has no live Samus owner.");

        switch (frameBehavior)
        {
            case SpeedBoosterEscapeFrameBehavior.WaitForSpeedBooster:
                RunWaitForSpeedBooster(slot, samus, fx);
                return;

            case SpeedBoosterEscapeFrameBehavior.WaitForSamusLeft:
                if (samus.Position.X.Whole > SpeedBoosterEscapeDefinitions.StartFxMotionSamusX)
                    return;
                fx.SetLiquidMotion(timer: 1);
                WakeAtNextInstruction(slot);
                return;

            case SpeedBoosterEscapeFrameBehavior.AdvanceLava:
                RunAdvanceLava(slot, samus, fx);
                return;

            case SpeedBoosterEscapeFrameBehavior.None:
            default:
                throw new InvalidOperationException(
                    $"Undefined Speed Booster escape pre-instruction {frameBehavior}.");
        }
    }

    /// <summary>Starts the lavaquake when Speed Booster is owned, or removes a disabled escape PLM.</summary>
    private void RunWaitForSpeedBooster(
        RoomObjectSlot slot,
        IRoomSamus samus,
        IRoomFxMotion fx)
    {
        if (!samus.CollectedItems.HasAny(SamusEquipmentFlags.SpeedBooster))
        {
            fx.RemoveLiquidTarget();
            fx.SetLiquidMotion(
                packedYVelocity: 0,
                timer: 0);
            (_earthquake ?? throw new InvalidOperationException(
                "Speed Booster escape PLM lost its earthquake owner.")).SetTimer(0);
            slot.Clear();
            return;
        }

        // A liquid with no target deletes the actor.
        if (fx.TargetSurface is null)
        {
            slot.Clear();
            return;
        }

        fx.SetLiquidMotion(
            packedYVelocity: SpeedBoosterEscapeDefinitions.InitialLavaquakeVelocity.RawValue);
        WakeAtNextInstruction(slot);
    }

    /// <summary>Advances one lavaquake stage when Samus reaches its X threshold and records escape on completion.</summary>
    private void RunAdvanceLava(
        RoomObjectSlot slot,
        IRoomSamus samus,
        IRoomFxMotion fx)
    {
        SpeedBoosterEscapeObjectState escape = slot.Family as SpeedBoosterEscapeObjectState
            ?? throw new InvalidOperationException("The Speed Booster escape advanced its lava without its escape state.");
        if (escape.IsEscaped)
        {
            (_events ?? throw new InvalidOperationException(
                "Speed Booster escape PLM lost its event writer.")).Set(
                    EventNumber.OutranSpeedBoosterLavaquake);
            return;
        }

        // The rise waits while Samus remains right of its threshold.
        SpeedBoosterEscapeStageDefinition stage = escape.NextRise;
        if (stage.TargetSamusX < samus.Position.X.Whole)
            return;

        // A lava with no surface lies below every line, so the stage gives it its highest line.
        fx.SetLiquidMotion(
            baseYPosition: stage.MaximumFxY < AbsentLiquidSurfaceDepth.Of(fx.BaseY?.Whole)
                ? stage.MaximumFxY
                : null,
            packedYVelocity: stage.YVelocity.RawValue);
        escape.PassRise();
    }

    /// <summary>
    /// Both wake steps set frame hold one and advance past the sleeping step. The ordinary handler then consumes the
    /// next install/sleep pair in the same frame, preserving the coroutine cadence.
    /// </summary>
    private static void WakeAtNextInstruction(RoomObjectSlot slot) => slot.WakePastSleep();
}
