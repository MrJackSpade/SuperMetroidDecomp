using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>A cinematic presentation's compositions keyed by the poses that show them, as its family's sheet composes them (#1392).</summary>
internal interface ICinematicSpriteFrames
{
    /// <summary>The family the compositions belong to.</summary>
    CinematicSpriteFamily Family { get; }

    /// <summary>Each composition, by the pose that shows it.</summary>
    IReadOnlyDictionary<CinematicSpritePose, SpriteComposition> Frames { get; }
}
