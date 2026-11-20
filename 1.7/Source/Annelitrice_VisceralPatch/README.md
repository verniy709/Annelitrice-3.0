# Annelitrice × Visceral Anatomy：手臂与袖子补丁说明

此补丁用于 RimWorld 1.7。只有启用 Visceral Anatomy 时，Annelitrice 的
`LoadFolders.xml` 才会加载 `LoadFolders/1.7/VisceralAnatomy/Assemblies/Annelitrice_VisceralPatch.dll`。
它让 Visceral 的伤口、血迹、布料破损和渐进缺损蒙版作用于 NareisLib 单独绘制的
Annelitrice 手臂与袖子。补丁不新增伤口图片，也不直接改写 Visceral 的蒙版像素。

## 为什么需要它

Visceral 根据人物身体轮廓，在身体或服装材质上生成蒙版。Annelitrice 的手臂和
袖子是 NareisLib 的独立 `TextureLevelsToNode`：左右各一条，右侧节点把同一套
手臂图像镜像绘制。默认的身体材质处理不足以覆盖这些独立节点。直接隐藏缺失的
手臂节点还会误伤安装仿生体的情况，因此补丁仍使用 Visceral 自己计算的缺损状态。

## 处理流程

1. `Bootstrap` 启动时注册 Harmony 补丁。`IsArm` 只选中 Annelitrice 的
   `Anneli_Arm_Generic_Left/Right` 节点，以及 `isSleeve=true` 的 NareisLib 袖子节点。
2. `RecognizeArmAsBodyLayer` 扩展 Visceral 的 `IsBodyLayer` 判断，让这些节点进入
   `AfterMaterial → Variant` 的身体蒙版流程。
3. `SelectSilhouette` 读取身体与当前手臂或袖子的 192×192 轮廓，按像素取两者
   alpha 的最大值，生成仅供 Visceral 计算位置的合成轮廓。正面和背面的肩部
   轮廓额外向外扩 8 像素，帮助肩部效果触及袖子。最终可见图像仍由原手臂或
   袖子贴图裁定。相同轮廓会复用，缓存创建受锁保护。
4. `MarksForArm` 保留对应侧的肩、臂、手伤口标记，并把标记坐标转到共享贴图的
   位置。正面源图像的手臂位于画布右侧，背面位于左侧；右侧节点绘制时再由
   NareisLib 镜像。其他节点的伤口标记保持原样；没有针对任何服装或袖子的
   特殊尺寸调整。
5. `MissingKeyForArm` 从 Visceral 原有的缺损 `key` 中，只提取当前手臂的肩和
   臂两段，再映射到共享源图像所在的一侧。Visceral 原有的 `BuildMask` 和
   `CutPixels` 继续负责渐进剪切。`TornCode` 对没有绑定 `apparel` 的袖子节点
   补上布料分支，使袖子获得相应的破损效果。
6. Visceral 在排队处理和完成处理时都会检查蒙版请求是否过期。由于手臂请求
   使用了映射后的 `key`，`KeepArmMaskRequestsCurrent` 在 `ProcessNext` 与 `Stale`
   两处把当前人物的 `key` 做同样的映射后再比较。新伤情仍会使旧请求失效。
7. `PreserveArmAnimation` 把 Visceral 在 `Early`、`FrameFull`、
   `BreastOvalsFor` 中的整棵渲染树刷新改为 `RefreshMaterials`。一般伤口变化只
   重新缓存材质，避免 NareisLib 的手臂动作不断重启；缺失部位集合变化时才
   重建一次渲染树，以便残肢、眼部等条件节点更新。这个方法虽然挂在
   Visceral 的通用刷新入口上，但遇到非 Annelitrice 人物会直接调用原来的
   `SetAllGraphicsDirty()`，不会改变其他种族的刷新行为。

Harmony 的挂钩位置在 Visceral 的通用方法中，因此这些方法会经过补丁检查；
实际改动由 `IsArm`/`IsSleeve` 的 Annelitrice 种族判断和补丁自己生成的
`AnneliVisceralArmSilhouette_` 轮廓标识限定。其他种族，以及 Annelitrice
身上非独立手臂或袖子的节点，收到的伤口标记、缺损 `key` 与身体轮廓
保持原值；其他种族的刷新调用也保持原样。

Visceral 的六个缺损半字节按 `左肩、左臂、左腿、右肩、右臂、右腿` 排列。
例如右臂节点取第 4、5 个半字节（从零计数为 3、4），放到共享源图像的
肩与臂位置；另侧的缺损不会剪切这条手臂。Visceral 在形成原始 `key` 时已
处理仿生体替换，补丁沿用其结果。

核心映射等价于：

```csharp
int from = right ? 3 : 0;
int shoulder = (key >> (from * 4)) & 15;
int arm = (key >> ((from + 1) * 4)) & 15;
int mappedKey = shoulder | (arm << 4);
```

`Variant` 把 `mappedKey` 写进蒙版请求；`ProcessNext` 和 `Stale` 用当前
人物的 `key` 再执行一次同样的映射，以验证请求是否仍有效。原始 `key` 本身
不会被修改。

## 文件与构建

- `ArmWoundsPatch.cs`：全部运行逻辑，按上面的处理阶段注释。
- `Annelitrice_VisceralPatch.csproj`：编译项目；引用 RimWorld、Harmony、
  NareisLib、Visceral Anatomy 与 Annelitrice 的现有程序集。
- `README.md`：本说明。
- `LoadFolders/1.7/VisceralAnatomy/Assemblies/Annelitrice_VisceralPatch.dll`：
  游戏实际加载的程序集。`bin/`、`obj/`、`.vs/` 只是构建缓存。

在 `1.7/Source/Annelitrice_VisceralPatch` 运行 `dotnet build -c Release`。
如需把新编译的 DLL 安装到上述可选加载目录，运行
`dotnet build -c Release -p:InstallOnBuild=true`。

## 审计边界

补丁通过 Harmony 的 IL 锚点接入 Visceral 的私有方法，并依赖其 192×192
蒙版格式和缺损 `key` 布局。Visceral 更新后需重新编译、检查锚点并在游戏中
复测。合成轮廓缓存在本次游戏进程内；大量不同动作贴图可能增加内存占用。
本补丁只处理指定种族与 NareisLib 的手臂、袖子节点。Visceral 自身的中文
部位标签问题不在补丁范围内。

建议复测：任意带 `isSleeve` 标记服装的双肩与双臂新伤、正背侧四个朝向、
缺臂和缺肩、仿生体替换、快速恢复伤口时的动作，以及不穿衣服的手臂。
