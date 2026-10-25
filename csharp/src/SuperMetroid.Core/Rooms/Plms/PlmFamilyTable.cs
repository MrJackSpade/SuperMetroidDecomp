namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Each PLM family's step, run at the start of a family slot's pass. Every family registers exactly once; a family with
/// no step of its own registers the shared path explicitly.
/// </summary>
internal sealed class PlmFamilyTable
{
    /// <summary>The step of each family, indexed by family.</summary>
    private readonly PlmFamilyStepHandler?[] _steps = new PlmFamilyStepHandler?[Enum.GetValues<PlmFamily>().Length];

    /// <summary>Adds <paramref name="family"/>'s step.</summary>
    /// <exception cref="InvalidOperationException">The family already has a step.</exception>
    internal void Add(PlmFamily family, PlmFamilyStepHandler step)
    {
        Ensure.NotNull(step);
        if (_steps[(int)family] is not null)
            throw new InvalidOperationException($"PLM family {family} added its step twice.");
        _steps[(int)family] = step;
    }

    /// <summary>Rejects a table in which some family registered no step.</summary>
    /// <exception cref="InvalidOperationException">A family has no step.</exception>
    internal void RequireEveryFamily()
    {
        PlmFamily[] missing = [.. Enum.GetValues<PlmFamily>().Where(family => _steps[(int)family] is null)];
        if (missing.Length != 0)
            throw new InvalidOperationException($"PLM families without a step: {string.Join(", ", missing)}.");
    }

    /// <summary>Runs the step of the family <paramref name="slot"/> carries.</summary>
    internal PlmStepOutcome Step(PlmSlot slot, PlmFamilyState family, PlmStepContext context) =>
        (_steps[(int)family.Family] ?? throw new InvalidOperationException($"PLM family {family.Family} has no step."))(slot, context);
}
