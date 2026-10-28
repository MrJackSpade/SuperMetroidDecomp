namespace SuperMetroid.Core.Enemies.Fireflea;

/// <summary>
/// The movement a Fireflea's population parameter one selects (#1461): circular motion around its centre, or vertical
/// motion between its saved extrema. This is the one place the parameter's movement bit is read.
/// </summary>
[EncodingOwner("Fireflea population parameter one movement bit")]
internal static class FirefleaMovementParameter
{
    /// <summary>The parameter-one bit that selects circular motion.</summary>
    private const int CircularMovementBit = 0x0002;

    /// <summary>Whether <paramref name="parameter1"/> selects circular motion rather than vertical motion.</summary>
    /// <param name="parameter1">The population record's parameter one.</param>
    internal static bool SelectsCircularMovement(int parameter1) => (parameter1 & CircularMovementBit) != 0;
}
