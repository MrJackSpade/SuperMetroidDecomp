namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>Where a cinematic actor's placement is read from: its asset id and the addresses of its X and Y words.</summary>
/// <param name="Id">The actor's asset id.</param>
/// <param name="XAddress">The address of its X word.</param>
/// <param name="YAddress">The address of its Y word.</param>
[SpatialComponents("The ROM word addresses of an actor's X and Y immediates, not a coordinate.")]
internal readonly record struct CinematicPlacementSourceDefinition(string Id, ushort XAddress, ushort YAddress);
