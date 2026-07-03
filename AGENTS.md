# AGENTS.md

Guidance for AI coding assistants (Claude Code, Codex, etc.) working in this repository.

## This repo has two independent toolchains

- `export/` — a Go module (`code-comments-sync`) that parses Go source and extracts doc comments to JSON.
- `import/` — a .NET 7 project (`SyncGoComments`) that injects that JSON's doc comments into C# source via Roslyn.

They don't share a build system, a `go.work`, or a solution file. Treat them as two separate projects that happen to live in one repo and are released together.

## Release artifacts merge both tools into one archive per platform

`release.yml` builds both `comments-sync-golang-export` (Go) and `comments-sync-csharp-import` (.NET) for all 5 targets (windows-386/amd64/arm64, darwin-amd64/arm64), writes them into the same per-platform directory, and packages each platform directory into a single archive (`.zip` for Windows, `.tar.gz` for macOS). Each downloaded archive therefore contains **both** executables. Don't split this into separate per-tool archives — that would be a breaking change to how existing users discover and use these tools.

## Two independent `Version` fields — keep them in sync

- `export/internal/version/version.go`: `const Version = "..."`
- `import/SyncGoComments.csproj`: `<Version>...</Version>`

Both need to be bumped together before tagging a release. There's no automation enforcing this — it's a manual step in the release checklist below.

## `scripts/build-test.sh` is not a test script

Despite the name, `scripts/build-test.sh` (at the repo root, not under `scripts/build/`) orchestrates the full local release pipeline: clean → build both tools → compress → upload to GitHub. It does not run `go test` or `dotnet test`. Don't confuse it with actual test execution, and don't repurpose it as a CI test step.

## Project layout

- `export/cmd/comments-sync-golang-export/main.go` — Go CLI entrypoint; `extractFuncComments` holds the core parsing logic and is unit tested in `main_test.go`
- `export/internal/version/version.go` — Go side version constant
- `import/Program.cs` — .NET CLI entrypoint; `Program.SyncCommentsCode` (internal, exposed to tests via `InternalsVisibleTo`) holds the core injection logic and is unit tested in `import/SyncGoComments.Tests/`
- `import/SyncGoComments.csproj` — the actual Exe project; excludes `SyncGoComments.Tests/**/*.cs` from its own compile items (they live in a subdirectory and would otherwise be picked up by default globbing)

## Build, test, lint

```sh
cd export
go build ./...
go test ./...
gofmt -l .              # must produce no output
golangci-lint run ./...  # must report 0 issues

cd ../import
dotnet build SyncGoComments.csproj
dotnet test SyncGoComments.Tests/SyncGoComments.Tests.csproj
```

## Commit messages

Write commit messages in English. Keep them short and describe the actual change — avoid placeholder messages like `init` or `update`.

## Release process

Releases are tag-triggered, not push-triggered:

1. Draft `release_notes.md` locally by reading the diff since the last tag (`git diff <last-tag>..HEAD`) — an AI assistant can draft this, but a human must review it before tagging.
2. Bump **both** `export/internal/version/version.go` and `import/SyncGoComments.csproj`'s `<Version>` to match the new tag.
3. `git tag vX.Y.Z && git push origin vX.Y.Z` — this triggers `.github/workflows/release.yml`, which cross-builds both tools for all 5 targets, packages them together per platform, and creates the GitHub Release using the committed `release_notes.md`.

Do not call any LLM API from within CI to generate release notes — that step happens locally, before tagging, to avoid paying per-run API costs in the pipeline.
