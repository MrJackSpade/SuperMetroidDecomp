namespace Game;

/// <summary>
/// One of Samus's glows being played by something that holds each colouring for its own updates. It shows nothing
/// until its first step, which shows its first colouring.
/// </summary>
/// <param name="glow">The glow played.</param>
public sealed class SamusGlowPlay(SamusGlow glow)
{
    /// <summary>The step it moves to next.</summary>
    private int _next;

    /// <summary>How many more updates what it shows now holds; its first step moves on at once.</summary>
    private int _left = 1;

    /// <summary>Whether a glow that ends has passed its last colouring.</summary>
    public bool HasEnded { get; private set; }

    /// <summary>The colouring it last moved to, which it goes on showing while that holds and once it has ended; null before its first step.</summary>
    public string? Shown { get; private set; }

    /// <summary>Whether a glow that ends passes its last colouring on its next step.</summary>
    public bool EndsOnNextStep => !HasEnded && _left == 1 && _next >= glow.Steps.Count && glow.LoopsFrom is null;

    /// <summary>
    /// Plays one update: counts the colouring shown down and, once that has run out, moves to the next, which is
    /// returned, the same colouring again included. Null while a colouring holds, and on the step a glow that ends
    /// passes its last and from then on.
    /// </summary>
    public string? Step()
    {
        if (HasEnded || --_left > 0)
            return null;
        if (_next >= glow.Steps.Count)
        {
            if (glow.LoopsFrom is not { } from)
            {
                HasEnded = true;
                return null;
            }

            _next = from;
        }

        SamusGlowStep step = glow.Steps[_next++];
        _left = step.Updates;
        Shown = step.Shade;
        return step.Shade;
    }
}
