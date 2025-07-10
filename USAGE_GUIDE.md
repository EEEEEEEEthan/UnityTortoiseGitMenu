# UnityTortoiseGitMenu 使用指南

## 安装说明

### 1. 系统要求
- Unity 2020.3.0b5 或更高版本
- Windows 操作系统
- 已安装 TortoiseGit
- 已安装 Git 命令行工具

### 2. 安装方法
1. 打开 Unity Package Manager
2. 点击左上角的 "+" 按钮
3. 选择 "Add package from git URL"
4. 输入：`https://github.com/EEEEEEEEthan/UnityTortoiseGitMenu.git`
5. 点击 "Add" 完成安装

## 主要功能

### 1. 可视化 Git 状态
安装后，Project 窗口会自动显示 Git 状态：
- **红点标记** (●)：表示文件已修改
- **提交信息**：显示文件的最后提交信息

### 2. TortoiseGit 菜单
在 Project 窗口中右键点击任意资源，可以看到 "TortoiseGit" 子菜单：

#### Git 操作类
- **Pull**：拉取远程更改
- **Push**：推送本地更改
- **Switch|CheckOut**：切换分支或检出
- **Merge**：合并分支

#### 文件操作类
- **Commit**：提交更改（支持AI生成提交信息）
- **Diff**：查看文件差异
- **ShowLog**：显示提交历史
- **Revert**：还原文件更改

#### 实用工具
- **Refresh Project (F5)**：手动刷新 Git 状态

### 3. AI 智能提交
启用 AI 功能后，提交操作会自动生成规范的提交信息。

## 配置设置

### 打开设置界面
1. 菜单栏：`Edit` → `Preferences`
2. 在左侧列表中找到 `Tortoise Git Menu`

### 显示选项
- **Mark Dirty Files**：在 Project 窗口标记已修改文件
- **Show Last Commit**：显示文件的最后提交信息

### AI 配置
- **Use AI**：启用AI生成提交信息功能
- **AI Provider**：选择AI服务提供商
  - DouBao（豆包/火山引擎）
  - DeepSeek
  - OpenAI
- **Model**：指定使用的AI模型名称
- **API Key**：输入对应服务的API密钥

### 自定义提示词
可以自定义AI生成提交信息的提示词，默认提示词为：
```
你是专业的Unity游戏开发者，现你需要对下面的git diff输出，产生一个git提交日志。
日志格式为不超过50字的一句话+回车+回车+200字以内的详细描述。
```

## 使用技巧

### 1. 快速提交工作流
1. 在 Project 窗口选择要提交的文件或文件夹
2. 右键选择 `TortoiseGit` → `Commit`
3. 如果启用了AI，系统会自动生成提交信息
4. 在TortoiseGit对话框中确认并提交

### 2. 查看文件状态
- **红点标记**：快速识别哪些文件被修改
- **提交信息**：了解文件的最后修改情况
- **使用F5**：手动刷新状态（通常自动更新）

### 3. 多仓库支持
插件会自动检测项目中的所有Git仓库，包括：
- 主项目仓库
- 子模块 (Git Submodules)
- 嵌套的独立仓库

### 4. 性能优化
- 状态检测在后台线程运行，不会卡顿Unity编辑器
- 提交信息会被缓存，减少重复的Git查询
- 只在必要时更新显示，节省系统资源

## 故障排除

### 1. 菜单不显示
**可能原因**：
- TortoiseGit未正确安装
- Git命令行工具未安装或不在PATH中

**解决方法**：
- 确保安装了TortoiseGit和Git
- 重启Unity编辑器
- 检查Windows环境变量PATH

### 2. 状态标记不更新
**解决方法**：
- 按F5手动刷新
- 检查Git仓库状态是否正常
- 重启Unity编辑器

### 3. AI功能不工作
**可能原因**：
- API Key未正确配置
- 网络连接问题
- AI服务配额不足

**解决方法**：
- 检查API Key设置
- 确认网络连接正常
- 查看Unity Console中的错误信息

### 4. 命令执行失败
**可能原因**：
- TortoiseGitProc.exe不在PATH中
- 权限不足

**解决方法**：
- 确保TortoiseGit正确安装
- 以管理员权限运行Unity
- 检查文件路径中是否包含特殊字符

## 最佳实践

### 1. 团队协作
- 统一团队的AI提示词设置
- 确保所有成员都安装了相同版本的TortoiseGit
- 使用统一的Git配置（如.gitignore文件）

### 2. 性能考虑
- 对于大型项目，考虑禁用某些可视化功能
- 定期清理Unity的临时缓存文件
- 避免在网络驱动器上使用Git仓库

### 3. AI使用建议
- 根据项目特点自定义提示词
- 定期检查生成的提交信息质量
- 保留手动编辑提交信息的习惯

## 常用快捷键

- **F5**：刷新Git状态
- **右键菜单**：快速访问TortoiseGit功能

## 支持的Git操作

### 完全支持
- Pull, Push, Commit, Diff, Log, Revert
- 分支切换和合并
- 状态查看和刷新

### 部分支持
- 复杂的合并冲突（建议使用TortoiseGit界面解决）
- Git LFS（基本支持，但无特殊显示）

### 不支持
- Git钩子配置
- 复杂的Git配置管理
- Git服务器管理

## 更新说明

插件会随Unity包管理器自动检查更新。建议定期更新以获得：
- 新功能和改进
- 性能优化
- 兼容性修复
- 安全更新

## 技术支持

如遇问题，请：
1. 查看Unity Console中的错误信息
2. 检查本文档的故障排除部分
3. 在GitHub项目页面提交Issue
4. 联系作者：tyx1993@live.cn

---

*本插件旨在提高Unity开发者的Git工作流效率，欢迎反馈和建议！*