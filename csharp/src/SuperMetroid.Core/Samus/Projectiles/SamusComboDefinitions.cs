using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The charged beam combos the beams fire (<see cref="SamusComboMechanics"/>).</summary>
internal static partial class SamusComboDefinitions
{
    /// <summary>The special attack each beam fires when it is the one beam equipped beside Power.</summary>
    internal static readonly IReadOnlyDictionary<SamusBeamFlags, SamusComboKind> KindsOfSingleBeams = new Dictionary<SamusBeamFlags, SamusComboKind>
    {
        [SamusBeamFlags.Wave] = SamusComboKind.Wave,
        [SamusBeamFlags.Ice] = SamusComboKind.Ice,
        [SamusBeamFlags.Spazer] = SamusComboKind.Spazer,
        [SamusBeamFlags.Plasma] = SamusComboKind.Plasma,
    };
}
