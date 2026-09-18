namespace SuperMetroid.Core.Game;

/// <summary>
/// The twelve enemy-$E1BF actors spawned when Lower Norfair Ridley's intact body breaks
/// apart. They are real members of the fixed 32-slot enemy pool, run bank-$A6 instruction
/// lists, flicker, drift under gravity, and expire independently just as $A6:C696-$C932 does.
/// </summary>
public sealed partial class RoomEnemySystem
{
    private void SpawnNorfairRidleyBreakupActors(
        RoomEnemySlot body,
        RidleyEnemyState state)
    {
        if (state.DeathBreakupSpawned)
            return;
        state.DeathBreakupSpawned = true;

        RoomEnemyDefinition definition = ReadDefinition(
            _bus!,
            RidleyExplosionDefinitions.EnemyDefinition);
        foreach (ushort parameter in RidleyExplosionDefinitions.SpawnOrder)
        {
            int slotIndex = Array.FindIndex(
                _slots,
                candidate => candidate.EnemyDefinitionPointer == 0);
            if (slotIndex < 0)
            {
                throw new InvalidOperationException(
                    "Lower Norfair Ridley breakup exhausted the 32-slot enemy pool.");
            }

            // The embedded records at $A6:C987-$CA46 all use the same zero position,
            // property $2C00, and one even parameter. Initialization replaces position and
            // instruction from the still-live shared Ridley joints below.
            RoomEnemyPopulationRecord population = new(
                RidleyExplosionDefinitions.EnemyDefinition,
                XPosition: 0,
                YPosition: 0,
                InitializationParameter: 0,
                Properties: (ushort)(
                    EnemyProperties.ProcessInstructions |
                    EnemyProperties.ProcessOffScreen |
                    EnemyProperties.IgnoreSamusCollision),
                ExtraProperties: 0,
                Parameter1: parameter,
                Parameter2: 0);
            RoomEnemySlot fragment = _slots[slotIndex];
            InitializeSlotFromDefinition(fragment, population, definition);
            InitializeNorfairRidleyExplosion(fragment, body, state);
            EnemyCount = unchecked((ushort)Math.Max(EnemyCount, slotIndex + 1));
            FirstFreeEnemyIndex = unchecked((ushort)((slotIndex + 1) * NativeSlotSize));
        }
    }

    /// <summary>Ports initialization AI $A6:C696 for one parameter $00..$16.</summary>
    private void InitializeNorfairRidleyExplosion(
        RoomEnemySlot fragment,
        RoomEnemySlot body,
        RidleyEnemyState state)
    {
        if ((fragment.Parameter1 & 1) != 0 || fragment.Parameter1 > 0x0016)
        {
            throw new InvalidDataException(
                $"Ridley explosion parameter ${fragment.Parameter1:X4} is outside $00..$16/even.");
        }

        fragment.InstructionTimer = 1;
        fragment.Timer = 0;
        fragment.VramTilesIndex = 0;
        fragment.PaletteIndex = EnemyPaletteBits.Palette7;
        fragment.VariableF = RidleyExplosionDefinitions.GetPart(fragment.Parameter1).Lifetime;

        ushort random = _nextRandom!();
        ushort horizontalMagnitude = unchecked((ushort)(random & 0x0130));
        fragment.VariableB = unchecked((short)random) < 0
            ? unchecked((ushort)-horizontalMagnitude)
            : horizontalMagnitude;
        fragment.VariableC = 0;

        ushort parameter = fragment.Parameter1;
        if (parameter <= 0x000c)
        {
            int tailIndex = parameter >> 1;
            RidleyTailSegment tail = state.TailSegments[tailIndex];
            fragment.XPosition = tail.XPosition;
            fragment.YPosition = tail.YPosition;
            int tailTipOrientation =
                (((tail.Angle & 0x00ff) +
                    (state.TailSegments[5].Angle & 0x00ff) + 8) & 0x00f0) >> 4;
            fragment.CurrentInstruction = RidleyExplosionDefinitions.SelectTailInstructionList(
                parameter,
                tailTipOrientation);
            return;
        }

        RidleyExplosionBodyPartDefinition bodyPart =
            RidleyExplosionDefinitions.SelectBodyPart(
                parameter,
                facingRight: state.FacingDirection != 0);
        fragment.XPosition = unchecked((ushort)(body.XPosition + bodyPart.XOffset));
        fragment.YPosition = unchecked((ushort)(body.YPosition + bodyPart.YOffset));
        fragment.CurrentInstruction = bodyPart.InstructionList;
    }

    /// <summary>Ports main AI $A6:C8D4: flicker, drag, gravity, movement, and lifetime.</summary>
    private void RunNorfairRidleyExplosionMain(RoomEnemySlot fragment)
    {
        if ((fragment.FrameCounter & 1) != 0)
            fragment.Properties = fragment.Properties.With(EnemyProperties.Invisible);
        else
            fragment.Properties = fragment.Properties.Without(EnemyProperties.Invisible);
        fragment.FrameCounter &= 1;

        int horizontal = unchecked((short)fragment.VariableB);
        int magnitude = Math.Max(0, Math.Abs(horizontal) - 4);
        fragment.VariableB = unchecked((ushort)(horizontal < 0 ? -magnitude : magnitude));
        fragment.VariableC = unchecked((ushort)(fragment.VariableC + 4));
        (fragment.XPosition, fragment.XSubposition) = AddEightBitVelocity(
            fragment.XPosition,
            fragment.XSubposition,
            fragment.VariableB);
        (fragment.YPosition, fragment.YSubposition) = AddEightBitVelocity(
            fragment.YPosition,
            fragment.YSubposition,
            fragment.VariableC);

        fragment.VariableF = unchecked((ushort)(fragment.VariableF - 1));
        if (unchecked((short)fragment.VariableF) >= 0)
            return;

        SpawnRidleyDust(fragment.XPosition, fragment.YPosition, variant: 3);
        fragment.Properties = fragment.Properties.With(EnemyProperties.Deleted);
    }
}
