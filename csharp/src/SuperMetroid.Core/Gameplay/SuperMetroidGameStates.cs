namespace SuperMetroid.Core.Gameplay;

/// <summary>Domain-owned transitions over <see cref="SuperMetroidGameState"/>.</summary>
public static class SuperMetroidGameStates
{
    /// <summary>
    /// The dispatcher state one above <paramref name="state"/>, as the native <c>INC $0998</c>
    /// advances it. <see cref="SuperMetroidGameState.TransitionFromDemoB"/> is the last table entry and has no successor.
    /// </summary>
    public static SuperMetroidGameState Next(this SuperMetroidGameState state) => EnumOrder.Next(state);
}
