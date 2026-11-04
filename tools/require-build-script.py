"""Claude Code PreToolUse hook: refuses a shell command that runs `dotnet build` directly, so every build goes through
tools/build.ps1, which removes the unused usings the build reports and builds again (#1530).

Reads the hook payload on stdin; prints a deny decision for a direct build and nothing otherwise."""
import json
import re
import sys

command = json.load(sys.stdin).get("tool_input", {}).get("command", "")
direct = re.search(r"\bdotnet(?:\.exe)?[\"']?\s+build\b", command)
through_script = re.search(r"tools[\\/]build\.ps1", command)
if direct and not through_script:
    print(json.dumps({"hookSpecificOutput": {
        "hookEventName": "PreToolUse",
        "permissionDecision": "deny",
        "permissionDecisionReason":
            "Build with `pwsh -File tools/build.ps1 [project or solution] [dotnet build arguments...]` instead of "
            "`dotnet build`: it removes the unused usings the build reports and rebuilds until none are left, then "
            "prints the remaining errors once.",
    }}))
