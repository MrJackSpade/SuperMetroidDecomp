namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Where one step of a Wave shot's horizontal reaction scan reads, as <see cref="StraddledBlockRead"/> walks it: the block
/// the read starts in and whether it straddles that block and the next one.
/// </summary>
/// <param name="Block">The block the read starts in, counted along the room's rows from its first block, 0 through 32767.</param>
/// <param name="StraddlesNext">
/// Whether the read starts halfway through <paramref name="Block"/>, reading the block straddling it and the next
/// (<see cref="StraddledBlockRead.Of"/>); otherwise it reads the block itself.
/// </param>
[CompatibilityService("where a Wave shot's scan past the room's ceiling reads: a block, or the block straddling it and the next")]
public readonly record struct WaveScanRead(int Block, bool StraddlesNext);
