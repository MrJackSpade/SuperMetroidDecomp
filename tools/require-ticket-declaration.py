"""Claude Code PreToolUse hook: refuses a shell command that opens a GitHub ticket by any route but
tools/new-ticket.py, and refuses a call of that tool that lacks the ticket's design declaration.

The owner's rule (2026-11-26): no ticket is opened without a boolean saying whether its change is exclusively
presentation; one that is says, by a boolean, whether the change is idiomatic; one that is exclusively presentation and
not idiomatic gives a justification for its design pattern, in words. A ticket that is not exclusively presentation, or
that is idiomatic, needs nothing further.

Reads the hook payload on stdin; prints a deny decision for a refused command and nothing otherwise."""
import json
import os
import re
import sys

#: The one way a ticket is opened.
TOOL = re.compile(r"tools[\\/]new-ticket\.py")

#: The tool being run, by Python: a command that only names its file, as one that commits it does, is not a call of it.
RUNS_TOOL = re.compile(r"\bpy(?:thon)?(?:3|\.exe)?[\"']?\s+(?:-\S+\s+)*[\"']?[^\s\"']*tools[\\/]new-ticket\.py")

#: This hook's own file.
OWN = re.compile(r"tools[\\/]require-ticket-declaration\.py")

#: `gh issue create` as a command line, as a list of arguments in a script, and as a post to the issues of a repository.
DIRECT = re.compile(r"\bgh(?:\.exe)?[\"']?\s+issue\s+create\b")
AS_ARGUMENTS = re.compile(r"[\"']issue[\"']\s*,\s*[\"']create[\"']")
BY_API = re.compile(r"\bgh(?:\.exe)?[\"']?\s+api\b[^\n|;&]*/issues(?![/\w])")

#: A script named in a command: its text is read for the same.
SCRIPT = re.compile(r"[\"']?((?:[A-Za-z]:)?[^\s\"'|;&<>]+\.(?:py|sh|ps1|cmd|bat))[\"']?")


def deny(reason: str) -> None:
    print(json.dumps({"hookSpecificOutput": {"hookEventName": "PreToolUse", "permissionDecision": "deny", "permissionDecisionReason": reason}}))
    sys.exit(0)


def opens_a_ticket(text: str) -> bool:
    return bool(DIRECT.search(text) or AS_ARGUMENTS.search(text) or BY_API.search(text))


HOW = ("Open a ticket with `python tools/new-ticket.py --title ... --body-file ... --milestone ... --label ... "
       "--presentation-only true|false [--idiomatic true|false] [--design-justification \"...\" --why-not-idiomatic \"...\"]`: "
       "every ticket says whether its change is exclusively presentation; one that is says whether it is idiomatic; one that is "
       "exclusively presentation and not idiomatic gives the justification for its design pattern and why the same effect "
       "cannot be produced idiomatically (the owner's rule, 2026-11-26).")

payload = json.load(sys.stdin)
command = payload.get("tool_input", {}).get("command", "")
cwd = payload.get("cwd") or os.getcwd()

if opens_a_ticket(command):
    deny("This command opens a GitHub ticket directly. " + HOW)

for match in SCRIPT.finditer(command):
    path = match.group(1)
    # The tool is the sanctioned way, and this hook only names the command it refuses: neither is read.
    if TOOL.search(path) or OWN.search(path):
        continue
    full = path if os.path.isabs(path) else os.path.join(cwd, path)
    if not os.path.isfile(full):
        continue
    with open(full, encoding="utf-8", errors="replace") as script:
        if opens_a_ticket(script.read()):
            deny(f"The script {path} opens a GitHub ticket directly. " + HOW)

if RUNS_TOOL.search(command):
    presentation = re.search(r"--presentation-only[=\s]+[\"']?(true|false)\b", command)
    if presentation is None:
        deny("tools/new-ticket.py is called without --presentation-only true|false. " + HOW)
    if presentation.group(1) == "true":
        idiomatic = re.search(r"--idiomatic[=\s]+[\"']?(true|false)\b", command)
        if idiomatic is None:
            deny("A ticket that is exclusively presentation says whether its change is idiomatic: --idiomatic true|false. " + HOW)
        if idiomatic.group(1) == "false" and re.search(r"--design-justification[=\s]+[\"']?[^\s\"'-]", command) is None:
            deny("A ticket that is exclusively presentation and not idiomatic gives the justification for its design pattern: "
                 "--design-justification \"...\". " + HOW)
        if idiomatic.group(1) == "false" and re.search(r"--why-not-idiomatic[=\s]+[\"']?[^\s\"'-]", command) is None:
            deny("A ticket that is exclusively presentation and not idiomatic also says why the same effect cannot be produced "
                 "idiomatically: --why-not-idiomatic \"...\". If it can be, that is the route to take. " + HOW)
