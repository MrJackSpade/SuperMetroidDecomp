using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// The installed key of each <see cref="SamusOverreadDocument.CueValueStepKind"/> (#2520), written out so renaming a member
/// never changes a name the installed overread document spells it by.
/// </summary>
internal static class CueValueStepKindKeys
{
    /// <summary>Each cue-value step kind's installed key.</summary>
    internal static InstalledKeys<SamusOverreadDocument.CueValueStepKind> Installed { get; } =
        new(new Dictionary<SamusOverreadDocument.CueValueStepKind, string>
        {
            [SamusOverreadDocument.CueValueStepKind.Delay] = "Delay",
            [SamusOverreadDocument.CueValueStepKind.Hold] = "Hold",
            [SamusOverreadDocument.CueValueStepKind.LoopUnlessLowEnergy] = "LoopUnlessLowEnergy",
            [SamusOverreadDocument.CueValueStepKind.InstallDrainedFall] = "InstallDrainedFall",
            [SamusOverreadDocument.CueValueStepKind.TurnTransition] = "TurnTransition",
            [SamusOverreadDocument.CueValueStepKind.ItemAirborneTransition] = "ItemAirborneTransition",
            [SamusOverreadDocument.CueValueStepKind.VerticalSpeedTransition] = "VerticalSpeedTransition",
            [SamusOverreadDocument.CueValueStepKind.WallJumpSelect] = "WallJumpSelect",
            [SamusOverreadDocument.CueValueStepKind.ItemTransition] = "ItemTransition",
            [SamusOverreadDocument.CueValueStepKind.Transition] = "Transition",
            [SamusOverreadDocument.CueValueStepKind.Repeat] = "Repeat",
            [SamusOverreadDocument.CueValueStepKind.Restart] = "Restart",
        });
}
