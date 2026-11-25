using Godot;

namespace Engine.Shared;

/// <summary>One colour of a picture shown as another: every pixel of exactly the first is drawn in the second.</summary>
/// <param name="From">The colour as the picture has it.</param>
/// <param name="To">The colour it is shown as.</param>
public readonly record struct ColourSwap(Color From, Color To);
