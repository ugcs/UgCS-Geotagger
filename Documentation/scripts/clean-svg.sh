#!/bin/bash

set -e
cd "$(dirname "$(readlink -f "${0}")")/../src/img/"

# Remove absoluthe paths starting with `/home/`
find . -name "*.svg" | xargs sed -i -E "s@/home/(.*/)+@@g"

