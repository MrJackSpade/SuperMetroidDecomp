using SuperMetroid.Core.Input;

namespace SuperMetroid.Verification.Runtime.Attract;

/// <summary>Checks scripted demo input edges against the preserved physical controller latch.</summary>
internal static class AttractDemoControllerVerification
{
    /// <summary>
    /// Checks that consecutive scripted Shoot presses retain their explicit new-press edges
    /// while the Start latch history survives each temporary override.
    /// A later real Start latch must see no edge leaked from the demo.
    /// </summary>
    internal static void VerifyAttractDemoControllerOverride()
    {
        var controller = new GameplayActionInput();
        controller.Latch(GameplayAction.Start);
        for (int frame = 0; frame < 2; frame++)
        {
            using (controller.UseDemoInput(GameplayAction.Shoot, GameplayAction.Shoot))
            {
                if (controller.Current != GameplayAction.Shoot || controller.NewlyPressed != GameplayAction.Shoot)
                    throw new InvalidDataException("Demo controller did not preserve the explicit scripted edge.");
                if (controller.Previous != GameplayAction.Start)
                    throw new InvalidDataException("Demo override modified the latch history.");
            }
            if (controller.Current != GameplayAction.Start || controller.NewlyPressed != GameplayAction.Start)
                throw new InvalidDataException("Demo beta failed to restore the player cancellation input.");
        }
        controller.Latch(GameplayAction.Start);
        if (controller.NewlyPressed != 0)
            throw new InvalidDataException("Demo input leaked into the next real controller edge calculation.");
        Console.WriteLine("  Attract controller: explicit script edges and restored hardware history agree.");
    }
}
