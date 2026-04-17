# CLAUDE.md — LegendsAwaken (LA)

Project-level instructions for Claude Code. These override default behavior.

---

## File Operations

- When organizing or processing files, always read ALL files in a directory/folder before making decisions — never assume structure from just the first file.
- When working with file organization tasks, always confirm the categorization plan with the user BEFORE executing moves. Present the plan as a list and wait for approval.

## Session Management

- After completing any significant work session, always update project documentation files (`Estrutura.md`, `ANALISE.md`, `ROADMAP.md`, `TODO.md`, `README.md`) and memory files to reflect current state before ending.

## Sub-Agents / Parallel Work

- When spawning sub-agents (Task/Agent tool), always ensure they have the necessary tool permissions (Bash, Write, Read). Verify permissions before dispatching parallel work.

## Content Extraction

- When extracting or transcribing text from images, NEVER fabricate or hallucinate content. If text is unclear, say so. Always faithfully transcribe only what is visible.

## Tool Preferences

- Prefer using the Write tool directly for creating files instead of Bash commands like `echo`/`cat` with heredoc. Bash file-writing is error-prone with special characters and escaping.
