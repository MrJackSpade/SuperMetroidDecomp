using SuperMetroid.Core.Enemies.Owtch;

namespace SuperMetroid.Verification.Enemies.Owtch;

/// <summary>The initial, program, loop and walking of one verify owtch behavior case.</summary>
/// <param name="Initial">The initial.</param>
/// <param name="Program">The program.</param>
/// <param name="Loop">The loop.</param>
/// <param name="Walking">The walking.</param>
internal readonly record struct VerifyOwtchBehaviorCase(OwtchBehaviorState Initial, OwtchProgram Program, OwtchProgram Loop, OwtchBehaviorState Walking);
