package main

import (
	"os"
	"path/filepath"
	"testing"
)

const sampleSource = `package sample

// DoThing does the thing.
func DoThing() {}

func unexported() {}

// Widget is a sample type.
type Widget struct{}

// Build builds the widget.
func (w Widget) Build() {}

func (w Widget) unexportedMethod() {}

// Runner can run things.
type Runner interface {
	// Run runs it.
	Run()
}
`

func writeSampleFile(t *testing.T) string {
	t.Helper()
	dir := t.TempDir()
	path := filepath.Join(dir, "sample.go")
	if err := os.WriteFile(path, []byte(sampleSource), 0o644); err != nil {
		t.Fatalf("failed to write sample source: %v", err)
	}
	return dir
}

func docFor(t *testing.T, comments FuncComments, name string) (string, bool) {
	t.Helper()
	for _, f := range comments.Funcs {
		if f.Name == name {
			return f.Doc, true
		}
	}
	return "", false
}

func TestExtractFuncCommentsExportedFunction(t *testing.T) {
	dir := writeSampleFile(t)

	comments, err := extractFuncComments(dir)
	if err != nil {
		t.Fatalf("extractFuncComments() error = %v", err)
	}

	for _, name := range []string{"DoThing", "DoThingEx", "DoThingAsync", "DoThingExAsync"} {
		doc, ok := docFor(t, comments, name)
		if !ok {
			t.Fatalf("expected entry %q, got none; entries: %+v", name, comments.Funcs)
		}
		if doc != "DoThing does the thing." {
			t.Fatalf("doc for %q = %q, want %q", name, doc, "DoThing does the thing.")
		}
	}
}

func TestExtractFuncCommentsUnexportedFunctionSkipped(t *testing.T) {
	dir := writeSampleFile(t)

	comments, err := extractFuncComments(dir)
	if err != nil {
		t.Fatalf("extractFuncComments() error = %v", err)
	}

	if _, ok := docFor(t, comments, "unexported"); ok {
		t.Fatalf("did not expect an entry for unexported function")
	}
}

func TestExtractFuncCommentsExportedMethod(t *testing.T) {
	dir := writeSampleFile(t)

	comments, err := extractFuncComments(dir)
	if err != nil {
		t.Fatalf("extractFuncComments() error = %v", err)
	}

	for _, name := range []string{"Widget.Build", "Widget.BuildEx", "Widget.BuildAsync", "Widget.BuildExAsync"} {
		doc, ok := docFor(t, comments, name)
		if !ok {
			t.Fatalf("expected entry %q, got none; entries: %+v", name, comments.Funcs)
		}
		if doc != "Build builds the widget." {
			t.Fatalf("doc for %q = %q, want %q", name, doc, "Build builds the widget.")
		}
	}

	if _, ok := docFor(t, comments, "Widget.unexportedMethod"); ok {
		t.Fatalf("did not expect an entry for unexported method")
	}
}

func TestExtractFuncCommentsInterfaceMethod(t *testing.T) {
	dir := writeSampleFile(t)

	comments, err := extractFuncComments(dir)
	if err != nil {
		t.Fatalf("extractFuncComments() error = %v", err)
	}

	for _, name := range []string{"RunnerRun", "RunnerRunEx", "RunnerRunInterface", "RunnerRunAsync", "RunnerRunExAsync"} {
		doc, ok := docFor(t, comments, name)
		if !ok {
			t.Fatalf("expected entry %q, got none; entries: %+v", name, comments.Funcs)
		}
		if doc != "Run runs it." {
			t.Fatalf("doc for %q = %q, want %q", name, doc, "Run runs it.")
		}
	}
}
