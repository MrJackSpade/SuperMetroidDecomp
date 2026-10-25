using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Draygon's swoops across the room and its climb straight up.</summary>
[RoomLifetime]
internal sealed class DraygonSwoops
{
    /// <summary>Provides the active room bus, random source, and persistent boss state.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Draygon's body, eye and tail: their setup, programs and cues.</summary>
    private readonly DraygonParts _parts;
    /// <summary>The wall turrets' firing cadence during Draygon's fight.</summary>
    private readonly DraygonTurrets _turrets;
    /// <summary>How Draygon moves along an angle or horizontally, and the breath bubbles it trails.</summary>
    private readonly DraygonMovement _movement;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal DraygonSwoops(EnemyRoomBinding room, DraygonParts parts, DraygonTurrets turrets, DraygonMovement movement)
    {
        _room = room;
        _parts = parts;
        _turrets = turrets;
        _movement = movement;
    }

    /// <summary>Ports swoop-right setup and table construction at <c>$A5:87F4-$88B0</c>.</summary>
    internal void SetupDraygonRightSwoop(
        DraygonEnemyState state,
        SamusState? samus,
        int frameCounterLow)
    {
        _turrets.ObserveDraygonTurretCadence(state, samus, frameCounterLow);
        if (samus is null)
            throw new InvalidOperationException("Draygon cannot aim a swoop without Samus.");

        BuildDraygonSwoopYPositions(state, samus);
        state.Function = DraygonAiFunction.SwoopRightDescending;
        _parts.StartDraygonProgram(state.Body, DraygonProgram.BodyFacingRightReset);
        state.FacingRight = true;
    }

    /// <summary>Captures Samus's height and computes the vertical waypoints for the next swoop.</summary>
    internal static void BuildDraygonSwoopYPositions(DraygonEnemyState state, SamusState samus)
    {
        int yPosition = 384;
        EightEightVector<RoomPixels> speed = default;
        int byteOffset = 0;
        Array.Clear(state.SwoopYPositions);

        while (true)
        {
            speed += state.SwoopAcceleration;
            int candidate = unchecked((ushort)(yPosition - (EightEight.WholeOf(speed.Y.RawValue))));
            if (unchecked((short)(candidate - state.LeftSideReset.Y)) < 0)
                break;

            yPosition = candidate;
            if (byteOffset >= 2048)
                throw new InvalidDataException("Draygon swoop Y table exceeded its $800-byte surface.");
            state.SwoopYPositions[byteOffset / 4] = (ushort)candidate;
            byteOffset += 4;
        }

        int frameCount = byteOffset / 4;
        if (frameCount == 0)
            throw new InvalidDataException("Draygon generated an empty swoop path.");
        state.SwoopPathEntryCount = frameCount;
        state.SwoopYPositions[frameCount] = state.Body.Position.Y.Whole;

        // UnsignedDivision_32bit divides the absolute whole-pixel distance promoted to
        // 16.16 by the number of path entries. Split the quotient into the same var_D/E
        // whole/subpixel words consumed by AddToHiLo during descent.
        int distance = EnemyMath.WrappedMagnitude(unchecked((ushort)(
            state.LeftSideReset.X - samus.Position.X.Whole)));
        SetDraygonSwoopVelocity(state, distance, frameCount);
        state.SwoopOffset = byteOffset;
        state.SwoopEndOffset = byteOffset;
    }

    /// <summary>Ports the descending half at <c>$A5:88B1</c> (right) and <c>$A5:8A00</c> (left).</summary>
    internal void DescendDraygonSwoop(
        DraygonEnemyState state,
        SamusState? samus,
        int frameCounterLow,
        DraygonBehavior.DraygonSwoopSide side)
    {
        _turrets.ObserveDraygonTurretCadence(state, samus, frameCounterLow);
        _movement.SpawnPeriodicDraygonBreathBubble(state);
        RoomEnemySlot body = state.Body;
        int pathIndex = state.SwoopOffset / 4;
        if ((uint)pathIndex >= state.SwoopYPositions.Length)
            throw new InvalidDataException($"Draygon descent path index {pathIndex} is outside WRAM.");
        if (state.SwoopOffset == DraygonBehavior.DraygonSwoopApexArmsByteOffset)
            _parts.StartDraygonProgram(state.Arms, side.ArmsNearApex);

        body.Position = body.Position.WithWholeY(state.SwoopYPositions[pathIndex]);
        state.SwoopOffset = state.SwoopOffset - 4;
        if (state.SwoopOffset == 0)
        {
            state.Function = side.Apex;
            return;
        }
        DraygonMovement.MoveDraygonHorizontally(state, side.Direction == HorizontalDirection.Right);
    }

    /// <summary>Ports the velocity recalculation at the bottom of the swoop, $A5:8922.</summary>
    internal void CrossDraygonRightSwoopApex(
        DraygonEnemyState state,
        SamusState? samus,
        int frameCounterLow)
    {
        _turrets.ObserveDraygonTurretCadence(state, samus, frameCounterLow);
        RoomEnemySlot body = state.Body;
        int frameCount = state.SwoopEndOffset / 4;
        if (frameCount == 0)
            throw new InvalidDataException("Draygon lost the swoop duration at the apex.");
        int distance = unchecked((ushort)(672 - body.Position.X.Whole));
        SetDraygonSwoopVelocity(state, distance, frameCount);
        state.Function = DraygonBehavior.RightSwoop.Ascending;
    }

    /// <summary>
    /// Ports the ascending half and random next-attack choice at $A5:8951 (right) and $A5:8A90 (left). Only the leftward
    /// ascent emits breath bubbles. At the top the rightward swoop chooses the left swoop or goop; the leftward one
    /// returns to the left reset point first and chooses the right swoop, at the reset height, or goop.
    /// </summary>
    internal void AscendDraygonSwoop(
        DraygonEnemyState state,
        SamusState? samus,
        int frameCounterLow,
        DraygonBehavior.DraygonSwoopSide side)
    {
        _turrets.ObserveDraygonTurretCadence(state, samus, frameCounterLow);
        if (side.Direction != HorizontalDirection.Right)
            _movement.SpawnPeriodicDraygonBreathBubble(state);
        RoomEnemySlot body = state.Body;
        int pathIndex = state.SwoopOffset / 4;
        if ((uint)pathIndex >= state.SwoopYPositions.Length)
            throw new InvalidDataException($"Draygon ascent path index {pathIndex} is outside WRAM.");
        if (state.SwoopOffset == DraygonBehavior.DraygonSwoopApexArmsByteOffset)
            _parts.StartDraygonProgram(state.Arms, side.ArmsIdle);

        body.Position = body.Position.WithWholeY(state.SwoopYPositions[pathIndex]);
        state.SwoopOffset = state.SwoopOffset + 4;
        if (state.SwoopOffset != state.SwoopEndOffset)
        {
            DraygonMovement.MoveDraygonHorizontally(state, side.Direction == HorizontalDirection.Right);
            return;
        }

        if (side.Direction == HorizontalDirection.Right)
        {
            state.SwoopOffset = 0;
            int random = _room.Random.Read();
            state.Function = (random & 1) != 0
                ? DraygonAiFunction.SwoopLeftSetup
                : DraygonAiFunction.GoopLeftSetup;
            return;
        }

        int choice = _room.Random.Read();
        body.Position = body.Position.WithWholeX(state.LeftSideReset.X);
        if ((choice & 1) != 0)
        {
            state.SwoopOffset = 0;
            body.Position = body.Position.WithWholeY(state.LeftSideReset.Y);
            state.Function = DraygonAiFunction.SwoopRightSetup;
        }
        else
        {
            state.Function = DraygonAiFunction.GoopRightSetup;
        }
    }

    /// <summary>Ports the mirrored left-swoop setup at <c>$A5:89B3</c>.</summary>
    internal void SetupDraygonLeftSwoop(
        DraygonEnemyState state,
        SamusState? samus,
        int frameCounterLow)
    {
        _turrets.ObserveDraygonTurretCadence(state, samus, frameCounterLow);
        if (samus is null)
            throw new InvalidOperationException("Draygon cannot aim a swoop without Samus.");
        int frameCount = state.SwoopEndOffset / 4;
        if (frameCount == 0 || frameCount != state.SwoopPathEntryCount)
            throw new InvalidDataException("Draygon lost the generated swoop duration.");

        int distance = EnemyMath.WrappedMagnitude(unchecked((ushort)(
            state.RightSideReset.X - samus.Position.X.Whole)));
        SetDraygonSwoopVelocity(state, distance, frameCount);
        state.Function = DraygonAiFunction.SwoopLeftDescending;
        _parts.StartDraygonProgram(state.Body, DraygonProgram.BodyFacingLeftReset);
        state.FacingRight = false;
        state.SwoopOffset = state.SwoopEndOffset;
    }

    /// <summary>Ports the mirrored velocity calculation at <c>$A5:8A50</c>.</summary>
    internal void CrossDraygonLeftSwoopApex(
        DraygonEnemyState state,
        SamusState? samus,
        int frameCounterLow)
    {
        _turrets.ObserveDraygonTurretCadence(state, samus, frameCounterLow);
        RoomEnemySlot body = state.Body;
        int frameCount = state.SwoopEndOffset / 4;
        if (frameCount == 0)
            throw new InvalidDataException("Draygon lost the swoop duration at the left apex.");

        // The reset coordinate is negative in the retail room. $A5:8A66 explicitly
        // negates it and adds the body's positive arena X, yielding the wrapped distance
        // from the current point back to X=$FFB0.
        int resetX = state.LeftSideReset.X;
        int distance = unchecked((short)resetX) < 0
            ? unchecked((ushort)(
                unchecked((ushort)-resetX) + body.Position.X.Whole))
            : unchecked((ushort)(body.Position.X.Whole - resetX));
        SetDraygonSwoopVelocity(state, distance, frameCount);
        state.Function = DraygonBehavior.LeftSwoop.Ascending;
    }

    /// <summary>
    /// Sets the swoop's 16.16 horizontal velocity: the whole-pixel distance promoted to 16.16 and divided by the path's
    /// frame count, the quotient native code splits into the var_D/E whole and subpixel words.
    /// </summary>
    /// <param name="state">Draygon's state, which receives the velocity.</param>
    /// <param name="distance">The whole-pixel distance, 0 through 65535.</param>
    /// <param name="frameCount">The path's entries.</param>
    private static void SetDraygonSwoopVelocity(DraygonEnemyState state, int distance, int frameCount) =>
        state.SwoopVelocity = SixteenSixteen.FromWhole((uint)distance) / (uint)frameCount;

    /// <summary>Ports the regional four-pixel upward retreat at <c>$A5:9154</c>.</summary>
    internal void FlyDraygonStraightUp(
        DraygonEnemyState state,
        SamusState? samus,
        int frameCounterLow)
    {
        _turrets.ObserveDraygonTurretCadence(state, samus, frameCounterLow);
        RoomEnemySlot body = state.Body;
        body.Position = body.Position.WithWholeY(unchecked((ushort)(body.Position.Y.Whole - 4)));
        if (unchecked((short)body.Position.Y.Whole) >= 0)
            return;

        body.Properties = body.Properties.Without(EnemyProperties.IgnoreSamusCollision);
        state.Function = DraygonAiFunction.SwoopRightSetup;
        state.SwoopOffset = 0;
        body.Position = body.Position.WithWhole(state.LeftSideReset);
    }
}
