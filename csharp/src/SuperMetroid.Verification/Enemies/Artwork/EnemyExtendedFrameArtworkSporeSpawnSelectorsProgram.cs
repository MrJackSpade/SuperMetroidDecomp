using SuperMetroid.Core.Bosses.SporeSpawn;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>The program and frames of one enemy extended frame artwork spore spawn selectors program.</summary>
/// <param name="Program">The program.</param>
/// <param name="Frames">The frames.</param>
internal readonly record struct EnemyExtendedFrameArtworkSporeSpawnSelectorsProgram(SporeSpawnBodyProgram Program, int Frames);
