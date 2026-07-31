#!/usr/bin/env bash

# builds nuget locally

set -e

SCRIPT_DIR=$(cd -- "$(dirname -- "$0")" && pwd)

dotnet pack LibMSPSharp/LibMSPSharp.csproj -c Release -v:detailed
