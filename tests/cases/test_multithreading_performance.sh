#!/bin/sh

echo "Testing multithreading performance..."

if [ -z "$1" ]; then
    echo "Usage: $0 path/to/Flames.dll"
    exit 1
fi

DLL_PATH="$1"

if [ ! -f "$DLL_PATH" ]; then
    echo "✗ DLL file '$DLL_PATH' does not exist."
    exit 1
fi

OUT_DIR="."
RESULTS_CSV="$OUT_DIR/performance_results.csv"

# Инициализация файла с результатами
echo "threads,duration_seconds" > "$RESULTS_CSV"

measure_time() {
    threads=$1
    output_file="$OUT_DIR/test_output_${threads}_threads.png"

    echo "Running with $threads threads..."

    START_TIME=$(date +%s)
    dotnet "$DLL_PATH" -- -w 3840 -h 2160 -i 20000000 --seed 12.345 \
        -f swirl:25,horseshoe:20,handk:25,spherical:25,exp:46,bubble:100 \
        -ap 0.6,0.5,0,-0.25,0.55,0.55 \
        -t "$threads" -g true --gamma 1.5 -s 2

    EXIT_CODE=$?
    END_TIME=$(date +%s)

    DURATION=$((END_TIME - START_TIME))

    if [ $EXIT_CODE -eq 0 ]; then
        echo "✓ Completed with $threads threads in ${DURATION} seconds"
        echo "$threads,$DURATION" >> "$RESULTS_CSV"
        return 0
    else
        echo "✗ Failed with $threads threads (exit code: $EXIT_CODE)"
        return 1
    fi
}

# 1, 2, 4, 8 потоков по ТЗ
for threads in 1 2 4 8; do
    if ! measure_time "$threads"; then
        echo "Performance test failed for $threads threads"
        exit 1
    fi
done

echo ""
echo "Performance test results:"
echo "------------------------"
cat "$RESULTS_CSV"
echo ""
echo "Multithreading performance test completed!"
