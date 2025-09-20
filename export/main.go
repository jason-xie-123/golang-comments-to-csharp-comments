package main

import (
	packageVersion "code-comments-sync/version"
	"encoding/json"
	"fmt"
	"go/ast"
	"go/doc"
	"go/parser"
	"go/token"
	"os"
	"path/filepath"
	"strings"
	"unicode"

	"github.com/urfave/cli/v2"
)

type FuncComment struct {
	Name string `json:"name"`
	Doc  string `json:"doc"`
}

type FuncComments struct {
	Funcs []FuncComment `json:"funComments"`
}

func main() {
	AppName := "comments-sync-golang-export"

	app := &cli.App{
		Name:    AppName,
		Usage:   "CLI Tool to sync golang comments into c# code",
		Version: packageVersion.Version,
		Flags: []cli.Flag{
			&cli.StringFlag{
				Name:  "go-folder",
				Usage: "golang folder",
			},
			&cli.StringFlag{
				Name:  "output-json-file",
				Usage: "output json file",
			},
		},
		Action: func(c *cli.Context) error {
			golangFolder := c.String("go-folder")
			outputJsonFile := c.String("output-json-file")

			fileSet := token.NewFileSet()

			packages, err := parser.ParseDir(fileSet, golangFolder, func(fi os.FileInfo) bool {
				return !fi.IsDir() &&
					filepath.Ext(fi.Name()) == ".go" &&
					!strings.HasSuffix(fi.Name(), "_test.go")
			}, parser.ParseComments)
			if err != nil {
				return err
			}

			var funcComments FuncComments = FuncComments{
				Funcs: make([]FuncComment, 0),
			}
			for _, pkg := range packages {
				docPkg := doc.New(pkg, "./", doc.AllDecls)

				for _, f := range docPkg.Funcs {
					if isExported(f.Name) && len(f.Doc) > 0 {
						funcComments.Funcs = append(funcComments.Funcs, FuncComment{
							Name: f.Name,
							Doc:  strings.TrimRight(f.Doc, "\r\n"),
						})
						funcComments.Funcs = append(funcComments.Funcs, FuncComment{
							Name: fmt.Sprintf("%sEx", f.Name),
							Doc:  strings.TrimRight(f.Doc, "\r\n"),
						})
						funcComments.Funcs = append(funcComments.Funcs, FuncComment{
							Name: fmt.Sprintf("%sAsync", f.Name),
							Doc:  strings.TrimRight(f.Doc, "\r\n"),
						})
					}
				}

				for _, t := range docPkg.Types {
					for _, m := range t.Methods {
						if isExported(t.Name) && isExported(m.Name) && len(m.Doc) > 0 {
							funcComments.Funcs = append(funcComments.Funcs, FuncComment{
								Name: fmt.Sprintf("%s.%s", t.Name, m.Name),
								Doc:  strings.TrimRight(m.Doc, "\r\n"),
							})
							funcComments.Funcs = append(funcComments.Funcs, FuncComment{
								Name: fmt.Sprintf("%s.%sEx", t.Name, m.Name),
								Doc:  strings.TrimRight(m.Doc, "\r\n"),
							})
							funcComments.Funcs = append(funcComments.Funcs, FuncComment{
								Name: fmt.Sprintf("%s.%sAsync", t.Name, m.Name),
								Doc:  strings.TrimRight(m.Doc, "\r\n"),
							})
						}
					}

					for _, s := range t.Decl.Specs {
						if iFace, ok := s.(*ast.TypeSpec).Type.(*ast.InterfaceType); ok {
							for _, field := range iFace.Methods.List {
								for _, name := range field.Names {
									if isExported(t.Name) && isExported(name.Name) && len(field.Doc.Text()) > 0 {
										funcComments.Funcs = append(funcComments.Funcs, FuncComment{
											Name: fmt.Sprintf("%s%s", t.Name, name.Name),
											Doc:  strings.TrimRight(field.Doc.Text(), "\r\n"),
										})
										funcComments.Funcs = append(funcComments.Funcs, FuncComment{
											Name: fmt.Sprintf("%s%sEx", t.Name, name.Name),
											Doc:  strings.TrimRight(field.Doc.Text(), "\r\n"),
										})
										funcComments.Funcs = append(funcComments.Funcs, FuncComment{
											Name: fmt.Sprintf("%s%sInterface", t.Name, name.Name),
											Doc:  strings.TrimRight(field.Doc.Text(), "\r\n"),
										})
										funcComments.Funcs = append(funcComments.Funcs, FuncComment{
											Name: fmt.Sprintf("%s%sAsync", t.Name, name.Name),
											Doc:  strings.TrimRight(field.Doc.Text(), "\r\n"),
										})
									}
								}
							}
						}
					}

				}
			}

			jsonData, err := json.Marshal(funcComments)
			if err != nil {
				return err
			}
			err = os.WriteFile(outputJsonFile, jsonData, 0644)
			if err != nil {
				return err
			}

			return nil
		},
	}

	err := app.Run(os.Args)
	if err != nil {
		os.Exit(1)
	}
}

func isExported(name string) bool {
	if name == "" {
		return false
	}

	return unicode.IsUpper(rune(name[0]))
}
