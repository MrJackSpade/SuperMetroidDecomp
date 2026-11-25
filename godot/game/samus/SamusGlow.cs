using Engine.Content;

namespace Game;

/// <summary>One step of a glow of Samus's: the colouring it shows, by name, and how many updates it shows for.</summary>
/// <param name="Shade">The colouring shown.</param>
/// <param name="Updates">How many updates it shows for.</param>
public readonly record struct SamusGlowStep(string Shade, int Updates);

/// <summary>
/// One of Samus's glows: the colourings it shows in turn, and the step it goes back to after its last if it goes
/// round. Her data lists them (<see cref="SamusColours"/>); what makes her glow keeps its own place in one.
/// </summary>
/// <remarks>
/// <code>
/// { "steps": [ { "shade": "stored_shine_power_0", "updates": 1 } ], "loops_from": 0 }
/// </code>
/// A glow with no "loops_from" ends after its last step.
/// </remarks>
/// <param name="Steps">Its steps, in order.</param>
/// <param name="LoopsFrom">The step it goes back to after its last; null for one that ends there.</param>
public sealed record SamusGlow(IReadOnlyList<SamusGlowStep> Steps, int? LoopsFrom)
{
    /// <summary>Reads a glow from its data.</summary>
    public static SamusGlow Read(DataNode data)
    {
        ArgumentNullException.ThrowIfNull(data);
        SamusGlowStep[] steps = [.. data.List("steps").Select(step => new SamusGlowStep(step.Text("shade"), step.Int("updates")))];
        int? loopsFrom = data.Has("loops_from") ? data.Int("loops_from") : null;
        if (steps.Length == 0 || steps.Any(step => step.Updates < 1))
            throw data.Problem("is a glow with no step, or with a step that shows for no updates");
        if (loopsFrom is < 0 || loopsFrom >= steps.Length)
            throw data.Problem("is a glow that goes back to a step it does not have");
        return new SamusGlow(steps, loopsFrom);
    }

    /// <summary>
    /// The colouring shown next from <paramref name="place"/> by something that shows one on every update, whatever
    /// the steps' own lengths: the place is moved past it. Null once a glow that ends has ended. A place kept in one
    /// glow goes on in another of as many steps, as her glow in one suit goes on from where it stood in another.
    /// </summary>
    public string? Next(ref int place)
    {
        if (place >= Steps.Count)
        {
            if (LoopsFrom is not { } from)
                return null;
            place = from;
        }

        return Steps[place++].Shade;
    }
}
