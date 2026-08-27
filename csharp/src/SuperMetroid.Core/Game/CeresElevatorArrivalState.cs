using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.Core.Game;

/// <summary>
/// The two bank-$86 enemy projectiles spawned by <c>$90:F1E9</c> when a new game enters
/// Ceres: the moving pad under Samus and the stationary elevator platform behind it.
/// </summary>
/// <remarks>
/// These are not host-authored cutscene sprites. Construction reads and validates the
/// retail definitions at <c>$86:A387/$A395</c>; frame stepping executes their real bank-$86
/// instruction lists and ports pre-instructions <c>$A328/$A364</c>. Keeping this tiny pair
/// separate avoids pretending that the much larger general enemy-projectile engine has
/// already been translated.
/// </remarks>
public sealed class CeresElevatorArrivalState
{
    private const ushort PadDefinitionPointer = 0xa387;
    private const ushort PlatformDefinitionPointer = 0xa395;
    private const ushort DeleteInstructionPointer = 0xa28b;
    private const ushort DeleteOpcode = 0x8154;
    private const ushort GotoOpcode = 0x81ab;

    private readonly ISnesAddressSpace bus;
    private readonly CeresElevatorProjectile pad;
    private readonly CeresElevatorProjectile platform;

    /// <summary>Reads both native definitions and performs their two initialization AIs.</summary>
    public CeresElevatorArrivalState(
        ISnesAddressSpace bus,
        SamusState samus,
        ushort graphicsIndex)
    {
        this.bus = bus ?? throw new ArgumentNullException(nameof(bus));
        ArgumentNullException.ThrowIfNull(samus);

        pad = LoadProjectile(
            PadDefinitionPointer,
            expectedInitialization: 0xa2ee,
            expectedPreInstruction: 0xa328,
            expectedInstructionList: 0xa28d,
            samus.XPosition,
            unchecked((ushort)(samus.YPosition + 28)),
            graphicsIndex);
        pad.WaitTimer = 60;

        platform = LoadProjectile(
            PlatformDefinitionPointer,
            expectedInitialization: 0xa31b,
            expectedPreInstruction: 0xa364,
            expectedInstructionList: 0xa299,
            samus.XPosition,
            yPosition: 97,
            graphicsIndex);
    }

    /// <summary>True after both projectiles delete themselves when Samus reaches Y=72.</summary>
    public bool IsComplete => !pad.Active && !platform.Active;

    /// <summary>Current pad Y word, exposed for deterministic regression tests.</summary>
    public ushort PadYPosition => pad.YPosition;

    /// <summary>Current stationary-platform Y word, exposed for debugger inspection.</summary>
    public ushort PlatformYPosition => platform.YPosition;

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
                pad.YPosition = unchecked((ushort)(samus.YPosition + 28));
                samus.YPosition = unchecked((ushort)(samus.YPosition + 1));
                if ((short)unchecked((ushort)(samus.YPosition - 73)) >= 0)
                {
                    samus.YPosition = 72;
                    pad.InstructionTimer = 1;
                    pad.InstructionPointer = DeleteInstructionPointer;
                }
            }
            ProcessInstructionList(pad);
        }

        if (platform.Active)
        {
            if (samus.YPosition == 72)
            {
                platform.InstructionTimer = 1;
                platform.InstructionPointer = DeleteInstructionPointer;
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

    private CeresElevatorProjectile LoadProjectile(
        ushort definitionPointer,
        ushort expectedInitialization,
        ushort expectedPreInstruction,
        ushort expectedInstructionList,
        ushort xPosition,
        ushort yPosition,
        ushort graphicsIndex)
    {
        int address = 0x860000 | definitionPointer;
        ushort initialization = ReadWord(address);
        ushort preInstruction = ReadWord(address + 2);
        ushort instructionList = ReadWord(address + 4);
        ushort radius = ReadWord(address + 6);
        ushort properties = ReadWord(address + 8);
        if (initialization != expectedInitialization ||
            preInstruction != expectedPreInstruction ||
            instructionList != expectedInstructionList ||
            radius != 0x0101 ||
            properties != 0x3000)
        {
            throw new InvalidDataException(
                $"Ceres elevator eproj $86:{definitionPointer:X4} does not match the " +
                "translated retail definition.");
        }

        return new CeresElevatorProjectile(
            definitionPointer,
            instructionList,
            xPosition,
            yPosition,
            graphicsIndex);
    }

    private void ProcessInstructionList(CeresElevatorProjectile projectile)
    {
        ushort oldTimer = projectile.InstructionTimer;
        projectile.InstructionTimer = unchecked((ushort)(projectile.InstructionTimer - 1));
        if (oldTimer != 1)
            return;

        ushort cursor = projectile.InstructionPointer;
        for (int commandCount = 0; commandCount < 16; commandCount++)
        {
            ushort word = ReadWord(0x860000 | cursor);
            if ((word & 0x8000) == 0)
            {
                projectile.InstructionTimer = word;
                projectile.SpritemapPointer = ReadWord(0x860000 | unchecked((ushort)(cursor + 2)));
                projectile.InstructionPointer = unchecked((ushort)(cursor + 4));
                return;
            }

            switch (word)
            {
                case DeleteOpcode:
                    projectile.Active = false;
                    return;
                case GotoOpcode:
                    cursor = ReadWord(0x860000 | unchecked((ushort)(cursor + 2)));
                    break;
                default:
                    throw new NotSupportedException(
                        $"Ceres elevator eproj $86:{projectile.DefinitionPointer:X4} " +
                        $"instruction $86:{cursor:X4} opcode ${word:X4} is not translated.");
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
        if (!projectile.Active || projectile.SpritemapPointer == 0x8000)
            return;

        ushort screenX = unchecked((ushort)(projectile.XPosition - cameraX));
        ushort screenY = unchecked((ushort)(projectile.YPosition - cameraY));
        if (((screenX + 128) & 0xfe00) != 0 ||
            ((screenY + 128) & 0xfe00) != 0)
        {
            return;
        }

        oam.AddEnemyProjectileSpritemap(
            bus,
            projectile.SpritemapPointer,
            screenX,
            screenY,
            projectile.GraphicsIndex,
            originYIsOnScreen: (screenY & 0xff00) == 0);
    }

    private ushort ReadWord(int address) => RomDataReader.ReadWordFixedBank(bus, address);

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
        public ushort SpritemapPointer { get; set; } = 0x8000;
    }
}
