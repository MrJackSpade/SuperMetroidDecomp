namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>Where a cinematic actor's placement is read from: its asset id and the bank-$8B words of its X and Y immediates.</summary>
/// <param name="Id">The actor's asset id.</param>
/// <param name="XAddress">The bank-$8B address of its X immediate.</param>
/// <param name="YAddress">The bank-$8B address of its Y immediate.</param>
internal readonly record struct CinematicPlacementSourceDefinition(string Id, ushort XAddress, ushort YAddress);
