namespace SuperMetroid.Verification.Harness;

/// <summary>The new input, held input and target pose of one synthetic samus cartridge record.</summary>
/// <param name="NewInput">The new input.</param>
/// <param name="HeldInput">The held input.</param>
/// <param name="TargetPose">The target pose.</param>
internal readonly record struct SyntheticSamusCartridgeRecord(ushort NewInput, ushort HeldInput, ushort TargetPose);
