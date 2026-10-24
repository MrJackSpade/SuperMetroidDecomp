using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Crawlers.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>The Crawlers species owner: its per-room state, AI routines and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class CrawlersBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } =
    [.. CrawlersPartDefinitions.Parts.Keys];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new CrawlerEntity(this, slot, common);

    /// <summary>Performs wall, floor, and slope probes for attached crawlers and falling Zoomers.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Signals the Super Missile quake that knocks orange Zoomers from surfaces.</summary>
    private readonly RoomEarthquake _earthquake;

    /// <summary>Binds common crawler motion and orange-Zoomer pursuit to terrain and quake state.</summary>
    internal CrawlersBehavior(EnemyTerrain terrain, RoomEarthquake earthquake)
    {
        _terrain = terrain;
        _earthquake = earthquake;
    }

    /// <summary>Carries out a crawler animation's cue: each surface's first step installs the movement function main AI runs.</summary>
    private static CrawlerProgram? ApplyCrawlerCue(CrawlerEnemyState state, CrawlerCue cue)
    {
        switch (cue.Kind)
        {
            case CrawlerCueKind.SetFunction:
                state.Function = cue.Function;
                return null;
            default:
                throw new InvalidOperationException($"Undefined {nameof(CrawlerCueKind)} {cue.Kind}.");
        }
    }

    /// <summary>
    /// Ports the species wrappers at $A3:96E3/$993B/$B66F/$E2D4/$E59C/$E669 and their
    /// shared initializer at $A3:E67A. Each wrapper differs only by its four-list table and
    /// the species byte offset later consumed by the shared surface-orientation tables.
    /// </summary>
    internal static void InitializeCrawler(
        RoomEnemySlot slot,
        CrawlerAnimationFamily animationFamily,
        ushort? speciesInstructionOffset = null)
    {
        var state = new CrawlerEnemyState
        {
            SpeedParameter = slot.Spawn.Population.Parameter1,
            SurfaceTableOffset = speciesInstructionOffset ?? slot.Spawn.Population.Parameter2,
        };
        Bind(slot, state);

        // The population initialization parameter is an orientation index, not an
        // instruction pointer. Masking to two bits before selector lookup is native.
        // InitAI_Viola ($A3:B678) alone selects with the property bits instead.
        var orientation = (CrawlerSurfaceOrientation)((animationFamily == CrawlerAnimationFamily.Viola
            ? slot.Properties.FamilyData
            : slot.CurrentInstruction) & 3);
        state.Program = AnimationProgramCursor<CrawlerProgram>.Start(CrawlerAnimationDefinitions.InitialProgram(
            animationFamily, orientation));
        slot.Frame = EnemyFrameId.Empty;
        slot.InstructionTimer = WrappingCountdown.Start(1);
        state.Function = CrawlerEnemyFunction.InstructionPending;

        state.Velocity = ResetCrawlerVelocitiesFromProperties(slot, state.SpeedParameter, state.Velocity);
    }

    /// <summary>Shared $A3:E67A velocity reset, also called by Yard after landing.</summary>
    /// <param name="slot">The crawling enemy, whose property bits sign the velocity.</param>
    /// <param name="speedParameter">The crawl speed selector: a speed-table index or the preserve-velocity selector.</param>
    /// <param name="velocity">The velocity kept when the selector preserves it.</param>
    /// <returns>The reset velocity: <paramref name="velocity"/> or the parameter's speed on both axes, signed by the properties.</returns>
    internal static EightEightVector<RoomPixels> ResetCrawlerVelocitiesFromProperties(
        RoomEnemySlot slot, int speedParameter, EightEightVector<RoomPixels> velocity)
    {
        if (speedParameter != CrawlerSpeedDefinitions.PreserveVelocity)
        {
            if (speedParameter >= CrawlerSpeedDefinitions.Count)
            {
                throw new InvalidDataException(
                    $"Crawler speed parameter ${speedParameter:X4} exceeds $A3:E5F0.");
            }
            ushort speed = CrawlerSpeedDefinitions.ForParameter(checked((ushort)speedParameter));
            velocity = new(new SnesSignedEightEight(speed), new SnesSignedEightEight(speed));
        }

        // The low two population-property bits encode which side of the starting block the
        // crawler occupies. They are not independent EnemyProperties flags.
        return (slot.Properties.FamilyData & 3) switch
        {
            0 => velocity.ReversedX(),
            2 => velocity.ReversedY(),
            _ => velocity,
        };
    }

    /// <summary>Ports the separate orange-Zoomer initializer at $A3:E043.</summary>
    internal static void InitializeHZoomer(RoomEnemySlot slot)
    {
        int speedParameter = slot.Spawn.Population.Parameter1;
        if (speedParameter >= CrawlerSpeedDefinitions.Count)
        {
            throw new InvalidDataException(
                $"HZoomer speed parameter ${speedParameter:X4} exceeds $A3:E5F0.");
        }

        var state = new CrawlerEnemyState
        {
            Function = CrawlerEnemyFunction.HZoomerInstructionPending,
            SpeedParameter = speedParameter,
            SurfaceTableOffset = slot.Spawn.Population.Parameter2,
        };
        Bind(slot, state);
        var orientation = (CrawlerSurfaceOrientation)(slot.CurrentInstruction & 3);
        state.Program = AnimationProgramCursor<CrawlerProgram>.Start(CrawlerAnimationDefinitions.InitialProgram(
            CrawlerAnimationFamily.HZoomer, orientation));
        slot.Frame = EnemyFrameId.Empty;
        slot.InstructionTimer = WrappingCountdown.Start(1);
        ushort velocity = CrawlerSpeedDefinitions.ForParameter(checked((ushort)speedParameter));
        state.Velocity = new(new SnesSignedEightEight(velocity), new SnesSignedEightEight(velocity));
        state.Velocity = (slot.Properties.FamilyData & 3) switch
        {
            0 => state.Velocity.ReversedX(),
            2 => state.Velocity.ReversedY(),
            _ => state.Velocity,
        };
    }

    /// <summary>
    /// Ports $A3:E08B-$E23B. This is not an alias of common crawler main: while attached,
    /// orange Zoomer reverses its tangent to pursue Samus and reacts to earthquake $14/$1E.
    /// </summary>
    private void RunHZoomerMain(
        RoomEnemySlot slot,
        SamusState? samus,
        RoomLevelData? level)
    {
        if (samus is null)
            throw new InvalidOperationException("HZoomer AI requires the active Samus actor.");
        if (level is null)
            throw new InvalidOperationException("HZoomer movement requires room collision data.");
        CrawlerEnemyState state = slot.RequireSpeciesState<CrawlerEnemyState>();
        switch (state.Function)
        {
            case CrawlerEnemyFunction.HZoomerInstructionPending:
                return;
            case CrawlerEnemyFunction.HZoomerCrawlingVertically:
                RunHZoomerVertical(slot, state, samus, level);
                return;
            case CrawlerEnemyFunction.HZoomerCrawlingHorizontally:
                RunHZoomerHorizontal(slot, state, samus, level);
                return;
            case CrawlerEnemyFunction.Falling:
                RunCrawlerFalling(slot, state, level);
                return;
            case CrawlerEnemyFunction.InstructionPending:
            case CrawlerEnemyFunction.CrawlingVertically:
            case CrawlerEnemyFunction.CrawlingHorizontally:
            default:
                throw new InvalidDataException(
                    $"HZoomer function {state.Function?.ToString() ?? "none"} is not translated.");
        }
    }

    /// <summary>Tracks Samus while attached to a wall, reversing its vertical tangent or falling after lost contact.</summary>
    private void RunHZoomerVertical(
        RoomEnemySlot slot,
        CrawlerEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        TriggerCrawlerEarthquakeFall(state);
        int wallProbe = Shift8AddMagnitude(state.Velocity.X.RawValue, 1);
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, wallProbe))
        {
            state.ConsecutiveTurnCounter = 0;
            EnemyTerrain.AlignEnemyYWithNonSquareSlope(level, slot);
            int tangent = SnesSixteenSixteen.FromEightEight(state.Velocity.Y.SignedRawValue);
            if (_terrain.MoveEnemyVertically(level, slot, tangent))
            {
                state.Velocity = state.Velocity.ReversedX();
                SetHZoomerVerticalInstruction(slot, state);
                return;
            }

            bool samusBelow = unchecked((short)(samus.Position.Y.Whole - slot.Position.Y.Whole)) >= 0;
            bool movingDown = state.Velocity.Y.SignedRawValue >= 0;
            if (samusBelow != movingDown)
                state.Velocity = state.Velocity.ReversedY();
            return;
        }

        state.ConsecutiveTurnCounter = unchecked((ushort)(state.ConsecutiveTurnCounter + 1));
        if (unchecked((short)(state.ConsecutiveTurnCounter - 4)) >= 0)
        {
            BeginCrawlerFall(state);
            return;
        }
        state.Velocity = state.Velocity.ReversedY();
        SetHZoomerVerticalInstruction(slot, state);
    }

    /// <summary>Tracks Samus along a floor or ceiling, accounting for slopes and repeated loss of surface contact.</summary>
    private void RunHZoomerHorizontal(
        RoomEnemySlot slot,
        CrawlerEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        TriggerCrawlerEarthquakeFall(state);
        int surfaceProbe = Shift8AddMagnitude(state.Velocity.Y.RawValue, 1);
        if (_terrain.MoveEnemyVertically(level, slot, surfaceProbe))
        {
            state.ConsecutiveTurnCounter = 0;
            int tangent = GetCrawlerSlopeAdjustedHorizontalDisplacement(slot, state, level);
            if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, tangent))
            {
                state.Velocity = state.Velocity.ReversedY();
                SetHZoomerHorizontalInstruction(slot, state);
                return;
            }

            EnemyTerrain.AlignEnemyYWithNonSquareSlope(level, slot);
            bool samusRight = unchecked((short)(samus.Position.X.Whole - slot.Position.X.Whole)) >= 0;
            bool movingRight = state.Velocity.X.SignedRawValue >= 0;
            if (samusRight != movingRight)
                state.Velocity = state.Velocity.ReversedX();
            return;
        }

        state.ConsecutiveTurnCounter = unchecked((ushort)(state.ConsecutiveTurnCounter + 1));
        if (unchecked((short)(state.ConsecutiveTurnCounter - 4)) >= 0)
        {
            BeginCrawlerFall(state);
            return;
        }
        state.Velocity = state.Velocity.ReversedX();
        SetHZoomerHorizontalInstruction(slot, state);
    }

    /// <summary>Changes an attached orange Zoomer to falling during the native Super Missile earthquake.</summary>
    private void TriggerCrawlerEarthquakeFall(CrawlerEnemyState state)
    {
        if (_earthquake.EarthquakeTimer.RawValue == SuperMissileDefinitions.EarthquakeDuration &&
            _earthquake.EarthquakeType == SuperMissileDefinitions.EarthquakeType)
            BeginCrawlerFall(state);
    }

    /// <summary>Selects orange Zoomer's wall-climbing animation from its vertical velocity sign.</summary>
    private static void SetHZoomerVerticalInstruction(
        RoomEnemySlot slot,
        CrawlerEnemyState state) =>
        SetHZoomerProgram(
            slot,
            state,
            state.Velocity.Y.SignedRawValue < 0
                ? CrawlerAnimationDefinitions.InitialProgram(
                    CrawlerAnimationFamily.HZoomer, CrawlerSurfaceOrientation.UpsideDown)
                : CrawlerAnimationDefinitions.InitialProgram(
                    CrawlerAnimationFamily.HZoomer, CrawlerSurfaceOrientation.UpsideUp));

    /// <summary>Selects orange Zoomer's floor or ceiling animation from its horizontal velocity sign.</summary>
    private static void SetHZoomerHorizontalInstruction(
        RoomEnemySlot slot,
        CrawlerEnemyState state) =>
        SetHZoomerProgram(
            slot,
            state,
            state.Velocity.X.SignedRawValue < 0
                ? CrawlerAnimationDefinitions.InitialProgram(
                    CrawlerAnimationFamily.HZoomer, CrawlerSurfaceOrientation.UpsideRight)
                : CrawlerAnimationDefinitions.InitialProgram(
                    CrawlerAnimationFamily.HZoomer, CrawlerSurfaceOrientation.UpsideLeft));

    /// <summary>Starts the chosen orange-Zoomer animation with fresh instruction and loop timers.</summary>
    private static void SetHZoomerProgram(
        RoomEnemySlot slot,
        CrawlerEnemyState state,
        CrawlerProgram program)
    {
        state.Program = AnimationProgramCursor<CrawlerProgram>.Start(program);
        slot.InstructionTimer = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);
    }

    /// <summary>Ports shared crawler main AI at $A3:E6C2.</summary>
    private void RunCrawlerMain(RoomEnemySlot slot, RoomLevelData? level)
    {
        if (level is null)
            throw new InvalidOperationException("Crawler movement requires active room collision data.");
        CrawlerEnemyState state = slot.RequireSpeciesState<CrawlerEnemyState>();
        switch (state.Function)
        {
            case CrawlerEnemyFunction.InstructionPending:
                return;
            case CrawlerEnemyFunction.CrawlingVertically:
                RunCrawlerVertical(slot, state, level);
                return;
            case CrawlerEnemyFunction.Falling:
                RunCrawlerFalling(slot, state, level);
                return;
            case CrawlerEnemyFunction.CrawlingHorizontally:
                RunCrawlerHorizontal(slot, state, level);
                return;
            case CrawlerEnemyFunction.HZoomerInstructionPending:
            case CrawlerEnemyFunction.HZoomerCrawlingVertically:
            case CrawlerEnemyFunction.HZoomerCrawlingHorizontally:
            default:
                throw new InvalidDataException(
                    $"Crawler function {state.Function?.ToString() ?? "none"} is not translated.");
        }
    }

    /// <summary>Ports $A3:E6C8: move while attached to a vertical wall.</summary>
    private void RunCrawlerVertical(
        RoomEnemySlot slot,
        CrawlerEnemyState state,
        RoomLevelData level)
    {
        // Native sets the next function but still executes this attached movement
        // call. Falling displacement begins on the following AI frame.
        TriggerCrawlerEarthquakeFall(state);
        int wallProbe = Shift8AddMagnitude(state.Velocity.X.RawValue, 1);
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, wallProbe))
        {
            state.ConsecutiveTurnCounter = 0;
            EnemyTerrain.AlignEnemyYWithNonSquareSlope(level, slot);
            int tangent = SnesSixteenSixteen.FromEightEight(state.Velocity.Y.SignedRawValue);
            if (!_terrain.MoveEnemyVertically(level, slot, tangent))
                return;

            // Inside corner: the wall normal changes from X to Y.
            state.Velocity = state.Velocity.ReversedX();
            SetCrawlerVerticalSurfaceInstruction(slot, state);
            return;
        }

        state.ConsecutiveTurnCounter = unchecked((ushort)(state.ConsecutiveTurnCounter + 1));
        if ((short)(state.ConsecutiveTurnCounter - 4) >= 0)
        {
            BeginCrawlerFall(state);
            return;
        }

        // Outside corner: reverse the tangent for up to three frames while the instruction
        // list rotates the actor around the exposed block edge.
        state.Velocity = state.Velocity.ReversedY();
        SetCrawlerVerticalSurfaceInstruction(slot, state);
    }

    /// <summary>Ports $A3:E7F2: move while attached to a floor or ceiling.</summary>
    private void RunCrawlerHorizontal(
        RoomEnemySlot slot,
        CrawlerEnemyState state,
        RoomLevelData level)
    {
        TriggerCrawlerEarthquakeFall(state);
        int surfaceProbe = Shift8AddMagnitude(state.Velocity.Y.RawValue, 1);
        if (_terrain.MoveEnemyVertically(level, slot, surfaceProbe))
        {
            state.ConsecutiveTurnCounter = 0;
            int tangent = GetCrawlerSlopeAdjustedHorizontalDisplacement(slot, state, level);
            if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, tangent))
            {
                // Inside corner: the surface normal changes from Y to X.
                state.Velocity = state.Velocity.ReversedY();
                SetCrawlerHorizontalSurfaceInstruction(slot, state);
            }
            else
            {
                EnemyTerrain.AlignEnemyYWithNonSquareSlope(level, slot);
            }
            return;
        }

        state.ConsecutiveTurnCounter = unchecked((ushort)(state.ConsecutiveTurnCounter + 1));
        if ((short)(state.ConsecutiveTurnCounter - 4) >= 0)
        {
            BeginCrawlerFall(state);
            return;
        }

        state.Velocity = state.Velocity.ReversedX();
        SetCrawlerHorizontalSurfaceInstruction(slot, state);
    }

    /// <summary>Ports the earthquake/outside-corner falling owner at $A3:E785.</summary>
    private void RunCrawlerFalling(
        RoomEnemySlot slot,
        CrawlerEnemyState state,
        RoomLevelData level)
    {
        int displacement = state.FallingYVelocity.RawValue;
        if (_terrain.MoveEnemyVertically(level, slot, displacement))
        {
            // Parameter $FF is the native "choose default speed after first landing" path.
            if (state.SpeedParameter == 255)
            {
                state.Velocity = new EightEightVector<RoomPixels>(128, 128);
            }
            state.FallingYVelocity = SnesSignedSixteenSixteen.Zero;
            state.ConsecutiveTurnCounter = 0;
            state.Function = state.NonFallingFunction;
            return;
        }

        state.ApplyFallingGravity();

        // The native routine contains this zero-velocity escape even though ordinary falls
        // immediately gain gravity. Retain it for ROM-modified parameter/state combinations.
        if (state.FallingYVelocity == SnesSignedSixteenSixteen.Zero)
            state.Function = CrawlerEnemyFunction.CrawlingVertically;
    }

    /// <summary>Remembers the attached surface function before entering gravity-controlled falling.</summary>
    private static void BeginCrawlerFall(CrawlerEnemyState state)
    {
        state.NonFallingFunction = state.Function;
        state.Function = CrawlerEnemyFunction.Falling;
    }

    /// <summary>Chooses a shared crawler's wall animation from its vertical tangent direction.</summary>
    internal static void SetCrawlerVerticalSurfaceInstruction(
        RoomEnemySlot slot,
        CrawlerEnemyState state)
    {
        CrawlerSurfaceOrientation orientation = state.Velocity.Y.SignedRawValue < 0
            ? CrawlerSurfaceOrientation.UpsideDown
            : CrawlerSurfaceOrientation.UpsideUp;
        SetCrawlerProgramFromTable(slot, state, orientation);
    }

    /// <summary>Chooses a shared crawler's floor or ceiling animation from its horizontal tangent direction.</summary>
    internal static void SetCrawlerHorizontalSurfaceInstruction(
        RoomEnemySlot slot,
        CrawlerEnemyState state)
    {
        CrawlerSurfaceOrientation orientation = state.Velocity.X.SignedRawValue < 0
            ? CrawlerSurfaceOrientation.UpsideRight
            : CrawlerSurfaceOrientation.UpsideLeft;
        SetCrawlerProgramFromTable(slot, state, orientation);
    }

    /// <summary>Indexes the species-specific six-word surface table and resets animation timers.</summary>
    private static void SetCrawlerProgramFromTable(
        RoomEnemySlot slot,
        CrawlerEnemyState state,
        CrawlerSurfaceOrientation orientation)
    {
        // The offset is already a byte offset into six-word species tables. Retail
        // Zoomers use zero; preserving byte addressing is required for the shared families.
        if ((state.SurfaceTableOffset & 1) != 0 || state.SurfaceTableOffset > 10)
        {
            throw new InvalidDataException(
                $"Crawler instruction-table offset ${state.SurfaceTableOffset:X4} is invalid.");
        }
        state.Program = AnimationProgramCursor<CrawlerProgram>.Start(CrawlerAnimationDefinitions.SurfaceProgram(
            checked((ushort)state.SurfaceTableOffset), orientation));
        slot.InstructionTimer = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);
    }

    /// <summary>Converts the current crawler velocity to a horizontal move adjusted for the supporting slope.</summary>
    private static int GetCrawlerSlopeAdjustedHorizontalDisplacement(
        RoomEnemySlot slot,
        CrawlerEnemyState state,
        RoomLevelData level) =>
        GetSurfaceSlopeAdjustedHorizontalDisplacement(slot, state.Velocity, level);

    /// <summary>
    /// Shared geometry from <c>MoveEnemyRightBy_14_12_ProcessSlopes</c>. Both the common
    /// crawlers and Yard multiply their tangent by the cartridge's non-square-slope factor.
    /// </summary>
    internal static int GetSurfaceSlopeAdjustedHorizontalDisplacement(
        RoomEnemySlot slot,
        EightEightVector<RoomPixels> velocity,
        RoomLevelData level)
    {
        ushort surfaceY = velocity.Y.SignedRawValue < 0
            ? unchecked((ushort)(slot.Position.Y.Whole - slot.Radius.Y))
            : unchecked((ushort)(slot.Position.Y.Whole + slot.Radius.Y - 1));
        int blockX = slot.Position.X.Whole >> 4;
        int blockY = surfaceY >> 4;
        if ((uint)blockX < level.Size.X &&
            (uint)blockY < level.Size.Y)
        {
            RoomCollisionBlock block = level.GetCollisionBlock(new(unchecked((ushort)blockX), unchecked((ushort)blockY)));
            int slopeShape = block.Bts.SlopeShape;
            if (block.CollisionType == RoomCollisionType.Slope && slopeShape >= 5)
            {
                return CrawlerSlopeDefinitions.Scale(velocity.X.RawValue, slopeShape);
            }
        }
        return SnesSixteenSixteen.FromEightEight(velocity.X.SignedRawValue);
    }

    /// <summary>Gives the occupant's entity its freshly initialized state.</summary>
    private static void Bind(RoomEnemySlot slot, CrawlerEnemyState state) =>
        slot.SetSpeciesState(state, state.ClearWithRecord);

    /// <summary>Adds a whole-pixel probe margin in the direction of a signed eight-eight velocity.</summary>
    internal static int Shift8AddMagnitude(ushort velocity, int wholePixels)
    {
        int signedVelocity = unchecked((short)velocity);
        int bias = SnesSixteenSixteen.FromWhole(wholePixels);
        return unchecked((SnesSixteenSixteen.FromEightEight(signedVelocity)) + (signedVelocity < 0 ? -bias : bias));
    }

    /// <summary>Reverses a native 16-bit velocity word with wraparound semantics.</summary>
    internal static ushort Negate16(ushort value) => unchecked((ushort)-value);

    /// <summary>Ports Sciser's initializer wrapper.</summary>
    internal static void InitializeSciserAi(RoomEnemySlot slot) =>
        InitializeCrawler(slot, CrawlerAnimationFamily.Sciser, speciesInstructionOffset: 8);

    /// <summary>Ports Zero's initializer wrapper.</summary>
    internal static void InitializeZeroAi(RoomEnemySlot slot) =>
        InitializeCrawler(slot, CrawlerAnimationFamily.Zero, speciesInstructionOffset: 10);

    /// <summary>Ports Viola's initializer wrapper.</summary>
    internal static void InitializeViolaAi(RoomEnemySlot slot) =>
        InitializeCrawler(slot, CrawlerAnimationFamily.Viola, speciesInstructionOffset: 6);

    /// <summary>Ports the shared initializer wrapper Zeela, Sova, Zoomer and MZoomer use.</summary>
    internal static void InitializeSharedCrawlerAi(RoomEnemySlot slot) =>
        InitializeCrawler(slot, CrawlerAnimationFamily.Shared);

    /// <summary>Ports the shared crawler main AI.</summary>
    internal void RunCrawlerMainAi(RoomEnemySlot slot, IEnemyAiFrame frame) => RunCrawlerMain(slot, frame.Level);

    /// <summary>Ports orange Zoomer's Samus-tracking main AI.</summary>
    internal void RunHZoomerMainAi(RoomEnemySlot slot, IEnemyAiFrame frame) =>
        RunHZoomerMain(slot, frame.Samus, frame.Level);

    /// <summary>Runs the crawlers' surface animations as typed programs (#641).</summary>
    internal static void AdvanceCrawlerTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _) =>
        EnemyProgramAnimation.Advance(slot, CrawlerAnimations.Programs, CrawlerFrameDefinitions.ShownFrames.Of,
            static (CrawlerEnemyState state) => state.Program, static (state, cursor) => state.Program = cursor,
            (state, cue, _) => ApplyCrawlerCue(state, cue));
}
