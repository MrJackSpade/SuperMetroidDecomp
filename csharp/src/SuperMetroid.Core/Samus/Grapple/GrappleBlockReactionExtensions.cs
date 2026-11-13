namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>The tests the grapple makes of a block reaction.</summary>
internal static class GrappleBlockReactionExtensions
{
    /// <summary>Whether the block stops the beam, whether or not it connects: a test that does not ask whether the beam connected.</summary>
    internal static bool Collides(this GrappleBlockReaction reaction) =>
        reaction is GrappleBlockReaction.Solid or GrappleBlockReaction.Connects;
}
