# Crystle Note Editor [![license](https://img.shields.io/badge/license-MIT-green.svg?style=flat-square)](https://github.com/Bnao-zh/CrystleNoteEditor/blob/main/LICENSE) 

Crystle的自制谱面编辑器
支持wav与ogg格式.
支持WebSocket获取信息.

![screenshot2](https://github.com/Bnao-zh/CrystleNoteEditor/blob/main/screenshot2.png?raw=true)


## 操作列表

### 一般操作

| 操作  | 按键       |
|:--- |:-------- |
| 撤销  | Ctrl + Z |
| 重做  | Ctrl + Y |
| 保存  | Ctrl + S |

### 谱面操作

| 操作       | 命令                    |
|:-------- |:--------------------- |
| 放大缩小网格间隔 | 上下箭头键 / Ctrl + 拖动鼠标滚轮 |
| 快速移动当前位置 | Ctrl + 左右箭头键          |
| 播放/暂停    | Space                 |
| 全选所有note | Ctrl + A              |
| 范围选择     | Ctrl + 在铺面区域拖动鼠标滚轮    |
| 复制       | Ctrl + C              |
| 剪切       | Ctrl + X              |
| 删除       | Delete / Back space   |
| 粘贴       | Ctrl + V              |
| 查看note时间 | 对note/节拍线按鼠标右键        |

### note编辑快捷键

| 操作        | 命令          |
|:--------- |:----------- |
| note样式切换  | Alt         |
| 编辑长note   | Shift + Alt |
| 取消编辑长note | 右键 / Esc    |

## WebSocket服务器
启动制谱器时会自动监听`ws://localhost:4649/SimCrySocket`
连接上后发送消息获取对应内容

### 发送的消息

| 发送消息        | 服务端返回       |
|:--------- |:----------- |
| {type:"getchart"}  | 当前正在编辑的谱面数据  |
| {type:"getinfo"}   | 当前所在的时间或其他信息 |

## 开发环境

Unity 2022.3.40f1c1

## LICENSE

[MIT](https://github.com/Bnao-zh/CrystleNoteEditor/blob/main/LICENSE)
