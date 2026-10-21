using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.SpriteObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Enemies.SpriteObjects;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// The bank-$86 enemy-projectile engine: the eighteen physical slots, allocation and definition setup, the
/// per-frame pass (pre-instructions, then each instruction list), the instruction interpreter, both draw-priority
/// passes and the Samus collision passes. Species behavior arrives through registered handlers; the engine names
/// no species.
/// </summary>
internal sealed class EnemyProjectileEngine
{
    /// <summary>Serialized handler owners; their delegate table is rebuilt after restore to avoid compiler-generated identities.</summary>
    private readonly List<IEnemyProjectileHandlerSource> _handlerSources = [];

    /// <summary>
    /// Adds an owner whose projectile handlers join the table. Owners are added while the enemy system is
    /// composed, before the first dispatch builds the table.
    /// </summary>
    internal void AddHandlerSource(IEnemyProjectileHandlerSource source) => _handlerSources.Add(source);

    /// <summary>Lazy runtime dispatch table rebuilt from the serialized handler owners.</summary>
    [field: NonSerialized]
    private EnemyProjectileHandlerTable Handlers => field ??= BuildHandlers();

    /// <summary>Registers inert native RTS addresses and every species-owned projectile handler.</summary>
    private EnemyProjectileHandlerTable BuildHandlers()
    {
        var table = new EnemyProjectileHandlerTable();
        // Native RTS pre-instructions and inert phases do nothing.
        table.RegisterPreInstruction(static _ => { },
            EnemyProjectilePreInstruction.None,
            EnemyProjectilePreInstruction.RTS_868170,
            EnemyProjectilePreInstruction.RTS_86A327,
            EnemyProjectilePreInstruction.RTS_8684FB,
            EnemyProjectilePreInstruction.RTS_86EC94,
            EnemyProjectilePreInstruction.RTS_86D0EB,
            EnemyProjectilePreInstruction.RTS_86CFF7,
            EnemyProjectilePreInstruction.RTS_86B4B0,
            EnemyProjectilePreInstruction.RTS_868D54,
            EnemyProjectilePreInstruction.RTS_86950C,
            EnemyProjectilePreInstruction.RTS_869A44,
            EnemyProjectilePreInstruction.RTS_86BBC6,
            EnemyProjectilePreInstruction.RTS_86A05B,
            EnemyProjectilePreInstruction.RTS_86EFDF,
            EnemyProjectilePreInstruction.RTS_86A919,
            EnemyProjectilePreInstruction.BombTorizoStatueFragment_Stopped,
            EnemyProjectilePreInstruction.RTS_86DD44,
            EnemyProjectilePreInstruction.RTS_86CAA3,
            EnemyProjectilePreInstruction.RTS_86C76D,
            EnemyProjectilePreInstruction.RTS_86E6D1,
            EnemyProjectilePreInstruction.DownwardGateInertPreInstruction,
            EnemyProjectilePreInstruction.EyeDoorSmokeInertPreInstruction);
        table.RegisterCue<EnemyProjectileCommonCue>((context, cue) => ApplyCommonCue(context.Projectile, cue));
        foreach (IEnemyProjectileHandlerSource source in _handlerSources)
            source.RegisterEnemyProjectileHandlers(table);
        return table;
    }

    /// <summary>Carries out one shared bank-$86 command of a typed projectile program.</summary>
    private void ApplyCommonCue(RoomEnemyProjectileSlot projectile, EnemyProjectileCommonCue cue)
    {
        switch (cue)
        {
            case EnemyProjectileCommonCue.Command { Kind: EnemyProjectileCommonCueKind.Delete }:
                projectile.Clear();
                break;
            case EnemyProjectileCommonCue.SetPreInstruction setPreInstruction:
                projectile.PreInstruction = setPreInstruction.PreInstruction;
                break;
            case EnemyProjectileCommonCue.Command { Kind: EnemyProjectileCommonCueKind.ClearPreInstruction }:
                projectile.PreInstruction = EnemyProjectilePreInstruction.RTS_868170;
                break;
            case EnemyProjectileCommonCue.Scatter scatter:
                Scatter(projectile, scatter.Spread);
                break;
            case EnemyProjectileCommonCue.Command { Kind: EnemyProjectileCommonCueKind.ReturnToOrigin }:
                projectile.Position = projectile.Position.WithWhole(new(checked((ushort)projectile.Variable0), checked((ushort)projectile.Variable1)));
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(EnemyProjectileCommonCueKind)} {cue}.");
        }
    }

    /// <summary>Supplies room RNG and artwork for projectile effects.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Owns bank-$B4 dust and explosion sprites separate from projectile slots.</summary>
    private readonly RoomSpriteObjectPool _spriteObjects;
    /// <summary>Publishes room quake effects requested by projectile instructions.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>Queues species projectile sounds under native queue limits.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Connects the physical bank-$86 pool to room sprites, earthquakes and audio.</summary>
    internal EnemyProjectileEngine(EnemyRoomBinding room, RoomSpriteObjectPool spriteObjects, RoomEarthquake earthquake, EnemyAudioQueue audio)
    {
        _room = room;
        _spriteObjects = spriteObjects;
        _earthquake = earthquake;
        _audio = audio;
    }
    /// <summary>Eighteen physical enemy-projectile indices $00..$22 in two-byte steps.</summary>
    internal const int RoomEnemyProjectileSlotCount = 18;

    /// <summary>Native pool including inactive entries, traversed from the highest physical slot down.</summary>
    private readonly RoomEnemyProjectileSlot[] _enemyProjectiles =
        Enumerable.Range(0, RoomEnemyProjectileSlotCount)
            .Select(index => new RoomEnemyProjectileSlot(index))
            .ToArray();
    /// <summary>The 8-bit frame counter sampled by the current projectile pass; verification drives it to step instructions.</summary>
    internal int CurrentFrame8 { get; set; }

    /// <summary>A shot meets an enemy projectile only inside the same 32-pixel cell: the shift that selects the cell.</summary>
    private const int CoarseContactCellShift = 5;

    /// <summary>The pixel offset within a 16-pixel block.</summary>
    private const int BlockPixelMask = 15;

    /// <summary>The pixel offset within an 8-pixel half block.</summary>
    private const int HalfBlockPixelMask = 7;

    /// <summary>The 16-bit NMI_FrameCounter ($05B6) for the current projectile pass.</summary>
    internal int CurrentFrame16 { get; private set; }

    /// <summary>All eighteen physical bank-$86 slots, including currently inactive slots.</summary>
    public IReadOnlyList<RoomEnemyProjectileSlot> EnemyProjectiles => _enemyProjectiles;

    /// <summary>
    /// Last library-one dud sound requested when an indestructible enemy projectile blocked
    /// a Samus shot. This is a frame publication; the audio mixer remains an outer seam.
    /// </summary>
    public SoundEffectId? LastEnemyProjectileDudSoundEffect { get; internal set; }

    /// <summary>
    /// Ports <c>EprojProjCollDet</c> and <c>HandleEprojCollWithProj</c> at
    /// $A0:996C-$9A30. Flag-one Nuclear Waffle links create duds and remain alive; flag-zero
    /// Kago bugs remember the incoming projectile type and switch to their ROM shot list.
    /// </summary>
    public int ResolveEnemyProjectileSamusProjectileHits(
        SamusProjectileSystem projectiles,
        SamusBombProjectileSystem sharedProjectiles)
    {
        Ensure.NotNull(projectiles);
        Ensure.NotNull(sharedProjectiles);
        _room.EnsureLoaded();

        int hitCount = 0;
        for (int enemyProjectileIndex = _enemyProjectiles.Length - 1;
             enemyProjectileIndex >= 0;
             enemyProjectileIndex--)
        {
            RoomEnemyProjectileSlot enemyProjectile =
                _enemyProjectiles[enemyProjectileIndex];
            if (!enemyProjectile.IsActive ||
                !enemyProjectile.BlocksSamusProjectiles ||
                enemyProjectile.CollisionOption == EnemyProjectileCollisionOption.NoCollision)
                continue;

            foreach (SamusProjectileSlot shot in projectiles.Slots.Take(5))
            {
                if (!shot.HasEnemyCollisionPayload)
                    continue;

                SamusProjectileTypeWord shotType = shot.PackedType;
                if (shotType.Family is SamusProjectileFamily.PowerBomb or SamusProjectileFamily.Bomb ||
                    shotType.FamilyValue >= (ushort)SamusProjectileFamily.BeamExplosion)
                    continue;

                // The native path intentionally compares only 32-pixel cells. It does not
                // run the radius overlap used by ordinary enemies, a coarse quirk preserved
                // here instead of “fixing” shots near a cell corner.
                if (enemyProjectile.Position.X.Whole >> CoarseContactCellShift != shot.Position.X.Whole >> CoarseContactCellShift ||
                    enemyProjectile.Position.Y.Whole >> CoarseContactCellShift != shot.Position.Y.Whole >> CoarseContactCellShift)
                {
                    continue;
                }

                // `$A0:99F9-$9A07` only sets direction lifecycle bit $10 for non-plasma
                // shots. It does not call the ordinary enemy-impact routine and therefore
                // cannot rewrite a freshly fired Super Missile (or its invisible link) to
                // family $0800 in the producer frame. Its own pre-instruction consumes the
                // mark on the next projectile pass.
                ushort collidedProjectileType = shot.PackedType.Raw;
                if ((collidedProjectileType & 8) == 0)
                    shot.Direction = shot.PackedDirection.WithCollisionLifecycleState();

                if (enemyProjectile.CollisionOption == EnemyProjectileCollisionOption.IndestructibleDud)
                {
                    LastEnemyProjectileDudSoundEffect = SoundEffectLibrary1Sounds.DudShot;
                }
                else
                {
                    enemyProjectile.CollidedProjectileType = collidedProjectileType;
                    // Every shot list without a typed reaction of its own is the shared delete list.
                    enemyProjectile.StartAnimation(Handlers.TryGetShotAnimation(enemyProjectile.Kind, out Func<EnemyProjectileAnimation>? shotAnimation)
                        ? shotAnimation()
                        : EnemyProjectileDeleteAnimation.Instance);
                    enemyProjectile.PreInstruction = EnemyProjectilePreInstruction.RTS_8684FB;

                    // Native masks properties with $0FFF. The typed fields below are the
                    // three high property bits represented by this runtime, so clearing
                    // them is the exact structural equivalent rather than a Kago special.
                    enemyProjectile.BlocksSamusProjectiles = false;
                    enemyProjectile.PersistsOnSamusContact = false;
                    enemyProjectile.CanDamageSamus = true;
                }
                hitCount++;
            }
        }

        return hitCount;
    }

    /// <summary>How many passes the standalone clock counts before it returns to zero, as the short NMI counter does.</summary>
    private const int StandaloneFrameCounter8Period = 256;

    /// <summary>Fallback frame clock for standalone calls without a supplied NMI counter, 0 through 255.</summary>
    private int _standaloneFrameCounter8;

    /// <summary>Returns the standalone clock's current frame and advances it, wrapping after 255.</summary>
    private int AdvanceStandaloneFrameCounter8()
    {
        int frame = _standaloneFrameCounter8;
        _standaloneFrameCounter8 = (frame + 1) % StandaloneFrameCounter8Period;
        return frame;
    }

    /// <summary>One bank-$86 projectile pass: every live slot's pre-instruction, then its instruction list.</summary>
    internal void RunPass(
        RoomLevelData level,
        SamusState? samus,
        Position16<RoomPixels> camera,
        int? nmiFrameCounter8,
        SamusBombProjectileSystem? samusBombs,
        int? nmiFrameCounter)
    {
        Ensure.NotNull(level);

        // `$86:810D-$8122` starts X at physical byte index `$22`, executes that slot, reloads
        // the unchanged outer index from `$1991`, subtracts two, and continues through `$00`.
        // Array index 17 is native `$22`, so this must be a live descending scan rather than
        // LINQ's ascending enumeration or a frame-start snapshot.
        //
        // The distinction is observable whenever a pre-instruction or instruction opcode
        // allocates another projectile. `$86:8027` searches `$22 -> $00` without changing
        // the outer `$1991`. A child allocated below the current physical index is therefore
        // reached later in THIS pass; a child allocated above it waits until the next pass.
        // Reusing the current slot also lets the replacement's instruction list run in this
        // pass after the pre-instruction returns. Reading each array entry at loop time
        // preserves all three cases and prevents host collection semantics from inventing a
        // universal one-frame spawn delay.
        int projectileFrame = nmiFrameCounter8 ?? AdvanceStandaloneFrameCounter8();
        CurrentFrame8 = projectileFrame;
        // Standalone audits seed both counters from the same projectile clock.
        CurrentFrame16 = nmiFrameCounter ?? projectileFrame;
        for (int projectileIndex = _enemyProjectiles.Length - 1;
             projectileIndex >= 0;
             projectileIndex--)
        {
            RoomEnemyProjectileSlot projectile = _enemyProjectiles[projectileIndex];
            if (!projectile.IsActive)
                continue;

            RunEnemyProjectilePreInstruction(
                projectile,
                level,
                samus,
                camera,
                projectileFrame,
                samusBombs);
            // `$86:8125` does not re-read the ID after the pre-instruction: a slot whose identity
            // the pre-instruction released still runs its instructions in this pass (a power-bombed
            // Draygon goop that reaches Samus attaches through its touch list). A slot the port
            // cleared outright has no timer or animation left, so processing it does nothing.
            ProcessEnemyProjectileInstructions(projectile, samus, camera);
        }

    }

    /// <summary>Publishes projectile hurt requests after PLMs; the next Samus phase consumes them.</summary>
    public void ResolveEnemyProjectileSamusHits(SamusState? samus)
    {
        _room.EnsureLoaded();
        // Native gameplay runs `$86:868B` for every projectile first, then enters the
        // separate `$A0:9894` Samus-collision pass. That pass samples invincibility and
        // contact-damage state once at entry; damage from one overlapping projectile does
        // not abort the remaining descending-slot scan. This distinction is observable for
        // Puromi/Nuclear Waffle, whose four persistent body links begin co-located.
        if (samus is null || !samus.InvincibilityTimer.IsZero ||
            samus.HorizontalSpeed.ContactDamageIndex != SamusContactDamageIndex.None)
            return;

        KnockbackHorizontalDirection? finalKnockbackXDirection = null;
        for (int index = _enemyProjectiles.Length - 1; index >= 0; index--)
        {
            RoomEnemyProjectileSlot projectile = _enemyProjectiles[index];
            if (projectile.IsActive)
            {
                finalKnockbackXDirection =
                    ResolveEnemyProjectileSamusCollision(projectile, samus) ??
                    finalKnockbackXDirection;
            }
        }

        // `$A0:9923` only publishes the five-frame request and overwrites its horizontal
        // direction for every hit. Bank $90 has already run this frame: do not initialize
        // a hurt pose or move Samus here. The next frame consumes the final request.
        if (finalKnockbackXDirection.HasValue)
        {
            samus.KnockbackXDirection = finalKnockbackXDirection.Value;
            samus.KnockbackTimer = NativeCountdown.Start(5);
        }
    }

    /// <summary>
    /// Ports <c>Draw_HighPriority_EnemyProjectile</c> at <c>$86:8390</c>, including the
    /// preceding room-sprite-object phase at <c>$A0:8855</c>.
    /// </summary>
    /// <param name="sprites">The frame's sprite description.</param>
    /// <param name="objects">The gameplay OBJ character page.</param>
    /// <param name="camera">Layer-one camera position in whole room pixels.</param>
    /// <param name="timeIsFrozen">Whether time is frozen, which stops the earthquake displacement.</param>
    public void DrawHighPriorityEnemyProjectiles(
        SpriteFrameBuffer sprites,
        SpriteArtwork objects,
        Position16<RoomPixels> camera,
        bool timeIsFrozen = false)
    {
        Ensure.NotNull(sprites);
        Ensure.NotNull(objects);
        _room.EnsureLoaded();

        // Sprite objects precede only the high pass. Repeating them in the low pass would
        // duplicate explosions, dust, and Spark's trail in the same frame.
        _spriteObjects.DrawRoomSpriteObjects(sprites, objects, camera);
        DrawEnemyProjectilePass(
            sprites,
            objects,
            camera,
            EnemyProjectileDrawPriority.High, timeIsFrozen);
    }

    /// <summary>Ports <c>Draw_LowPriority_EnemyProjectile</c> at <c>$86:83B2</c>.</summary>
    /// <param name="sprites">The frame's sprite description.</param>
    /// <param name="objects">The gameplay OBJ character page.</param>
    /// <param name="camera">Layer-one camera position in whole room pixels.</param>
    /// <param name="timeIsFrozen">Whether time is frozen, which stops the earthquake displacement.</param>
    public void DrawLowPriorityEnemyProjectiles(
        SpriteFrameBuffer sprites,
        SpriteArtwork objects,
        Position16<RoomPixels> camera,
        bool timeIsFrozen = false)
    {
        Ensure.NotNull(sprites);
        Ensure.NotNull(objects);
        _room.EnsureLoaded();

        DrawEnemyProjectilePass(
            sprites,
            objects,
            camera,
            EnemyProjectileDrawPriority.Low, timeIsFrozen);
    }

    /// <summary>Publishes one priority class in descending slot order so earlier entries win overlaps.</summary>
    internal void DrawEnemyProjectilePass(
        SpriteFrameBuffer sprites,
        SpriteArtwork objects,
        Position16<RoomPixels> camera,
        EnemyProjectileDrawPriority priority,
        bool timeIsFrozen)
    {
        Offset16<RoomPixels> shake = _earthquake.GetEnemyProjectileShake(timeIsFrozen);
        // Both cartridge routines scan native indexes $22,$20,...,$00. Physical array
        // index seventeen therefore reaches OAM first and wins equal-priority overlap.
        for (int projectileIndex = _enemyProjectiles.Length - 1;
             projectileIndex >= 0;
             projectileIndex--)
        {
            RoomEnemyProjectileSlot projectile = _enemyProjectiles[projectileIndex];

            if (!projectile.IsActive ||
                projectile.DrawPriority != priority ||
                projectile.Frame is not { } frame)
                continue;

            Position16<ScreenPixels> screen = (projectile.Position.Whole + shake).ToScreen(camera);
            if (unchecked((ushort)(screen.X + 128)) >= 512 || unchecked((ushort)(screen.Y + 128)) >= 512)
                continue;

            var artwork = _room.Artwork?.ProjectileSpritemaps ?? throw new InvalidOperationException(
                "Room enemy projectiles require installed sprite artwork.");
            ReadOnlyMemory<SpritemapPart> parts = EnemyProjectileFrames.Parts(artwork, frame);
            ushort paletteBits = new SnesObjAttributeWord(checked((ushort)projectile.GraphicsIndex)).PaletteBits;
            EnemyPresentation.PublishParts(sprites, objects, new SpriteSource(SpriteSourceKind.EnemyProjectile, projectileIndex),
                EnemySpriteFrames.Frame(parts, unchecked((byte)projectile.GraphicsIndex), paletteBits), screen, paletteBits);
        }
    }

    /// <summary>Searches physical slots $22 down to $00 and returns null when the finite pool is full.</summary>
    internal RoomEnemyProjectileSlot? AllocateEnemyProjectile()
    {
        // SpawnEnemyProjectile searches from native index $22 toward zero.
        for (int index = _enemyProjectiles.Length - 1; index >= 0; index--)
        {
            RoomEnemyProjectileSlot projectile = _enemyProjectiles[index];
            if (!projectile.IsActive)
            {
                projectile.Clear();
                return projectile;
            }
        }
        return null;
    }

    /// <summary>
    /// Copies the seven-word bank-$86 definition record installed by
    /// <c>SpawnEprojInner</c>. Family initializers may then replace fields exactly as their
    /// cartridge routine does; centralizing the copy prevents each projectile translation
    /// from inventing subtly different radius/property semantics.
    /// </summary>
    internal static void InitializeEnemyProjectileFromDefinition(
        RoomEnemyProjectileSlot projectile,
        RoomEnemyProjectileKind kind,
        ushort graphicsIndex)
    {
        EnemyProjectileDefinition definition = EnemyProjectileDefinitionCatalog.Get(kind);
        projectile.Kind = kind;
        projectile.PreInstruction = definition.PreInstruction;
        // SpawnEprojInner starts the instruction timer at one, so the first step runs on the first pass. A kind without
        // a typed start has its spawner start the program its producer chooses.
        projectile.InstructionTimer = NativeCountdown.Start(1);
        if (EnemyProjectileTypedStarts.Initial(kind) is { } animation)
            projectile.StartAnimation(animation);

        // SpawnEprojInner shows the blank frame before the first list tick. It is intentionally not the program's first
        // frame; bank-$86 advances to that on its own pass.
        projectile.Frame = EnemyProjectileFrameId.Blank;
        projectile.Radius = definition.Radius;
        projectile.Damage = definition.Damage;
        projectile.InvincibilityFrames = 96;
        projectile.DrawPriority = definition.DrawPriority;
        projectile.CanDamageSamus = definition.CanDamageSamus;
        projectile.PersistsOnSamusContact = definition.PersistsOnSamusContact;
        projectile.BlocksSamusProjectiles = definition.BlocksSamusProjectiles;
        projectile.CollisionOption = EnemyProjectileCollisionOption.RunShotList;
        projectile.CollidedProjectileType = 0;
        projectile.GraphicsIndex = graphicsIndex;
    }

    /// <summary>Dispatches the slot's current bank-$86 movement callback before its timed instruction list.</summary>
    private void RunEnemyProjectilePreInstruction(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level,
        SamusState? samus,
        Position16<RoomPixels> camera,
        int nmiFrameCounter8,
        SamusBombProjectileSystem? samusBombs)
    {
        if (Handlers.TryInterceptPreInstruction(projectile)) return;
        EnemyProjectilePreInstruction id = projectile.PreInstruction;
        if (!Handlers.TryGetPreInstruction(id, out EnemyProjectilePreInstructionHandler? handler))
        {
            throw new InvalidDataException(
                $"Enemy projectile pre-instruction {projectile.PreInstruction} is not translated.");
        }
        handler(new EnemyProjectilePreInstructionContext(projectile, level, samus, camera, nmiFrameCounter8, samusBombs));
    }

    /// <summary>Moves one projectile axis through each crossed room block and clamps it flush at the first collision.</summary>
    internal static bool MoveProjectileAxis(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level,
        Axis axis)
    {
        ushort position = axis == Axis.Horizontal ? projectile.Position.X.Whole : projectile.Position.Y.Whole;
        ushort subposition = axis == Axis.Horizontal ? projectile.Position.X.Fraction : projectile.Position.Y.Fraction;
        short velocity = unchecked((short)(axis == Axis.Horizontal ? projectile.Velocity.X.RawValue : projectile.Velocity.Y.RawValue));
        int fixedPosition = unchecked((int)SnesSixteenSixteen.Compose(position, subposition));
        fixedPosition = unchecked(fixedPosition + (SnesSixteenSixteen.FromEightEight(velocity)));
        ushort nextPosition = SnesFixedPosition.WholeOf(fixedPosition);
        ushort nextSubposition = unchecked((ushort)fixedPosition);

        ushort movementRadius = axis == Axis.Horizontal ? projectile.Radius.X : projectile.Radius.Y;
        ushort perpendicularPosition = axis == Axis.Horizontal
            ? projectile.Position.Y.Whole
            : projectile.Position.X.Whole;
        ushort perpendicularRadius = axis == Axis.Horizontal
            ? projectile.Radius.Y
            : projectile.Radius.X;

        // Bank $86 checks every room block crossed by the projectile's perpendicular
        // diameter. The positive edge is inclusive, hence radius - 1; using +radius made
        // right/down projectiles collide one pixel earlier than their native counterparts.
        ushort movementEdge = unchecked((ushort)(nextPosition +
            (velocity < 0 ? -movementRadius : movementRadius - 1)));
        int firstPerpendicularBlock = (perpendicularPosition - perpendicularRadius) >> 4;
        int lastPerpendicularBlock = (perpendicularPosition + perpendicularRadius - 1) >> 4;
        for (int block = firstPerpendicularBlock; block <= lastPerpendicularBlock; block++)
        {
            int blockX = axis == Axis.Horizontal ? movementEdge >> 4 : block;
            int blockY = axis == Axis.Horizontal ? block : movementEdge >> 4;
            if (ProjectileAxisProbeHitsRoom(
                    level,
                    projectile,
                    new(unchecked((ushort)(blockX)), unchecked((ushort)(blockY))),
                    axis,
                    movingNegative: velocity < 0,
                    targetEdge: movementEdge,
                    slopeAlignedPosition: out ushort? slopeAlignedPosition))
            {
                // `$86:894F-$897A` / `$86:8A0D-$8A38` do not merely reject the
                // attempted movement. They clear the subposition and place the
                // projectile flush against the 16-pixel block boundary it reached.
                // Without this correction a fast downward projectile retains its last
                // pre-collision coordinate and visibly hovers above the floor.
                ushort snappedPosition = slopeAlignedPosition ?? (velocity < 0
                    ? unchecked((ushort)((movementEdge | BlockPixelMask) + movementRadius + 1))
                    : unchecked((ushort)((movementEdge & ~BlockPixelMask) - movementRadius)));
                bool snapDoesNotMoveBackwards = slopeAlignedPosition.HasValue || (velocity < 0
                    ? snappedPosition <= position
                    : snappedPosition >= position);

                if (axis == Axis.Horizontal)
                {
                    projectile.Position = projectile.Position.WithFractionX(0);
                    if (snapDoesNotMoveBackwards)
                        projectile.Position = projectile.Position.WithWholeX(snappedPosition);
                }
                else
                {
                    projectile.Position = projectile.Position.WithFractionY(0);
                    if (snapDoesNotMoveBackwards)
                        projectile.Position = projectile.Position.WithWholeY(snappedPosition);
                }
                return true;
            }
        }

        if (axis == Axis.Horizontal)
        {
            projectile.Position = projectile.Position.WithX(new(nextPosition, nextSubposition));
        }
        else
        {
            projectile.Position = projectile.Position.WithY(new(nextPosition, nextSubposition));
        }
        return false;
    }

    /// <summary>Tests a leading-edge block through extension and slope geometry for one projectile move.</summary>
    private static bool ProjectileAxisProbeHitsRoom(
        RoomLevelData level,
        RoomEnemyProjectileSlot projectile,
        Position16<RoomBlocks> block,
        Axis axis,
        bool movingNegative,
        ushort targetEdge,
        out ushort? slopeAlignedPosition)
    {
        slopeAlignedPosition = null;
        if (block.X >= (uint)level.Size.X ||
            block.Y >= (uint)level.Size.Y)
        {
            return true;
        }

        // Resolve type-$5/$D BTS links before dispatching the final block family, just as
        // the native projectile collision loop does. Type zero is air; type nine is a door
        // and remains a wall to an enemy projectile.
        int blockIndex = EnemyTerrain.ResolveEnemyCollisionBlockIndex(level, block);
        if (blockIndex < 0)
            return true;

        RoomCollisionBlock collisionBlock = level.GetCollisionBlockByIndex(blockIndex);
        RoomCollisionType type = collisionBlock.CollisionType;
        if (type == RoomCollisionType.Slope && !collisionBlock.Bts.IsNonSquareSlope)
        {
            // Bank $86 tests only occupied eight-pixel quadrants touched by the
            // leading edge. Empty halves of square slopes are not solid walls.
            int perpendicularPosition = (axis == Axis.Horizontal) ? projectile.Position.Y.Whole : projectile.Position.X.Whole;
            int perpendicularRadius = (axis == Axis.Horizontal) ? projectile.Radius.Y : projectile.Radius.X;
            int blockStart = ((axis == Axis.Horizontal) ? block.Y : block.X) << 4;
            int firstPixel = Math.Max(blockStart, perpendicularPosition - perpendicularRadius);
            int lastPixel = Math.Min(blockStart + 15, perpendicularPosition + perpendicularRadius - 1);
            for (int half = firstPixel >> 3; half <= lastPixel >> 3; half++)
            {
                int quadrant = (axis == Axis.Horizontal)
                    ? ((targetEdge & 8) >> 3) | ((half & 1) << 1)
                    : ((targetEdge & 8) >> 2) | (half & 1);
                int tableIndex = 4 * collisionBlock.Bts.SlopeShape +
                    (quadrant ^ collisionBlock.Bts.SlopeOrientation);
                if (!SquareSlopeDefinitions.EnemyQuadrantIsSolid(tableIndex))
                    continue;

                // The square-slope reaction places the projectile at an eight-pixel
                // boundary before the caller's full-block clamp.
                int radius = (axis == Axis.Horizontal) ? projectile.Radius.X : projectile.Radius.Y;
                slopeAlignedPosition = movingNegative
                    ? unchecked((ushort)((targetEdge | HalfBlockPixelMask) + radius + 1))
                    : unchecked((ushort)((targetEdge & ~HalfBlockPixelMask) - radius));
                return true;
            }
            return false;
        }

        if (type == RoomCollisionType.Slope && collisionBlock.Bts.IsNonSquareSlope)
        {
            // Horizontal bank-$86 motion ignores non-square slopes; the following
            // vertical pass owns their height profile. Treating one as a full wall can
            // consume every Kago falling frame before Y collision is ever attempted.
            if ((axis == Axis.Horizontal))
                return false;

            // Bank $86's vertical projectile collision uses the same 16-sample slope
            // profiles as bank $94. Only the column containing the projectile center owns
            // non-square geometry; neighboring columns touched by its radius do not turn
            // the slope into a full-height wall.
            if (block.X != projectile.Position.X.Whole >> 4)
                return false;

            bool movingUp = movingNegative;
            if (movingUp != collisionBlock.Bts.SlopeFlipsVertically)
                return false;

            int height = EnemyTerrain.ReadNonSquareSlopeHeight(
                collisionBlock.Bts,
                (collisionBlock.Bts.SlopeFlipsHorizontally
                    ? projectile.Position.X.Whole ^ BlockPixelMask
                    : projectile.Position.X.Whole) & BlockPixelMask);
            int edgeWithinBlock = movingUp
                ? (targetEdge & BlockPixelMask) ^ BlockPixelMask
                : targetEdge & BlockPixelMask;
            int adjustment = height - edgeWithinBlock - 1;
            if (movingUp)
            {
                if (adjustment > 0)
                    return false;
            }
            else if (height - edgeWithinBlock != 1 && adjustment >= 0)
            {
                return false;
            }

            slopeAlignedPosition = movingUp
                ? unchecked((ushort)((block.Y << 4) + 16 - height + projectile.Radius.Y))
                : unchecked((ushort)((block.Y << 4) + height - projectile.Radius.Y));
            return true;
        }

        return EnemyProjectileBlockIsWall(type);
    }

    /// <summary>
    /// The unconditional rows of <c>$86:8846</c>/<c>$8866</c>, the enemy-projectile block
    /// reaction tables. Slopes and extensions are resolved before reaching this test.
    /// Spike blocks stop projectiles; grapple blocks do not.
    /// </summary>
    private static bool EnemyProjectileBlockIsWall(RoomCollisionType type) => type is
        RoomCollisionType.Slope or
        RoomCollisionType.HorizontalExtension or
        RoomCollisionType.SolidBlock or
        RoomCollisionType.DoorBlock or
        RoomCollisionType.SpikeBlock or
        RoomCollisionType.SpecialBlock or
        RoomCollisionType.ShootableBlock or
        RoomCollisionType.VerticalExtension or
        RoomCollisionType.BombableBlock;

    /// <summary>
    /// <c>CheckForCollisionWithNonAirBlock</c> ($A6:D4F9): any block type other than air,
    /// read directly without following extensions.
    /// </summary>
    internal static bool NonAirBlockAt(RoomLevelData level, Position16<RoomPixels> pixel) =>
        level.GetCollisionBlock(new(unchecked((ushort)(pixel.X >> 4)), unchecked((ushort)(pixel.Y >> 4)))).CollisionType != RoomCollisionType.Air;

    /// <summary>Checks radius overlap and publishes touch-list or damage behavior for a projectile that reaches Samus.</summary>
    internal KnockbackHorizontalDirection? ResolveEnemyProjectileSamusCollision(
        RoomEnemyProjectileSlot projectile,
        SamusState samus)
    {
        // `$A0:9894` already made the pass-level invincibility/contact-damage decision.
        // Do not re-read the timer here: the first hit writes it, but the original loop
        // deliberately continues testing the other projectile slots in this same pass.
        // $A0:98C7-98DB skips either zero radius before overlap testing. A zero
        // radius is not a point-shaped hazard: decorative wall shards use it to
        // remain harmless even while passing straight through Samus's body.
        if (!projectile.CanDamageSamus || projectile.Radius.X == 0 || projectile.Radius.Y == 0)
            return null;

        int xDistance = Math.Abs(unchecked((short)(projectile.Position.X.Whole - samus.Position.X.Whole)));
        int yDistance = Math.Abs(unchecked((short)(projectile.Position.Y.Whole - samus.Position.Y.Whole)));
        if (xDistance >= projectile.Radius.X + samus.Kinematics.Radius.X ||
            yDistance >= projectile.Radius.Y + samus.Kinematics.Radius.Y)
        {
            return null;
        }

        ushort damage = SamusSuitDamage.Reduce(checked((ushort)projectile.Damage), samus.EquippedItems);
        samus.Health = samus.Health <= damage
            ? 0
            : samus.Health - damage;
        samus.InvincibilityTimer = NativeCountdown.FromRaw(checked((ushort)projectile.InvincibilityFrames));
        KnockbackHorizontalDirection knockbackXDirection = unchecked((short)(
            samus.Position.X.Whole - projectile.Position.X.Whole)) >= 0
            ? KnockbackHorizontalDirection.Right
            : KnockbackHorizontalDirection.Left;

        // `$A0:9930-$993F` installs the definition's touch list before consulting property
        // $4000. Mother Brain's turret bullet depends on that ordering: it survives contact
        // but immediately changes to its smoke list. Keeping this in the common path also
        // prevents later persistent projectile families from needing bespoke hit effects.
        ushort touchInstruction =
            EnemyProjectileDefinitionCatalog.Get(projectile.Kind).TouchInstructionList;
        // Every touch list without a typed reaction of its own is the shared delete list.
        if (Handlers.TryGetTouchAnimation(projectile.Kind, out Func<EnemyProjectileAnimation>? touchAnimation))
            projectile.StartAnimation(touchAnimation());
        else if (touchInstruction != 0)
            projectile.StartAnimation(EnemyProjectileDeleteAnimation.Instance);

        // Generic enemy-projectile contact deletes Ridley's fireball. Afterburn is a wall-
        // impact feature from $86:940E, so a Samus contact does not create the wall bloom.
        if (!projectile.PersistsOnSamusContact)
            projectile.Clear();

        return knockbackXDirection;
    }

    /// <summary>Advances a slot's typed program one projectile pass.</summary>
    internal void ProcessEnemyProjectileInstructions(
        RoomEnemyProjectileSlot projectile,
        SamusState? samus,
        Position16<RoomPixels> camera)
    {
        if (projectile.Animation is { } animation)
        {
            animation.Advance(new(projectile, samus, camera), Handlers);
            return;
        }

        // A slot its pre-instruction cleared this pass has no program left, so its instructions do nothing.
        if (!projectile.IsActive)
            return;
        throw new InvalidOperationException($"Enemy projectile {projectile.Kind} in slot {projectile.SlotIndex} runs no program.");
    }

    /// <summary>
    /// Ports $86:ECE3/$ED17. Both instructions consume one object-number operand and one
    /// RNG word; its low and high bytes offset X and Y around the death actor respectively.
    /// CreateSpriteAtPos owns a separate finite bank-$B4 pool, so these decorations never
    /// consume one of the eighteen pickup/death projectile slots.
    /// </summary>
    internal void SpawnRandomEnemyDeathSprite(
        RoomEnemyProjectileSlot projectile,
        RoomSpriteObjectKind kind,
        int mask,
        int center)
    {
        int random = _room.Random.Generate();
        ushort x = unchecked((ushort)(
            projectile.Position.X.Whole + (random & mask) - center));
        ushort y = unchecked((ushort)(
            projectile.Position.Y.Whole + (SnesLittleEndian.HighByte(unchecked((ushort)random)) & mask) - center));
        _ = _spriteObjects.SpawnRoomSpriteObject(new(x, y), kind, graphicsIndex: 0);
    }

    /// <summary>
    /// Uses native RNG rejection and sign bits to reposition a projectile within <paramref name="scatter"/>'s masks.
    /// Each axis rejects negative candidate offsets, while two independent bits from the first random sample choose
    /// the eventual signs. This peculiar rejection loop is observable in Bomb Torizo's sonic-boom wall impact, so a
    /// host RNG approximation would produce a visibly different debris cloud and desynchronize later randomness.
    /// </summary>
    internal void Scatter(RoomEnemyProjectileSlot projectile, EnemyProjectileScatter scatter)
    {
        Ensure.NotNull(projectile);
        byte xMask = checked((byte)scatter.Range.X);
        byte xCenter = checked((byte)scatter.Center.X);
        byte yMask = checked((byte)scatter.Range.Y);
        byte yCenter = checked((byte)scatter.Center.Y);
        int signSample = _room.Random.Generate();

        int xOffset;
        do
        {
            xOffset = (xMask & unchecked((byte)_room.Random.Generate())) - xCenter;
        }
        while (xOffset < 0);
        if (unchecked((short)signSample) < 0)
            xOffset = -xOffset;
        projectile.Position = projectile.Position.WithWholeX(unchecked((ushort)(projectile.Position.X.Whole + xOffset)));

        int yOffset;
        do
        {
            yOffset = (yMask & unchecked((byte)_room.Random.Generate())) - yCenter;
        }
        while (yOffset < 0);
        if ((signSample >> 14 & 1) != 0)
            yOffset = -yOffset;
        projectile.Position = projectile.Position.WithWholeY(unchecked((ushort)(projectile.Position.Y.Whole + yOffset)));
    }

    /// <summary>
    /// Allocates the shared room-graphics dust/explosion actor at <c>$86:E509</c>.
    /// The caller selects the animation the native spawn parameter indexes; this is shared by
    /// special Rinka deaths, Boulder's impact clouds, and later families that invoke the same
    /// engine primitive.
    /// </summary>
    internal void SpawnRoomGraphicsDustExplosion(
        Position16<RoomPixels> position,
        MiscDustAnimation animation)
    {
        MiscDustProjectileProgram program = MiscDustProjectileDefinitions.Program(animation);
        RoomEnemyProjectileSlot? projectile = AllocateEnemyProjectile();
        if (projectile is null)
            return;

        InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.MiscDustExplosion,
            graphicsIndex: 0);
        projectile.Position = projectile.Position.WithWhole(position);

        // EprojInit_DustCloudOrExplosion ignores the definition's placeholder list and
        // indexes this literal thirty-word table with the spawn parameter. Parameter three
        // selects the small explosion for Rinka; Boulder passes $11 for its impact cloud.
        projectile.StartAnimation(new MiscDustProjectileAnimation(program));
    }

    /// <summary>
    /// Allocates the light or dark Ceres falling-tile actor requested by room main
    /// <c>$8F:E525</c>. The X coordinate and palette variant come directly from the room's
    /// current RNG word; this initializer owns only bank-$86's projectile fields.
    /// </summary>
    public void SpawnCeresFallingDebris(ushort xPosition, bool dark)
    {
        _room.EnsureLoaded();
        RoomEnemyProjectileSlot? projectile = AllocateEnemyProjectile();
        if (projectile is null)
            return;

        RoomEnemyProjectileKind kind = dark
            ? RoomEnemyProjectileKind.CeresFallingDebrisDark
            : RoomEnemyProjectileKind.CeresFallingDebrisLight;
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(projectile, kind, graphicsIndex: EnemyPaletteBits.Palette7);
        projectile.Position = projectile.Position.WithWhole(new(xPosition, 42));
        projectile.Velocity = new(new SnesSignedEightEight(0), new SnesSignedEightEight(16));
        projectile.Variable0 = 0;
        projectile.Variable1 = 0;
        projectile.StartAnimation(new CeresFallingDebrisAnimation(dark ? CeresFallingDebrisProgram.Dark : CeresFallingDebrisProgram.Light));
    }

    /// <summary>Deletes a gunship dust cloud when its program ends.</summary>
    internal static void RegisterGunshipDustCues(EnemyProjectileHandlerTable table) =>
        table.RegisterCue<GunshipDustCue>((context, cue) =>
        {
            RoomEnemyProjectileSlot projectile = context.Projectile;
            switch (cue)
            {
                case GunshipDustCue.Delete:
                    projectile.Clear();
                    return;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(GunshipDustCue)} {cue}.");
            }
        });

    /// <summary>
    /// Ports <c>SpawnEprojWithRoomGfx($E6D2, 0)</c> and initializer $86:E6AD.
    /// The cartridge derives the actor origin from the currently executing save-station
    /// PLM: one block right and two blocks above its trigger block.
    /// </summary>
    public void SpawnSaveStationElectricity(int plmBlockIndex, int roomWidthInBlocks)
    {
        _room.EnsureLoaded();
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(roomWidthInBlocks);
        Ensure.AtLeastZero(plmBlockIndex);

        RoomEnemyProjectileSlot? projectile = AllocateEnemyProjectile();
        if (projectile is null)
        {
            // SpawnEprojInner deliberately drops a spawn when all eighteen physical slots
            // are occupied. This is the cartridge's finite-pool behavior, not a host error.
            return;
        }

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.SaveStationElectricity,
            graphicsIndex: 0);
        int blockX = plmBlockIndex % roomWidthInBlocks;
        int blockY = plmBlockIndex / roomWidthInBlocks;
        projectile.Position = projectile.Position.WithWhole(new(unchecked((ushort)(16 * (blockX + 1))), unchecked((ushort)(16 * (blockY - 2)))));
    }

    /// <summary>
    /// Allocates one of gunship function <c>$A2:AC1B</c>'s six room-graphics dust actors.
    /// Parameter values are even byte offsets <c>0..A</c> into the native X/list tables.
    /// </summary>
    internal void SpawnGunshipLiftoffDustCloud(ushort parameter, SamusState samus)
    {
        // Validate before allocation, including when the native projectile pool is full.
        var definition = GunshipDustDefinitions.ForParameter(parameter);

        RoomEnemyProjectileSlot? projectile = AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.GunshipLiftoffDustCloud,
            graphicsIndex: 0);
        projectile.Position = projectile.Position.WithWhole(new(unchecked((ushort)(samus.Position.X.Whole + definition.XOffset)), unchecked((ushort)(samus.Position.Y.Whole + GunshipDustDefinitions.YOffset))));
        projectile.Velocity = new(new SnesSignedEightEight(0), new SnesSignedEightEight(0));
        projectile.Variable0 = parameter;
        projectile.StartAnimation(new GunshipDustAnimation(definition.Program));
    }

    /// <summary>Spawns the four bank-$86 particles emitted by a dying/burrowing Skree.</summary>
    internal void SpawnSkreeParticleBurst(RoomEnemySlot skree) => SpawnMetalParticleBurst(skree,
        RoomEnemyProjectileKind.SkreeParticleDownRight, RoomEnemyProjectileKind.SkreeParticleUpRight,
        RoomEnemyProjectileKind.SkreeParticleDownLeft, RoomEnemyProjectileKind.SkreeParticleUpLeft);

    /// <summary>
    /// Spawns Metaree's four metal-particle definitions. Their motion initializers are
    /// shared byte-for-byte with Skree, but pointer <c>$86:8AC5</c> selects Metaree's ROM
    /// spritemap instead of silently reusing the visually different Skree debris.
    /// </summary>
    internal void SpawnMetareeParticleBurst(RoomEnemySlot metaree) => SpawnMetalParticleBurst(metaree,
        RoomEnemyProjectileKind.MetareeParticleDownRight, RoomEnemyProjectileKind.MetareeParticleUpRight,
        RoomEnemyProjectileKind.MetareeParticleDownLeft, RoomEnemyProjectileKind.MetareeParticleUpLeft);

    /// <summary>
    /// The four particles of a Skree or Metaree burst, six pixels either side of the source: the outer pair flies at
    /// 1.25 pixels outward and just over 3 up per update, the inner pair at 0.375 outward and just over 4 up, in 8.8.
    /// </summary>
    private void SpawnMetalParticleBurst(RoomEnemySlot source, RoomEnemyProjectileKind downRight, RoomEnemyProjectileKind upRight,
        RoomEnemyProjectileKind downLeft, RoomEnemyProjectileKind upLeft)
    {
        SpawnSkreeOrMetareeParticle(source, downRight, 6, new(320, -769));
        SpawnSkreeOrMetareeParticle(source, upRight, 6, new(96, -1025));
        SpawnSkreeOrMetareeParticle(source, downLeft, -6, new(-320, -769));
        SpawnSkreeOrMetareeParticle(source, upLeft, -6, new(-96, -1025));
    }

    /// <summary>
    /// Spawns one particle through <c>SpawnEprojInner</c>'s definition copy (damage four,
    /// contact enabled, Skree or Metaree list), then applies the shared $86:8ACD-$8B39
    /// initializer's position and 8.8 velocity.
    /// </summary>
    private void SpawnSkreeOrMetareeParticle(
        RoomEnemySlot source,
        RoomEnemyProjectileKind kind,
        int xOffset,
        EightEightVector<RoomPixels> velocity)
    {
        RoomEnemyProjectileSlot? particle = AllocateEnemyProjectile();
        if (particle is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            particle,
            kind,
            unchecked((ushort)(source.VramTilesIndex | source.PaletteIndex)));
        particle.Position = particle.Position.WithWhole(new(unchecked((ushort)(source.Position.X.Whole + xOffset)), source.Position.Y.Whole));
        particle.Velocity = velocity;
    }

    /// <summary>Ports common bank-$A0 <c>CreateADudShot</c> for a protected head hitbox.</summary>
    internal void CreateEnemyProjectileDudShot(SamusProjectileSlot projectile)
    {
        _ = _spriteObjects.SpawnRoomSpriteObject(
            projectile.Position.Whole,
            RoomSpriteObjectKind.EnemyProjectileDud,
            graphicsIndex: 0);
        LastEnemyProjectileDudSoundEffect = SoundEffectLibrary1Sounds.DudShot;
        projectile.Direction = unchecked((ushort)(projectile.Direction | SamusProjectileDirectionMasks.Spent));
    }

    /// <summary>
    /// Ports <c>Delete_EnemyProjectile_IfVerticallyOffScreen</c> at $86:B5B9, shared by
    /// Dragon's fireball and both n00b-tube shard pre-instructions.
    /// </summary>
    /// <remarks>
    /// A position above the camera's top row (signed-negative difference) is kept; only a
    /// difference of <see cref="EnemyProjectileOffScreenDefinitions.VerticalDeletionDepth"/>
    /// or more below it deletes the projectile.
    /// </remarks>
    internal static void DeleteEnemyProjectileIfVerticallyOffScreen(
        RoomEnemyProjectileSlot projectile,
        ushort cameraY)
    {
        ushort screenY = unchecked((ushort)(projectile.Position.Y.Whole - cameraY));
        if (unchecked((short)screenY) >= 0 &&
            screenY >= EnemyProjectileOffScreenDefinitions.VerticalDeletionDepth)
        {
            projectile.Clear();
        }
    }

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        if (LastEnemyProjectileDudSoundEffect is { } dudSound)
            audio.QueueEnemySound(dudSound, maximumQueued: 3);
    }

    /// <summary>Registers Skree/Metaree particle movement with falling acceleration and camera-square deletion.</summary>
    internal static void RegisterMetalSkreeParticlePreInstruction(EnemyProjectileHandlerTable table)
    {
        table.RegisterPreInstruction(context =>
        {
            context.Projectile.Position = context.Projectile.Position.Advance(context.Projectile.Velocity.ToSixteenSixteen());
            context.Projectile.Velocity = context.Projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(context.Projectile.Velocity.Y.RawValue + 80))));
            if (unchecked((ushort)(context.Projectile.Position.X.Whole - context.Camera.X)) >= 256 ||
                unchecked((ushort)(context.Projectile.Position.Y.Whole - context.Camera.Y)) >= 256)
            {
                context.Projectile.Clear();
            }
        }, EnemyProjectilePreInstruction.MetalSkreeParticle);
    }

    /// <summary>Registers tile gravity and impact dust/sound when Ceres debris strikes room terrain.</summary>
    internal void RegisterCeresFallingTilePreInstruction(EnemyProjectileHandlerTable table)
    {
        table.RegisterPreInstruction(context =>
        {
            context.Projectile.Velocity = context.Projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(context.Projectile.Velocity.Y.RawValue + 16))));
            if (EnemyProjectileEngine.MoveProjectileAxis(context.Projectile, context.Level, axis: Axis.Vertical))
            {
                ushort impactX = context.Projectile.Position.X.Whole;
                ushort impactY = context.Projectile.Position.Y.Whole;
                context.Projectile.Clear();
                SpawnRoomGraphicsDustExplosion(new(impactX, impactY), MiscDustAnimation.SmallDustCloud);
                _audio.QueueEnemySound(SoundEffectLibrary2Sounds.CeresFallingTileImpact, maximumQueued: 6);
            }
        }, EnemyProjectilePreInstruction.CeresFallingTile);
    }

    /// <summary>
    /// Draws the bank-$86 one-in-four branch ($86:A456): it is taken when both high bits of the room's next random
    /// number are set.
    /// </summary>
    internal bool TakeOneInFourBranch() => _room.Random.Generate() >> 14 == 3;

    /// <summary>Sets every behavior bit in <paramref name="flags"/>, as a list's property OR does.</summary>
    internal static void SetPropertyFlags(RoomEnemyProjectileSlot projectile, EnemyProjectilePropertyFlags flags)
    {
        Ensure.NotNull(projectile);
        if (flags.HasFlag(EnemyProjectilePropertyFlags.HighPriority))
            projectile.DrawPriority = EnemyProjectileDrawPriority.High;
        if (flags.HasFlag(EnemyProjectilePropertyFlags.HarmlessToSamus))
            projectile.CanDamageSamus = false;
        if (flags.HasFlag(EnemyProjectilePropertyFlags.PersistsOnContact))
            projectile.PersistsOnSamusContact = true;
        if (flags.HasFlag(EnemyProjectilePropertyFlags.BlocksShots))
            projectile.BlocksSamusProjectiles = true;
    }

    /// <summary>Clears every behavior bit in <paramref name="flags"/>, as a list's property AND clears the bits its mask leaves out.</summary>
    internal static void ClearPropertyFlags(RoomEnemyProjectileSlot projectile, EnemyProjectilePropertyFlags flags)
    {
        Ensure.NotNull(projectile);
        if (flags.HasFlag(EnemyProjectilePropertyFlags.HighPriority))
            projectile.DrawPriority = EnemyProjectileDrawPriority.Low;
        if (flags.HasFlag(EnemyProjectilePropertyFlags.HarmlessToSamus))
            projectile.CanDamageSamus = true;
        if (flags.HasFlag(EnemyProjectilePropertyFlags.PersistsOnContact))
            projectile.PersistsOnSamusContact = false;
        if (flags.HasFlag(EnemyProjectilePropertyFlags.BlocksShots))
            projectile.BlocksSamusProjectiles = false;
    }
}
