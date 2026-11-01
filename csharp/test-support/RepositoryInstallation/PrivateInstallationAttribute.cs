namespace SuperMetroid.TestSupport.RepositoryInstallation;

/// <summary>
/// States why a verification member builds an installation of its own (#1487, SME6455). Verification reads the shared
/// <see cref="RepositoryInstallation.Installation"/>, or writes into a writable <c>InstalledFixtures.CreatePrivateCopy</c>
/// copy of it; a member that instead creates, extracts, repairs or reinstalls an installation from the ROM carries this
/// attribute and names its reason: it writes into the installation in a way a copy cannot serve, or it exercises the
/// installer itself. A full install from the ROM costs minutes and tens of gigabytes allocated, so every such member is a
/// stated decision.
/// </summary>
/// <param name="reason">Why the member cannot read the shared installation or write into a copy of it.</param>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Property, AllowMultiple = false)]
internal sealed class PrivateInstallationAttribute(string reason) : Attribute
{
    /// <summary>Why the member builds an installation of its own instead of reading or copying the shared one.</summary>
    public string Reason { get; } = reason;
}
