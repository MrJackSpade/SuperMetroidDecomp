namespace SuperMetroid.Core.Frontend;

internal sealed partial class CeresDestructionCinematicState
{
    /// <summary>
    /// Advances Ceres explosion scheduling, moves active actors, runs their instruction lists, and removes actors that finish.
    /// </summary>
    private void StepCeresActors()
    {
        explosionSpawnerFrame++;

        // CF33 remains invisible for $80 frames, spawns five staggered small blasts,
        // waits $50 BEFORE installing the repeating-blast pre-instruction, then runs
        // it during the final $40-frame wait. The pre-instruction stops on departure;
        // the instruction list still reaches its four terminal blasts afterward.
        CeresExplosionSpawnEvents events = CeresExplosionDefinitions.EventsAtFrame(explosionSpawnerFrame,
            Phase < CeresDestructionPhase.FlyingAwayFromExplosion, ref explosionRepeatCountdown);
        if (events.Initial)
        {
            for (int index = 0; index < CeresExplosionDefinitions.InitialExplosionCount; index++)
                SpawnCeresExplosion(
                    CeresExplosionDefinitions.InitialActor,
                    CeresExplosionDefinitions.InitialExplosion(index));
        }
        if (events.Repeating)
        {
            int offset = explosionOffsetIndex++ &
                (CeresExplosionDefinitions.RepeatingExplosionCount - 1);
            SpawnCeresExplosion(
                CeresExplosionDefinitions.RepeatingActor,
                CeresExplosionDefinitions.RepeatingExplosion(offset));
        }
        if (events.Final)
        {
            for (int index = 0; index < CeresExplosionDefinitions.FinalExplosionCount; index++)
                SpawnCeresExplosion(
                    CeresExplosionDefinitions.FinalWaveActor,
                    CeresExplosionDefinitions.FinalExplosion(index));
        }

        for (int index = actors.Count - 1; index >= 0; index--)
        {
            IntroDiscoverySprite actor = actors[index];
            if (index < CeresDestructionActorDefinitions.InitialActorCount)
            {
                CeresDestructionActorDefinition definition =
                    CeresDestructionActorDefinitions.InitialActor(index);
                if (actor.PreInstructionPointer != definition.ActivePreInstruction)
                {
                    throw new InvalidDataException(
                        $"Ceres destruction actor {index} names invalid pre-instruction " +
                        $"$8B:{actor.PreInstructionPointer:X4}.");
                }
                if (definition.WrapX)
                    AddWrappedX(actor, unchecked((ushort)definition.HorizontalDelta));
            }
            else
                MoveExplosion(actor);

            actor.Step(bus, instructionWord:
                CeresDestructionSpriteInstructionDefinitions.ReadWord);
            if (!actor.IsActive)
            {
                ceresActorSlots.Remove(actor);
                actors.RemoveAt(index);
            }
        }
    }

    /// <summary>
    /// Creates the station's final blast and queues the Mode-7 map transfers that prepare the gunship and clear the lower map half.
    /// </summary>
    private void SpawnFinalCeresExplosion()
    {
        ushort x = unchecked((ushort)(CeresDestructionRomData.Rendering.CeresCenterX - backgroundX));
        ushort y = unchecked((ushort)(CeresDestructionRomData.Rendering.CeresCenterY - backgroundY));
        // C345 calls the ordinary bank-$88 explosion with screen-space coordinates,
        // before replacing the station tilemap. Keep its ROM curves/colors/lifetime.
        stationExplosion.Spawn(x, y);
        audio?.QueueSound(SuperMetroid.Core.Audio.SoundEffectLibrary1Sounds.PowerBombExplosion,
            maximumQueued: 15);
        var cinematicExplosion = new IntroDiscoverySprite(
            x,
            y,
            CeresDestructionRomData.Sprites.ExplosionPalette.Raw,
            CeresExplosionDefinitions.StationBlastActor.InstructionList);
        cinematicExplosion.PreInstructionPointerForDiscovery(
            CeresExplosionDefinitions.StationBlastActor.PreInstruction);
        _ = TryAddCeresActor(cinematicExplosion);

        // `$8B:C345` queues these two Mode-7 transfers on the same dispatcher call that
        // creates the final explosion. The upper 24 rows become the gunship viewed from
        // the front; the lower 24 rows are explicitly cleared. Leaving the original Ceres
        // screens in either half makes the station itself flee the explosion and later
        // attaches those stale tiles to the rear view used on the Zebes approach.
        vram.LoadMode7MapBytes(
            ceresTilemaps.AsSpan(0, CeresDestructionRomData.Vram.MapHalfBytes),
            destinationWord: 0);
        vram.LoadMode7MapBytes(
            ceresTilemaps.AsSpan(
                CeresDestructionRomData.Vram.ClearMapSourceOffset,
                CeresDestructionRomData.Vram.MapHalfBytes),
            destinationWord: CeresDestructionRomData.Vram.ClearMapDestinationWord);
    }

    /// <summary>
    /// Creates a delayed Ceres blast actor at the placement's screen-relative offset from the station center.
    /// </summary>
    /// <param name="definition">The instruction-list and pre-instruction pair that drives this blast's animation and motion.</param>
    /// <param name="placement">The spawn offset and first-instruction delay for this blast.</param>
    private void SpawnCeresExplosion(
        CeresExplosionActorDefinition definition,
        CeresExplosionPlacement placement)
    {
        ushort x = unchecked((ushort)(CeresDestructionRomData.Rendering.CeresCenterX -
            unchecked((short)backgroundX) + placement.X));
        ushort y = unchecked((ushort)(CeresDestructionRomData.Rendering.CeresCenterY -
            unchecked((short)backgroundY) + placement.Y));
        var actor = new IntroDiscoverySprite(
            x,
            y,
            CeresDestructionRomData.Sprites.ExplosionPalette.Raw,
            definition.InstructionList);
        actor.PreInstructionPointerForDiscovery(definition.PreInstruction);

        // The native initializer writes its stagger directly to the instruction timer.
        // GeneralTimer is a different WRAM array used by decrement-and-goto opcodes; using
        // it here would make all five blasts appear immediately despite distinct delays.
        actor.DelayFirstInstruction(placement.DelayFrames);
        _ = TryAddCeresActor(actor);
    }

    /// <summary>
    /// Adds an actor to the highest available cinematic slot while reserving the explosion spawner's slot until its final frame.
    /// </summary>
    /// <param name="actor">The cinematic sprite to register in both actor collections.</param>
    /// <returns><see langword="true"/> if a slot was assigned; otherwise, <see langword="false"/> when the native slot range is full.</returns>
    private bool TryAddCeresActor(IntroDiscoverySprite actor)
    {
        // Native allocation searches the highest free slot. The invisible spawner
        // owns its slot until its final spawn instruction and deletion have run.
        for (int slot = CeresDestructionRomData.Sprites.AsteroidSlot; slot >= 0; slot--)
        {
            if (slot == CeresDestructionRomData.Sprites.SpawnerSlot &&
                explosionSpawnerFrame <= CeresExplosionDefinitions.SpawnerFinalFrame)
                continue;
            if (ceresActorSlots.ContainsValue(slot)) continue;
            ceresActorSlots.Add(actor, slot);
            actors.Add(actor);
            return true;
        }
        // $8B:938A returns carry set when all slots are occupied. These cinematic
        // callers intentionally do not branch on it: this is native capacity
        // behavior, not an unsupported instruction or swallowed runtime failure.
        return false;
    }

    /// <summary>
    /// Applies the gunship's fixed-point horizontal and vertical velocities to an explosion actor for one update.
    /// </summary>
    /// <param name="actor">The explosion sprite whose position and subposition are advanced.</param>
    private static void MoveExplosion(IntroDiscoverySprite actor)
    {
        ushort x = actor.XPosition;
        ushort xSub = actor.XSubPosition;
        ushort y = actor.YPosition;
        ushort ySub = actor.YSubPosition;
        IntroCinematicMotion.AddSixteenSixteen(
            ref x,
            ref xSub,
            0,
            CeresDestructionRomData.Motion.GunshipXSubvelocity);
        IntroCinematicMotion.AddSixteenSixteen(
            ref y,
            ref ySub,
            CeresDestructionRomData.Motion.GunshipYVelocity,
            CeresDestructionRomData.Motion.GunshipYSubvelocity);
        actor.XPosition = x;
        actor.XSubPosition = xSub;
        actor.YPosition = y;
        actor.YSubPosition = ySub;
    }

    /// <summary>
    /// Advances the Zebes planet, stars, and title actors, applying slide acceleration and scene-completion handoff when requested.
    /// </summary>
    /// <param name="slidingAway">Whether actors should receive their slide pre-instructions and move upward with acceleration.</param>
    private void StepZebesActors(bool slidingAway)
    {
        for (int index = actors.Count - 1; index >= 0; index--)
        {
            IntroDiscoverySprite actor = actors[index];
            if (slidingAway)
            {
                CeresDestructionActorDefinition definition = ReferenceEquals(actor, zebesPlanetActor)
                    ? CeresDestructionActorDefinitions.ZebesActor(0)
                    : ReferenceEquals(actor, zebesCompletionStarActor)
                        ? CeresDestructionActorDefinitions.ZebesActor(4)
                        : ReferenceEquals(actor, zebesTitleActor)
                            ? CeresDestructionActorDefinitions.ZebesActor(5)
                            : CeresDestructionActorDefinitions.ZebesActor(1);
                if (definition.SlidePreInstruction == 0)
                {
                    throw new InvalidDataException(
                        "PLANET ZEBES title actor remained active when scene sliding began.");
                }
                actor.PreInstructionPointerForDiscovery(definition.SlidePreInstruction);
                // Zebes accelerates by $40 in 8.8; all four star sheets use $20. Star
                // sheet five is the native completion owner at C8F2. Identify the actors
                // by ownership, not their mutable list index: native actors delete as they
                // cross -$80, so indexes necessarily shift during this loop.
                ushort acceleration = definition.SlideAcceleration;
                actor.GeneralTimer = unchecked((ushort)(actor.GeneralTimer + acceleration));
                SubtractEightEightY(actor, actor.GeneralTimer);
                if (unchecked((short)actor.YPosition) < -128)
                {
                    // `$8B:C885/$C8F2/$C95D/$C987` delete each object before its 8-bit
                    // OAM Y coordinate can wrap below the screen. Only star sheet five
                    // additionally installs CADF, handing the cinematic back to game load.
                    if (definition.CompletesScene)
                    {
                        Phase = CeresDestructionPhase.Finished;
                        return;
                    }

                    actor.Delete();
                    actors.RemoveAt(index);
                    continue;
                }
            }

            actor.Step(bus, HandleZebesInstruction,
                CeresDestructionSpriteInstructionDefinitions.ReadWord);
            if (!actor.IsActive)
                actors.RemoveAt(index);
        }
    }

    /// <summary>
    /// Handles the cinematic opcodes that fade the Zebes title or begin the camera flight toward the planet.
    /// </summary>
    /// <param name="opcode">The instruction opcode reached by the title actor.</param>
    /// <param name="cursor">The instruction cursor after the opcode and any operands have been consumed.</param>
    /// <returns>The cursor to resume for handled no-op title instructions, or <see langword="null"/> when normal actor handling should process the opcode.</returns>
    private ushort? HandleZebesInstruction(ushort opcode, ushort cursor)
    {
        switch (opcode)
        {
            case CinematicCodePointers.Instruction_FadeInPlanetZebesText:
            case CinematicCodePointers.Instruction_SpawnPlanetZebesJapanTextIfNeeded:
            case CinematicCodePointers.Instruction_FadeOutPlanetZebesText:
                return cursor;

            case CinematicCodePointers.Instruction_StartFlyingToZebes:
                // The title actor, not a host timer, publishes the camera flight exactly
                // where its cartridge instruction list reaches C9C7.
                backgroundX = CeresDestructionRomData.Motion.Mode7ExitX;
                backgroundY = unchecked((ushort)-112);
                angle = CeresDestructionRomData.Motion.ApproachAngle;
                zoom = CeresDestructionRomData.Motion.MinimumScale;
                Phase = CeresDestructionPhase.FlyingTowardZebesA;
                return cursor;

            default:
                return null;
        }
    }

    /// <summary>
    /// Adds a fractional horizontal displacement and wraps the resulting world X coordinate to the cinematic map width.
    /// </summary>
    /// <param name="actor">The sprite whose X position and subposition are updated.</param>
    /// <param name="fractionalDelta">The unsigned 16-bit fractional displacement to add.</param>
    private static void AddWrappedX(IntroDiscoverySprite actor, ushort fractionalDelta)
    {
        ushort x = actor.XPosition;
        ushort sub = actor.XSubPosition;
        IntroCinematicMotion.AddSixteenSixteen(ref x, ref sub, 0, fractionalDelta);
        actor.XPosition = (ushort)(x & CeresDestructionRomData.Rendering.WorldXMask);
        actor.XSubPosition = sub;
    }

    /// <summary>
    /// Subtracts an unsigned 8.8 vertical velocity from the actor's 16.16 fixed-point Y position.
    /// </summary>
    /// <param name="actor">The sprite whose Y position and subposition are updated.</param>
    /// <param name="velocity">The 8.8 speed to subtract, including its fractional component.</param>
    private static void SubtractEightEightY(IntroDiscoverySprite actor, ushort velocity)
    {
        // The bank-$8B pre-instructions use XBA to split an unsigned 8.8 velocity into
        // whole/subposition words before a 16-bit subtraction with borrow.
        uint fixedPosition = ((uint)actor.YPosition << 16) | actor.YSubPosition;
        uint fixedVelocity = (uint)velocity << 8;
        fixedPosition = unchecked(fixedPosition - fixedVelocity);
        actor.YPosition = unchecked((ushort)(fixedPosition >> 16));
        actor.YSubPosition = unchecked((ushort)fixedPosition);
    }
}
