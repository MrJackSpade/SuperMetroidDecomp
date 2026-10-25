using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>A fade-target colour Spore Spawn stages for the door's fade (#1405): <paramref name="Run"/>'s colour <paramref name="Colour"/>.</summary>
/// <param name="Run">The colours the staged colour is among.</param>
/// <param name="Colour">The colour within the run.</param>
/// <param name="Value">The target colour.</param>
internal readonly record struct SporeSpawnStagedColour(IColourSetRun Run, int Colour, RgbColor Value);
