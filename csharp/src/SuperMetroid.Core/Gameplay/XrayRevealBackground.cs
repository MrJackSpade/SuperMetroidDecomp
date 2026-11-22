using SuperMetroid.Core.Samus.Abilities;

namespace SuperMetroid.Core.Gameplay;

/// <summary>The X-ray's reveal (#1392, #1514), which the background shows in place of the room's own background inside the cone.</summary>
public sealed partial record XrayRevealBackground : ShownRoomBackground
{
    /// <summary>Keeps the revealed blocks, drawn from the room's block pictures.</summary>
    /// <param name="reveal">The revealed blocks, drawn from the room's block pictures.</param>
    public XrayRevealBackground(XrayRevealPlane reveal) => Reveal = Ensure.NotNull(reveal);

    /// <summary>The revealed blocks, drawn from the room's block pictures.</summary>
    internal XrayRevealPlane Reveal { get; }

    /// <inheritdoc/>
    public override TResult Map<TResult>(IShownRoomBackgroundMapping<TResult> mapping) => Ensure.NotNull(mapping).XrayReveal(Reveal);
}
