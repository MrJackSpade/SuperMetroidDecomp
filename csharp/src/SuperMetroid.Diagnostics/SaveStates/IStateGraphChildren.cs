using SuperMetroid.Core;

namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>
/// Receives each value one object of a debugger state holds that can reach another object (#2602), with the step that
/// reaches it: a field (<c>IStateGraphVisitor.Child</c>), an element's index, or a named part such as a dictionary
/// entry's key or a loaded part's value.
/// </summary>
internal interface IStateGraphChildren : IStateGraphVisitor
{
    /// <summary>Receives an element of an array or collection.</summary>
    /// <param name="index">The element's place.</param>
    /// <param name="value">The element.</param>
    void Element(int index, object? value);

    /// <summary>Receives a part a step names, such as a dictionary entry's key.</summary>
    /// <param name="step">The step.</param>
    /// <param name="value">The part.</param>
    void Part(string step, object? value);
}
