#!/bin/bash

OLD_PWD=$(pwd)
SHELL_FOLDER=$(
    cd "$(dirname "$0")" || exit
    pwd
)
PROJECT_FOLDER=$SHELL_FOLDER/../..

cd "$SHELL_FOLDER" || exit >/dev/null 2>&1

# shellcheck source=/dev/null
source "$PROJECT_FOLDER/scripts/base/env.sh"


echo "Creating release on GitHub..."

VERSION="v$(go run main-version.go)"

# shellcheck disable=SC2034
TITLE="Release $VERSION"

# shellcheck disable=SC2034
EXISTING_RELEASE=$(gh release view "$VERSION" 2>/dev/null)
# shellcheck disable=SC2181
if [ $? -eq 0 ]; then
    echo "Release $VERSION already exists. Aborting."
    exit 1
fi

check_gh_exist

RELEASE_DIR="$PROJECT_FOLDER/release"

NOTES_FILE="$PROJECT_FOLDER/release_notes.md"

if [ ! -f "$NOTES_FILE" ]; then
    echo "Error: Release notes file '$NOTES_FILE' not found."
    exit 1
fi

COMMAND="gh release create $VERSION --title \"$TITLE\" --notes-file \"$NOTES_FILE\" $RELEASE_DIR/*"
echo exec: "$COMMAND"
if eval "$COMMAND"; then
    echo "Release $VERSION successfully created and binaries uploaded."
else
    echo "Failed to create GitHub release."
    exit 1
fi

cd "$OLD_PWD" || exit >/dev/null 2>&1
