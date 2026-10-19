namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>The tests the grapple routines make of a block reaction.</summary>
internal static class GrappleBlockReactionExtensions
{
    /// <summary>Whether the block stops the beam, whether or not it connects: the cartridge's carry-only test.</summary>
    internal static bool Collides(this GrappleBlockReaction reaction) =>
        reaction is GrappleBlockReaction.Solid or GrappleBlockReaction.Connects;
}
