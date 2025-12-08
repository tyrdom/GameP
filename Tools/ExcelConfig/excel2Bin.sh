#!/usr/bin/env bash

# 设置工作目录（注意Linux路径格式）
WORKSPACE="../../"

# 定义路径变量
LUBAN_DLL="${WORKSPACE}/Tools/ExcelConfig/Luban/Luban.dll"
CONF_ROOT="${WORKSPACE}/Tools/ExcelConfig"

# 执行Luban工具命令
dotnet "$LUBAN_DLL" \
    -t all \
    -c cs-bin \
    -d bin \
    --conf "${CONF_ROOT}/luban.conf" \
    -x "outputCodeDir=${WORKSPACE}/Project/Assets/Scripts/Configs/AutoGen" \
    -x "outputDataDir=${WORKSPACE}/Project/Assets/Resources/ConfigsExcel" \
    -x "pathValidator.rootDir=${WORKSPACE}/Project" \
    -x "l10n.textProviderFile=${WORKSPACE}/Tools/ExcelConfig/l10n/texts.json" \
    -x "l10n.input_text_files=${WORKSPACE}/Tools/ExcelConfig/l10n/text.xlsx" \
    -x "l10n.output_not_translated_text_file=${WORKSPACE}/Tools/ExcelConfig/l10n/NotLocalized.txt"
