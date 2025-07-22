# 钓鱼系统设置说明

## 概述
这个钓鱼系统允许玩家走到指定位置并按下F键来启动钓鱼小游戏。

## 组件说明

### 1. FishingPromptTrigger (钓鱼触发器)
- **功能**: 检测玩家进入钓鱼区域，处理F键输入
- **位置**: 需要添加到钓鱼区域的GameObject上
- **必需组件**: Collider (设置为IsTrigger)

### 2. FishingMiniGameCopy (钓鱼小游戏)
- **功能**: 钓鱼小游戏逻辑
- **位置**: 钓鱼游戏UI的根GameObject上

## 设置步骤

### 步骤1: 设置钓鱼区域
1. 在场景中创建一个空GameObject，命名为"FishingZone"
2. 添加Collider组件，设置为IsTrigger
3. 调整Collider大小覆盖钓鱼区域
4. 添加FishingPromptTrigger脚本

### 步骤2: 设置UI组件
1. 创建钓鱼提示UI (显示"按F键钓鱼"等提示)
2. 创建钓鱼游戏UI (包含FishingMiniGameCopy脚本)
3. 在FishingPromptTrigger中分配UI组件:
   - Fishing Prompt UI: 钓鱼提示UI
   - Fishing Game UI: 钓鱼游戏UI

### 步骤3: 配置FishingMiniGameCopy
在钓鱼游戏UI上设置以下组件:
- Bar Background: 背景条
- Catch Bar: 玩家控制条  
- Fish Icon: 鱼图标
- Catch Progress Bar: 进度条

## 操作说明

### 玩家操作
- **进入钓鱼区域**: 显示钓鱼提示UI
- **按F键**: 开始钓鱼小游戏
- **按ESC键**: 退出钓鱼小游戏
- **离开钓鱼区域**: 自动退出钓鱼游戏

### 游戏玩法
- **鼠标左键**: 控制钓鱼条向右移动
- **松开鼠标**: 钓鱼条向左移动
- **目标**: 让钓鱼条覆盖鱼的位置来增加进度

## 调试功能
- 在FishingPromptTrigger中启用"Enable Debug Logs"可以看到详细日志
- 所有关键事件都会在Console中显示

## 注意事项
1. 确保玩家GameObject有"Player"标签
2. 钓鱼区域必须有Collider组件且设置为IsTrigger
3. 所有UI组件必须在Inspector中正确分配
4. 钓鱼游戏UI初始时应该是隐藏的

## 自定义事件
FishingPromptTrigger提供了以下事件:
- On Player Enter Fishing Zone: 玩家进入钓鱼区域
- On Player Exit Fishing Zone: 玩家离开钓鱼区域  
- On Fishing Game Start: 钓鱼游戏开始
- On Fishing Game End: 钓鱼游戏结束

你可以在Inspector中为这些事件添加自定义行为。 