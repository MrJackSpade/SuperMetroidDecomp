"""Claude Code PreToolUse hook: refuses a shell command that runs `dotnet build` directly, so every build goes through
tools/build.ps1, the one sanctioned build entry point, which builds once and prints the errors once (#1530).

Reads the hook payload on stdin; prints a deny decision for a direct build and nothing otherwise."""
import json
import re
import sys

command = json.load(sys.stdin).get("tool_input", {}).get("command", "")
# A verification run goes through tools/run-verification.ps1, which builds first and runs nothing when the build fails,
# so no run ever uses a binary older than the source (owner rule, 2026-11-11).
direct_run = re.search(r"SuperMetroid\.(?:Verification|RenderVerification|DesktopVerification|IntegrationVerification)\.(?:exe|dll)\b", command)
if direct_run and not re.search(r"tools[\\/]run-verification\.ps1", command):
    print(json.dumps({"hookSpecificOutput": {
        "hookEventName": "PreToolUse",
        "permissionDecision": "deny",
        "permissionDecisionReason":
            "Run verification with `pwsh -File tools/run-verification.ps1 <Verification|RenderVerification|"
            "DesktopVerification|IntegrationVerification> [arguments...]`: it builds first and runs nothing when the build "
            "fails, so a run never uses a stale binary.",
    }}))
    sys.exit(0)
direct = re.search(r"\bdotnet(?:\.exe)?[\"']?\s+build\b", command)
through_script = re.search(r"tools[\\/]build\.ps1", command)
if direct and not through_script:
    print(json.dumps({"hookSpecificOutput": {
        "hookEventName": "PreToolUse",
        "permissionDecision": "deny",
        "permissionDecisionReason":
            "Build with `pwsh -File tools/build.ps1 [project or solution] [dotnet build arguments...]` instead of "
            "`dotnet build`: it builds once, keeps the full output in a log and prints the errors once.",
    }}))
