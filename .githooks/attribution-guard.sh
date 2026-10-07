#!/bin/sh
# Shared check: no commit may name or be attributed to an AI assistant vendor,
# in its author, committer, or anywhere in its message.

blocked_pattern='claude|anthropic|codex|openai'

# Prints the matching lines of stdin and fails when any blocked name occurs.
reject_blocked_names() {
    matches=$(grep -inE "$blocked_pattern")
    if [ -n "$matches" ]; then
        printf '%s\n' "$matches" >&2
        return 1
    fi
    return 0
}
