namespace SuperMetroid.Core.Frontend;

internal sealed partial class CeresDestructionCinematicState
{
    private static readonly short[] Explosion2XOffsets =
        [14, 8, -16, -8, 0, 16, -12, -8];
    private static readonly short[] Explosion2YOffsets =
        [-8, 12, 12, -14, 0, 14, 4, -16];

    private void StepCeresActors()
    {
        explosionSpawnerFrame++;

        // CF33 remains invisible for $80 frames, spawns five staggered small blasts,
        // waits $50, creates one repeating blast every $0C frames during that interval,
        // then waits $40 and creates the four large terminal blasts.
        if (explosionSpawnerFrame == CeresDestructionRomData.Timing.FirstExplosionFrame)
        {
            short[] x = [16, -16, 16, -16, 0];
            short[] y = [-16, 16, 16, -16, 0];
            int[] delays = [1, 16, 32, 48, 64];
            for (int index = 0; index < x.Length; index++)
                SpawnCeresExplosion(
                    x[index],
                    y[index],
                    CeresDestructionRomData.Sprites.InitialExplosionList,
                    delays[index]);
        }
        else if (explosionSpawnerFrame is
                    > CeresDestructionRomData.Timing.FirstExplosionFrame and
                    <= CeresDestructionRomData.Timing.SecondaryExplosionLastFrame &&
                 (explosionSpawnerFrame -
                    CeresDestructionRomData.Timing.SecondaryExplosionFirstFrame) %
                    CeresDestructionRomData.Timing.SecondaryExplosionPeriod == 0)
        {
            int offset = explosionOffsetIndex++ & 7;
            SpawnCeresExplosion(
                Explosion2XOffsets[offset],
                Explosion2YOffsets[offset],
                CeresDestructionRomData.Sprites.SecondaryExplosionList,
                delay: 1);
        }
        else if (explosionSpawnerFrame == CeresDestructionRomData.Timing.FinalExplosionFrame)
        {
            short[] x = [8, 12, -8, -12];
            short[] y = [-4, 8, -10, 12];
            int[] delays = [1, 4, 8, 16];
            for (int index = 0; index < x.Length; index++)
                SpawnCeresExplosion(
                    x[index],
                    y[index],
                    CeresDestructionRomData.Sprites.FinalExplosionList,
                    delays[index]);
        }

        for (int index = actors.Count - 1; index >= 0; index--)
        {
            IntroDiscoverySprite actor = actors[index];
            if (index == 0)
                AddWrappedX(actor, CeresDestructionRomData.Motion.ExplosionFastXSubvelocity);
            else if (index == 1)
                AddWrappedX(actor, CeresDestructionRomData.Motion.ExplosionSlowXSubvelocity);
            else if (index >= 3)
                MoveExplosion(actor);

            actor.Step(bus);
            if (!actor.IsActive)
                actors.RemoveAt(index);
        }
    }

    private void SpawnFinalCeresExplosion()
    {
        ushort x = unchecked((ushort)(CeresDestructionRomData.Rendering.CeresCenterX - backgroundX));
        ushort y = unchecked((ushort)(CeresDestructionRomData.Rendering.CeresCenterY - backgroundY));
        // C345 calls the ordinary bank-$88 explosion with screen-space coordinates,
        // before replacing the station tilemap. Keep its ROM curves/colors/lifetime.
        stationExplosion.Spawn(x, y);
        audio?.QueueSound(SuperMetroid.Core.Audio.SoundEffectLibrary1Sounds.PowerBombExplosion,
            maximumQueued: 15);
        actors.Add(new IntroDiscoverySprite(
            x,
            y,
            CeresDestructionRomData.Sprites.ExplosionPalette.Raw,
            CeresDestructionRomData.Sprites.GunshipList));

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

    private void SpawnCeresExplosion(short xOffset, short yOffset, ushort list, int delay)
    {
        ushort x = unchecked((ushort)(52 - unchecked((short)backgroundX) + xOffset));
        ushort y = unchecked((ushort)(48 - unchecked((short)backgroundY) + yOffset));
        var actor = new IntroDiscoverySprite(
            x,
            y,
            CeresDestructionRomData.Sprites.ExplosionPalette.Raw,
            list);

        // The native initializer writes its stagger directly to the instruction timer.
        // GeneralTimer is a different WRAM array used by decrement-and-goto opcodes; using
        // it here would make all five blasts appear immediately despite distinct delays.
        actor.DelayFirstInstruction(unchecked((ushort)delay));
        actors.Add(actor);
    }

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

    private void StepZebesActors(bool slidingAway)
    {
        for (int index = actors.Count - 1; index >= 0; index--)
        {
            IntroDiscoverySprite actor = actors[index];
            if (slidingAway)
            {
                // Zebes accelerates by $40 in 8.8; all four star sheets use $20. Star
                // sheet five is the native completion owner at C8F2. Identify the actors
                // by ownership, not their mutable list index: native actors delete as they
                // cross -$80, so indexes necessarily shift during this loop.
                ushort acceleration = ReferenceEquals(actor, zebesPlanetActor)
                    ? CeresDestructionRomData.Motion.PlanetEightEightAcceleration
                    : CeresDestructionRomData.Motion.StarEightEightAcceleration;
                actor.GeneralTimer = unchecked((ushort)(actor.GeneralTimer + acceleration));
                SubtractEightEightY(actor, actor.GeneralTimer);
                if (unchecked((short)actor.YPosition) < -128)
                {
                    // `$8B:C885/$C8F2/$C95D/$C987` delete each object before its 8-bit
                    // OAM Y coordinate can wrap below the screen. Only star sheet five
                    // additionally installs CADF, handing the cinematic back to game load.
                    if (ReferenceEquals(actor, zebesCompletionStarActor))
                    {
                        Phase = CeresDestructionPhase.Finished;
                        return;
                    }

                    actor.Delete();
                    actors.RemoveAt(index);
                    continue;
                }
            }

            actor.Step(bus, HandleZebesInstruction);
            if (!actor.IsActive)
                actors.RemoveAt(index);
        }
    }

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

    private static void AddWrappedX(IntroDiscoverySprite actor, ushort fractionalDelta)
    {
        ushort x = actor.XPosition;
        ushort sub = actor.XSubPosition;
        IntroCinematicMotion.AddSixteenSixteen(ref x, ref sub, 0, fractionalDelta);
        actor.XPosition = (ushort)(x & CeresDestructionRomData.Rendering.WorldXMask);
        actor.XSubPosition = sub;
    }

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
