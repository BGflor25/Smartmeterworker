---
name: build-and-pr
description: Verify the SmartMeter solution builds (and tests pass), then push the changes to a new branch on github.com/BGflor25/SmartMeterApiParser and open a pull request. Use when the user asks to "ship", "open a PR", "push and PR", or "check it builds and upload".
argument-hint: "[branch-name] [PR title]"
allowed-tools: Bash(dotnet *), Bash(git *), Bash(gh *)
---

# Build and open a PR

Target repository: `https://github.com/BGflor25/SmartMeterApiParser.git` (base branch: `main`).
Solution file: `SmartMeter.slnx` in the project root.

Arguments (optional): `$ARGUMENTS` — first word is the branch name, the rest is the PR title.
If not given, derive a short kebab-case branch name (e.g. `feature/<topic>`) and a title from the changes.

## 1. Preflight

- `gh auth status` — if not logged in, STOP and tell the user to run `! gh auth login` themselves. Do not try to log in for them.
- Make sure `dotnet` is available (`dotnet --version`).

## 2. Build and test — stop on failure

Run from the project root:

```bash
dotnet restore SmartMeter.slnx
dotnet build SmartMeter.slnx --no-restore -c Release -warnaserror:false
dotnet test SmartMeter.slnx --no-build -c Release
```

If restore, build or tests fail: show the relevant errors (file:line), do NOT commit or push, and offer to fix them. Only continue when everything is green.

## 3. Prepare git

If the folder is **not yet a git repository** (`git rev-parse --is-inside-work-tree` fails), connect it to the existing remote history without losing local files:

```bash
git init
git remote add origin https://github.com/BGflor25/SmartMeterApiParser.git
git fetch origin
git checkout -b <branch>
git reset origin/main        # mixed reset: keeps working tree, index = remote main
```

If it **is** a repo:

- Ensure `origin` points at the URL above (`git remote -v`; add or `set-url` if needed).
- `git fetch origin`.
- If currently on `main`, create the branch: `git checkout -b <branch>`. If already on a feature branch, reuse it.
- If the branch name already exists on the remote (`git ls-remote --heads origin <branch>`), pick a different name rather than overwriting it.

## 4. Commit

- Check `.gitignore` covers `bin/`, `obj/`, `.vs/`, `*.user`. Never commit build output, `.env`, secrets, or `.claude/settings.local.json`.
- `git status` and `git diff --stat origin/main` — review what will be committed. If nothing differs from `origin/main`, stop and say so.
- `git add -A` (then unstage anything that shouldn't be there).
- Commit with a concise message describing the change, ending with the attribution lines from the current session's instructions.

## 5. Push and open the PR

```bash
git push -u origin <branch>
gh pr create --repo BGflor25/SmartMeterApiParser --base main --head <branch> \
  --title "<title>" --body "<body>"
```

PR body should contain:
- **Summary** — 2-4 bullets of what changed and why.
- **Verification** — build + test results (e.g. "dotnet build: succeeded, dotnet test: 12 passed").
- The PR attribution line from the current session's instructions.

If a PR for this branch already exists (`gh pr view <branch> --repo BGflor25/SmartMeterApiParser`), just push and report the existing PR URL instead of creating a new one.

## 6. Report

Reply with: build/test result, branch name, commit hash, and the PR URL.
