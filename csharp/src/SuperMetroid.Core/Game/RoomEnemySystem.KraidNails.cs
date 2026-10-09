using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Kraid's two reusable fingernail actors (`$E43F/$E47F`). Their delayed activation,
/// cartridge-selected 16.16 velocities, room collision, body-contour reflection, and spawn
/// alternation remain independent for the two physical enemy slots.
/// </summary>
public sealed partial class RoomEnemySystem
{
    /// <summary>Dispatches a fingernail's initialization, wait, or active-flight behavior for this room frame.</summary>
    /// <param name="nail">Physical fingernail actor slot whose function is being advanced.</param>
    /// <param name="level">Room collision geometry required while the nail is flying.</param>
    private void RunKraidNailMain(RoomEnemySlot nail, RoomLevelData? level)
    {
        KraidEnemyState state = RequireKraidState(nail);
        if (_slots[0].Health == 0)
        {
            nail.Properties = nail.Properties.With(
                EnemyProperties.Deleted | EnemyProperties.Invisible);
            return;
        }

        KraidPartState part = state.Parts[nail.SlotIndex];
        switch ((KraidAiFunction)nail.VariableA)
        {
            case KraidAiFunction.HandleFunctionTimer:
                TickKraidFunctionTimer(nail, part);
                return;

            case KraidAiFunction.FingernailInitialize:
                InitializeKraidNailFlight(nail, part);
                return;

            case KraidAiFunction.FingernailWaitForLint:
                if (unchecked((short)(_slots[2].XPosition - 256)) >= 0)
                {
                    nail.VariableA = (ushort)part.NextFunction;
                    nail.Properties = nail.Properties.Without(
                        EnemyProperties.IgnoreSamusCollision | EnemyProperties.Invisible);
                }
                return;

            case KraidAiFunction.FingernailFire:
                if (level is null)
                {
                    throw new InvalidOperationException(
                        "Active Kraid fingernail movement requires room collision data.");
                }
                TickKraidNailFlight(nail, level);
                return;

            default:
                throw new InvalidDataException(
                    $"Kraid fingernail function $A7:{nail.VariableA:X4} is not translated.");
        }
    }

    /// <summary>Seeds a nail's velocity and launch position, including the paired actor's alternating lint wait.</summary>
    /// <param name="nail">Fingernail actor slot being initialized.</param>
    /// <param name="part">Per-nail state that tracks its spawn alternation and next function.</param>
    private void InitializeKraidNailFlight(RoomEnemySlot nail, KraidPartState part)
    {
        // The pair coordinates its next launch through the other actor's previous
        // velocity and spawn flag. Read that actor before overwriting this one's state.
        int siblingIndex = nail.SlotIndex == 6 ? 7 : 6;
        RoomEnemySlot sibling = _slots[siblingIndex];
        KraidPartState siblingPart = RequireKraidState(nail).Parts[siblingIndex];
        ushort random = RequireRandomNumber();
        (nail.VariableB, nail.VariableC, nail.VariableD, nail.VariableE) =
            KraidNailLaunchDefinitions.FromSiblingVelocity(sibling.VariableE);
        nail.Parameter1 = 1;
        nail.Properties = nail.Properties.Without(
            EnemyProperties.IgnoreSamusCollision | EnemyProperties.Invisible);
        nail.InstructionTimer = 1;
        nail.CurrentInstruction = KraidNailInstructionProgramDefinitions.Loop;
        nail.VariableA = (ushort)KraidAiFunction.FingernailFire;

        if ((random & 1) == 0 || siblingPart.AlternateSpawnFlag == 1)
        {
            part.AlternateSpawnFlag = 0;
            RoomEnemySlot body = _slots[0];
            nail.XPosition = unchecked((ushort)(
                (body.XPosition - body.XRadius - nail.XRadius) & 0xfff0));
            nail.YPosition = unchecked((ushort)(_slots[1].YPosition + 128));
            return;
        }

        part.AlternateSpawnFlag = 1;
        nail.XPosition = 50;
        nail.YPosition = 240;
        nail.VariableB = 0;
        nail.VariableC = 1;
        nail.VariableD = 0;
        nail.VariableE = 0;
        nail.VariableA = (ushort)KraidAiFunction.FingernailWaitForLint;
        part.NextFunction = KraidAiFunction.FingernailFire;
        nail.Properties = nail.Properties.With(
            EnemyProperties.IgnoreSamusCollision | EnemyProperties.Invisible);
    }

    /// <summary>Moves a flying nail through room geometry and reflects its velocity at wall, body, or floor contacts.</summary>
    /// <param name="nail">Active fingernail actor whose position and velocity are updated.</param>
    /// <param name="level">Room collision geometry used to resolve movement contacts.</param>
    private void TickKraidNailFlight(RoomEnemySlot nail, RoomLevelData level)
    {
        int horizontal = CombineKraidVelocity(nail.VariableB, nail.VariableC);
        if (MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, nail, horizontal))
        {
            (nail.VariableB, nail.VariableC) =
                ReflectKraidNailHorizontalVelocity(nail.VariableB, nail.VariableC);
        }
        else if (KraidBodyContourReflectsNail(nail))
        {
            (nail.VariableB, nail.VariableC) =
                ReflectKraidNailHorizontalVelocity(nail.VariableB, nail.VariableC);
        }

        int vertical = CombineKraidVelocity(nail.VariableD, nail.VariableE);
        if (MoveEnemyVertically(level, nail, vertical))
        {
            (nail.VariableD, nail.VariableE) =
                NegateKraidVelocity(nail.VariableD, nail.VariableE);
        }
    }

    /// <summary>Tests whether a right-moving nail has reached Kraid's height-dependent body contour.</summary>
    /// <param name="nail">Fingernail actor being checked against the body outline.</param>
    /// <returns><see langword="true"/> when the nail's right edge crosses the contour while moving right.</returns>
    private bool KraidBodyContourReflectsNail(RoomEnemySlot nail)
    {
        RoomEnemySlot body = _slots[0];
        ushort contourX = unchecked((ushort)(
            body.XPosition + KraidNailContour.LeftOffset(
                unchecked((ushort)(nail.YPosition - body.YPosition)))));
        return unchecked((short)(nail.XPosition + nail.XRadius - contourX)) >= 0 &&
            unchecked((short)nail.VariableC) >= 0;
    }

    /// <summary>Combines the low and high words of a signed 16.16 velocity into its 32-bit representation.</summary>
    /// <param name="low">Fractional low word of the velocity.</param>
    /// <param name="high">Whole-pixel high word of the velocity.</param>
    /// <returns>The signed fixed-point velocity used by room movement.</returns>
    private static int CombineKraidVelocity(ushort low, ushort high) =>
        unchecked((int)(((uint)high << 16) | low));

    /// <summary>
    /// Native horizontal wall/contour reflections at $A7:BE9E and BEE2 negate
    /// each word independently. Do not propagate fractional borrow as a normal
    /// fixed-point negation would; the resulting nonzero-fraction quirk is native.
    /// </summary>
    private static (ushort Low, ushort High) ReflectKraidNailHorizontalVelocity(ushort low, ushort high) =>
        (unchecked((ushort)-low), unchecked((ushort)-high));

    /// <summary>Negates a complete 16.16 velocity, carrying between its fractional and whole words.</summary>
    /// <param name="low">Fractional low word of the velocity.</param>
    /// <param name="high">Whole-pixel high word of the velocity.</param>
    /// <returns>The low and high words of the negated velocity.</returns>
    private static (ushort Low, ushort High) NegateKraidVelocity(ushort low, ushort high)
    {
        int negated = unchecked(-CombineKraidVelocity(low, high));
        return (
            unchecked((ushort)negated),
            unchecked((ushort)((uint)negated >> 16)));
    }
}
