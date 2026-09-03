using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Movement, attack-selection, transfer, explosion, beam, and damage helpers.
/// </summary>
public sealed partial class MotherBrainRainbowBeamAttackSequence
{
    private void BeginExtendingNeckForAttack()
    {
        // `$A9:B8EB-$B916` resets the neutral phase-two head program and selects the
        // ordinary 2/4 neck geometry before beginning its first 256-count wait.
        SetHeadInstructionList(HeadNeutralPhase2InstructionList);
        NeckAngleDelta = 0x0040;
        NeckMovementEnabled = 1;
        LowerNeckMovementIndex = 2;
        UpperNeckMovementIndex = 4;
        Phase = MotherBrainRainbowBeamAttackPhase.StartCharging;
        FunctionTimer = 0x0100;
    }

    private void SetHeadInstructionList(ushort pointer)
    {
        // `$A9:C447` writes the separate brain-list pointer and timer. Unlike ordinary
        // enemy programs it has no loop-counter write here.
        HeadInstructionList = pointer;
        HeadInstructionPointer = pointer;
        HeadInstructionTimer = 1;
        _headInstructionListRequestSerial++;
    }

    private void AimOnionRings(short deltaX, short deltaY)
    {
        // `$A0:C0B1` returns the game's byte angle. `$A9:9E77` converts it to the projectile
        // convention (`$80-angle`) and performs a circular signed clamp: `$10-$47` survive,
        // `$48-$BF` clamp to `$48`, and `$C0-$FF/$00-$0F` clamp to `$10`.
        SnesAngle sourceAngle = SamusGrappleMovement.CalculateAngleFromXY(deltaX, deltaY);
        byte candidate = SnesAngle.HalfTurn.AddRaw(-sourceAngle.RawValue).TableIndex;
        OnionRingTargetAngle = SnesAngle.FromTableIndex(candidate switch
        {
            >= 0x10 and < 0x48 => candidate,
            >= 0x48 and < 0xc0 => 0x48,
            _ => 0x10,
        });
    }

    private static ushort ReadBankA9Word(ISnesAddressSpace bus, ushort address)
    {
        int lowAddress = 0xa90000 | address;
        return unchecked((ushort)(
            bus.ReadByte(lowAddress) |
            (bus.ReadByte(0xa90000 | unchecked((ushort)(address + 1))) << 8)));
    }

    private void RetractHead()
    {
        // NTSC takes `$0050` from the regional `$0050/$0063` constant at `$A9:BB51`.
        NeckAngleDelta = 0x0050;
        NeckMovementEnabled = 1;
        LowerNeckMovementIndex = 8;
        UpperNeckMovementIndex = 6;
    }

    private bool StepPainfulWalking()
    {
        // The native stores four separate function pointers (request forward, wait forward,
        // request backward, wait backward). Two booleans express the same state without
        // hiding when a body instruction list is actually installed.
        if (PainfulWalkingFunctionTimer != 0)
        {
            PainfulWalkingFunctionTimer = unchecked((ushort)(PainfulWalkingFunctionTimer - 1));
            if (PainfulWalkingFunctionTimer == 0)
            {
                PainfulWalkingStage++;
                PainfulWalkingForward = !PainfulWalkingForward;
            }
            return false;
        }

        bool reachedTarget;
        bool requested;
        if (PainfulWalkingForward)
        {
            requested = RequestWalkForward(0x0048, PainfulWalkingAnimationDelay);
            reachedTarget = unchecked((short)(0x0048 - Body.XPosition)) < 0 ||
                NativeAtLeast(Body.XPosition, 0x0080);
        }
        else
        {
            requested = RequestWalkBackward(0x0028, PainfulWalkingAnimationDelay);
            reachedTarget = HasReachedBackwardTarget(0x0028);
        }

        if (reachedTarget)
        {
            int timerIndex = Math.Min(PainfulWalkingStage, (ushort)7);
            PainfulWalkingFunctionTimer = PainfulWalkingFunctionTimers[timerIndex];
        }
        return requested;
    }

    private bool StepPhase3WalkingHandler(ushort randomNumberSeed)
    {
        // `$C25A` refuses to call the walking function while body bytecode advertises any
        // nonzero pose. This makes each request edge-triggered: the requested walk runs to
        // its standing opcode before the scheduler is allowed to inspect its target again.
        if (Body.Pose != 0)
            return false;

        switch (Phase3WalkingPhase)
        {
            case MotherBrainPhase3WalkingPhase.Inactive:
                return false;

            case MotherBrainPhase3WalkingPhase.TryToInchForward:
                if (Phase3WalkCounter == 0)
                {
                    // Zero credit does not merely pause. `$C2A1` chooses a point fourteen
                    // pixels left, installs the quick-retreat function, and falls into it.
                    Phase3TargetXPosition = unchecked((ushort)(Body.XPosition - 0x000e));
                    Phase3WalkingPhase = MotherBrainPhase3WalkingPhase.RetreatQuickly;
                    goto case MotherBrainPhase3WalkingPhase.RetreatQuickly;
                }

                // Native ADC/CMP is unsigned here. A wrapped counter can therefore spend
                // more calls below `$0100`; no host integer widening or saturation belongs.
                Phase3WalkCounter = unchecked((ushort)(Phase3WalkCounter + 0x0020));
                if (Phase3WalkCounter < 0x0100)
                    return false;

                Phase3TargetXPosition = unchecked((ushort)(Body.XPosition + 1));
                ushort forwardDelay = unchecked((ushort)((randomNumberSeed & 2) + 4));

                // MakeMotherBrainWalkForwards reports carry only for a strictly overshot
                // target or the independent X `$80` limit. Equality still requests a walk.
                bool reachedForwardTarget =
                    unchecked((short)(Phase3TargetXPosition - Body.XPosition)) < 0 ||
                    NativeAtLeast(Body.XPosition, 0x0080);
                if (reachedForwardTarget)
                {
                    Phase3WalkCounter = 0x0080;
                    return false;
                }

                return RequestWalkForward(Phase3TargetXPosition, forwardDelay);

            case MotherBrainPhase3WalkingPhase.RetreatQuickly:
                if (!HasReachedBackwardTarget(Phase3TargetXPosition))
                    return RequestWalkBackward(Phase3TargetXPosition, animationDelay: 0x0002);

                // Reaching the first target chooses another point fourteen pixels left but
                // does not fall through to the slow request. That request starts only on the
                // next standing AI call.
                Phase3TargetXPosition = unchecked((ushort)(Body.XPosition - 0x000e));
                Phase3WalkingPhase = MotherBrainPhase3WalkingPhase.RetreatSlowly;
                return false;

            case MotherBrainPhase3WalkingPhase.RetreatSlowly:
                if (!HasReachedBackwardTarget(Phase3TargetXPosition))
                    return RequestWalkBackward(Phase3TargetXPosition, animationDelay: 0x0004);

                // `$C313` writes all three words: forty walk-credit, the inch-forward
                // function, and a one-pixel-ahead target used by later calls.
                SetPhase3WalkingToTryToInchForward(0x0040);
                return false;

            default:
                throw new InvalidOperationException(
                    $"Unsupported phase-three walking function {Phase3WalkingPhase}.");
        }
    }

    private void SetPhase3WalkingToTryToInchForward(ushort counter)
    {
        Phase3WalkCounter = counter;
        Phase3WalkingPhase = MotherBrainPhase3WalkingPhase.TryToInchForward;
        Phase3TargetXPosition = unchecked((ushort)(Body.XPosition + 1));
    }

    private void StepPhase3NeckHandler()
    {
        switch (Phase3NeckPhase)
        {
            case MotherBrainPhase3NeckPhase.Inactive:
                return;

            case MotherBrainPhase3NeckPhase.Normal:
                // `$C330` briefly writes lower index one and immediately overwrites it with
                // two before any other code can observe the word. Preserve the observable
                // result while documenting the retail no-op (`>_<` in the disassembly).
                NeckAngleDelta = 0x0080;
                LowerNeckMovementIndex = 2;
                UpperNeckMovementIndex = 4;
                Phase3NeckPhase = MotherBrainPhase3NeckPhase.Inactive;
                return;

            case MotherBrainPhase3NeckPhase.SetupRecoilRecovery:
                NeckMovementEnabled = 1;
                NeckAngleDelta = 0x0500;
                LowerNeckMovementIndex = 6;
                UpperNeckMovementIndex = 6;
                Phase3NeckPhase = MotherBrainPhase3NeckPhase.RecoilRecovery;
                Phase3NeckFunctionTimer = 0x0010;

                // `$C354` falls directly into `$C37B`; the newly loaded sixteen is already
                // fifteen when this single handler call returns.
                goto case MotherBrainPhase3NeckPhase.RecoilRecovery;

            case MotherBrainPhase3NeckPhase.RecoilRecovery:
                if (Phase3NeckFunctionTimer != 0)
                {
                    Phase3NeckFunctionTimer =
                        unchecked((ushort)(Phase3NeckFunctionTimer - 1));
                    return;
                }

                // DEC of zero branches negative without storing `$FFFF`, so the readable
                // timer remains zero while the four-ring recovery list is installed.
                SetHeadInstructionList(HeadAttackingFourOnionRingsPhase3InstructionList);
                Phase3NeckPhase = MotherBrainPhase3NeckPhase.Normal;
                return;

            case MotherBrainPhase3NeckPhase.SetupHyperBeamRecoil:
                NeckMovementEnabled = 1;
                Phase3DisableAttacks = 1;
                SetHeadInstructionList(HeadHyperBeamRecoilInstructionList);
                BrainMainShakeTimer = 0x0032;
                NeckAngleDelta = 0x0900;
                LowerNeckMovementIndex = 8;
                UpperNeckMovementIndex = 8;
                Phase3NeckPhase = MotherBrainPhase3NeckPhase.HyperBeamRecoil;
                Phase3NeckFunctionTimer = 0x000b;

                // Setup also falls through, making the externally visible first timer `$0A`.
                goto case MotherBrainPhase3NeckPhase.HyperBeamRecoil;

            case MotherBrainPhase3NeckPhase.HyperBeamRecoil:
                if (Phase3NeckFunctionTimer != 0)
                {
                    Phase3NeckFunctionTimer =
                        unchecked((ushort)(Phase3NeckFunctionTimer - 1));
                    return;
                }

                NeckAngleDelta = 0x0080;
                Phase3DisableAttacks = 0;
                Phase3NeckPhase = MotherBrainPhase3NeckPhase.SetupRecoilRecovery;
                return;

            default:
                throw new InvalidOperationException(
                    $"Unsupported phase-three neck function {Phase3NeckPhase}.");
        }
    }

    private bool RequestWalkForward(ushort targetX, ushort animationDelay)
    {
        if (unchecked((short)(targetX - Body.XPosition)) < 0 || Body.Pose != 0 ||
            NativeAtLeast(Body.XPosition, 0x0080))
            return false;

        ushort pointer = animationDelay switch
        {
            0x0002 => BodyWalkingForwardReallyFastInstructionList,
            0x0004 => BodyWalkingForwardFastInstructionList,
            0x0006 => BodyWalkingForwardMediumInstructionList,
            0x0008 => BodyWalkingForwardSlowInstructionList,
            0x000a => BodyWalkingForwardReallySlowInstructionList,
            _ => throw new InvalidOperationException(
                $"Unsupported painful forward animation delay ${animationDelay:X4}."),
        };
        Body.SetInstructionList(pointer);
        return true;
    }

    private bool RequestWalkBackward(ushort targetX, ushort animationDelay)
    {
        if (HasReachedBackwardTarget(targetX) || Body.Pose != 0)
            return false;

        ushort pointer = animationDelay switch
        {
            0x0002 => BodyWalkingBackwardReallyFastInstructionList,
            0x0004 => BodyWalkingBackwardFastInstructionList,
            0x0006 => BodyWalkingBackwardMediumInstructionList,
            0x0008 => BodyWalkingBackwardSlowInstructionList,
            0x000a => BodyWalkingBackwardReallySlowInstructionList,
            _ => throw new InvalidOperationException(
                $"Unsupported painful backward animation delay ${animationDelay:X4}."),
        };
        Body.SetInstructionList(pointer);
        return true;
    }

    private bool RequestWalkForwardReallySlow(ushort targetX)
    {
        // `$A9:C601` first performs signed CMP/BMI against the target. At equality it still
        // examines pose and the hard `$80` arena limit; only a strictly overshot target sets
        // carry at the first branch.
        if (unchecked((short)(targetX - Body.XPosition)) < 0)
            return false;
        if (Body.Pose != 0)
            return false;
        if (NativeAtLeast(Body.XPosition, 0x0080))
            return false;

        Body.SetInstructionList(BodyWalkingForwardReallySlowInstructionList);
        return true;
    }

    private bool RequestWalkBackwardReallySlow(ushort targetX)
    {
        if (HasReachedBackwardTarget(targetX) || Body.Pose != 0)
            return false;

        Body.SetInstructionList(BodyWalkingBackwardReallySlowInstructionList);
        return true;
    }

    private bool HasReachedBackwardTarget(ushort targetX) =>
        // `$A9:C647` reports carry at target/left of target or below the independent `$30`
        // room limit. This check runs before pose, so an in-progress walk can complete the AI
        // phase on the exact frame one of its movement opcodes reaches X `$28`.
        unchecked((short)(targetX - Body.XPosition)) >= 0 ||
        unchecked((short)(Body.XPosition - 0x0030)) < 0;

    private static ushort CalculateFinishOffHealthThreshold(SamusState samus, ushort nominalDamage)
    {
        // `$A0:A45E` gives Gravity Suit priority and divides damage by four. Otherwise
        // Varia's bit zero divides by two; Power Suit leaves it untouched. `$A9:BD4C`
        // then multiplies the divided `$50` result by four, while `$BD68` deliberately
        // does not multiply the divided `$A0` result. The caller selects that distinction.
        ushort divided = samus.EquippedItems.HasAny(SamusEquipmentFlags.GravitySuit)
            ? (ushort)(nominalDamage >> 2)
            : samus.EquippedItems.HasAny(SamusEquipmentFlags.VariaSuit)
                ? (ushort)(nominalDamage >> 1)
                : nominalDamage;

        return nominalDamage == 0x0050
            ? unchecked((ushort)(divided * 4 + 0x0014))
            : unchecked((ushort)(divided + 0x0014));
    }

    private bool MaybeRequestStandUpOrLeanDown(ushort randomNumberSeed)
    {
        // `$A9:C1A7` ignores every pose except standing (zero) and leaning (six), and its
        // low-byte threshold is inclusive at `$C0`. A request still executes as body
        // instruction bytecode later in the enemy-processing stage.
        if ((randomNumberSeed & 0x00ff) < 0x00c0)
            return false;

        if (Body.Pose == 0)
        {
            Body.SetInstructionList(BodyLeaningDownInstructionList);
            return true;
        }

        if (Body.Pose == 6)
        {
            MakeBodyStandUp(out bool requested);
            return requested;
        }

        return false;
    }

    private bool MakeBodyStandUp(out bool animationRequested)
    {
        animationRequested = false;
        if (Body.Pose == 0)
            return true;

        ushort instructionList = Body.Pose switch
        {
            3 => BodyStandingUpAfterCrouchingFastInstructionList,
            6 => BodyStandingUpAfterLeaningDownInstructionList,
            _ => 0,
        };
        if (instructionList != 0)
        {
            Body.SetInstructionList(instructionList);
            animationRequested = true;
        }

        return false;
    }

    private MotherBrainSpriteTileTransferRequest CreateNextBabyMetroidTileTransfer()
    {
        int index = BabyMetroidTileTransferIndex;
        if ((uint)index >= (uint)BabyMetroidTileSources.Length)
            throw new InvalidOperationException("Baby Metroid sprite-tile transfer list is already complete.");

        var request = new MotherBrainSpriteTileTransferRequest(
            EntryIndex: (ushort)index,
            Size: 0x0200,
            SourceAddress: BabyMetroidTileSources[index],
            VramDestination: BabyMetroidTileDestinations[index]);
        BabyMetroidTileTransferIndex++;
        return request;
    }

    private MotherBrainSpriteTileTransferRequest CreateNextCorpseTileTransfer()
    {
        int index = CorpseTileTransferIndex;
        if ((uint)index >= (uint)CorpseTileSources.Length)
            throw new InvalidOperationException("Mother Brain corpse tile transfer list is already complete.");

        var request = new MotherBrainSpriteTileTransferRequest(
            EntryIndex: (ushort)index,
            Size: 0x01c0,
            SourceAddress: CorpseTileSources[index],
            VramDestination: CorpseTileDestinations[index]);
        CorpseTileTransferIndex++;
        return request;
    }

    private MotherBrainSpriteTileTransferRequest CreateNextEscapeTimerTileTransfer()
    {
        int index = EscapeTimerTileTransferIndex;
        if ((uint)index >= (uint)EscapeTimerTileTransfers.Length)
            throw new InvalidOperationException("Escape-timer sprite-tile transfer list is already complete.");

        MotherBrainSpriteTileTransferRequest request = EscapeTimerTileTransfers[index];
        EscapeTimerTileTransferIndex++;
        return request;
    }

    private MotherBrainSpriteTileTransferRequest CreateNextExplodedDoorTileTransfer()
    {
        int index = ExplodedDoorTileTransferIndex;
        if ((uint)index >= (uint)ExplodedDoorTileTransfers.Length)
            throw new InvalidOperationException("Exploded-door sprite-tile transfer list is already complete.");

        MotherBrainSpriteTileTransferRequest request = ExplodedDoorTileTransfers[index];
        ExplodedDoorTileTransferIndex++;
        return request;
    }

    private void GenerateDeathExplosions(
        bool mixed,
        Func<ushort>? nextRandomNumber,
        List<MotherBrainDeathExplosionRequest> requests)
    {
        // `$B03E` decrements before testing BPL. A cleared timer therefore wraps and emits
        // immediately. Crucially, the interval literal remains in A across the memory DEC,
        // so the smoky/mixed caller writes exactly `$10`/`$08` after an expiry.
        DeathExplosionIntervalTimer = unchecked((ushort)(DeathExplosionIntervalTimer - 1));
        if ((DeathExplosionIntervalTimer & 0x8000) == 0)
            return;

        DeathExplosionIntervalTimer = mixed ? (ushort)0x0008 : (ushort)0x0010;
        DeathExplosionIndex = unchecked((ushort)(DeathExplosionIndex - 1));
        if ((DeathExplosionIndex & 0x8000) != 0)
            DeathExplosionIndex = 6;

        int simultaneousCount = mixed ? 4 : 2;
        int pairIndex = DeathExplosionIndex * 4;
        for (int explosionIndex = 0; explosionIndex < simultaneousCount; explosionIndex++)
        {
            // The global RNG is called once per projectile, not once per visual batch. A
            // callback is required here so the owning frame runtime advances its real global
            // state; silently deriving private random values would desynchronize later AI.
            if (nextRandomNumber is null)
            {
                throw new InvalidOperationException(
                    "Mother Brain death explosions require the global next-random-number producer.");
            }

            ushort random = nextRandomNumber();
            ushort parameter = mixed
                ? random < 0x4000 ? (ushort)0 : random < 0xe000 ? (ushort)1 : (ushort)2
                : (ushort)1;
            (short xOffset, short yOffset) = DeathExplosionOffsets[pairIndex + explosionIndex];
            requests.Add(new MotherBrainDeathExplosionRequest(
                PatternIndex: DeathExplosionIndex,
                XOffset: xOffset,
                YOffset: yOffset,
                XPosition: unchecked((ushort)(Body.XPosition + xOffset)),
                YPosition: unchecked((ushort)(Body.YPosition + yOffset)),
                ProjectileParameter: parameter,
                SoundEffect: 0x0013));
        }
    }

    private MotherBrainEscapeDoorExplosionRequest? GenerateEscapeDoorExplosion(
        Func<ushort>? nextRandomNumber)
    {
        // `$B346` shares the old death-explosion interval word but replaces its cadence with
        // four. A cleared word underflows on the first call, so a new explosion is immediate.
        DeathExplosionIntervalTimer = unchecked((ushort)(DeathExplosionIntervalTimer - 1));
        if ((DeathExplosionIntervalTimer & 0x8000) == 0)
            return null;

        DeathExplosionIntervalTimer = 0x0004;
        EscapeDoorIndex = unchecked((ushort)(EscapeDoorIndex - 1));
        if ((EscapeDoorIndex & 0x8000) != 0)
            EscapeDoorIndex = 3;

        // The table is stored as interleaved X/Y words and indexed by `index * 4` bytes.
        // Spell out the semantic pairs so neither host endianness nor array stride can alter
        // the native 3,2,1,0 repeating order.
        (ushort x, ushort y) = EscapeDoorIndex switch
        {
            0 => ((ushort)0x0008, (ushort)0x006c),
            1 => ((ushort)0x0018, (ushort)0x0080),
            2 => ((ushort)0x0009, (ushort)0x0090),
            3 => ((ushort)0x0018, (ushort)0x0074),
            _ => throw new InvalidOperationException(
                $"Escape-door explosion index ${EscapeDoorIndex:X4} escaped its native 0..3 range."),
        };

        if (nextRandomNumber is null)
        {
            throw new InvalidOperationException(
                "Escape-door explosions require the global next-random-number producer.");
        }

        // Native `$B377` advances the one shared RNG exactly once per emitted projectile.
        // Values below `$4000` select the bright explosion parameter `$000C`; all remaining
        // values retain the preloaded smoky-dust parameter `$0003`.
        ushort random = nextRandomNumber();
        ushort projectileParameter = random < 0x4000 ? (ushort)0x000c : (ushort)0x0003;
        return new MotherBrainEscapeDoorExplosionRequest(
            PatternIndex: EscapeDoorIndex,
            XPosition: x,
            YPosition: y,
            ProjectileParameter: projectileParameter,
            SoundEffect: 0x0024);
    }

    private void IncreaseWidthAndAim(SamusState samus)
    {
        ushort widened = unchecked((ushort)(AngularWidth + 0x0180));
        AngularWidth = NativeAtLeast(widened, 0x0c00) ? (ushort)0x0c00 : widened;
        AimBeamAtSamus(samus);
    }

    private void RunContinuingBeamEffects(
        SamusState samus,
        ushort enemyFrameCounter,
        bool increaseWidth,
        ref bool soundQueued,
        ref bool paletteRequested,
        ref MotherBrainRainbowExplosionRequest? explosion)
    {
        // `$A9:BB2E` checks the signed counter before DEC. Starting at six therefore queues
        // on values 6,5,4,3,2,1,0 and leaves `$FFFF` to suppress all later calls.
        if ((SoundQueueCount & 0x8000) == 0)
        {
            SoundQueueCount = unchecked((ushort)(SoundQueueCount - 1));
            RainbowBeamSoundPlaying = true;
            soundQueued = true;
        }

        // Palette handling runs only when enemy frame-counter bit one is set. The palette
        // program itself remains presentation work, but its native cadence is observable.
        paletteRequested = (enemyFrameCounter & 2) != 0;

        if (increaseWidth)
            IncreaseWidthAndAim(samus);
        else
            AimBeamAtSamus(samus);
        explosion = StepExplosionTimer();
    }

    private void AimBeamAtSamus(SamusState samus)
    {
        // `$A9:BB82` aims from brain position (+16,+4) to Samus. Bank `$A0:C0B1` consumes
        // signed 16-bit offsets; `$A9:BBA0-$BBA8` converts its clockwise-up angle to the
        // rainbow beam's anti-clockwise-down convention with `-$angle + $80`.
        short deltaX = unchecked((short)(samus.XPosition - BrainXPosition - 0x0010));
        short deltaY = unchecked((short)(samus.YPosition - BrainYPosition - 0x0004));
        SnesAngle sourceAngle = SamusGrappleMovement.CalculateAngleFromXY(deltaX, deltaY);
        _movement.RainbowBeamAngle = SnesAngle.HalfTurn.AddRaw(-sourceAngle.RawValue);
    }

    private MotherBrainRainbowExplosionRequest? StepExplosionTimer()
    {
        ExplosionTimer = unchecked((ushort)(ExplosionTimer - 1));
        if ((ExplosionTimer & 0x8000) == 0)
            return null;

        ExplosionTimer = 8;
        ExplosionIndex++;
        int offsetIndex = ExplosionIndex & 7;
        return new MotherBrainRainbowExplosionRequest(
            ExplosionIndex,
            ExplosionXOffsets[offsetIndex],
            ExplosionYOffsets[offsetIndex],
            SoundEffect: 0x0024);
    }

    private static void DamageSamusDueToRainbowBeam(SamusState samus)
    {
        // `$A9:C57D` loads `$FFFE` in both branches. The meaningful Varia distinction is
        // carry left by LSR EquippedItems: bit zero adds one back, producing -1 with Varia
        // and -2 without it. CMP #1/BPL then clamps wrapped/below-one health to zero.
        int damage = samus.EquippedItems.HasAny(SamusEquipmentFlags.VariaSuit) ? 1 : 2;
        ushort candidate = unchecked((ushort)(samus.Health - damage));
        samus.Health = NativeAtLeast(candidate, 1) ? candidate : (ushort)0;
    }

    private static void DecrementAmmoDueToRainbowBeam(
        SamusState samus,
        ushort mainEnemyExecutionCounter)
    {
        // Missiles and supers are visited only every fourth main-enemy pass. Power bombs
        // are visited every call, matching the branch layout rather than sharing the gate.
        if ((mainEnemyExecutionCounter & 3) == 0)
        {
            samus.Missiles = DecrementOneAmmoType(samus, samus.Missiles, selectedItem: 1);
            samus.SuperMissiles = DecrementOneAmmoType(samus, samus.SuperMissiles, selectedItem: 2);
        }

        samus.PowerBombs = DecrementOneAmmoType(samus, samus.PowerBombs, selectedItem: 3);
    }

    private static ushort DecrementOneAmmoType(
        SamusState samus,
        ushort current,
        ushort selectedItem)
    {
        if (current == 0)
            return 0;

        ushort decremented = unchecked((ushort)(current - 1));
        if (NativeAtLeast(decremented, 1))
            return decremented;

        // The 65816 briefly reloads A with SelectedHUDItem for the comparison, but both the
        // matching and nonmatching branches converge at a label whose first instruction is
        // `LDA #$0000`. Therefore the depleted count is always zero; only the selected-item
        // store itself is conditional. Spell that convergence out to avoid the tempting but
        // incorrect interpretation that the HUD item number leaks into the ammo count.
        if (samus.SelectedHudItem == selectedItem)
            samus.SelectedHudItem = 0;

        samus.AutoCancelHudItemIndex = 0;
        return 0;
    }

    private static bool NativeAtLeast(ushort value, ushort threshold) =>
        unchecked((short)(value - threshold)) >= 0;
}
