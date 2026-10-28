namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>Chooses the turn record a crawling Yard's movement state uses.</summary>
internal static class YardTurns
{
    /// <summary>The turn of <paramref name="movement"/>, or its transition-disabled alternative when it has one.</summary>
    /// <exception cref="InvalidDataException"><paramref name="movement"/> is not a crawling state.</exception>
    internal static YardTurnDefinition ForMovement(
        YardMovementFunction movement,
        bool transitionDisabled)
    {
        if (transitionDisabled && YardTurnDefinitions.DisabledTurns.TryGetValue(movement, out YardTurnDefinition disabled))
            return disabled;
        return YardTurnDefinitions.Turns.TryGetValue(movement, out YardTurnDefinition turn)
            ? turn
            : throw new InvalidDataException(
                $"Yard crawl function {movement} has no turn definition.");
    }
}
