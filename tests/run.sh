#!/bin/sh

# Цвета для вывода
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[0;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Директория, переданная в качестве аргумента
TARGET_DIR="$1"
DLL_PATH="$2"

# Переменные для подсчета тестов
total_tests=0
successful_tests=0

# Перебираем все .sh файлы в указанной директории
echo "${BLUE}Ищем все .sh файлы в директории: $TARGET_DIR${NC}"

for script in "$TARGET_DIR"/*.sh; do
    # Проверяем, существует ли файл
    if [ -f "$script" ]; then
        total_tests=$((total_tests + 1))
        echo "${YELLOW}Запускаем: $script${NC}"
        # Делаем файл исполняемым (на случай, если он не имеет прав на выполнение)
        chmod +x "$script"
        # Запускаем файл
        sh "$script" "$DLL_PATH"
        # Проверяем код возврата
        if [ $? -ne 0 ]; then
            echo "${RED}Ошибка при выполнении $script${NC}"
        else
            echo "${GREEN}$script выполнен успешно${NC}"
            successful_tests=$((successful_tests + 1))
        fi
    else
        echo "${RED}Файл $script не найден или не является исполняемым${NC}"
    fi
done

# Вывод результатов
echo ""
echo "${BLUE}Результаты тестирования:${NC}"
echo "Всего тестов: $total_tests"
echo "Успешных тестов: $successful_tests"

# Проверяем, было ли выполнено ровно 3 теста
if [ "$total_tests" -ne 3 ]; then
    echo "${RED}Ошибка: Ожидалось 3 теста, но выполнено $total_tests.${NC}"
    exit 1
fi

# Если все тесты успешны
if [ "$total_tests" -eq "$successful_tests" ]; then
    echo "${GREEN}Все тесты успешно пройдены!${NC}"
    exit 0
else
    echo "${RED}Некоторые тесты завершились с ошибками.${NC}"
    exit 1
fi
