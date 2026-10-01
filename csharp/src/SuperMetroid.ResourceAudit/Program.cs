using System.Runtime.InteropServices;
using System.Text.Json;

namespace SuperMetroid.ResourceAudit;

internal static partial class Program
{
    private static int Main(string[] args)
    {
        // Install before parsing paths or loading definitions. An audit error must
        // never escape to the CLR/Windows reporter or open a modal error dialog.
        try
        {
            if (OperatingSystem.IsWindows()) SetErrorMode(ConsoleErrorPolicy.FailCriticalErrors |
                ConsoleErrorPolicy.NoFaultDialog | ConsoleErrorPolicy.NoOpenFileDialog);
            if (args is ["--self-check"])
            {
                AuditContractChecks.Run();
                return 0;
            }
            if (args is ["--work-robot-resource-check"])
            {
                WorkRobotResourceChecks.Run();
                return 0;
            }
            if (args is ["--mama-turtle-resource-check"])
            {
                MamaTurtleResourceChecks.Run();
                return 0;
            }
            if (args is ["--zero-resource-check"])
            {
                ZeroResourceChecks.Run();
                return 0;
            }
            if (args is ["--friendly-animal-resource-check", string family])
            {
                FriendlyAnimalResourceChecks.Run(family);
                return 0;
            }
            if (args is ["--ordinary-enemy-resource-check", string enemyFamily])
            {
                OrdinaryEnemyResourceChecks.Run(enemyFamily);
                return 0;
            }
            if (args is ["--crocomire-skeleton-resource-check"])
            {
                CrocomireSkeletonResourceChecks.Run();
                return 0;
            }
            if (args is ["--kraid-part-resource-check", string part])
            {
                KraidPartResourceChecks.Run(part);
                return 0;
            }
            if (args is ["--nuclear-waffle-resource-check"])
            {
                NuclearWaffleResourceChecks.Run();
                return 0;
            }
            string root = Directory.GetCurrentDirectory();
            string? jsonPath = null;
            for (int index = 0; index < args.Length; index++)
            {
                switch (args[index])
                {
                    case "--root" when index + 1 < args.Length: root = args[++index]; break;
                    case "--json" when index + 1 < args.Length: jsonPath = args[++index]; break;
                    default: throw new ArgumentException("Usage: --root REPOSITORY [--json REPORT] | " +
                        "--self-check | --work-robot-resource-check | --mama-turtle-resource-check | --zero-resource-check | " +
                        "--friendly-animal-resource-check FAMILY | --ordinary-enemy-resource-check FAMILY | " +
                        "--crocomire-skeleton-resource-check | --kraid-part-resource-check Foot|Lint | " +
                        "--nuclear-waffle-resource-check");
                }
            }
            root = Path.GetFullPath(root);
            var report = new AuditReport();
            ResourceIndex exports = DefinitionAudit.Collect(root, report);
            ConsumerAudit.Run(root, exports, report);
            report.Sort();
            Console.WriteLine($"Static resource audit: {report.ReferenceCount} resource references; " +
                $"{report.Consumers.Count} resource consumer sites; {report.MissingResourceCount} missing identities " +
                $"({report.MissingCount} references); " +
                $"{report.UnresolvedCount} unresolved. No ROM, saves or gameplay were opened.");
            foreach (AuditCoverage coverage in report.Coverage)
                Console.WriteLine($"  {coverage.Domain}: {coverage.References} references / {coverage.Exports} exports");
            AuditFinding[] displayed = [.. report.Findings.Where(item => item.Code == AuditReport.Missing)
                .DistinctBy(item => (item.Domain, item.Resource)).Take(25),
                .. report.Findings.Where(item => item.Code != AuditReport.Missing).Take(15)];
            foreach (AuditFinding finding in displayed)
                Console.Error.WriteLine($"{finding.Code} {finding.Source}: {finding.Owner}: {finding.Message}");
            if (report.Findings.Count > displayed.Length)
                Console.Error.WriteLine($"{report.Findings.Count - displayed.Length} additional reference findings; use --json for the complete report.");
            if (jsonPath is not null)
            {
                string absolute = Path.GetFullPath(jsonPath);
                Directory.CreateDirectory(Path.GetDirectoryName(absolute)!);
                File.WriteAllText(absolute, JsonSerializer.Serialize(report, AuditReport.JsonOptions) + Environment.NewLine);
                Console.WriteLine("Report: " + absolute);
            }
            // Unresolved coverage is a failure, not a warning pretending to be a
            // clean audit. Ordinary source compilation and this audit are distinct.
            return report.Findings.Count == 0 ? 0 : 1;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.ToString());
            return 2;
        }
    }

    [LibraryImport("kernel32.dll")]
    private static partial uint SetErrorMode(uint mode);
}
