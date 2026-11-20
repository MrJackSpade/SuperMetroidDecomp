namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// One encounter's looks and what each copies over the sprite characters to show it (#1538): a look shows every copy of
/// the looks up to it, in order, over the room's load.
/// </summary>
/// <param name="Encounter">The encounter.</param>
/// <param name="Looks">What each look copies, by look in the order the encounter shows them; nothing for the room's load.</param>
internal sealed record EncounterLookSteps(ComposedEncounter Encounter, IReadOnlyList<EncounterLookStep> InOrder)
{
    /// <summary>The encounter's looks, in the order the encounter takes them, the room's load first.</summary>
    internal IEnumerable<ComposedLook> Looks => InOrder.Select(step => step.Look);

    /// <summary>What <paramref name="look"/> copies, in the order its copies land: nothing for the room's load.</summary>
    /// <exception cref="InvalidDataException">The look is none of the encounter's.</exception>
    internal IReadOnlyList<EncounterArtworkUpload> CopiesOf(ComposedLook look) =>
        InOrder.FirstOrDefault(step => step.Look == look)?.Copies
            ?? throw new InvalidDataException($"{Encounter} has no look {look}.");

    /// <summary>What every look up to and including <paramref name="look"/> copies, in the order the copies land.</summary>
    /// <exception cref="InvalidDataException">The look is none of the encounter's.</exception>
    internal IEnumerable<EncounterArtworkUpload> CopiesThrough(ComposedLook look)
    {
        _ = CopiesOf(look);
        foreach (EncounterLookStep step in InOrder)
        {
            foreach (EncounterArtworkUpload copy in step.Copies)
                yield return copy;
            if (step.Look == look)
                yield break;
        }
    }
}

/// <summary>One look of an encounter and what it copies, in the order its copies land.</summary>
/// <param name="Look">The look.</param>
/// <param name="Copies">What the look copies: nothing for the room's load.</param>
internal sealed record EncounterLookStep(ComposedLook Look, IReadOnlyList<EncounterArtworkUpload> Copies);
