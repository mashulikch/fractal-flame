#!/bin/sh

echo "Testing basic functionality..."

# Проверка аргумента
if [ -z "$1" ]; then
    echo "Usage: $0 path/to/Flames.dll"
    exit 1
fi

DLL_PATH="$1"

if [ ! -f "$DLL_PATH" ]; then
    echo "✗ DLL file '$DLL_PATH' does not exist"
    exit 1
fi

OUT_DIR="test_output"
mkdir -p "$OUT_DIR"

OUTPUT_IMAGE="$OUT_DIR/basic_test_output.png"
ARGS=" -w 800 -h 600 -i 20000 --seed 12.345 -f swirl:10,horseshoe:0.8,handk:1,spherical:1,exp:0.5,bubble:0.7 -ap 0.6,0.5,0,-0.25,0.55,0.55 -t 200 -g true --gamma 1.5  -s 10 -o $OUTPUT_IMAGE"

echo "Running: dotnet $DLL_PATH $ARGS"
dotnet "$DLL_PATH" $ARGS
EXIT_CODE=$?

if [ $EXIT_CODE -eq 0 ]; then
    echo "✓ Application exited successfully (exit code: $EXIT_CODE)"
else
    echo "✗ Application failed with exit code: $EXIT_CODE"
    exit 1
fi

if [ -f "$OUTPUT_IMAGE" ]; then
    echo "✓ Image file '$OUTPUT_IMAGE' was created"
else
    echo "✗ Image file '$OUTPUT_IMAGE' was not created"
    exit 1
fi

echo "Basic functionality test passed!"
