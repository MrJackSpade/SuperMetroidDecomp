using SuperMetroid.Core.Hardware;
namespace SuperMetroid.Core.Game;

public sealed partial class RoomEnemySystem
{
    /// <summary>$A6:B8A9 chooses the next bounce's direction from position and the current seed.</summary>
    private void SetRidleyPogoHorizontalDirection(RoomEnemySlot slot, RidleyEnemyState state, SamusState? samus)
    {
        if (state.Velocity.X.RawValue == 0)
            state.Velocity = state.Velocity.WithX(new SnesSignedEightEight((slot.Position.X.Whole & 0x80) == 0 ? (ushort)0x00c0 : unchecked((ushort)-0x00c0)));
        bool negative = state.Velocity.X.SignedRawValue < 0;
        bool reverse;
        if ((short)(slot.Position.X.Whole - state.MovementMinimum.X) < 0) reverse = negative;
        else if ((short)(slot.Position.X.Whole - state.MovementMaximum.X) >= 0) reverse = !negative;
        else
        {
            bool oppositeSigns = ((ushort)(slot.Position.X.Whole - (samus?.Position.X.Whole ?? 0)) ^ state.Velocity.X.RawValue) >= 0x8000;
            reverse = oppositeSigns ? RequireRandomNumber() < 0x0555 : RequireRandomNumber() >= 0x0555;
        }
        if (reverse) state.Velocity = state.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)-state.Velocity.X.RawValue)));
    }

    /// <summary>Live pogo tail controllers $A6:CB33..CBD3 and $CD24..CE64.</summary>
    private void TickRidleyPogoTail(RoomEnemySlot slot, RidleyEnemyState state, SamusState? samus)
    {
        RidleyTailFunction function = state.TailFunctionIndex;
        bool pointed = function is RidleyTailFunction.PogoSetup or RidleyTailFunction.PointDown or RidleyTailFunction.Stab;
        if (!pointed && function is not (RidleyTailFunction.Pogo or RidleyTailFunction.StabSetup))
            throw new InvalidDataException($"Ridley tail function {function} is not translated.");

        state.TailMinimumClockwiseAngle = RidleyTailDefinitions.MinimumClockwise(state.FacingDirection);
        state.TailMaximumCounterClockwiseAngle = RidleyTailDefinitions.MaximumCounterClockwise(state.FacingDirection);
        if (pointed)
        {
            state.TailAngleDelta = function == RidleyTailFunction.Stab ? (ushort)3 : (ushort)8;
            state.TailWhipTargetClockwiseAngle = state.FacingDirection == RidleyFacing.Left ? ushort.MaxValue : RidleyTailDefinitions.DownAngle;
            state.TailWhipTargetCounterClockwiseAngle = state.FacingDirection == RidleyFacing.Left ? RidleyTailDefinitions.DownAngle : ushort.MaxValue;
        }
        HandleRidleyPogoTailControl(slot, state, samus, function == RidleyTailFunction.StabSetup);
        for (int index = 0; index < state.TailSegments.Length; index++)
            TickRidleyTailSegment(state, index);
        if (pointed && !state.TailSegments.Any(segment => segment.Active))
            state.TailFunctionIndex = RidleyTailFunction.Pogo;
        // The setup/stab wrappers overwrite the result of the shared pointed-tail routine.
        if (function == RidleyTailFunction.PogoSetup)
            state.TailFunctionIndex = RidleyTailFunction.PointDown;
        else if (function == RidleyTailFunction.Stab)
            state.TailFunctionIndex = RidleyTailFunction.Stab;
    }

    private void HandleRidleyPogoTailControl(RoomEnemySlot slot, RidleyEnemyState state, SamusState? samus, bool stabbing)
    {
        if (state.TailSegments.All(segment => segment.Active) &&
            ((SnesLittleEndian.LowByte(RequireRandomNumber())) >= 0xf0 ||
             samus is not null && Math.Abs((short)(samus.Position.X.Whole - slot.Position.X.Whole)) < 128) &&
            (state.TailWhipTargetClockwiseAngle & state.TailWhipTargetCounterClockwiseAngle & 0x8000) != 0)
        {
            state.TailWhipTargetClockwiseAngle = RidleyTailDefinitions.PogoWhipAngle;
            state.TailAngleDelta = 8;
            return;
        }
        if (state.TailSegments.Any(segment => segment.Active)) return;
        if (state.Velocity.Y.SignedRawValue >= 0)
        {
            state.TailFunctionIndex = stabbing ? RidleyTailFunction.Stab : RidleyTailFunction.StabSetup;
            if (stabbing)
            {
                foreach (var segment in state.TailSegments)
                {
                    segment.TargetDistance = RidleyTailDefinitions.StabDistance;
                    segment.Angle = RidleyTailDefinitions.DownAngle;
                }
                state.TailSegments[0].Active = true;
            }
        }
        state.TailWhipTargetClockwiseAngle = ushort.MaxValue;
        state.TailWhipTargetCounterClockwiseAngle = ushort.MaxValue;
        // Native pogoTailActivationTimer is initialized to zero and never assigned
        // a nonzero value by any live routine; CD90/CE61 therefore write zero here.
        state.TailAngleDelta = 0;
    }
}
