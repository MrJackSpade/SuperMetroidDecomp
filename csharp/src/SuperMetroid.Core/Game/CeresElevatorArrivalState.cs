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
    /// <summary>Non-null cartridge address space supplied with the arrival state; its compiled projectile programs are decoded from definitions.</summary>
    private readonly ISnesAddressSpace bus;
    /// <summary>Moving pad projectile that carries Samus down to the Ceres landing point.</summary>
    private readonly CeresElevatorProjectile pad;
    /// <summary>Stationary elevator platform removed when Samus reaches the landing height.</summary>
    private readonly CeresElevatorProjectile platform;
    /// <summary>Installed spritemap data used to draw the projectiles' current animation frames.</summary>
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

    /// <summary>Creates an active projectile with its definition, initial instruction, position, and graphics bank.</summary>
    /// <param name="definition">Compiled retail projectile definition supplying its pointer and entry instruction.</param>
    /// <param name="xPosition">Initial room-space horizontal position.</param>
    /// <param name="yPosition">Initial room-space vertical position.</param>
    /// <returns>The initialized projectile state used by this arrival sequence.</returns>
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

    /// <summary>Advances one projectile instruction timer and executes frame, goto, or delete commands on expiry.</summary>
    /// <param name="projectile">Projectile whose instruction state is updated.</param>
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

    /// <summary>Draws an active, on-screen projectile using its installed current-frame spritemap.</summary>
    /// <param name="projectile">Projectile whose position and animation frame are rendered.</param>
    /// <param name="oam">Destination buffer for low-priority enemy sprite entries.</param>
    /// <param name="cameraX">Current room camera horizontal offset.</param>
    /// <param name="cameraY">Current room camera vertical offset.</param>
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

        var installed = projectileSpritemaps ?? throw new InvalidOperationException(
            "Ceres elevator projectiles require installed sprite artwork.");
        oam.AddEnemySpritemap(installed.Get(projectile.SpritemapPointer).Span,
            screenX, screenY,
            new SnesObjAttributeWord(projectile.GraphicsIndex).PaletteBits,
            unchecked((byte)projectile.GraphicsIndex),
            clipVerticalWrap: true,
            originYIsOnScreen: (screenY & 0xff00) == 0);
    }

    /// <summary>Mutable runtime state for one of the Ceres arrival's bank-$86 enemy projectiles.</summary>
    private sealed class CeresElevatorProjectile
    {
        /// <summary>Creates a projectile at its spawn coordinates with its initial instruction and graphics identity.</summary>
        /// <param name="definitionPointer">Native bank-$86 projectile definition identity.</param>
        /// <param name="instructionPointer">Initial bank-$86 instruction-list address.</param>
        /// <param name="xPosition">Initial room-space horizontal coordinate.</param>
        /// <param name="yPosition">Initial room-space vertical coordinate.</param>
        /// <param name="graphicsIndex">Graphics and palette word copied into the projectile state.</param>
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

        /// <summary>Native bank-$86 projectile definition used to identify invalid instruction streams in diagnostics.</summary>
        public ushort DefinitionPointer { get; }
        /// <summary>Graphics and palette identity used when the current spritemap is submitted to OAM.</summary>
        public ushort GraphicsIndex { get; }
        /// <summary>Whether this projectile remains eligible for stepping and drawing.</summary>
        public bool Active { get; set; } = true;
        /// <summary>Room-space horizontal coordinate assigned at spawn.</summary>
        public ushort XPosition { get; }
        /// <summary>Room-space vertical coordinate, advanced by the pad pre-instruction when applicable.</summary>
        public ushort YPosition { get; set; }
        /// <summary>Arrival-specific delay before the moving pad begins carrying Samus downward.</summary>
        public ushort WaitTimer { get; set; }
        /// <summary>Bank-$86 address of the next instruction command.</summary>
        public ushort InstructionPointer { get; set; }
        /// <summary>Remaining frame delay before the instruction pointer is processed again.</summary>
        public ushort InstructionTimer { get; set; } = 1;
        /// <summary>Bank-$8D spritemap selected by the most recently executed frame command.</summary>
        public ushort SpritemapPointer { get; set; } = CeresElevatorArrivalDefinitions.NoSpritemap;
    }
}
