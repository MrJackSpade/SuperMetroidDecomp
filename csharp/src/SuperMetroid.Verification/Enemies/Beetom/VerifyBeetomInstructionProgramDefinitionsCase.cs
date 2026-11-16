using SuperMetroid.Core.Enemies.Beetom;

namespace SuperMetroid.Verification.Enemies.Beetom;

/// <summary>The side and drain animation of one Beetom drain case.</summary>
/// <param name="Left">Whether the Beetom drains facing left.</param>
/// <param name="Drain">The drain animation, which loops its red-gem run after its green-gem lead-in (#2144).</param>
internal readonly record struct VerifyBeetomInstructionProgramDefinitionsCase(bool Left, BeetomSegment Drain);
