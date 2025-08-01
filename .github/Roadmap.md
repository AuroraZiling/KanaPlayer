# KanaPlayer Roadmap

## Alpha 计划 (~ 1.0.0)

Alpha 阶段的目标是实现**基本的功能和用户体验**，确保播放器能够正常运行并提供基础的音频播放功能。

- [x] 音频播放
  - [x] 播放行为控制
  - [x] 音量控制
  - [x] 音频元数据联动
- [x] 用户
  - [x] 二维码登录
  - [x] 用户信息获取
  - [x] 登录态刷新 *待验证*
- [x] 历史记录
- [x] 收藏夹
  - [x] 本地歌单
    - [x] 歌单管理 *新建/删除/重命名*
    - [x] 歌曲管理 *从B站链接添加/删除*
    - [x] 与播放列表联动 *播放全部/添加*
  - [x] B站收藏夹/合集
    - [x] 用户收藏夹/合集列表
    - [x] 导入 / 同步 / 删除
    - [x] 与播放列表联动 *播放全部/添加*
    - [x] 导入 / 同步时去除失效歌曲
- [x] 首页
  - [x] 音乐分区滚动推送
- [x] 播放列表
- [x] 缓存
  - [x] 音频缓存
  - [x] 图片缓存
  - [x] 音频元数据缓存
- [x] 托盘图标
  - [x] 播放/上一首/下一首/播放模式联动
  - [x] 退出
- [x] 设置
  - [x] 打开播放器的重要文件夹
  - [x] 自定义软件关闭行为
  - [x] 主页 -> 播放列表行为 *(默认) 添加并立即播放, 添加到下一首, 添加到末尾*
  - [x] 收藏夹 -> 播放列表行为 *(默认) 替换当前播放列表, 添加并立即播放, 添加到下一首, 添加到末尾*
- [x] 日志
- [x] Toast 通知
- [x] Dialog 弹窗

## Beta 计划 (~ 2.0.0)

Beta 阶段着重于**提升用户体验和功能的稳定性**，确保播放器在大多数情况下都能正常工作。

- [ ] 任务栏缩略图工具栏 歌曲信息与操作 *Windows Only*
- [ ] WebView 模态登录
- [ ] 更改播放列表/本地歌单歌曲顺序
- [ ] 数据库操作优化
- [ ] 视频分区
- [ ] 分P视频
- [ ] 跟踪本地下载音频
- [ ] 音质选择
- [ ] 歌词页面
- [ ] 软件更新
- [ ] Bilibili API 健壮性检查