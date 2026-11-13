using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.Core.Frontend.Intro;

namespace SuperMetroid.Verification.Frontend.Intro.Artwork;

/// <summary>A Ceres actor's program and the colouring its draws name, as Core defines them (#1513).</summary>
/// <param name="Program">The program the actor runs, whose poses its draws show.</param>
/// <param name="Colouring">The colouring the actor's draws name.</param>
internal readonly record struct CeresDrawnProgram(CinematicSpriteSegment Program, CeresSpriteColouring Colouring);
