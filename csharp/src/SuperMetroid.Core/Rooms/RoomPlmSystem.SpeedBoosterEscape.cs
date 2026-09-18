using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Cartridge translation of room PLM $84:B8AC, the Speed Booster escape lava controller.
/// </summary>
public sealed partial class RoomPlmSystem
{
    private RoomLayer3FxState? _speedBoosterEscapeFx;
    private Action<ushort>? _writeEarthquakeTimer;

    private void ResetSpeedBoosterEscapeState()
    {
        _speedBoosterEscapeFx = null;
        _writeEarthquakeTimer = null;
    }

    /// <summary>
    /// Runs setup $84:B89C. Event $15 suppresses this actor on subsequent room loads; an
    /// unset event leaves the preallocated slot and its ROM-selected $B88A list untouched.
    /// </summary>
    private void SetupSpeedBoosterEscapeSlot(PlmSlot slot)
    {
        if (_hasEvent is null || _setEvent is null || _speedBoosterEscapeFx is null ||
            _writeEarthquakeTimer is null)
        {
            throw new InvalidOperationException(
                "Speed Booster escape PLM requires event, room-FX, and earthquake owners.");
        }

        if (_hasEvent(EventNumber.OutranSpeedBoosterLavaquake))
            ClearSlot(slot);
    }

    /// <summary>Dispatches the exact callback pointer installed by the cartridge list.</summary>
    private void RunSpeedBoosterEscapePreInstruction(PlmSlot slot)
    {
        if (slot.HeaderPointer != RoomPlmHeaders.SpeedBoosterEscape)
            return;

        RoomLayer3FxState fx = _speedBoosterEscapeFx
            ?? throw new InvalidOperationException(
                "Resident Speed Booster escape PLM lost its room-FX owner.");
        SamusState samus = _collectibleSamus?.Invoke()
            ?? throw new InvalidOperationException(
                "Resident Speed Booster escape PLM has no live Samus owner.");

        switch (slot.PreInstruction)
        {
            case 0:
                return;

            case SpeedBoosterEscapePlmRomData.WaitForSpeedBoosterPreInstruction:
                RunWaitForSpeedBooster(slot, samus, fx);
                return;

            case SpeedBoosterEscapePlmRomData.WaitForSamusLeftPreInstruction:
                if (samus.XPosition > SpeedBoosterEscapePlmRomData.StartFxMotionSamusX)
                    return;
                fx.ApplyCartridgeMotionWrites(timer: 1);
                WakeAtNextInstruction(slot);
                return;

            case SpeedBoosterEscapePlmRomData.AdvanceLavaPreInstruction:
                RunAdvanceLava(slot, samus, fx);
                return;

            default:
                throw new InvalidDataException(
                    $"Speed Booster escape PLM installed unknown pre-instruction " +
                    $"$84:{slot.PreInstruction:X4}.");
        }
    }

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
            (_writeEarthquakeTimer ?? throw new InvalidOperationException(
                "Speed Booster escape PLM lost its earthquake owner."))(0);
            ClearSlot(slot);
            return;
        }

        // BMI at $84:B80D treats any $8000-$FFFF target as disabled and deletes the actor.
        if (unchecked((short)fx.TargetYPosition) < 0)
        {
            ClearSlot(slot);
            return;
        }

        fx.ApplyCartridgeMotionWrites(
            packedYVelocity: SpeedBoosterEscapePlmRomData.InitialLavaquakeVelocity);
        WakeAtNextInstruction(slot);
    }

    private void RunAdvanceLava(
        PlmSlot slot,
        SamusState samus,
        RoomLayer3FxState fx)
    {
        SpeedBoosterEscapeStageDefinition? stage =
            SpeedBoosterEscapeStageDefinitions.Resolve(slot.LoopTimer);
        if (stage is null)
        {
            (_setEvent ?? throw new InvalidOperationException(
                "Speed Booster escape PLM lost its event writer."))(
                    EventNumber.OutranSpeedBoosterLavaquake);
            return;
        }

        // BCC at $84:B852 returns while Samus remains left-to-right beyond this stage.
        if (stage.Value.TargetSamusX < samus.XPosition)
            return;

        fx.ApplyCartridgeMotionWrites(
            baseYPosition: stage.Value.MaximumFxY < fx.BaseYPosition
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
        slot.InstructionTimer = 1;
        slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer + 2));
        slot.LoopTimer = 0;
    }
}
