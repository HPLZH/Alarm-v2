# 配置文件使用指南

## 通过配置文件启动
```
Alarm start <config-file>

config-file: 配置文件路径
             配置文件是 UTF-8 编码的 JSON 文件
```

## 将命令行参数转化为配置文件

将启动命令的 `run` 命令替换为 `mkconf` 命令，即可在控制台输出对应的配置文件内容

配置文件的各字段对应各命令行参数（`opts` `shell` `extra` 除外）

## 实验性选项

`opts` 字段中是关于实验性选项的配置

参见[实验性选项](ExperimentalOptions.md)

## 额外序列功能

### 概述

额外序列是插入到随机播放序列前的、在程序开始运行时确定的一段播放序列

这一段播放序列可以由多个部分拼接而成，每个部分的条件与内容均可通过程序生成

此功能必须通过配置文件启用

### 配置 Shell

配置文件的 `shell` 字段用于配置使用的 shell 程序

在 Windows 上，推荐使用 Powershell:
```json
// Powershell 7
"shell": 
{
    "file": "pwsh",
    "args": [
        "-NoProfile",
        "-Command",
        "$0"
    ]
}
```

```json
// Windows Powershell 5.1
"shell": 
{
    "file": "powershell",
    "args": [
        "-NoProfile",
        "-Command",
        "$0"
    ]
}
```

#### `file`

`file` 指定 shell 程序的文件名

由于通常你的 shell 程序在 PATH 环境变量的目录下，因此只需要输入名称，而不需要完整路径

#### `args`

`args` 指定 shell 的参数列表

其中 `$0` 项代替输入的命令

### 额外内容列表

`extra` 字段指定额外内容的列表

额外内容列表的各处均为从上到下计算

```json
"extra": 
[
    {
        // if 可以省略，省略时直接认为条件成立
        "if": 
        {
            "command": "(Get-Random % 2) -eq 0",
            // input 为 null 时通常省略
            "input": null
        },
        "content":
        [
            // 字符串: 直接使用文件
            "path1",
            "path2",
            // Command 对象: 使用程序生成
            {
                "command": "",
                // input 为 null 时通常省略
                "input": null
            },
            // ......
        ],
        // break 为 false 时通常省略
        "break": false
    },
    // ......
]
```

#### `if`

一个 Command 对象，由 `command` `input` 两个字段组成

`command` 是替换 `$0` 在 shell 中执行的命令

`input` 是输入到 stdin 中的内容，没有则省略

最终，闹钟程序会读取 shell 输出的内容，并判断结果是否为 true

整数结果 0 会被视为 false ，非零整数则被视为 true

#### `content`

内容列表，其中字符串项会被直接作为路径使用

列表中的 Command 对象会被 shell 执行，输出的每一行结果都是一个路径

#### `break`

含义：如果为真，则停止计算后续项目
