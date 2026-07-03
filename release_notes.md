## Changelog for v0.2.0

This release is a repository relaunch — both CLI tools' flags, output, and behavior are unchanged. It focuses on making the project properly usable, testable, and maintainable by others:

- **Licensing**: added an MIT `LICENSE` (previously the repo had none).
- **Testability**: both toolchains previously had zero automated tests. Added Go tests covering `export`'s doc-comment extraction (exported functions, methods, and interface methods, including the `Ex`/`Async`/`ExAsync` name variants), and a new `SyncGoComments.Tests` xUnit project covering `import`'s Roslyn-based comment injection (summary/param/returns tags, the overwrite flag).
- **CI/CD migrated from Azure DevOps to GitHub Actions**: `ci.yml` runs the Go and .NET checks as separate jobs; `release.yml` cross-builds both tools for all 5 targets and packages them together per platform exactly as before — each archive still contains both `comments-sync-golang-export` and `comments-sync-csharp-import`. Releases are now triggered by pushing a `vX.Y.Z` tag instead of every push to `main`.
- **Project layout**: moved the Go side (`export/`) to the standard `cmd/comments-sync-golang-export/` + `internal/version/` layout.
- **Docs**: expanded `README.md` with install/usage/dev instructions, added `AGENTS.md` for AI coding assistants — including a note that this repo has two independent `Version` fields (`export/internal/version/version.go` and `import/SyncGoComments.csproj`) that need to be bumped together, and that `scripts/build-test.sh` is a release-orchestration script, not a test runner.
- Translated the remaining Chinese comment in `import/Program.cs` to English.
- Removed the now-unused Azure-specific scripts (`scripts/config/gh-config.sh`, `scripts/upload/`).
