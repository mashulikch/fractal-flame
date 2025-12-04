#!/usr/bin/env bash
set -euo pipefail

if [[ $# -lt 1 ]]; then
  echo "Usage: $0 path/to/FractalFlame.dll"
  exit 1
fi

DLL_PATH="$1"

if [[ ! -f "$DLL_PATH" ]]; then
  echo "[ERROR] DLL not found: $DLL_PATH"
  exit 1
fi

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

echo "=== Running basic functionality test ==="
bash "$SCRIPT_DIR/cases/test_basic_functionality.sh" "$DLL_PATH"

echo "=== Running image properties test ==="
bash "$SCRIPT_DIR/cases/test_image_properties.sh" "$DLL_PATH"

echo "=== Running multithreading performance test ==="
bash "$SCRIPT_DIR/cases/test_multithreading_performance.sh" "$DLL_PATH"

echo "=== All tests finished successfully ==="
