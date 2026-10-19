using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The Speed Booster escape PLMs: rising lava/acid FX and the earthquake the escape triggers.</summary>
[RoomLifetime]
internal sealed class SpeedBoosterEscapePlms
{
    /// <summary>Live room owners for Samus, events, layer 3 FX, and earthquake timing.</summary>
    private readonly PlmRoomBindings _bindings;

    /// <summary>Binds the escape PLM behavior to its room-level state owners.</summary>
    internal SpeedBoosterEscapePlms(PlmRoomBindings bindings)
    {
        _bindings = bindings;
    }

    /// <summary>
    /// Runs setup $84:B89C. Event $15 suppresses this actor on subsequent room loads; an
    /// unset event leaves the preallocated slot and its ROM-selected $B88A list untouched.
    /// </summary>
    internal void SetupSpeedBoosterEscapeSlot(PlmSlot slot)
    {
        if (_bindings.Events is null || _bindings.RoomFx is null || _bindings.Earthquake is null)
        {
            throw new InvalidOperationException(
                "Speed Booster escape PLM requires event, room-FX, and earthquake owners.");
        }

        if (_bindings.Events.Has(EventNumber.OutranSpeedBoosterLavaquake))
            slot.Clear();
        else
            slot.StartAnimation(new SpeedBoosterEscapePlmAnimation());
    }

    /// <summary>Dispatches the exact callback pointer installed by the cartridge list.</summary>
    internal void RunSpeedBoosterEscapePreInstruction(PlmSlot slot, SpeedBoosterEscapePreInstruction preInstruction)
    {
        RoomLayer3FxState fx = _bindings.RoomFx
            ?? throw new InvalidOperationException(
                "Resident Speed Booster escape PLM lost its room-FX owner.");
        SamusState samus = _bindings.Samus?.Current
            ?? throw new InvalidOperationException(
                "Resident Speed Booster escape PLM has no live Samus owner.");

        switch (preInstruction)
        {
            case SpeedBoosterEscapePreInstruction.WaitForSpeedBooster:
                RunWaitForSpeedBooster(slot, samus, fx);
                return;

            case SpeedBoosterEscapePreInstruction.WaitForSamusLeft:
                if (samus.Position.X.Whole > SpeedBoosterEscapePlmRomData.StartFxMotionSamusX)
                    return;
                fx.ApplyCartridgeMotionWrites(timer: 1);
                WakeAtNextInstruction(slot);
                return;

            case SpeedBoosterEscapePreInstruction.AdvanceLava:
                RunAdvanceLava(slot, samus, fx);
                return;

            default:
                throw new InvalidOperationException(
                    $"Undefined Speed Booster escape pre-instruction {preInstruction}.");
        }
    }

    /// <summary>Starts the lavaquake when Speed Booster is owned, or removes a disabled escape PLM.</summary>
    private void RunWaitForSpeedBooster(
        PlmSlot slot,
        SamusState samus,
        RoomLayer3FxState fx)
    {
        if (!samus.CollectedItems.HasAny(SamusEquipmentFlags.SpeedBooster))
        {
            fx.ApplyCartridgeMotionWrites(
                targetYPosition: ushort.MaxValue,
                packedYVelocity: 0,
                timer: 0);
            (_bindings.Earthquake ?? throw new InvalidOperationException(
                "Speed Booster escape PLM lost its earthquake owner.")).SetTimer(0);
            slot.Clear();
            return;
        }

        // BMI at $84:B80D treats any $8000-$FFFF target as disabled and deletes the actor.
        if (unchecked((short)fx.TargetSurface.Y) < 0)
        {
            slot.Clear();
            return;
        }

        fx.ApplyCartridgeMotionWrites(
            packedYVelocity: SpeedBoosterEscapePlmRomData.InitialLavaquakeVelocity);
        WakeAtNextInstruction(slot);
    }

    /// <summary>Advances one lavaquake stage when Samus reaches its X threshold and records escape on completion.</summary>
    private void RunAdvanceLava(
        PlmSlot slot,
        SamusState samus,
        RoomLayer3FxState fx)
    {
        SpeedBoosterEscapeStageDefinition? stage =
            SpeedBoosterEscapeStageDefinitions.Resolve(checked((ushort)slot.LoopTimer));
        if (stage is null)
        {
            (_bindings.Events ?? throw new InvalidOperationException(
                "Speed Booster escape PLM lost its event writer.")).Set(
                    EventNumber.OutranSpeedBoosterLavaquake);
            return;
        }

        // BCC at $84:B852 returns while Samus remains left-to-right beyond this stage.
        if (stage.Value.TargetSamusX < samus.Position.X.Whole)
            return;

        fx.ApplyCartridgeMotionWrites(
            baseYPosition: stage.Value.MaximumFxY < fx.BaseY.Whole
                ? stage.Value.MaximumFxY
                : null,
            packedYVelocity: stage.Value.PackedYVelocity);
        slot.LoopTimer = unchecked((ushort)(
            slot.LoopTimer + SpeedBoosterEscapeStageDefinitions.RecordByteCount));
    }

    /// <summary>
    /// Both $B7EF and $B82A set instruction timer one, advance past the sleeping word, and
    /// clear PLM_Timers. The ordinary handler then consumes the next install/sleep pair in
    /// the same frame, preserving the native coroutine cadence.
    /// </summary>
    private static void WakeAtNextInstruction(PlmSlot slot)
    {
        slot.WakePastSleep();
        slot.LoopTimer = 0;
    }
}
