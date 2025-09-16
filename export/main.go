package main

import (
	packageVersion "code-comments-sync/version"
	"encoding/json"
	"fmt"
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
	Funcs []FuncComment `json:"funcs"`
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
							Doc:  f.Doc,
						})
					}
				}

				for _, t := range docPkg.Types {
					for _, m := range t.Methods {
						if isExported(t.Name) && isExported(m.Name) && len(m.Doc) > 0 {
							funcComments.Funcs = append(funcComments.Funcs, FuncComment{
								Name: fmt.Sprintf("%s.%s", t.Name, m.Name),
								Doc:  m.Doc,
							})
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
