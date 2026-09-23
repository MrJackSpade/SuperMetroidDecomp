namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled bank-$86 mechanics for the two enemy projectiles used by the initial
/// Ceres elevator arrival. Spritemap payloads remain presentation data in bank $8D.
/// </summary>
internal static class CeresElevatorArrivalDefinitions
{
    /// <summary>Bank containing the two definitions and their instruction programs.</summary>
    internal const int BankBase = 0x860000;

    /// <summary>$86:A387, moving pad spawned first into enemy-projectile slot $22.</summary>
    internal static readonly CeresElevatorProjectileDefinition MovingPad = new(
        DefinitionPointer: 0xa387,
        Initialization: 0xa2ee,
        PreInstruction: 0xa328,
        InitialInstruction: 0xa28d,
        PackedRadius: 0x0101,
        Properties: 0x3000);

    /// <summary>$86:A395, stationary level-data concealer spawned into slot $20.</summary>
    internal static readonly CeresElevatorProjectileDefinition StationaryPlatform = new(
        DefinitionPointer: 0xa395,
        Initialization: 0xa31b,
        PreInstruction: 0xa364,
        InitialInstruction: 0xa299,
        PackedRadius: 0x0101,
        Properties: 0x3000);

    /// <summary>$86:A301 clears the transient enemy graphics word in both projectiles.</summary>
    internal const ushort NativeGraphicsIndex = 0;

    /// <summary>$86:A316 seeds the moving pad's pre-instruction delay to sixty frames.</summary>
    internal const ushort MovingPadWaitFrames = 60;

    /// <summary>$86:A2FE places the moving pad twenty-eight pixels below Samus.</summary>
    internal const ushort MovingPadYOffset = 28;

    /// <summary>$86:A31B places the stationary platform at world Y=97.</summary>
    internal const ushort StationaryPlatformY = 97;

    /// <summary>$86:A34D clamps Samus to world Y=72 at the end of the descent.</summary>
    internal const ushort LandingSamusY = 72;

    /// <summary>$86:A345 compares the incremented Samus Y against 73 before clamping.</summary>
    internal const ushort LandingThresholdY = 73;

    /// <summary>$86:A28B, common enemy-projectile delete instruction.</summary>
    internal const ushort DeleteInstructionPointer = 0xa28b;

    /// <summary>$86:8154, Instruction_Delete opcode stored at $86:A28B.</summary>
    internal const ushort DeleteOpcode = 0x8154;

    /// <summary>$86:81AB, Instruction_Goto opcode used by both looping programs.</summary>
    internal const ushort GotoOpcode = 0x81ab;

    /// <summary>$8000 is the inactive/no-spritemap sentinel used before the first frame.</summary>
    internal const ushort NoSpritemap = 0x8000;

    /// <summary>Host corruption guard; valid programs resolve a frame/delete within two commands.</summary>
    internal const int MaximumCommandsPerStep = 16;

    /// <summary>Resolves the complete reachable instruction program for both projectiles.</summary>
    /// <remarks>
    /// The moving pad begins at <c>$86:A28D</c>: two one-tick frames at $A28D
    /// and $A291, then the $81AB goto at $A295 returns to $A28D. Landing
    /// redirects to the shared $8154 delete at $A28B. All five mechanics
    /// words match the pinned NTSC J/U v1.0 ROM; the reachable pointer set is
    /// $A28D, $A291, $A295, and $A28B. Other pointers fail descriptively.
    /// Spritemap operands at $A28F/$A293 are separate presentation values.
    /// The two pinned-ROM operands are $B1BA and $B1D0. Bank-$8D record
    /// $B1BA has four five-byte OBJ components after its two-byte count,
    /// so its exact next-record address is <c>$B1BA + 2 + 5 * 4 = $B1D0</c>.
    /// Both authored four-component spritemaps remain live draw data and are
    /// also referenced by dust-cloud/explosion instruction lists.
    /// </remarks>
    internal static CeresElevatorProjectileInstruction ReadInstruction(ushort pointer) =>
        pointer switch
        {
            // $86:A28B, Instruction_Delete.
            0xa28b => new(CeresElevatorProjectileOperation.Delete),
            // $86:A28D, one frame using Spritemap_CeresElevatorPad_0.
            0xa28d => new(CeresElevatorProjectileOperation.Frame, 0xa291, 1, 0xb1ba),
            // $86:A291, one frame using Spritemap_CeresElevatorPad_1.
            0xa291 => new(CeresElevatorProjectileOperation.Frame, 0xa295, 1, 0xb1d0),
            // $86:A295, Instruction_Goto $A28D.
            0xa295 => new(CeresElevatorProjectileOperation.Goto, 0xa28d),
            // $86:A299, one frame using Spritemap_CeresElevatorPlatform.
            0xa299 => new(CeresElevatorProjectileOperation.Frame, 0xa29d, 1, 0x846d),
            // $86:A29D, Instruction_Goto $A299.
            0xa29d => new(CeresElevatorProjectileOperation.Goto, 0xa299),
            _ => throw new InvalidDataException(
                $"Unknown Ceres elevator projectile instruction $86:{pointer:X4}."),
        };
}

/// <summary>One immutable bank-$86 enemy-projectile header.</summary>
internal readonly record struct CeresElevatorProjectileDefinition(
    ushort DefinitionPointer,
    ushort Initialization,
    ushort PreInstruction,
    ushort InitialInstruction,
    ushort PackedRadius,
    ushort Properties);

/// <summary>The three operations reachable from the two Ceres arrival programs.</summary>
internal enum CeresElevatorProjectileOperation
{
    Frame,
    Delete,
    Goto,
}

/// <summary>A decoded frame or control transfer from the compiled bank-$86 program.</summary>
internal readonly record struct CeresElevatorProjectileInstruction(
    CeresElevatorProjectileOperation Operation,
    ushort NextInstruction = 0,
    ushort Duration = 0,
    ushort SpritemapPointer = 0);
