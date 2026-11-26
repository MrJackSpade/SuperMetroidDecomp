using Engine.Shared;

namespace Engine.Actors;

/// <summary>The form a state is entered in, and how it is placed where it is not fitted by feeling for room.</summary>
/// <param name="Form">The form's name.</param>
/// <param name="AroundMiddle">Whether the body keeps its middle where it can, on the ground as in the air.</param>
/// <param name="Unfitted">
/// Whether the form is taken where the player is, with no feeling for room: the body changes about its middle, into
/// whatever is there, and the state is entered.
/// </param>
public readonly record struct FormAsked(string Form, bool AroundMiddle = false, bool Unfitted = false);

/// <summary>
/// What fitting a form to the player's body came to (<see cref="Player.FitForm"/>): the form they take, where that
/// leaves them, and whether the view takes them to have stood there.
/// </summary>
/// <param name="Form">
/// The form taken: the one asked for where it fits; another the fitting answers with in its place, as a body with
/// no room to stand is put in a crouch; or null where the player stays in the form they have.
/// </param>
/// <param name="Place">Where the fitting leaves the player.</param>
/// <param name="StoodThere">
/// Whether the view takes the player to have stood at that height already: a change of form that moves the middle of
/// the body is not a move of the player's, and the view follows none of it.
/// </param>
public readonly record struct FormFit(string? Form, PixelPosition Place, bool StoodThere);
