#!/bin/sh

echo "Testing basic functionality..."

# Аргументы для запуска программы
DLL_PATH="$1"
ARGS="-w 800 -h 600 -o test_output.png"

# Запуск программы
echo "Running: dotnet $DLL_PATH $ARGS"
dotnet "$DLL_PATH" $ARGS

# Проверка кода возврата
EXIT_CODE=$?
if [ $EXIT_CODE -eq 0 ]; then
    echo "✓ Application exited successfully (exit code: $EXIT_CODE)"
else
    echo "✗ Application failed with exit code: $EXIT_CODE"
    exit 1
fi

# Проверка, был ли создан файл изображения
if [ -f "test_output.png" ]; then
    echo "✓ Image file 'test_output.png' was created"
else
    echo "✗ Image file 'test_output.png' was not created"
    exit 1
fi

echo "Basic functionality test passed!"
