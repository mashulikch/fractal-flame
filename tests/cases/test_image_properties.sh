#!/bin/sh

echo "Testing image properties..."

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

OUTPUT_IMAGE="test_output.png"
ARGS="--seed 12.345 -w 640 -h 480 -i 20000  -f swirl:1.0,horseshoe:0.8,handk:1.0,spherical:1.0,exp:0.5,bubble:0.7 -ap 0.9,0.4,0,-0.35,0.85,0.4 -o $OUTPUT_IMAGE -t 200 --gamma 2 -s 5"

# Генерация тестового изображения, если оно не существует
if [ ! -f "$OUTPUT_IMAGE" ]; then
    echo "Generating test image..."
    echo "Running: dotnet $DLL_PATH $ARGS"
    dotnet "$DLL_PATH" $ARGS
    if [ $? -ne 0 ]; then
        echo "✗ Failed to generate test image"
        exit 1
    fi
fi

# Проверка, существует ли файл
if [ ! -f "$OUTPUT_IMAGE" ]; then
    echo "✗ Image file '$OUTPUT_IMAGE' does not exist"
    exit 1
fi

# Проверка расширения файла
case "$OUTPUT_IMAGE" in
    *.png)
        echo "✓ Image file has .png extension"
        ;;
    *)
        echo "✗ Image file does not have .png extension"
        exit 1
        ;;
esac

# Проверка, имеет ли файл содержимое (ненулевой размер)
FILE_SIZE=$(stat -c%s "$OUTPUT_IMAGE" 2>/dev/null || stat -f%z "$OUTPUT_IMAGE" 2>/dev/null)
if [ "$FILE_SIZE" -gt 0 ]; then
    echo "✓ Image file has content (size: $FILE_SIZE bytes)"
else
    echo "✗ Image file is empty"
    exit 1
fi

# Проверка сигнатуры PNG
PNG_SIGNATURE=$(dd if="$OUTPUT_IMAGE" bs=8 count=1 2>/dev/null | xxd -p)
if [ "$PNG_SIGNATURE" = "89504e470d0a1a0a" ]; then
    echo "✓ Image file has valid PNG signature"
else
    echo "✗ Image file does not have valid PNG signature"
    exit 1
fi

echo "Image properties test passed!"
