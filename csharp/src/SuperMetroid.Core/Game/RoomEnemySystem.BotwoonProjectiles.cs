using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Botwoon's bank-$86 projectile half: thirteen articulated body links and the aimed spit
/// actors. Body fields intentionally retain their native aliases—X velocity is a function
/// pointer while the actor is alive, Y velocity becomes a quadratic fall accumulator during
/// death, and variables zero/one hold orientation-list/fraction words depending on kind.
/// </summary>
public sealed partial class RoomEnemySystem
{
    private const int BotwoonBodyInstructionTable = 0x86e9f1;
    private const ushort BotwoonBodyMainFunction = 0xea98;
    private const ushort BotwoonBodyBeginDyingFunction = 0xeaf4;
    private const ushort BotwoonBodyDyingDelayFunction = 0xeb04;
    private const ushort BotwoonBodyFallingFunction = 0xeb1f;
    private const ushort BotwoonBodyLandedFunction = 0xeb8f;
    private const ushort BotwoonBodyLandedInstruction = 0xe208;
    private const ushort BotwoonSpitInstruction = 0xebae;

    /// <summary>Ports <c>EprojInit_BotwoonsBody</c> at <c>$86:EA31</c>.</summary>
    private void SpawnBotwoonBodySegment(
        RoomEnemySlot head,
        BotwoonEnemyState state,
        ushort spawnArgument)
    {
        RoomEnemyProjectileSlot? segment = AllocateEnemyProjectile();
        if (segment is null)
        {
            throw new InvalidOperationException(
                "Botwoon requires thirteen free enemy-projectile slots during room load.");
        }

        InitializeEnemyProjectileFromDefinition(
            segment,
            RoomEnemyProjectileKind.BotwoonBody,
            unchecked((ushort)(head.VramTilesIndex | head.PaletteIndex)));
        segment.XPosition = head.XPosition;
        segment.YPosition = head.YPosition;
        segment.XSubposition = 0;
        segment.YSubposition = 0;
        segment.YVelocity = 0;

        // ai_var_A is the live spawn argument. Every nonzero segment begins with orientation
        // $10; argument zero is the tail cap and begins at $30.
        ushort orientation = spawnArgument != 0 ? (ushort)16 : (ushort)48;
        ushort instruction = ReadWord(
            _bus!, BotwoonBodyInstructionTable + orientation);
        segment.InstructionPointer = instruction;
        segment.InstructionTimer = 1;
        segment.Variable0 = instruction;
        segment.DirectionParameter = orientation;
        segment.XVelocity = BotwoonBodyMainFunction;

        int segmentIndex = spawnArgument >> 1;
        state.BodySegments[segmentIndex] = segment;
        state.SegmentInsideHole[segmentIndex] = true;
        segment.CollisionOption = 2;
        segment.CanDamageSamus = false;
    }

    /// <summary>Ports <c>EprojPreInstr_BotwoonsBody</c> and its four function states.</summary>
    private void RunBotwoonBodyPreInstruction(
        RoomEnemyProjectileSlot segment,
        byte randomEnemyCounter)
    {
        BotwoonEnemyState state = _botwoonState ?? throw new InvalidOperationException(
            "A live Botwoon body segment has no owning head state.");
        if (state.BodyDeathStarted && segment.XVelocity == BotwoonBodyMainFunction)
            segment.XVelocity = BotwoonBodyBeginDyingFunction;

        switch (segment.XVelocity)
        {
            case BotwoonBodyMainFunction:
                AnimateBotwoonBodySegment(segment, randomEnemyCounter);
                return;

            case BotwoonBodyBeginDyingFunction:
                // The delay is based on native projectile index, not body order. Botwoon
                // normally owns indexes $0A..$22 because allocation descended from $22.
                segment.DirectionParameter = unchecked((ushort)(
                    4 * (segment.SlotIndex * 2) + 96));
                segment.XVelocity = BotwoonBodyDyingDelayFunction;
                goto case BotwoonBodyDyingDelayFunction;

            case BotwoonBodyDyingDelayFunction:
                segment.DirectionParameter = unchecked((ushort)(segment.DirectionParameter + 1));
                if (unchecked((short)(segment.DirectionParameter - 256)) >= 0)
                    segment.XVelocity = BotwoonBodyFallingFunction;
                segment.InstructionTimer = 0;
                ApplyBotwoonBodyHurtPalette(segment, randomEnemyCounter);
                return;

            case BotwoonBodyFallingFunction:
                RunBotwoonBodyFall(segment, state, randomEnemyCounter);
                return;

            case BotwoonBodyLandedFunction:
                return;

            default:
                throw new InvalidDataException(
                    $"Botwoon body function $86:{segment.XVelocity:X4} is not translated.");
        }
    }

    private void AnimateBotwoonBodySegment(
        RoomEnemyProjectileSlot segment,
        byte randomEnemyCounter)
    {
        ushort instruction = ReadWord(
            _bus!, BotwoonBodyInstructionTable + segment.DirectionParameter);
        if (instruction != segment.Variable0)
        {
            segment.InstructionPointer = instruction;
            segment.Variable0 = instruction;
            segment.InstructionTimer = 1;
        }
        ApplyBotwoonBodyHurtPalette(segment, randomEnemyCounter);
    }

    private void ApplyBotwoonBodyHurtPalette(
        RoomEnemyProjectileSlot segment,
        byte randomEnemyCounter)
    {
        segment.GraphicsIndex = new SnesObjAttributeWord(segment.GraphicsIndex)
            .WithPaletteBits(SnesObjPalettes.Index7.PaletteBits);
        RoomEnemySlot head = _slots[0];
        if (head.FlashTimer != 0 && (randomEnemyCounter & 2) != 0)
            segment.GraphicsIndex = new SnesObjAttributeWord(segment.GraphicsIndex)
                .WithPaletteIndex(0);
    }

    private void RunBotwoonBodyFall(
        RoomEnemyProjectileSlot segment,
        BotwoonEnemyState state,
        byte randomEnemyCounter)
    {
        int displacement = ReadQuadraticEnemySpeed(
            unchecked((ushort)(segment.YVelocity >> 8)),
            negative: false);
        (segment.YPosition, segment.YSubposition) = AddBotwoonFixed(
            segment.YPosition,
            segment.YSubposition,
            displacement);
        if (unchecked((short)(segment.YPosition - 200)) < 0)
        {
            segment.YVelocity = unchecked((ushort)(segment.YVelocity + 192));
            segment.InstructionTimer = 0;
            ApplyBotwoonBodyHurtPalette(segment, randomEnemyCounter);
            return;
        }

        segment.YPosition = 200;
        segment.XVelocity = BotwoonBodyLandedFunction;
        segment.InstructionPointer = BotwoonBodyLandedInstruction;
        segment.InstructionTimer = 1;
        segment.GraphicsIndex = EnemyPaletteBits.Palette5;
        segment.CanDamageSamus = false;
        segment.CollisionOption = 2;
        LastBotwoonSoundEffect = 0x0024;

        // Native projectile index $0A is the last allocated body point and owns the
        // synchronization flag. Mapping by retained spawn argument is equivalent while also
        // remaining correct in an isolated audit that preoccupies lower projectile slots.
        if (ReferenceEquals(state.BodySegments[0], segment))
            state.LastBodySegmentLanded = true;
    }

    /// <summary>Ports <c>EprojInit_BotwoonsSpit</c> at <c>$86:EBC6</c>.</summary>
    private void SpawnBotwoonSpit(RoomEnemySlot head, byte angle, ushort speed)
    {
        RoomEnemyProjectileSlot? spit = AllocateEnemyProjectile();
        if (spit is null)
            return;

        InitializeEnemyProjectileFromDefinition(
            spit,
            RoomEnemyProjectileKind.BotwoonSpit,
            unchecked((ushort)(head.VramTilesIndex | head.PaletteIndex)));
        spit.XPosition = head.XPosition;
        spit.YPosition = head.YPosition;
        spit.XSubposition = 0;
        spit.YSubposition = 0;
        spit.InstructionPointer = BotwoonSpitInstruction;
        spit.InstructionTimer = 1;
        spit.DirectionParameter = angle;

        // ConvertAngleToXy stores unsigned quarter-circle magnitudes; E73E applies signs
        // from the retained angle every frame. Keep both halves so shallow trajectories do
        // not lose their subpixel motion.
        int xMagnitude = ReadUnsignedSineMagnitudeProduct(angle, speed, 0x40);
        int yMagnitude = ReadUnsignedSineMagnitudeProduct(angle, speed, 0x80);
        spit.XVelocity = unchecked((ushort)(xMagnitude >> 16));
        spit.Variable0 = unchecked((ushort)xMagnitude);
        spit.YVelocity = unchecked((ushort)(yMagnitude >> 16));
        spit.Variable1 = unchecked((ushort)yMagnitude);
    }

    /// <summary>Ports <c>EprojPreInstr_BotwoonsSpit</c> at <c>$86:EC05</c>.</summary>
    private static void RunBotwoonSpitPreInstruction(
        RoomEnemyProjectileSlot spit,
        ushort cameraX,
        ushort cameraY)
    {
        int xMagnitude = unchecked((spit.XVelocity << 16) | spit.Variable0);
        int yMagnitude = unchecked((spit.YVelocity << 16) | spit.Variable1);
        int xDisplacement = ((spit.DirectionParameter + 64) & 0x80) != 0
            ? -xMagnitude
            : xMagnitude;
        int yDisplacement = ((spit.DirectionParameter + 128) & 0x80) != 0
            ? -yMagnitude
            : yMagnitude;
        (spit.XPosition, spit.XSubposition) = AddBotwoonFixed(
            spit.XPosition,
            spit.XSubposition,
            xDisplacement);
        (spit.YPosition, spit.YSubposition) = AddBotwoonFixed(
            spit.YPosition,
            spit.YSubposition,
            yDisplacement);

        if (unchecked((short)(spit.XPosition - cameraX)) < 0 ||
            unchecked((short)(cameraX + 256 - spit.XPosition)) < 0 ||
            unchecked((short)(spit.YPosition - cameraY)) < 0 ||
            unchecked((short)(cameraY + 256 - spit.YPosition)) < 0)
        {
            spit.Clear();
        }
    }
}
