using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Bomb and Golden Torizo's projectiles: drool, swipes, sonic booms, Chozo orbs, eggs, super missiles, eye beams and explosions.</summary>
[RoomLifetime]
internal sealed class TorizoAttacks
{
    /// <summary>Supplies the live room services, boss flag, random state, and artwork for both statue variants.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Owns the eighteen-slot enemy projectile pool used by Torizo attacks.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Queues the statue-break sound with the enemy audio limits.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Spawns the orb drops and the death scatter after the authored sequences.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;
    /// <summary>Drop requests published by the Chozo-orb projectile instruction.</summary>
    internal readonly List<TorizoOrbDropRequest> TorizoOrbDropRequests = new();
    /// <summary>Bomb and Golden Torizo's state lookups, facing, posture and the function word their branches test.</summary>
    private readonly TorizoEncounterAccess _access;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal TorizoAttacks(EnemyRoomBinding room, EnemyProjectileEngine projectiles, EnemyAudioQueue audio, EnemyDeathsAndDrops deathsAndDrops, TorizoEncounterAccess access)
    {
        _room = room;
        _projectiles = projectiles;
        _audio = audio;
        _deathsAndDrops = deathsAndDrops;
        _access = access;
    }

    /// <summary>Golden Torizo's state while definition $EF7F owns slot zero.</summary>
    public TorizoEnemyState? GoldenTorizo =>
        _access.TorizoState is { IsGolden: true } ? _access.TorizoState : null;

    /// <summary>Installs movement handlers and typed program cues for statue fragments, orbs, attacks, and reflected missiles.</summary>
    internal void RegisterBombTorizoProjectiles(EnemyProjectileHandlerTable table)
    {
        table.AddPreInstruction(context =>
        {
            RunBombTorizoStatueBreakingPreInstruction(context.Projectile, context.Level);
        }, EnemyProjectilePreInstruction.BombTorizoChozoBreaking_Falling);

        table.AddPreInstruction(context =>
        {
            RunBombTorizoChozoOrbPreInstruction(context.Projectile, context.Level);
        }, EnemyProjectilePreInstruction.BombTorizosChozoOrbs);

        table.AddPreInstruction(context =>
        {
            RunGoldenTorizoChozoOrbPreInstruction(context.Projectile, context.Level);
        }, EnemyProjectilePreInstruction.GoldenTorizosChozoOrbs);

        table.AddPreInstruction(context =>
        {
            RunBombTorizoSonicBoomPreInstruction(context.Projectile, context.Level);
        }, EnemyProjectilePreInstruction.TorizoSonicBoom);

        table.AddPreInstruction(context =>
        {
            RunBombTorizoDroolPreInstruction(context.Projectile, context.Level);
        }, EnemyProjectilePreInstruction.BombTorizoLowHealthDrool_Falling);

        table.AddPreInstruction(context =>
        {
            RunGoldenTorizoEggPreInstruction(context.Projectile, context.Level);
        }, EnemyProjectilePreInstruction.GoldenTorizoEgg_Bouncing);

        table.AddPreInstruction(context =>
        {
            RunGoldenTorizoEggHorizontalCharge(context.Projectile, context.Level);
        }, EnemyProjectilePreInstruction.GoldenTorizoEgg_Hatched);

        table.AddPreInstruction(context =>
        {
            RunGoldenTorizoEggFall(context.Projectile, context.Level);
        }, EnemyProjectilePreInstruction.GoldenTorizoEgg_HitWall);

        table.AddPreInstruction(context =>
        {
            RunGoldenTorizoSuperMissilePreInstruction(context.Projectile);
        }, EnemyProjectilePreInstruction.GoldenTorizoSuperMissile_Held);

        table.AddPreInstruction(context =>
        {
            RunGoldenTorizoSuperMissileFlight(context.Projectile, context.Level);
        }, EnemyProjectilePreInstruction.GoldenTorizoSuperMissile_Thrown);

        table.AddPreInstruction(context =>
        {
            RunGoldenTorizoEyeBeamPreInstruction(context.Projectile, context.Level);
        }, EnemyProjectilePreInstruction.GoldenTorizoEyeBeam);

        table.AddBranchingCue<TorizoProjectileProgram, TorizoProjectileCue>((context, cue) => ApplyTorizoProjectileCue(context.Projectile, context.Samus, cue));
        table.AddShotAnimation(RoomEnemyProjectileKind.BombTorizoChozoOrb, () => new TorizoProjectileAnimation(TorizoProjectileProgram.ChozoOrbShot));
        table.AddShotAnimation(RoomEnemyProjectileKind.GoldenTorizoChozoOrb, () => new TorizoProjectileAnimation(TorizoProjectileProgram.ChozoOrbShot));
        // The egg's definition names the Chozo orb's wall-impact list as its shot list.
        table.AddShotAnimation(RoomEnemyProjectileKind.GoldenTorizoEgg, () => new TorizoProjectileAnimation(TorizoProjectileProgram.ChozoOrbWallImpact));
        table.AddShotAnimation(RoomEnemyProjectileKind.GoldenTorizoSuperMissile, () => new TorizoProjectileAnimation(TorizoProjectileProgram.SuperMissileImpact));
    }

    /// <summary>
    /// Carries out one cue of a Torizo projectile's program, returning the program a conditional cue continues at, or
    /// null to run on.
    /// </summary>
    internal TorizoProjectileProgram? ApplyTorizoProjectileCue(RoomEnemyProjectileSlot projectile, SamusState? samus, TorizoProjectileCue cue)
    {
        switch (cue)
        {
            case TorizoProjectileCue.Command { Kind: TorizoProjectileCueKind.ClearPreInstruction }:
                projectile.PreInstruction = EnemyProjectilePreInstruction.RTS_868170;
                return null;
            case TorizoProjectileCue.SetPreInstruction setPreInstruction:
                projectile.PreInstruction = setPreInstruction.PreInstruction;
                return null;
            case TorizoProjectileCue.SetProperties setProperties:
                EnemyProjectileEngine.SetPropertyFlags(projectile, setProperties.Properties);
                return null;
            case TorizoProjectileCue.ClearProperties clearProperties:
                EnemyProjectileEngine.ClearPropertyFlags(projectile, clearProperties.Properties);
                return null;
            case TorizoProjectileCue.PlaySound playSound:
                _audio.QueueEnemySound(playSound.Sound, playSound.MaximumQueued);
                return null;
            case TorizoProjectileCue.Command { Kind: TorizoProjectileCueKind.Delete }:
                projectile.Clear();
                return null;
            case TorizoProjectileCue.Command { Kind: TorizoProjectileCueKind.Hatch }:
                // Variable 0 holds the launching Torizo's facing word, negative when it faces right.
                return unchecked((short)projectile.Variable0) < 0 ? TorizoProjectileProgram.EggHatchedRight : TorizoProjectileProgram.EggHatchedLeft;
            case TorizoProjectileCue.GoToIfEyeBeamExplosionsDisabled goToIfEyeBeamExplosionsDisabled:
            {
                TorizoEnemyState state = GoldenTorizo ??
                    throw new InvalidOperationException("Golden Torizo's eye beam has no owning Torizo state.");
                return state.EyeBeamExplosionsEnabled ? null : goToIfEyeBeamExplosionsDisabled.Target;
            }
            case TorizoProjectileCue.GoToSometimes goToSometimes:
                return _projectiles.TakeOneInFourBranch() ? goToSometimes.Target : null;
            case TorizoProjectileCue.Command { Kind: TorizoProjectileCueKind.AimSuperMissileTowardSamus } or TorizoProjectileCue.Command { Kind: TorizoProjectileCueKind.AimSuperMissileAwayFromSamus }:
                SetGoldenTorizoSuperMissileVelocity(
                    projectile,
                    samus ?? throw new InvalidOperationException("Golden Torizo super-missile aiming requires the active Samus actor."),
                    awayFromSamus: cue is TorizoProjectileCue.Command { Kind: TorizoProjectileCueKind.AimSuperMissileAwayFromSamus });
                return null;
            case TorizoProjectileCue.SetRadius setRadius:
                projectile.Radius = setRadius.Radius;
                return null;
            case TorizoProjectileCue.Command { Kind: TorizoProjectileCueKind.SpawnOrbDrops }:
                RequestTorizoChozoOrbDrop(projectile);
                return null;
            case TorizoProjectileCue.Scatter scatter:
                _projectiles.Scatter(projectile, scatter.Spread);
                return null;
            case TorizoProjectileCue.Command { Kind: TorizoProjectileCueKind.ReturnToOrigin }:
                projectile.Position = projectile.Position.WithWhole(new(checked((ushort)projectile.Variable0), checked((ushort)projectile.Variable1)));
                return null;
            case TorizoProjectileCue.Command { Kind: TorizoProjectileCueKind.RaiseDust }:
                projectile.Position = projectile.Position.WithWholeY(unchecked((ushort)(projectile.Position.Y.Whole - 4)));
                return null;
            case TorizoProjectileCue.Command { Kind: TorizoProjectileCueKind.StopBlockingShots }:
                projectile.BlocksSamusProjectiles = false;
                return null;
            case TorizoProjectileCue.Command { Kind: TorizoProjectileCueKind.StopHarmingSamus }:
                projectile.CanDamageSamus = false;
                return null;
            case TorizoProjectileCue.Command { Kind: TorizoProjectileCueKind.DrawInFront }:
                projectile.DrawPriority = EnemyProjectileDrawPriority.High;
                return null;
            case TorizoProjectileCue.Command { Kind: TorizoProjectileCueKind.NoOperation }:
                return null;
            default:
                throw new InvalidOperationException($"Undefined {nameof(TorizoProjectileCueKind)} {cue}.");
        }
    }

    /// <summary>The program a Torizo projectile's typed animation is running.</summary>
    private static TorizoProjectileProgram RunningTorizoProjectileProgram(RoomEnemyProjectileSlot projectile) =>
        projectile.Animation is TorizoProjectileAnimation animation
            ? animation.Cursor.Program
            : throw new InvalidOperationException($"Enemy projectile {projectile.Kind} is not running a Torizo program.");

    /// <summary>Requests the recurring hurt-state drool variant with randomized flight.</summary>
    internal void SpawnBombTorizoLowHealthDrool(RoomEnemySlot torizo) =>
        SpawnBombTorizoDrool(torizo, RoomEnemyProjectileKind.BombTorizoLowHealthDrool);

    /// <summary>Allocates either drool program and consumes room RNG in the native variant-specific order.</summary>
    internal void SpawnBombTorizoDrool(
        RoomEnemySlot torizo,
        RoomEnemyProjectileKind kind)
    {
        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            kind,
            unchecked((ushort)(torizo.TileIndexBase | torizo.PaletteIndex)));
        projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileProgram.DroolNoDelay));

        int random;
        if (kind == RoomEnemyProjectileKind.BombTorizoLowHealthDrool)
        {
            projectile.StartAnimation(new TorizoProjectileAnimation(SelectLowHealthDroolProgram(_room.Random.Generate())));
            random = _room.Random.Generate();
        }
        else
        {
            random = _room.Random.Generate();
        }
        if (kind == RoomEnemyProjectileKind.BombTorizoInitialDrool)
        {
            // $86:A66D-$A683 chain the carry: CLC; ADC Y; ADC #$FFFB, then the Y velocity's
            // ADC #$0030 adds that second addition's carry (set whenever Y + jitter >= 5).
            int jitteredY = (random & 3) + torizo.Position.Y.Whole;
            int yWithOffset = jitteredY + TorizoMotionDefinitions.InitialDroolLift;
            projectile.Position = projectile.Position.WithWholeY(unchecked((ushort)yWithOffset));
            projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)((random & TorizoRandomDefinitions.DroolFallJitterMask) + 48 + SnesCarry.OutOfWord(yWithOffset)))));
            // $86:A68D-$A6C3: the X offset's ADC carries out of CLC; ADC X. Every branch
            // stores a zero X velocity.
            int jitteredX = (_room.Random.Generate() & 3) + torizo.Position.X.Whole;
            int carry = SnesCarry.OutOfWord(jitteredX);
            projectile.Position = projectile.Position.WithWholeX(_access.Posture(torizo).HasFlag(TorizoPostureFlags.TurningAround)
                ? unchecked((ushort)jitteredX)
                : _access.FacesRight(torizo)
                    ? unchecked((ushort)(jitteredX + 8 + carry))
                    : unchecked((ushort)(jitteredX + TorizoMotionDefinitions.InitialDroolLeftOffset + carry)));
            projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(0));
            return;
        }
        projectile.Position = projectile.Position.WithWholeY(unchecked((ushort)(torizo.Position.Y.Whole - 5)));

        // The recurring low-health drool chooses a random direction ($86:A5F9-$A62C). The
        // turning branch's word index is random & $1FE; the X velocity is the sign-extended
        // sine and the Y velocity the negative cosine of that angle.
        int angle;
        if (_access.Posture(torizo).HasFlag(TorizoPostureFlags.TurningAround))
        {
            angle = (random & TorizoRandomDefinitions.DroolTurningAngleMask) >> 1;
        }
        else
        {
            int baseAngle = _access.FacesRight(torizo) ? 32 : 224;
            angle = unchecked((byte)(baseAngle + (random & TorizoRandomDefinitions.DroolFacingAngleMask) - 8));
        }
        projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)
            EnemyTrigonometryTables.SignedSine(unchecked(new SnesByteAngle((byte)angle))))));
        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)
            EnemyTrigonometryTables.SignedNegativeCosineWord(angle))));
        projectile.Position = projectile.Position.WithWholeX(_access.FacesRight(torizo)
            ? unchecked((ushort)(torizo.Position.X.Whole + 8))
            : unchecked((ushort)(torizo.Position.X.Whole - 8)));
    }

    /// <summary>
    /// Applies the native <c>LSR / AND #$000E</c> index into the eight-entry <c>$86:A64D</c> low-health-drool list table,
    /// whose entries repeat the no-delay, two-frame and four-frame lists.
    /// </summary>
    internal static TorizoProjectileProgram SelectLowHealthDroolProgram(int random) =>
        (((random >> 2) & 7) % 3) switch
        {
            0 => TorizoProjectileProgram.DroolNoDelay,
            1 => TorizoProjectileProgram.DroolTwoFrameDelay,
            _ => TorizoProjectileProgram.DroolFourFrameDelay,
        };

    /// <summary>Places a swipe explosion at the attack-table hand offset, mirroring it for right-facing Torizo.</summary>
    internal void SpawnBombTorizoExplosiveSwipe(RoomEnemySlot torizo, int parameter)
    {
        Offset16<RoomPixels> placement = BombTorizoAttackDefinitions.Swipe(parameter);

        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.BombTorizoExplosiveSwipe,
            graphicsIndex: 0);
        projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileProgram.SwipeInitial));

        // A right-facing Torizo mirrors the left-facing hand placement horizontally.
        projectile.Position = projectile.Position.WithWhole(torizo.Position.Whole +
            (_access.FacesRight(torizo) ? placement.MirroredX() : placement));
    }

    /// <summary>Places a body-break explosion at the facing-adjusted table offset and records its impact coordinates.</summary>
    internal void SpawnBombTorizoLowHealthExplosion(RoomEnemySlot torizo, int parameter)
    {
        Offset16<RoomPixels> placement =
            BombTorizoAttackDefinitions.LowHealthExplosion(
                parameter,
                facingRight: _access.FacesRight(torizo));

        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.BombTorizoLowHealthExplosion,
            graphicsIndex: 0);
        projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileProgram.ExplosionLowHealthInitial));
        projectile.Position = projectile.Position.WithWhole(torizo.Position.Whole + placement);
        projectile.Variable0 = projectile.Position.X.Whole;
        projectile.Variable1 = projectile.Position.Y.Whole;
    }

    /// <summary>Starts a death explosion at the statue center and stores that center for its later instruction frames.</summary>
    internal void SpawnBombTorizoDeathExplosion(RoomEnemySlot torizo)
    {
        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.BombTorizoDeathExplosion,
            graphicsIndex: 0);
        projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileProgram.ExplosionDeathInitial));
        projectile.Position = projectile.Position.WithWhole(torizo.Position.Whole);
        projectile.Variable0 = torizo.Position.X.Whole;
        projectile.Variable1 = torizo.Position.Y.Whole;
    }

    /// <summary>Allocates Bomb Torizo's Chozo orb with facing-dependent placement and randomized velocity.</summary>
    internal void SpawnBombTorizoChozoOrb(RoomEnemySlot torizo)
    {
        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.BombTorizoChozoOrb,
            unchecked((ushort)(torizo.TileIndexBase | torizo.PaletteIndex)));

        InitializeTorizoRandomizedProjectile(
            projectile,
            torizo,
            TorizoRandomizedProjectileDefinitions.BombChozoOrb(
                facingRight: _access.FacesRight(torizo)));
    }

    /// <summary>Starts Bomb Torizo's sonic-boom program at one of two randomized body heights.</summary>
    internal void SpawnBombTorizoSonicBoom(RoomEnemySlot torizo, int parameter)
        => SpawnTorizoSonicBoom(
            torizo,
            parameter,
            RoomEnemyProjectileKind.BombTorizoSonicBoom);

    /// <summary>Starts Golden Torizo's sonic-boom variant with the shared directional flight setup.</summary>
    internal void SpawnGoldenTorizoSonicBoom(RoomEnemySlot torizo, int parameter)
        => SpawnTorizoSonicBoom(
            torizo,
            parameter,
            RoomEnemyProjectileKind.GoldenTorizoSonicBoom);

    /// <summary>Places a sonic boom 32 pixels ahead and selects the fired-left or fired-right list.</summary>
    private void SpawnTorizoSonicBoom(
        RoomEnemySlot torizo,
        int parameter,
        RoomEnemyProjectileKind kind)
    {
        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            kind,
            unchecked((ushort)(torizo.TileIndexBase | torizo.PaletteIndex)));

        projectile.DirectionParameter = checked((ushort)parameter);
        projectile.Position = projectile.Position.WithWholeY(unchecked((ushort)(
            torizo.Position.Y.Whole + ((_room.Random.Generate() & 1) != 0 ? -12 : 20))));
        if (_access.FacesRight(torizo))
        {
            projectile.Position = projectile.Position.WithWholeX(unchecked((ushort)(torizo.Position.X.Whole + 32)));
            projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(624));
            projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileProgram.SonicBoomFiredRight));
        }
        else
        {
            projectile.Position = projectile.Position.WithWholeX(unchecked((ushort)(torizo.Position.X.Whole - 32)));
            projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)-624)));
            projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileProgram.SonicBoomFiredLeft));
        }
    }

    /// <summary>Allocates Golden Torizo's orb using the variant's facing-specific offset and randomized speed.</summary>
    internal void SpawnGoldenTorizoChozoOrb(RoomEnemySlot torizo)
    {
        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.GoldenTorizoChozoOrb,
            unchecked((ushort)(torizo.TileIndexBase | torizo.PaletteIndex)));
        InitializeTorizoRandomizedProjectile(
            projectile,
            torizo,
            TorizoRandomizedProjectileDefinitions.GoldenChozoOrb(
                facingRight: _access.FacesRight(torizo)));
    }

    /// <summary>Starts a Golden egg with the cartridge's fixed 66-frame hatch timer and randomized launch speed.</summary>
    internal void SpawnGoldenTorizoEgg(RoomEnemySlot torizo)
    {
        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.GoldenTorizoEgg,
            unchecked((ushort)(torizo.TileIndexBase | torizo.PaletteIndex)));

        projectile.Variable1 = TorizoMotionDefinitions.EggHatchTimer;
        TorizoEnemyState state = _access.RequireBombTorizoState(torizo);
        projectile.Variable0 = TorizoParameterFlagCodec.ToParameterWord(state.Posture, state.PostureParameterLowBits);
        InitializeTorizoRandomizedProjectile(
            projectile,
            torizo,
            TorizoRandomizedProjectileDefinitions.GoldenEgg(
                movingRight: state.Posture.HasFlag(TorizoPostureFlags.FacingRight)));
    }

    /// <summary>Attaches a caught super missile above the active hand and selects its facing-dependent program.</summary>
    internal void SpawnGoldenTorizoSuperMissile(RoomEnemySlot torizo)
    {
        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.GoldenTorizoSuperMissile,
            unchecked((ushort)(torizo.TileIndexBase | torizo.PaletteIndex)));
        bool facingRight = _access.FacesRight(torizo);
        projectile.Variable0 = unchecked((ushort)(torizo.SlotIndex * 2));
        projectile.Position = projectile.Position.WithWhole(new(unchecked((ushort)(torizo.Position.X.Whole + (facingRight ? 30 : -30))), unchecked((ushort)(torizo.Position.Y.Whole - 52))));
        projectile.StartAnimation(new TorizoProjectileAnimation(facingRight ? TorizoProjectileProgram.SuperMissileRightInitial : TorizoProjectileProgram.SuperMissileLeftInitial));
    }

    /// <summary>Fires an eye beam from the facing-specific offset with room-RNG angular spread.</summary>
    internal void SpawnGoldenTorizoEyeBeam(RoomEnemySlot torizo, int parameter)
    {
        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.GoldenTorizoEyeBeam,
            unchecked((ushort)(torizo.TileIndexBase | torizo.PaletteIndex)));
        projectile.DirectionParameter = checked((ushort)parameter);
        bool facingRight = _access.FacesRight(torizo);
        InitializeTorizoRandomizedProjectile(
            projectile,
            torizo,
            TorizoRandomizedProjectileDefinitions.GoldenEyeBeam(facingRight));

        int angle = ((_room.Random.Generate() & TorizoRandomDefinitions.EyeBeamSpreadMask) - 16 + 192 + (facingRight ? 0 : 128)) & TorizoRandomDefinitions.EyeBeamAngleMask;
        int sineIndex = angle >> 1;
        // The native combined table starts at negative cosine, not sine. Its
        // quarter-turn offset selects sine for X; the unshifted sample gives Y.
        projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)(8 *
            EnemyTrigonometryTables.SignedNegativeCosineWord(sineIndex + 64)))));
        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(8 *
            EnemyTrigonometryTables.SignedNegativeCosineWord(sineIndex)))));
    }

    /// <summary>Applies the projectile definition and draws horizontal jitter before vertical jitter.</summary>
    internal void InitializeTorizoRandomizedProjectile(
        RoomEnemyProjectileSlot projectile,
        RoomEnemySlot torizo,
        TorizoRandomizedProjectileDefinition definition)
    {
        projectile.StartAnimation(new TorizoProjectileAnimation(definition.Program));
        projectile.Position = projectile.Position.WithWhole(torizo.Position.Whole + definition.Offset);
        // The native initializer draws the X jitter before the Y jitter.
        short xVelocity = unchecked((short)(
            definition.BaseVelocity.X.SignedRawValue + unchecked((byte)_room.Random.Generate()) - 128));
        short yVelocity = unchecked((short)(
            definition.BaseVelocity.Y.SignedRawValue + unchecked((byte)_room.Random.Generate()) - 128));
        projectile.Velocity = new EightEightVector<RoomPixels>(xVelocity, yVelocity);
    }

    /// <summary>Places a foot-specific dust effect 48 pixels below the statue after a landing step.</summary>
    internal void SpawnBombTorizoLandingDust(RoomEnemySlot torizo, HorizontalDirection foot)
    {
        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;
        RoomEnemyProjectileKind kind = foot == HorizontalDirection.Right
            ? RoomEnemyProjectileKind.BombTorizoRightFootDust
            : RoomEnemyProjectileKind.BombTorizoLeftFootDust;
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(projectile, kind, graphicsIndex: 0);
        projectile.StartAnimation(new TorizoProjectileAnimation(foot == HorizontalDirection.Right ? TorizoProjectileProgram.LandingDustRightFoot : TorizoProjectileProgram.LandingDustLeftFoot));
        projectile.Position = projectile.Position.WithWhole(new(unchecked((ushort)(torizo.Position.X.Whole + (foot == HorizontalDirection.Right ? 24 : -24))), unchecked((ushort)(torizo.Position.Y.Whole + 48))));
    }

    /// <summary>Moves Bomb Torizo's orb through walls and floor, disabling damage on impact and adding gravity in flight.</summary>
    internal static void RunBombTorizoChozoOrbPreInstruction(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Horizontal))
        {
            projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileProgram.ChozoOrbWallImpact));
            projectile.CanDamageSamus = false;
            return;
        }

        bool verticalCollision = EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Vertical);
        if (unchecked((short)projectile.Velocity.Y.RawValue) >= 0 && verticalCollision)
        {
            projectile.Position = projectile.Position.WithWholeY(unchecked((ushort)(
                (projectile.Position.Y.Whole & TorizoMotionDefinitions.BlockMask) + 6)));
            projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileProgram.ChozoOrbFloorImpact));
            projectile.CanDamageSamus = false;
            return;
        }

        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(projectile.Velocity.Y.RawValue + 18))));
        if (TorizoMotionDefinitions.IsRunaway(projectile.Velocity.Y))
            projectile.Clear();
    }

    /// <summary>Accelerates the sonic boom horizontally and switches to its non-damaging wall-impact program.</summary>
    internal static void RunBombTorizoSonicBoomPreInstruction(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Horizontal))
        {
            projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileProgram.SonicBoomWallImpact));
            projectile.Variable0 = projectile.Position.X.Whole;
            projectile.Variable1 = projectile.Position.Y.Whole;
            projectile.CanDamageSamus = false;
            return;
        }

        projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)(projectile.Velocity.X.RawValue +
            (unchecked((short)projectile.Velocity.X.RawValue) < 0 ? -16 : 16)))));
        if (TorizoMotionDefinitions.IsRunaway(projectile.Velocity.X))
            projectile.Clear();
    }

    /// <summary>
    /// Ports <c>$86:A887</c>, shared by Bomb Torizo's six gut-break droplets and its
    /// low-health initial drool. The peculiar positive-three clamp is authentic: both the
    /// negative and nonnegative drag branches load <c>$0003</c> when they cross zero.
    /// </summary>
    internal static void RunBombTorizoDroolPreInstruction(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Horizontal))
        {
            projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileProgram.DroolWallImpact));
            return;
        }

        short xVelocity = unchecked((short)projectile.Velocity.X.RawValue);
        if (xVelocity < 0)
        {
            int dragged = xVelocity + 4;
            projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)(short)(dragged < 0 ? dragged : 3))));
        }
        else
        {
            int dragged = xVelocity - 4;
            projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)(short)(dragged >= 0 ? dragged : 3))));
        }

        bool verticalCollision = EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Vertical);
        if (unchecked((short)projectile.Velocity.Y.RawValue) >= 0 && verticalCollision)
        {
            projectile.Position = projectile.Position.WithWholeY(unchecked((ushort)(projectile.Position.Y.Whole - 3)));
            projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileProgram.DroolFloorImpact));
            return;
        }

        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(projectile.Velocity.Y.RawValue + 16))));
        if (TorizoMotionDefinitions.IsRunaway(projectile.Velocity.Y))
            projectile.Clear();
    }

    /// <summary>Bounces Golden Torizo's orb off walls and the floor, slowing horizontal motion until floor impact.</summary>
    internal static void RunGoldenTorizoChozoOrbPreInstruction(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Horizontal))
            projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)-unchecked((short)projectile.Velocity.X.RawValue))));

        bool verticalCollision = EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Vertical);
        if (verticalCollision && unchecked((short)projectile.Velocity.Y.RawValue) >= 0)
        {
            short xVelocity = unchecked((short)projectile.Velocity.X.RawValue);
            projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)(xVelocity >= 0
                ? xVelocity - 64
                : xVelocity + 64))));
            projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)-(projectile.Velocity.Y.RawValue >> 1))));
            if (TorizoMotionDefinitions.HasSettled(projectile.Velocity.Y))
            {
                projectile.Position = projectile.Position.WithWholeY(unchecked((ushort)(
                    (projectile.Position.Y.Whole & TorizoMotionDefinitions.BlockMask) + 6)));
                projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileProgram.ChozoOrbFloorImpact));
                projectile.CanDamageSamus = false;
                return;
            }
        }

        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(projectile.Velocity.Y.RawValue + 24))));
    }

    /// <summary>Counts down the egg's hatch timer while bouncing it off terrain with horizontal drag and gravity.</summary>
    internal static void RunGoldenTorizoEggPreInstruction(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        CountdownStep hatch = WrappingCountdown.FromRaw(checked((ushort)projectile.Variable1)).Decrement();
        projectile.Variable1 = hatch.Value;
        if (hatch.IsNegative)
        {
            // The hatch timer steps the sleeping list past its sleep.
            projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileAnimations.AfterSleep(RunningTorizoProjectileProgram(projectile))));
            projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(((TorizoPostureFlags)projectile.Variable0).HasFlag(TorizoPostureFlags.FacingRight)
                ? (ushort)256
                : unchecked((ushort)-256)));
            return;
        }

        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Horizontal))
        {
            projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)-unchecked((short)projectile.Velocity.X.RawValue))));
            projectile.Variable0 = ((TorizoPostureFlags)projectile.Variable0 ^ TorizoPostureFlags.FacingRight).ToNativeWord();
        }

        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Vertical) &&
            unchecked((short)projectile.Velocity.Y.RawValue) >= 0)
        {
            short xVelocity = unchecked((short)projectile.Velocity.X.RawValue);
            projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)(xVelocity + (xVelocity < 0 ? 32 : -32)))));
            projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)-unchecked((short)projectile.Velocity.Y.RawValue))));
        }

        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(projectile.Velocity.Y.RawValue + 48))));
        if (TorizoMotionDefinitions.IsRunaway(projectile.Velocity.Y))
            projectile.Clear();
    }

    /// <summary>Accelerates the hatched egg sideways and changes to its fall phase when it hits a wall.</summary>
    internal static void RunGoldenTorizoEggHorizontalCharge(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Horizontal))
        {
            projectile.PreInstruction =
                EnemyProjectilePreInstruction.GoldenTorizoEgg_HitWall;
            projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(0));
            return;
        }

        projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)(projectile.Velocity.X.RawValue +
            (((TorizoPostureFlags)projectile.Variable0).HasFlag(TorizoPostureFlags.FacingRight) ? 48 : -48)))));
    }

    /// <summary>Accelerates the egg downward until floor contact selects the facing-specific break program.</summary>
    internal static void RunGoldenTorizoEggFall(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Vertical))
        {
            projectile.StartAnimation(new TorizoProjectileAnimation(((TorizoPostureFlags)projectile.Variable0).HasFlag(TorizoPostureFlags.FacingRight) ? TorizoProjectileProgram.EggBreakRight : TorizoProjectileProgram.EggBreakLeft));
            return;
        }

        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(projectile.Velocity.Y.RawValue + 48))));
    }

    /// <summary>Keeps the held super missile beside Golden Torizo's hand, clearing it if its owner is gone.</summary>
    internal void RunGoldenTorizoSuperMissilePreInstruction(RoomEnemyProjectileSlot projectile)
    {
        TorizoEnemyState? state = GoldenTorizo;
        if (state is null)
        {
            projectile.Clear();
            return;
        }

        RoomEnemySlot torizo = state.Slot;
        projectile.Position = projectile.Position.WithWhole(new(unchecked((ushort)(torizo.Position.X.Whole +
            (_access.FacesRight(torizo) ? 32 : -32))), unchecked((ushort)(torizo.Position.Y.Whole - 52))));
    }

    /// <summary>Moves the thrown super missile until wall or downward floor impact, applying gravity in flight.</summary>
    internal static void RunGoldenTorizoSuperMissileFlight(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        bool impact = EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Horizontal);
        if (!impact)
        {
            bool verticalCollision = EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Vertical);
            impact = verticalCollision && unchecked((short)projectile.Velocity.Y.RawValue) >= 0;
        }

        if (impact)
        {
            projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileProgram.SuperMissileImpact));
            return;
        }

        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(projectile.Velocity.Y.RawValue + 16))));
        if (TorizoMotionDefinitions.IsRunaway(projectile.Velocity.Y))
            projectile.Clear();
    }

    /// <summary>Aims the thrown missile toward or away from Samus using the native up-zero angle convention.</summary>
    internal static void SetGoldenTorizoSuperMissileVelocity(
        RoomEnemyProjectileSlot projectile,
        SamusState samus,
        bool awayFromSamus)
    {
        byte angle = EnemyMath.CalculateBearing(
            new(unchecked((short)(samus.Position.X.Whole - projectile.Position.X.Whole)), unchecked((short)(samus.Position.Y.Whole - projectile.Position.Y.Whole)))).ReinterpretAsTableAngle().RawValue;
        angle = awayFromSamus
            ? unchecked((byte)(angle | TorizoMotionDefinitions.AwayHalfTurn))
            : unchecked((byte)(angle & TorizoMotionDefinitions.TowardHalfTurn));

        // Preserve the native up-zero angle convention: X is sine, Y is
        // negative cosine. Starting both reads at sine rotates the shot.
        projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)(4 *
            EnemyTrigonometryTables.SignedNegativeCosineWord(angle + 64)))));
        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(4 *
            EnemyTrigonometryTables.SignedNegativeCosineWord(angle)))));
    }

    /// <summary>Selects wall or floor impact artwork when the eye beam strikes terrain.</summary>
    internal static void RunGoldenTorizoEyeBeamPreInstruction(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Horizontal))
        {
            projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileProgram.EyeBeamWallImpact));
            return;
        }

        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Vertical))
        {
            projectile.Position = projectile.Position.WithWholeY(unchecked((ushort)(
                (projectile.Position.Y.Whole & TorizoMotionDefinitions.BlockMask) + 6)));
            projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileProgram.EyeBeamFloorImpact));
        }
    }

    /// <summary>
    /// Ports instruction <c>$86:AB8A</c>. Its two inline header operands are Bomb Torizo's area-zero orb and Golden
    /// Torizo's nonzero-area orb, which the cue carries as its typed meaning. This runtime already identifies the
    /// concrete loaded variant, which is the exact room-scoped equivalent of consulting the global area byte.
    /// </summary>
    internal void RequestTorizoChozoOrbDrop(RoomEnemyProjectileSlot projectile)
    {
        if (projectile.Kind is not (
                RoomEnemyProjectileKind.BombTorizoChozoOrb or
                RoomEnemyProjectileKind.GoldenTorizoChozoOrb))
        {
            throw new InvalidOperationException(
                $"Projectile {projectile.Kind} reached the Torizo-orb drop.");
        }

        EnemyDefinitionId header = GoldenTorizo is not null
            ? EnemyDefinitionId.GoldenTorizoOrb
            : EnemyDefinitionId.BombTorizoOrb;

        RoomEnemyDefinition definition = _room.LoadedDefinitions.ReadEnemyDefinition(header);
        TorizoOrbDropRequests.Add(new TorizoOrbDropRequest());
        _deathsAndDrops.SpawnEnemyDropFromChanceTable(
            projectile.Position.Whole,
            definition.DropChances);
    }

    /// <summary>
    /// Allocates the room-graphics enemy projectile requested by PLM instruction
    /// <c>$84:D357</c>, then runs initializer <c>$86:A764</c> against the publishing PLM's
    /// block coordinates.
    /// </summary>
    public RoomEnemyProjectileSlot? SpawnBombTorizoStatueBreakingProjectile(
        BombTorizoStatueProjectileRequest request)
    {
        _room.EnsureLoaded();
        BombTorizoStatueFragmentDefinition definition =
            BombTorizoStatueFragmentDefinitions.ForFragment(request.Fragment);

        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return null; // SpawnEprojInner silently drops work when all eighteen slots fill.

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.BombTorizoStatueBreaking,
            graphicsIndex: 0);

        projectile.StartAnimation(new TorizoProjectileAnimation(definition.Program));
        projectile.Position = projectile.Position.WithWhole(request.PlmBlock.ToRoomPixels() + definition.Offset);
        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(definition.YVelocity));
        projectile.Variable1 = definition.Acceleration;
        return projectile;
    }

    /// <summary>
    /// Ports pre-instruction <c>$86:A8EF</c>, including its authentic unindexed read from
    /// physical slot zero's variable-F word when applying acceleration.
    /// </summary>
    internal void RunBombTorizoStatueBreakingPreInstruction(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        bool collided = EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Vertical);
        if (unchecked((short)projectile.Velocity.Y.RawValue) < 0 || !collided)
        {
            // The initializer writes $0010 to every fragment's own F word, but A8EF's ROM
            // instruction omits X and reads `$1B23` (physical slot zero) literally. The
            // retail quirk normally leaves these first eight actors at constant speed;
            // retaining it matters if another projectile already occupies slot zero.
            projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(
                projectile.Velocity.Y.RawValue + _projectiles.EnemyProjectiles[0].Variable1))));
            if (TorizoMotionDefinitions.IsRunaway(projectile.Velocity.Y))
                projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(TorizoMotionDefinitions.RunawaySpeed));
            return;
        }

        projectile.PreInstruction =
            EnemyProjectilePreInstruction.BombTorizoStatueFragment_Stopped;
    }
}
