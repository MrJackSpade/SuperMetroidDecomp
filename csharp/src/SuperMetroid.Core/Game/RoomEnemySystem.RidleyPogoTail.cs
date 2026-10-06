namespace SuperMetroid.Core.Game;

public sealed partial class RoomEnemySystem
{
    /// <summary>$A6:B8A9 chooses the next bounce's direction from position and the current seed.</summary>
    private void SetRidleyPogoHorizontalDirection(RoomEnemySlot slot, RidleyEnemyState state, SamusState? samus)
    {
        if (state.HorizontalVelocity == 0)
            state.HorizontalVelocity = (slot.XPosition & 0x80) == 0 ? (ushort)0x00c0 : unchecked((ushort)-0x00c0);
        bool negative = (short)state.HorizontalVelocity < 0;
        bool reverse;
        if ((short)(slot.XPosition - state.MinimumX) < 0) reverse = negative;
        else if ((short)(slot.XPosition - state.MaximumX) >= 0) reverse = !negative;
        else
        {
            bool oppositeSigns = ((ushort)(slot.XPosition - (samus?.XPosition ?? 0)) ^ state.HorizontalVelocity) >= 0x8000;
            reverse = oppositeSigns ? RequireRandomNumber() < 0x0555 : RequireRandomNumber() >= 0x0555;
        }
        if (reverse) state.HorizontalVelocity = unchecked((ushort)-state.HorizontalVelocity);
    }

    /// <summary>Live pogo tail controllers $A6:CB33..CBD3 and $CD24..CE64.</summary>
    private void TickRidleyPogoTail(RoomEnemySlot slot, RidleyEnemyState state, SamusState? samus)
    {
        ushort function = state.TailFunctionIndex;
        bool pointed = function is RidleyTailDefinitions.PogoSetup or RidleyTailDefinitions.PointDown or RidleyTailDefinitions.Stab;
        if (!pointed && function is not (RidleyTailDefinitions.Pogo or RidleyTailDefinitions.StabSetup))
            throw new InvalidDataException($"Ridley tail function {function} is not translated.");

        state.TailMinimumClockwiseAngle = RidleyTailDefinitions.MinimumClockwise(state.FacingDirection);
        state.TailMaximumCounterClockwiseAngle = RidleyTailDefinitions.MaximumCounterClockwise(state.FacingDirection);
        if (pointed)
        {
            state.TailAngleDelta = function == RidleyTailDefinitions.Stab ? (ushort)3 : (ushort)8;
            state.TailWhipTargetClockwiseAngle = state.FacingDirection == 0 ? ushort.MaxValue : RidleyTailDefinitions.DownAngle;
            state.TailWhipTargetCounterClockwiseAngle = state.FacingDirection == 0 ? RidleyTailDefinitions.DownAngle : ushort.MaxValue;
        }
        HandleRidleyPogoTailControl(slot, state, samus, function == RidleyTailDefinitions.StabSetup);
        for (int index = 0; index < state.TailSegments.Length; index++)
            TickRidleyTailSegment(state, index);
        if (pointed && !state.TailSegments.Any(segment => segment.Active))
            state.TailFunctionIndex = RidleyTailDefinitions.Pogo;
        // The setup/stab wrappers overwrite the result of the shared pointed-tail routine.
        if (function == RidleyTailDefinitions.PogoSetup)
            state.TailFunctionIndex = RidleyTailDefinitions.PointDown;
        else if (function == RidleyTailDefinitions.Stab)
            state.TailFunctionIndex = RidleyTailDefinitions.Stab;
    }

    private void HandleRidleyPogoTailControl(RoomEnemySlot slot, RidleyEnemyState state, SamusState? samus, bool stabbing)
    {
        if (state.TailSegments.All(segment => segment.Active) &&
            ((RequireRandomNumber() & 0xff) >= 0xf0 ||
             samus is not null && Math.Abs((short)(samus.XPosition - slot.XPosition)) < 128) &&
            (state.TailWhipTargetClockwiseAngle & state.TailWhipTargetCounterClockwiseAngle & 0x8000) != 0)
        {
            state.TailWhipTargetClockwiseAngle = RidleyTailDefinitions.PogoWhipAngle;
            state.TailAngleDelta = 8;
            return;
        }
        if (state.TailSegments.Any(segment => segment.Active)) return;
        if ((short)state.VerticalVelocity >= 0)
        {
            state.TailFunctionIndex = stabbing ? RidleyTailDefinitions.Stab : RidleyTailDefinitions.StabSetup;
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
