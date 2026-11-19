using SuperMetroid.EnsureAnalyzer.Contracts;
using SuperMetroid.EnsureAnalyzer.Modules;
using static SuperMetroid.EnsureVerification.Harness.AnalyzerTests;

namespace SuperMetroid.EnsureVerification.Contracts;

/// <summary>
/// Checks SME6470: in Core, a bool or nullable request flag that one step sets and another consumes and clears is reported,
/// as is a <c>Consume…</c> method that reads and clears one (#2562).
/// </summary>
internal static class OneUpdateRequestAnalyzerTests
{
    /// <summary>The compatibility-service attribute the cases read, declared as Core declares it.</summary>
    private const string Declarations =
        "namespace SuperMetroid.Core.Compatibility { [System.AttributeUsage(System.AttributeTargets.All)]" +
        " public sealed class CompatibilityServiceAttribute(string behaviour) : System.Attribute { public string Behaviour { get; } = behaviour; } }";

    /// <summary>Each request flag the rule reports, and the members it leaves alone.</summary>
    internal static void VerifyOneUpdateRequestAnalyzer()
    {
        var analyzer = new OneUpdateRequestAnalyzer();
        const string Id = OneUpdateRequestAnalyzer.OneUpdateRequestId;
        void Expect(string body, string assembly = CoreLayerAnalyzer.CoreAssembly, string attributes = "", params string[] ids) =>
            VerifySnippetIds(Declarations + " namespace SuperMetroid.Core.World { using SuperMetroid.Core.Compatibility; " +
                attributes + " class Probe { " + body + " } }", analyzer, assembly, ids);

        // A request flag set by one step and cleared by another is reported, a bool, a nullable value or a nullable reference.
        Expect("public bool JumpRequested { get; set; } void Ask() => JumpRequested = true; void Take() { if (JumpRequested) JumpRequested = false; }",
            ids: [Id]);
        Expect("int? _pendingHeight; void Ask(int y) => _pendingHeight = y; void Take() => _pendingHeight = null;", ids: [Id]);
        Expect("string? QueuedText { get; set; } void Ask() => QueuedText = \"x\"; void Take() => QueuedText = default;", ids: [Id]);
        Expect("bool SavesChangedThisFrame { get; set; } void Begin() => SavesChangedThisFrame = false; void Change() => SavesChangedThisFrame = true;",
            ids: [Id]);
        // A Consume… method that reads and clears a flag of its own type is reported with the flag's own finding.
        Expect("bool _armed; void Arm() => _armed = true; bool ConsumeArm() { bool armed = _armed; _armed = false; return armed; }", ids: [Id]);
        // A flag that is only ever set, or only ever cleared, a state not named as a request, an int, a readonly field, a
        // compatibility service's own record and another assembly's flag are left alone.
        Expect("public bool StartRequested { get; private set; } void Start() => StartRequested = true;");
        Expect("bool HoleLatch { get; set; } void Latch() => HoleLatch = true; void Release() => HoleLatch = false;");
        Expect("int PendingCount; void Add() => PendingCount = 3; void Clear() => PendingCount = 0;");
        Expect("readonly bool _pending = true;");
        Expect("bool Pending { get; set; } void Ask() => Pending = true; void Take() => Pending = false;",
            attributes: "[CompatibilityService(\"a strike waits for its object's update\")]");
        Expect("bool Pending { get; set; } void Ask() => Pending = true; void Take() => Pending = false;", assembly: "SuperMetroid.Verification");
        Console.WriteLine("One-update request rule: bool, nullable value and nullable reference request flags set and cleared by different steps, " +
            "and Consume methods that read and clear one, reported; set-only flags, states not named as requests, counts, " +
            "readonly fields, compatibility services and other assemblies left alone.");
    }
}
