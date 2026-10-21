using SuperMetroid.Core.Areas.Ceres;

namespace SuperMetroid.Verification.Bosses.Ridley;

/// <summary>The nmi frame and wings of one verify ceres ridley body publication case.</summary>
/// <param name="NmiFrame">The nmi frame.</param>
/// <param name="Wings">The wings.</param>
internal readonly record struct VerifyCeresRidleyBodyPublicationCase(int NmiFrame, CeresMode7TransferList Wings);
