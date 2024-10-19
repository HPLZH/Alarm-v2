# Alarm for PC

这是一个闹钟

## 功能
- 使用随机的本地音乐作为铃声
- 使用 HTTP 获取闹钟的状态并控制闹钟
- 降低最近播放过的音乐再次出现的概率 (PB)
- 降低来自相近目录的音乐连续出现的概率 (PL)
- 使用指定设备播放
- 调节闹钟音量并在关闭后恢复
- 使用配置文件进行配置
- 在随机播放序列前插入额外的序列

## 使用指南

[使用指南-命令行参数](Guide.md)
[使用指南-配置文件](Guide-ConfigFile.md)

内置帮助：
```pwsh
Alarm --help
Amarm run --help
Alarm start --help
Alarm mkconf --help
Alarm rc --help
Alarm show --help
```

## 注意事项

此软件的工作目录固定为可执行文件所在目录 `Path.GetDirectoryName(Environment.ProcessPath)`
