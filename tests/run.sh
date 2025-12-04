#!/bin/sh

CASES_DIR="$1"
DLL_PATH="$2"

# Проверяем аргументы
if [ -z "$CASES_DIR" ] || [ -z "$DLL_PATH" ]; then
    echo "Usage: $0 <cases_dir> <path/to/Flames.dll>"
    exit 1
fi

# Проверяем, что директория с тестами существует
if [ ! -d "$CASES_DIR" ]; then
    echo "[ERROR] Cases directory not found: $CASES_DIR"
    exit 1
fi

# Проверяем, что DLL существует
if [ ! -f "$DLL_PATH" ]; then
    echo "[ERROR] DLL not found: $DLL_PATH"
    exit 1
fi

echo "Using test cases from: $CASES_DIR"
echo "Using DLL: $DLL_PATH"

for script in "$CASES_DIR"/*.sh; do
    if [ -x "$script" ]; then
        echo ""
        echo "===== Running $(basename "$script") ====="
        "$script" "$DLL_PATH"
        EXIT_CODE=$?
        if [ $EXIT_CODE -ne 0 ]; then
            echo "✗ $(basename "$script") failed with exit code $EXIT_CODE"
            exit $EXIT_CODE
        fi
    else
        echo "Skipping non-executable file: $script"
    fi
done

echo ""
echo "All bash tests completed successfully."
