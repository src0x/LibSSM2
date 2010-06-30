#!/bin/bash

# Clean up source code or text files a bit in order
# to avoid unnecessary software version control differences.

# pass file list via variable FILES
function CLEAN() {
  declare -i COUNT=0
  for FILE in $FILES; do
    let COUNT++
  done

  declare -i NUM=0
  for FILE in $FILES; do
    let NUM++
    TMPFILE="$FILE.tmp"

    echo -n "$NUM/$COUNT: $FILE    "

    # remove trailing white space (spaces, tabs)
    # must use temp file since redirecting into input won't work
    sed -r 's/[[:blank:]]+$//' $FILE > $TMPFILE && mv $TMPFILE $FILE && echo "OK"
  done
}

declare FILES

# find all C# files and clean them
FILES=$(find . -type f -name '*.cs' )
CLEAN

# same for text files
FILES=$(find . -type f -name '*.txt' )
CLEAN
