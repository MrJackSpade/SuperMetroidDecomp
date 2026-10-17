namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>One compiled command from Kraid's four private bank-$A7 head programs.</summary>
/// <param name="Pointer">The command's cursor in the private head stream.</param>
/// <param name="Kind">Whether the command is a timed frame, a roar sound or the program's end.</param>
/// <param name="Duration">Updates a timed frame stays displayed; zero for other commands.</param>
/// <param name="Tilemap">The installed head tilemap a timed frame shows; zero for other commands.</param>
/// <param name="VulnerableHitbox">The mouth shape that admits damaging shots during a timed frame; zero for other commands.</param>
/// <param name="InvulnerableHitbox">The mouth shape for nondamaging shot reactions during a timed frame, or the full word when the closed mouth has none; zero for other commands.</param>
/// <param name="SoundId">The sound a roar command requests; zero for other commands.</param>
/// <param name="MouthStage">How far a timed frame opens the mouth, from zero (closed) to <see cref="KraidHeadInstructionDefinitions.FullyOpenMouthStage"/>; zero for other commands.</param>
internal readonly record struct KraidHeadInstructionDefinition(
    ushort Pointer,
    KraidHeadInstructionKind Kind,
    ushort Duration,
    ushort Tilemap,
    ushort VulnerableHitbox,
    ushort InvulnerableHitbox,
    ushort SoundId,
    int MouthStage);
