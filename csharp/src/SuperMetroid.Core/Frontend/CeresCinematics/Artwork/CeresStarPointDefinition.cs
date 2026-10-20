namespace SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

/// <summary>One star of the Ceres star field: where it is drawn and its tile.</summary>
/// <param name="X">The star's X offset.</param>
/// <param name="Y">The star's Y offset.</param>
/// <param name="Tile">The star's tile.</param>
internal readonly record struct CeresStarPointDefinition(int X, byte Y, int Tile);
