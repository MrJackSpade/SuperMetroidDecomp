using System.Reflection;
using Godot;

namespace Engine.Tests;

/// <summary>
/// Runs the project's checks and ends the process: exit code 0 when every check passed, 1 otherwise. A check is a
/// public, parameterless method marked <see cref="CheckAttribute"/> on a class with a parameterless constructor; each
/// check gets a fresh instance. Names given after "--" on the command line limit the run to checks whose class or
/// method name contains one of them.
/// </summary>
public partial class TestRunner : Node
{
    private static Node? _stage;

    /// <summary>
    /// A node inside the scene tree, for the few checks whose subject works only there, such as playing sound. A check
    /// that puts something under it takes it away again.
    /// </summary>
    public static Node Stage => _stage ?? throw new InvalidOperationException("The checks are not being run by the test runner.");

    /// <inheritdoc/>
    public override void _Ready()
    {
        _stage = this;
        int exitCode;
        try
        {
            exitCode = RunChecks(OS.GetCmdlineUserArgs());
        }
        catch (Exception exception)
        {
            // The process boundary: nothing may escape to the engine, which would log it and carry on.
            Console.Error.WriteLine(exception.ToString());
            exitCode = 1;
        }

        GetTree().Quit(exitCode);
    }

    /// <summary>Runs every check the filters admit, printing each failure in full, and returns the exit code.</summary>
    private static int RunChecks(string[] filters)
    {
        var checks = Assembly.GetExecutingAssembly().GetTypes()
            .OrderBy(type => type.FullName, StringComparer.Ordinal)
            .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(method => method.GetCustomAttribute<CheckAttribute>() is not null)
                .OrderBy(method => method.Name, StringComparer.Ordinal)
                .Select(method => (Type: type, Method: method)))
            .Where(check => filters.Length == 0 || filters.Any(filter =>
                check.Type.Name.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                check.Method.Name.Contains(filter, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        if (checks.Count == 0)
        {
            Console.Error.WriteLine(filters.Length == 0
                ? "No checks were found."
                : $"No check matches: {string.Join(", ", filters)}.");
            return 1;
        }

        int failed = 0;
        foreach (var (type, method) in checks)
        {
            string name = $"{type.Name}.{method.Name}";
            try
            {
                object instance = Activator.CreateInstance(type)
                    ?? throw new InvalidOperationException($"{type.FullName} could not be created.");
                method.Invoke(instance, null);
                Console.Out.WriteLine($"pass  {name}");
            }
            catch (TargetInvocationException invocation) when (invocation.InnerException is not null)
            {
                // The runner's job is to report every check's result, so a failing check is recorded and the run goes on.
                failed++;
                Console.Out.WriteLine($"FAIL  {name}");
                Console.Error.WriteLine($"{name}: {invocation.InnerException}");
            }
        }

        Console.Out.WriteLine($"{checks.Count - failed} passed, {failed} failed.");
        return failed == 0 ? 0 : 1;
    }
}
