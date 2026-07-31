#!/usr/bin/env bash

set -e

VERSION="1.0.7"

SCRIPT_DIR=$(cd -- "$(dirname -- "$0")" && pwd)

dotnet pack LibMSPSharp/LibMSPSharp.csproj -c Release -v:detailed /p:Version="${VERSION}"


# Should have an active github classic token with "package write" permission
dotnet nuget push "${SCRIPT_DIR}/LibMSPSharp/bin/Release/slafniy.LibMSPSharp.${VERSION}.nupkg" \
  --source "https://nuget.pkg.github.com/slafniy/index.json" \
  --api-key $(secret-tool lookup github nuget)