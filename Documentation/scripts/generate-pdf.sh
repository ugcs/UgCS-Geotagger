#!/bin/bash

set -e
cd "$(dirname "$(readlink -f "${0}")")/../src"

doker --pdf ugcs-ppk-user-manual.yaml 1> /dev/null

if [ "$1" == "-o" ]; then
  cd pdf
  find *.pdf -exec xdg-open '{}' \; &
fi
