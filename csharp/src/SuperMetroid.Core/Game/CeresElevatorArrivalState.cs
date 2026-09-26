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
    private readonly ISnesAddressSpace bus;
    private readonly CeresElevatorProjectile pad;
    private readonly CeresElevatorProjectile platform;
    [NonSerialized] private EnemyProjectileSpritemapCatalog? projectileSpritemaps;

    /// <summary>Installs both compiled definitions and performs their initialization AIs.</summary>
    public CeresElevatorArrivalState(
        ISnesAddressSpace bus,
        SamusState samus,
        EnemyProjectileSpritemapCatalog? projectileSpritemaps = null)
    {
        this.bus = bus ?? throw new ArgumentNullException(nameof(bus));
        ArgumentNullException.ThrowIfNull(samus);
        this.projectileSpritemaps = projectileSpritemaps;

        // SpawnEprojWithGfx first copies enemy slot zero's tile/palette word. The shared
        // initializer then clears it, so both actors use room-loaded OBJ tiles and the
        // palettes authored by their bank-$8D spritemaps. Retaining the transient word is
        // what previously made the level-data concealer teal until touchdown.
        pad = LoadProjectile(
            CeresElevatorArrivalDefinitions.MovingPad,
            samus.XPosition,
            unchecked((ushort)(samus.YPosition + CeresElevatorArrivalDefinitions.MovingPadYOffset)));
        pad.WaitTimer = CeresElevatorArrivalDefinitions.MovingPadWaitFrames;

        platform = LoadProjectile(
            CeresElevatorArrivalDefinitions.StationaryPlatform,
            samus.XPosition,
            CeresElevatorArrivalDefinitions.StationaryPlatformY);
    }

    /// <summary>True after both projectiles delete themselves when Samus reaches Y=72.</summary>
    public bool IsComplete => !pad.Active && !platform.Active;

    /// <summary>Current pad Y word, exposed for deterministic regression tests.</summary>
    public ushort PadYPosition => pad.YPosition;

    /// <summary>Current stationary-platform Y word, exposed for debugger inspection.</summary>
    public ushort PlatformYPosition => platform.YPosition;

    /// <summary>Rebinds installed compositions after a debugger-state restore.</summary>
    public void BindProjectileSpritemaps(EnemyProjectileSpritemapCatalog? catalog) =>
        projectileSpritemaps = catalog;

    /// <summary>
    /// Executes <c>EprojRunAll</c>'s relative order for the two freshly spawned slots.
    /// </summary>
    /// <returns>True on and after the frame that calls Samus command <c>$0E</c>.</returns>
    public bool Step(SamusState samus)
    {
        ArgumentNullException.ThrowIfNull(samus);

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
                pad.YPosition = unchecked((ushort)(
                    samus.YPosition + CeresElevatorArrivalDefinitions.MovingPadYOffset));
                samus.YPosition = unchecked((ushort)(samus.YPosition + 1));
                if ((short)unchecked((ushort)(
                    samus.YPosition - CeresElevatorArrivalDefinitions.LandingThresholdY)) >= 0)
                {
                    samus.YPosition = CeresElevatorArrivalDefinitions.LandingSamusY;
                    pad.InstructionTimer = 1;
                    pad.InstructionPointer = CeresElevatorArrivalDefinitions.DeleteInstructionPointer;
                }
            }
            ProcessInstructionList(pad);
        }

        if (platform.Active)
        {
            if (samus.YPosition == CeresElevatorArrivalDefinitions.LandingSamusY)
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
    public void Draw(OamBuffer oam, ushort cameraX, ushort cameraY)
    {
        ArgumentNullException.ThrowIfNull(oam);
        DrawProjectile(pad, oam, cameraX, cameraY);
        DrawProjectile(platform, oam, cameraX, cameraY);
    }

    private static CeresElevatorProjectile LoadProjectile(
        CeresElevatorProjectileDefinition definition,
        ushort xPosition,
        ushort yPosition)
    {
        return new CeresElevatorProjectile(
            definition.DefinitionPointer,
            definition.InitialInstruction,
            xPosition,
            yPosition,
            CeresElevatorArrivalDefinitions.NativeGraphicsIndex);
    }

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

    private void DrawProjectile(
        CeresElevatorProjectile projectile,
        OamBuffer oam,
        ushort cameraX,
        ushort cameraY)
    {
        if (!projectile.Active ||
            projectile.SpritemapPointer == CeresElevatorArrivalDefinitions.NoSpritemap)
            return;

        ushort screenX = unchecked((ushort)(projectile.XPosition - cameraX));
        ushort screenY = unchecked((ushort)(projectile.YPosition - cameraY));
        if (((screenX + 128) & 0xfe00) != 0 ||
            ((screenY + 128) & 0xfe00) != 0)
        {
            return;
        }

        if (projectileSpritemaps is { } installed)
            oam.AddEnemySpritemap(installed.Get(projectile.SpritemapPointer).Span,
                screenX, screenY,
                unchecked((ushort)(projectile.GraphicsIndex & 0xff00)),
                unchecked((byte)projectile.GraphicsIndex),
                clipVerticalWrap: true,
                originYIsOnScreen: (screenY & 0xff00) == 0);
        else
            oam.AddEnemyProjectileSpritemap(
                bus,
                projectile.SpritemapPointer,
                screenX,
                screenY,
                projectile.GraphicsIndex,
                originYIsOnScreen: (screenY & 0xff00) == 0);
    }

    private sealed class CeresElevatorProjectile
    {
        public CeresElevatorProjectile(
            ushort definitionPointer,
            ushort instructionPointer,
            ushort xPosition,
            ushort yPosition,
            ushort graphicsIndex)
        {
            DefinitionPointer = definitionPointer;
            InstructionPointer = instructionPointer;
            XPosition = xPosition;
            YPosition = yPosition;
            GraphicsIndex = graphicsIndex;
        }

        public ushort DefinitionPointer { get; }
        public ushort GraphicsIndex { get; }
        public bool Active { get; set; } = true;
        public ushort XPosition { get; }
        public ushort YPosition { get; set; }
        public ushort WaitTimer { get; set; }
        public ushort InstructionPointer { get; set; }
        public ushort InstructionTimer { get; set; } = 1;
        public ushort SpritemapPointer { get; set; } = CeresElevatorArrivalDefinitions.NoSpritemap;
    }
}
