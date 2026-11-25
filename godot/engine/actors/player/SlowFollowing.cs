using Engine.Shared;

namespace Engine.Actors;

/// <summary>
/// How the view follows a player slowly: by so many pixels an update toward keeping them within a part of the view,
/// the speeds of its ordinary following left as they were.
/// </summary>
/// <param name="Step">How many pixels the view moves an update, across and down.</param>
/// <param name="Middle">
/// The part of the view the player is kept in, in the view's own pixels: the view moves left or up while they are
/// before its near edges, and right or down from its far edges on.
/// </param>
public readonly record struct SlowFollowing(int Step, PixelRect Middle);
