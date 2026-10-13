using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>
/// The two bank-$86 enemy projectiles spawned by <c>$90:F1E9</c> when a new game enters
/// Ceres: the moving pad under Samus and the stationary elevator platform behind it.
/// </summary>
/// <remarks>
/// These are not host-authored cutscene sprites. Construction consumes compiled copies of
/// the retail definitions at <c>$86:A387/$A395</c>; frame stepping executes their translated
/// bank-$86 instruction programs and pre-instructions <c>$A328/$A364</c>. Keeping this tiny
/// pair separate avoids pretending that the larger general enemy-projectile engine has
/// already been translated.
/// </remarks>
public sealed class CeresElevatorArrivalState
{
    /// <summary>Address space bound at arrival construction for the projectile lifecycle.</summary>
    private readonly ISnesAddressSpace bus;
    /// <summary>First spawned projectile, which moves Samus before the platform checks landing.</summary>
    private readonly CeresElevatorProjectile pad;
    /// <summary>Second spawned projectile, which waits behind the pad until touchdown.</summary>
    private readonly CeresElevatorProjectile platform;
    /// <summary>Installed visual compositions rebound after restoring a debugger snapshot.</summary>
    [NonSerialized] private EnemyProjectileSpritemapCatalog? projectileSpritemaps;

    /// <summary>Installs both compiled definitions and performs their initialization AIs.</summary>
    public CeresElevatorArrivalState(
        ISnesAddressSpace bus,
        SamusState samus,
        EnemyProjectileSpritemapCatalog? projectileSpritemaps = null)
    {
        this.bus = Ensure.NotNull(bus);
        Ensure.NotNull(samus);
        this.projectileSpritemaps = projectileSpritemaps;

        // SpawnEprojWithGfx first copies enemy slot zero's tile/palette word. The shared
        // initializer then clears it, so both actors use room-loaded OBJ tiles and the
        // palettes authored by their bank-$8D spritemaps. Retaining the transient word is
        // what previously made the level-data concealer teal until touchdown.
        pad = LoadProjectile(
            CeresElevatorArrivalDefinitions.MovingPad,
            MovingPadUnder(samus));
        pad.WaitTimer = CeresElevatorArrivalDefinitions.MovingPadWaitFrames;

        platform = LoadProjectile(
            CeresElevatorArrivalDefinitions.StationaryPlatform,
            samus.Position.Whole.WithY(CeresElevatorArrivalDefinitions.StationaryPlatformY));
    }

    /// <summary>True after both projectiles delete themselves when Samus reaches Y=72.</summary>
    public bool IsComplete => !pad.Active && !platform.Active;

    /// <summary>Rebinds installed compositions after a debugger-state restore.</summary>
    public void BindProjectileSpritemaps(EnemyProjectileSpritemapCatalog? catalog) =>
        projectileSpritemaps = catalog;

    /// <summary>
    /// Executes <c>EprojRunAll</c>'s relative order for the two freshly spawned slots.
    /// </summary>
    /// <returns>True on and after the frame that calls Samus command <c>$0E</c>.</returns>
    public bool Step(SamusState samus)
    {
        Ensure.NotNull(samus);

        // SpawnEprojInner takes the highest free physical slot. The pad is spawned first
        // into slot $22, the platform second into $20, and EprojRunAll scans downward;
        // therefore the pad pre-instruction must move Samus before the platform tests Y=72.
        if (pad.Active)
        {
            if (pad.WaitTimer > 1)
            {
                pad.WaitTimer--;
            }
            else
            {
                if (pad.WaitTimer == 1)
                    pad.WaitTimer = 0;

                // `$86:A342` samples the old Samus position for the pad, then increments
                // Samus. That one-pixel separation is visible throughout the descent.
                pad.Position = pad.Position.WithY(MovingPadUnder(samus).Y);
                samus.Position += new Offset16<RoomPixels>(0, 1);
                if ((short)unchecked((ushort)(
                    samus.Position.Y.Whole - CeresElevatorArrivalDefinitions.LandingThresholdY)) >= 0)
                {
                    samus.Position = samus.Position.WithWholeY(CeresElevatorArrivalDefinitions.LandingSamusY);
                    pad.InstructionTimer = 1;
                    pad.InstructionPointer = CeresElevatorArrivalDefinitions.DeleteInstructionPointer;
                }
            }
            ProcessInstructionList(pad);
        }

        if (platform.Active)
        {
            if (samus.Position.Y.Whole == CeresElevatorArrivalDefinitions.LandingSamusY)
            {
                platform.InstructionTimer = 1;
                platform.InstructionPointer = CeresElevatorArrivalDefinitions.DeleteInstructionPointer;
            }
            ProcessInstructionList(platform);
        }

        return IsComplete;
    }

    /// <summary>
    /// Emits the low-priority bank-$8D spritemaps selected by the live instruction lists.
    /// </summary>
    public void Draw(OamBuffer oam, Position16<RoomPixels> camera)
    {
        Ensure.NotNull(oam);
        DrawProjectile(pad, oam, camera);
        DrawProjectile(platform, oam, camera);
    }

    /// <summary>The moving pad's position: Samus's whole position, <c>MovingPadYOffset</c> lower.</summary>
    private static Position16<RoomPixels> MovingPadUnder(SamusState samus) =>
        samus.Position.Whole + new Offset16<RoomPixels>(0, (short)CeresElevatorArrivalDefinitions.MovingPadYOffset);

    /// <summary>Creates one arrival actor from its compiled definition and room position.</summary>
    private static CeresElevatorProjectile LoadProjectile(
        CeresElevatorProjectileDefinition definition,
        Position16<RoomPixels> position)
    {
        return new CeresElevatorProjectile(
            definition.DefinitionPointer,
            definition.InitialInstruction,
            position,
            CeresElevatorArrivalDefinitions.NativeGraphicsIndex);
    }

    /// <summary>Advances a projectile's timed frame, delete, or branch commands with a bounded interpreter pass.</summary>
    private static void ProcessInstructionList(CeresElevatorProjectile projectile)
    {
        ushort oldTimer = projectile.InstructionTimer;
        projectile.InstructionTimer = unchecked((ushort)(projectile.InstructionTimer - 1));
        if (oldTimer != 1)
            return;

        ushort cursor = projectile.InstructionPointer;
        for (int commandCount = 0;
            commandCount < CeresElevatorArrivalDefinitions.MaximumCommandsPerStep;
            commandCount++)
        {
            CeresElevatorProjectileInstruction instruction =
                CeresElevatorArrivalDefinitions.ReadInstruction(cursor);
            switch (instruction.Operation)
            {
                case CeresElevatorProjectileOperation.Frame:
                    projectile.InstructionTimer = instruction.Duration;
                    projectile.SpritemapPointer = instruction.SpritemapPointer;
                    projectile.InstructionPointer = instruction.NextInstruction;
                    return;
                case CeresElevatorProjectileOperation.Delete:
                    projectile.Active = false;
                    return;
                case CeresElevatorProjectileOperation.Goto:
                    cursor = instruction.NextInstruction;
                    break;
                default:
                    throw new InvalidDataException(
                        $"Ceres elevator eproj $86:{projectile.DefinitionPointer:X4} " +
                        $"instruction $86:{cursor:X4} has invalid operation {instruction.Operation}.");
            }
        }

        throw new InvalidDataException(
            $"Ceres elevator eproj $86:{projectile.DefinitionPointer:X4} exceeded its command guard.");
    }

    /// <summary>Draws an active on-screen projectile through its installed bank-8D composition.</summary>
    private void DrawProjectile(
        CeresElevatorProjectile projectile,
        OamBuffer oam,
        Position16<RoomPixels> camera)
    {
        if (!projectile.Active ||
            projectile.SpritemapPointer == CeresElevatorArrivalDefinitions.NoSpritemap)
            return;

        Position16<ScreenPixels> screen = projectile.Position.ToScreen(camera);
        if (((screen.X + 128) & 0xfe00) != 0 ||
            ((screen.Y + 128) & 0xfe00) != 0)
        {
            return;
        }

        var installed = projectileSpritemaps ?? throw new InvalidOperationException(
            "Ceres elevator projectiles require installed sprite artwork.");
        oam.AddEnemySpritemap(installed.Get(projectile.SpritemapPointer).Span,
            screen.ToOam(),
            new SnesObjAttributeWord(projectile.GraphicsIndex).PaletteBits,
            unchecked((byte)projectile.GraphicsIndex),
            clipVerticalWrap: true,
            originYIsOnScreen: (SnesLittleEndian.HighByteInPlace(screen.Y)) == 0);
    }

    /// <summary>Small per-slot state for the two Ceres arrival enemy projectiles.</summary>
    private sealed class CeresElevatorProjectile
    {
        /// <summary>Initializes a projectile's definition, instruction entry, position, and graphics index.</summary>
        public CeresElevatorProjectile(
            ushort definitionPointer,
            ushort instructionPointer,
            Position16<RoomPixels> position,
            ushort graphicsIndex)
        {
            DefinitionPointer = definitionPointer;
            InstructionPointer = instructionPointer;
            Position = position;
            GraphicsIndex = graphicsIndex;
        }

        /// <summary>Bank-86 definition identity used to report malformed instruction programs.</summary>
        public ushort DefinitionPointer { get; }
        /// <summary>Native graphics word supplying palette and tile-base bits for drawing.</summary>
        public ushort GraphicsIndex { get; }
        /// <summary>Whether the projectile still occupies its arrival slot.</summary>
        public bool Active { get; set; } = true;
        /// <summary>Whole room position; the pre-instruction only rewrites Y.</summary>
        public Position16<RoomPixels> Position { get; set; }
        /// <summary>Pad-only startup delay before it begins moving Samus toward the platform.</summary>
        public ushort WaitTimer { get; set; }
        /// <summary>Next bank-86 program word to interpret when the timer expires.</summary>
        public ushort InstructionPointer { get; set; }
        /// <summary>Frames until this projectile's next instruction pass.</summary>
        public ushort InstructionTimer { get; set; } = 1;
        /// <summary>Current bank-8D visual identity or the no-spritemap sentinel.</summary>
        public ushort SpritemapPointer { get; set; } = CeresElevatorArrivalDefinitions.NoSpritemap;
    }
}
