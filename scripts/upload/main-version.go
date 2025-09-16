package main

import (
	packageVersion "code-comments-sync/version"
	"fmt"
)

func main() {
	fmt.Printf("%v\n", packageVersion.Version)
}
