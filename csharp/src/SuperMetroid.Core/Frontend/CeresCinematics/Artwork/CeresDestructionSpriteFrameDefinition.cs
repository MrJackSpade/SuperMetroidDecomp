using SuperMetroid.Core.Frontend.Intro;

namespace SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

/// <summary>One stable destruction/reveal composition identity with its original native OAM geometry; stock part count is descriptive and does not restrict independently edited compositions.</summary>
/// <param name="Name">Stable JSON key for a backdrop or numbered blast frame.</param>
/// <param name="Pose">The cinematic sprite pose that shows this composition.</param>
/// <param name="StockPartCount">Number of ordered five-byte OAM parts in the original record, excluding its two-byte count header.</param>
/// <param name="Backdrop">The backdrop this frame draws, or <see langword="null"/> for a blast frame.</param>
public readonly record struct CeresDestructionSpriteFrameDefinition(
    string Name, CinematicSpritePose Pose, int StockPartCount, CeresDestructionBackdrop? Backdrop = null);
