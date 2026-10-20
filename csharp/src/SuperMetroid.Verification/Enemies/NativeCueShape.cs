namespace SuperMetroid.Verification.Enemies;

/// <summary>
/// The cue a native owner instruction must become, for a family whose cues are a closed hierarchy: which variant (and,
/// for an operand-less command, exactly which command), and the roles of the instruction's operand words.
/// </summary>
/// <typeparam name="TCue">The family's cue type.</typeparam>
/// <param name="Name">The expected cue, for failure messages.</param>
/// <param name="Matches">Whether a typed cue is the one the instruction becomes.</param>
/// <param name="Operands">One role letter per operand word, in native order.</param>
internal sealed record NativeCueShape<TCue>(string Name, Func<TCue, bool> Matches, string Operands)
    where TCue : class
{
    /// <summary>An instruction that becomes exactly <paramref name="expected"/>, with no operands.</summary>
    internal static NativeCueShape<TCue> Exactly(TCue expected) => new(expected.ToString() ?? typeof(TCue).Name, cue => cue.Equals(expected), "");

    /// <summary>An instruction that becomes a <typeparamref name="TVariant"/>, with operands in the roles <paramref name="operands"/> names.</summary>
    internal static NativeCueShape<TCue> Of<TVariant>(string operands)
        where TVariant : TCue =>
        new(typeof(TVariant).Name, cue => cue is TVariant, operands);

    /// <summary>An instruction that becomes a <typeparamref name="TVariant"/> that <paramref name="kind"/> accepts.</summary>
    internal static NativeCueShape<TCue> Of<TVariant>(string operands, string name, Func<TVariant, bool> kind)
        where TVariant : TCue =>
        new(name, cue => cue is TVariant variant && kind(variant), operands);

    /// <summary>Requires <paramref name="cue"/> to be the cue this instruction becomes.</summary>
    internal void Check(TCue cue, string at) => AssertTrue(Matches(cue), $"{at}: cue {cue} is {Name}");
}
