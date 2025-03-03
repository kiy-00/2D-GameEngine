using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using GameEngine.Common;
using GameEngine.Core;
using SkiaSharp;
using Keys = GameEngine.Core.Keys;
using Time = GameEngine.Core.Time;
using WinFormsTimer = System.Windows.Forms.Timer;
using System.Drawing;

namespace GameEngineTest
{
    public class GameEngineTest : Form
    {
        private GameLoop gameLoop;
        private World world;
        private Renderer renderer;
        private InputManager inputManager;
        private SceneManager sceneManager;
        private Scene mainScene;
        private Entity cameraEntity;
        private Entity playerEntity;
        private List<Entity> testEntities = new List<Entity>();
        private SKBitmap renderBitmap;
        private bool isInitialized = false;
        private bool showDebugInfo = true;
        private int currentTestIndex = 0;
        private string[] testNames = {
            "基础移动测试",
            "精灵渲染测试",
            "场景切换测试",
            "输入响应测试",
            "相机跟随测试"
        };

        // 添加此属性以减少闪烁
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000; // WS_EX_COMPOSITED 标志
                return cp;
            }
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new GameEngineTest());
        }

        public GameEngineTest()
        {
            // 窗体设置
            Text = "GameEngine 综合测试程序";
            Size = new System.Drawing.Size(1024, 768);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            DoubleBuffered = true;

            // 初始化引擎
            InitializeEngine();

            // 创建测试场景
            CreateMainScene();

            // 创建渲染位图
            renderBitmap = new SKBitmap(ClientSize.Width, ClientSize.Height);

            // 绑定事件
            Paint += GameEngineTest_Paint;
            KeyDown += GameEngineTest_KeyDown;
            KeyUp += GameEngineTest_KeyUp;
            MouseMove += GameEngineTest_MouseMove;
            MouseDown += GameEngineTest_MouseDown;
            MouseUp += GameEngineTest_MouseUp;
            FormClosed += GameEngineTest_FormClosed;
            Resize += GameEngineTest_Resize;

            // 在这里添加调试代码
            KeyDown += (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine($"键盘按下: {e.KeyCode}");
            };

            // 创建重绘计时器
            WinFormsTimer timer = new WinFormsTimer();
            timer.Interval = 16; // 约60 FPS
            timer.Tick += (s, e) =>
            {
                // 只在需要时重绘
                if (renderBitmap != null && !renderBitmap.IsEmpty)
                    Invalidate(false); // false 参数表示不立即验证客户区域
            };
            timer.Start();

            isInitialized = true;
            gameLoop.Start();

            // 记录启动
            System.Diagnostics.Debug.WriteLine("测试程序已启动");
        }

        private void InitializeEngine()
        {
            // 游戏循环
            gameLoop = new GameLoop();
            gameLoop.TargetElapsedTime = 1.0f / 60.0f;

            // 游戏世界
            world = new World("TestWorld");

            // 渲染器
            renderer = new Renderer();
            world.AddSystem(renderer);

            // 输入管理器
            inputManager = new InputManager();
            inputManager.Initialize();
            world.AddSystem(inputManager);

            // 场景管理器
            // 场景管理器
            sceneManager = SceneManager.Instance;
            sceneManager.SetEnableUpdate(true);
          

            // 设置回调
            gameLoop.UpdateCallback = GameUpdate;
            gameLoop.RenderCallback = GameRender;

            // 添加可更新对象
            gameLoop.AddUpdateable(sceneManager);
            gameLoop.AddUpdateable(world);

            // 时间系统
            Time.Initialize();

            System.Diagnostics.Debug.WriteLine("引擎初始化完成");
        }

        private void CreateMainScene()
        {
            mainScene = new Scene("MainScene");
            sceneManager.RegisterScene(mainScene);

            // 创建相机
            cameraEntity = world.CreateEntity("MainCamera");
            var camera = new Camera2D();
            camera.ViewportSize = new SKSize(ClientSize.Width, ClientSize.Height);
            camera.Position = new Vector2(ClientSize.Width / 2, ClientSize.Height / 2);
            cameraEntity.AddComponent(camera);
            renderer.Camera = camera;

            // 创建玩家
            playerEntity = CreatePlayerEntity();

            // 创建测试实体
            CreateTestEntities();

            // 激活场景
            mainScene.Load();
            mainScene.Activate();
            sceneManager.ChangeScene("MainScene", true);
            // 在CreateMainScene方法的末尾添加
            //mainScene.World.AddEntity(cameraEntity);
            //mainScene.World.AddEntity(playerEntity);

            System.Diagnostics.Debug.WriteLine("主场景创建完成");
        }

        private Entity CreatePlayerEntity()
        {
            var entity = world.CreateEntity("Player");

            // 变换组件
            var transform = new Transform2D(new Vector2(ClientSize.Width / 2, ClientSize.Height / 2));
            entity.AddComponent(transform);

            // 精灵组件
            var sprite = new Sprite();
            sprite.Origin = new Vector2(0.5f, 0.5f);
            sprite.Scale = new Vector2(1, 1);
            entity.AddComponent(sprite);

            // 创建玩家图像
            SKBitmap bitmap = new SKBitmap(50, 50);
            using (SKCanvas canvas = new SKCanvas(bitmap))
            {
                canvas.Clear(SKColors.Red);
                using (SKPaint paint = new SKPaint
                {
                    Color = SKColors.White,
                    Style = SKPaintStyle.Fill,
                    IsAntialias = true
                })
                {
                    canvas.DrawCircle(25, 25, 20, paint);
                }

                // 添加方向指示
                using (SKPaint paint = new SKPaint
                {
                    Color = SKColors.Black,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 3,
                    IsAntialias = true
                })
                {
                    canvas.DrawLine(25, 25, 45, 25, paint);
                }
            }
            sprite.Texture = bitmap;

            return entity;
        }

        private void CreateTestEntities()
        {
            Random random = new Random();

            // 清除旧的测试实体
            foreach (var entity in testEntities)
            {
                world.DestroyEntity(entity);
            }
            testEntities.Clear();

            // 创建围绕玩家的多个测试实体
            for (int i = 0; i < 20; i++)
            {
                var entity = world.CreateEntity($"TestEntity_{i}");

                // 随机位置
                float angle = (float)i / 20 * MathHelper.TwoPi;
                float distance = 200 + random.Next(0, 100);
                float x = ClientSize.Width / 2 + MathF.Cos(angle) * distance;
                float y = ClientSize.Height / 2 + MathF.Sin(angle) * distance;

                // 变换组件
                var transform = new Transform2D(new Vector2(x, y));
                transform.Rotation = angle;
                entity.AddComponent(transform);

                // 精灵组件
                var sprite = new Sprite();
                sprite.Origin = new Vector2(0.5f, 0.5f);
                sprite.Scale = new Vector2(1, 1);
                entity.AddComponent(sprite);

                // 创建随机颜色的图像
                byte r = (byte)random.Next(100, 255);
                byte g = (byte)random.Next(100, 255);
                byte b = (byte)random.Next(100, 255);

                SKBitmap bitmap = new SKBitmap(30, 30);
                using (SKCanvas canvas = new SKCanvas(bitmap))
                {
                    canvas.Clear(new SKColor(r, g, b));

                    // 添加边框
                    using (SKPaint paint = new SKPaint
                    {
                        Color = SKColors.White,
                        Style = SKPaintStyle.Stroke,
                        StrokeWidth = 2
                    })
                    {
                        canvas.DrawRect(2, 2, 26, 26, paint);
                    }
                }
                sprite.Texture = bitmap;

                testEntities.Add(entity);
            }

            System.Diagnostics.Debug.WriteLine($"创建了 {testEntities.Count} 个测试实体");
        }

        private void GameUpdate(float deltaTime)
        {
            if (!isInitialized)
                return;

            try
            {
                // 更新时间
                Time.Update();

                // 使用合适的时间步长
                float effectiveDeltaTime = deltaTime > 0 ? deltaTime : Time.DeltaTime;

                // 处理输入
                HandleInput(effectiveDeltaTime);

                // 根据当前测试场景执行特定逻辑
                ExecuteTestLogic(effectiveDeltaTime);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"更新错误: {ex.Message}");
            }
        }

        private void HandleInput(float deltaTime)
        {
            // 切换测试场景
            if (inputManager.IsKeyPressed(Keys.Tab))
            {
                currentTestIndex = (currentTestIndex + 1) % testNames.Length;
                System.Diagnostics.Debug.WriteLine($"切换到测试: {testNames[currentTestIndex]}");

                // 为新测试准备场景
                if (currentTestIndex == 2) // 场景切换测试
                {
                    PrepareSceneSwitchingTest();
                }
                else if (currentTestIndex == 4) // 相机跟随测试
                {
                    PrepareFollowCameraTest();
                }
                else
                {
                    // 重置相机
                    Camera2D camera = cameraEntity.GetComponent<Camera2D>();
                    if (camera != null)
                    {
                        camera.Position = new Vector2(ClientSize.Width / 2, ClientSize.Height / 2);
                        camera.Target = null;
                    }
                }
            }

            // 切换调试信息显示
            if (inputManager.IsKeyPressed(Keys.F1))
            {
                showDebugInfo = !showDebugInfo;
            }

            // 基础移动测试
            if (currentTestIndex == 0)
            {
                MovePlayer(deltaTime);
            }

            // 输入响应测试
            if (currentTestIndex == 3)
            {
                HandleInputResponseTest(deltaTime);
            }

            // ESC退出应用
            if (inputManager.IsKeyPressed(Keys.Escape))
            {
                Application.Exit();
            }
        }

        private void MovePlayer(float deltaTime)
        {
            Transform2D transform = playerEntity.GetComponent<Transform2D>();
            if (transform == null)
                return;

            const float moveSpeed = 200.0f;
            Vector2 movement = Vector2.Zero;

            // 键盘移动
            if (inputManager.IsKeyDown(Keys.W) || inputManager.IsKeyDown(Keys.Up))
                movement.Y -= 1;

            if (inputManager.IsKeyDown(Keys.S) || inputManager.IsKeyDown(Keys.Down))
                movement.Y += 1;

            if (inputManager.IsKeyDown(Keys.A) || inputManager.IsKeyDown(Keys.Left))
                movement.X -= 1;

            if (inputManager.IsKeyDown(Keys.D) || inputManager.IsKeyDown(Keys.Right))
                movement.X += 1;

            // 规范化并应用移动
            if (movement.Length > 0)
            {
                movement = movement.Normalized;
                transform.Position += movement * moveSpeed * deltaTime;
            }

            // 旋转 (根据鼠标位置)
            Vector2 mousePos = inputManager.MouseState.Position;
            Vector2 direction = mousePos - transform.Position;

            if (direction.Length > 0)
            {
                transform.Rotation = MathF.Atan2(direction.Y, direction.X);
            }
        }

        private void PrepareFollowCameraTest()
        {
            // 设置相机跟随玩家
            Camera2D camera = cameraEntity.GetComponent<Camera2D>();
            if (camera != null)
            {
                camera.Target = playerEntity;
                camera.LerpFactor = 0.1f; // 平滑跟随
                System.Diagnostics.Debug.WriteLine("相机设置为跟随玩家");
            }
        }

        private void PrepareSceneSwitchingTest()
        {
            // 检查SecondScene是否已存在
            Scene existingScene = sceneManager.GetScene("SecondScene");
            if (existingScene != null)
            {
                System.Diagnostics.Debug.WriteLine("第二场景已存在，无需重新创建");
                return;
            }
            System.Diagnostics.Debug.WriteLine("开始创建第二场景");

            // 创建第二个场景
            var secondScene = new Scene("SecondScene");
            sceneManager.RegisterScene(secondScene);
            System.Diagnostics.Debug.WriteLine("第二场景已注册到场景管理器");

            // 直接使用场景的World创建实体（这样实体已经自动与场景的World关联）
            var backgroundEntity = secondScene.World.CreateEntity("Background");

            // 添加组件
            var bgTransform = new Transform2D(new Vector2(ClientSize.Width / 2, ClientSize.Height / 2));
            backgroundEntity.AddComponent(bgTransform);

            var bgSprite = new Sprite();
            bgSprite.Origin = new Vector2(0.5f, 0.5f);
            bgSprite.Scale = new Vector2(1, 1);
            backgroundEntity.AddComponent(bgSprite);

            // 创建渐变背景
            SKBitmap bgBitmap = new SKBitmap(ClientSize.Width, ClientSize.Height);
            using (SKCanvas canvas = new SKCanvas(bgBitmap))
            {
                // 原有的渐变背景创建代码...
                using (SKPaint paint = new SKPaint())
                {
                    paint.Shader = SKShader.CreateLinearGradient(
                        new SKPoint(0, 0),
                        new SKPoint(0, ClientSize.Height),
                        new SKColor[] {
                    SKColors.DarkBlue,
                    SKColors.Purple,
                    SKColors.DarkRed
                        },
                        new float[] { 0, 0.5f, 1 },
                        SKShaderTileMode.Clamp
                    );
                    canvas.DrawRect(0, 0, ClientSize.Width, ClientSize.Height, paint);
                }

                // 添加一些星星
                Random random = new Random();
                using (SKPaint paint = new SKPaint { Color = SKColors.White })
                {
                    for (int i = 0; i < 200; i++)
                    {
                        float x = (float)random.NextDouble() * ClientSize.Width;
                        float y = (float)random.NextDouble() * ClientSize.Height;
                        float size = (float)random.NextDouble() * 3 + 1;
                        canvas.DrawCircle(x, y, size, paint);
                    }
                }

                // 添加文字 - 使用支持中文的字体
                using (SKTypeface chineseTypeface = SKTypeface.FromFamilyName(
                    "Microsoft YaHei",
                    SKFontStyleWeight.Normal,
                    SKFontStyleWidth.Normal,
                    SKFontStyleSlant.Upright))
                using (SKPaint paint = new SKPaint
                {
                    Color = SKColors.White,
                    TextSize = 40,
                    IsAntialias = true,
                    TextAlign = SKTextAlign.Center,
                    Typeface = chineseTypeface ?? SKTypeface.FromFamilyName("SimHei") ?? SKTypeface.Default
                })
                {
                    canvas.DrawText("第二场景", ClientSize.Width / 2, ClientSize.Height / 2, paint);
                }
            }
            bgSprite.Texture = bgBitmap;

            // 确保场景被正确加载和激活
            secondScene.Load();
            System.Diagnostics.Debug.WriteLine("第二场景已加载");
        }
        private void HandleInputResponseTest(float deltaTime)
        {
            // 获取鼠标位置和状态
            Vector2 mousePos = inputManager.MouseState.Position;
            bool leftButtonDown = inputManager.MouseState.IsButtonDown(MouseButton.Left);

            // 更新玩家位置到鼠标位置
            if (leftButtonDown)
            {
                Transform2D transform = playerEntity.GetComponent<Transform2D>();
                if (transform != null)
                {
                    // 使用插值平滑移动
                    transform.Position = Vector2.Lerp(transform.Position, mousePos, 5.0f * deltaTime);
                }
            }

            // 处理键盘按下效果
            if (inputManager.IsKeyDown(Keys.Space))
            {
                // 空格键按下时改变玩家外观
                Sprite sprite = playerEntity.GetComponent<Sprite>();
                if (sprite != null && sprite.Texture != null)
                {
                    // 脉动效果
                    float pulseFactor = 1.0f + 0.2f * MathF.Sin((float)Time.TotalTime * 10.0f);
                    sprite.Scale = new Vector2(pulseFactor, pulseFactor);
                }
            }
            else
            {
                // 恢复正常大小
                Sprite sprite = playerEntity.GetComponent<Sprite>();
                if (sprite != null)
                {
                    sprite.Scale = new Vector2(1.0f, 1.0f);
                }
            }
        }

        private void ExecuteTestLogic(float deltaTime)
        {
            switch (currentTestIndex)
            {
                case 0: // 基础移动测试
                    // 基础移动在HandleInput中处理
                    break;

                case 1: // 精灵渲染测试
                    AnimateTestEntities(deltaTime);
                    break;

                case 2: // 场景切换测试
                        // 添加调试输出以确认空格键被按下
                    if (inputManager.IsKeyPressed(Keys.Space))
                    {
                        System.Diagnostics.Debug.WriteLine("空格键被按下，尝试切换场景");

                        // 获取活动场景
                        Scene activeScene = sceneManager.GetActiveScene();
                        if (activeScene != null)
                        {
                            string currentSceneName = activeScene.Name;
                            string targetSceneName = currentSceneName == "MainScene" ? "SecondScene" : "MainScene";

                            System.Diagnostics.Debug.WriteLine($"当前场景: {currentSceneName}, 目标场景: {targetSceneName}");

                            // 确保目标场景存在
                            Scene targetScene = sceneManager.GetScene(targetSceneName);
                            if (targetScene == null)
                            {
                                System.Diagnostics.Debug.WriteLine($"错误: 目标场景 {targetSceneName} 不存在");
                                if (targetSceneName == "SecondScene")
                                {
                                    // 如果第二场景不存在，尝试创建它
                                    PrepareSceneSwitchingTest();
                                }
                                return;
                            }

                            // 尝试切换场景
                            bool success = sceneManager.ChangeScene(targetSceneName, true);
                            System.Diagnostics.Debug.WriteLine($"场景切换 {(success ? "成功" : "失败")}: {targetSceneName}");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("错误: 没有活动场景");
                        }
                    }
                    break;

                case 3: // 输入响应测试
                    // 输入响应在HandleInput中处理
                    break;

                case 4: // 相机跟随测试
                    // 相机跟随在PrepareFollowCameraTest中设置，这里我们只需确保玩家可以移动
                    MovePlayer(deltaTime);

                    // 添加一些额外效果：旋转背景实体
                    foreach (var entity in testEntities)
                    {
                        Transform2D transform = entity.GetComponent<Transform2D>();
                        if (transform != null)
                        {
                            transform.Rotation += 0.5f * deltaTime;
                        }
                    }
                    break;
            }
        }

        private void AnimateTestEntities(float deltaTime)
        {
            // 使测试实体围绕玩家旋转
            Transform2D playerTransform = playerEntity.GetComponent<Transform2D>();
            if (playerTransform == null)
                return;

            Vector2 playerPos = playerTransform.Position;
            float baseRotationSpeed = 0.2f;

            for (int i = 0; i < testEntities.Count; i++)
            {
                Entity entity = testEntities[i];
                Transform2D transform = entity.GetComponent<Transform2D>();

                if (transform != null)
                {
                    // 计算当前角度和距离
                    Vector2 relativePos = transform.Position - playerPos;
                    float distance = relativePos.Length;
                    float angle = MathF.Atan2(relativePos.Y, relativePos.X);

                    // 根据索引调整旋转速度
                    float rotationSpeed = baseRotationSpeed * (1.0f + (float)i / testEntities.Count);

                    // 更新角度
                    angle += rotationSpeed * deltaTime;

                    // 更新位置
                    float newX = playerPos.X + MathF.Cos(angle) * distance;
                    float newY = playerPos.Y + MathF.Sin(angle) * distance;
                    transform.Position = new Vector2(newX, newY);

                    // 更新旋转
                    transform.Rotation = angle;

                    // 脉动效果
                    Sprite sprite = entity.GetComponent<Sprite>();
                    if (sprite != null)
                    {
                        float pulseFactor = 0.8f + 0.4f * MathF.Sin((float)Time.TotalTime * 2.0f + i * 0.2f);
                        sprite.Scale = new Vector2(pulseFactor, pulseFactor);
                    }
                }
            }
        }

        private void GameRender(float deltaTime)
        {
            if (!isInitialized || renderBitmap == null)
                return;

            try
            {
                // 确保位图尺寸正确
                if (renderBitmap.Width != ClientSize.Width || renderBitmap.Height != ClientSize.Height)
                {
                    renderBitmap?.Dispose();
                    renderBitmap = new SKBitmap(
                        Math.Max(1, ClientSize.Width),
                        Math.Max(1, ClientSize.Height));

                    // 更新相机视口
                    Camera2D camera = cameraEntity?.GetComponent<Camera2D>();
                    if (camera != null)
                    {
                        camera.ViewportSize = new SKSize(ClientSize.Width, ClientSize.Height);
                    }
                }

                // 渲染到位图
                using (SKCanvas canvas = new SKCanvas(renderBitmap))
                {
                    // 清除背景
                    canvas.Clear(SKColors.Black);

                    // 渲染所有实体
                    RenderEntities(canvas);

                    // 绘制调试信息
                    if (showDebugInfo)
                    {
                        DrawDebugInfo(canvas);
                    }
                }

                // 让UI处理完所有待处理事件，减少闪烁
                Application.DoEvents();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"渲染错误: {ex.Message}");
            }
        }

        private void RenderEntities(SKCanvas canvas)
        {
            // 获取相机
            Camera2D camera = cameraEntity?.GetComponent<Camera2D>();
            if (camera == null)
                return;

            // 获取当前活动场景
            Scene activeScene = sceneManager.GetActiveScene();

            // 主场景使用主World的实体
            if (activeScene == null || activeScene.Name == "MainScene")
            {
                RenderMainWorldEntities(canvas, camera);
            }
            // 其他场景使用各自场景World的实体
            else
            {
                var sceneEntities = activeScene.World.GetAllEntities();
                System.Diagnostics.Debug.WriteLine($"渲染场景 {activeScene.Name} 中的 {sceneEntities.Count} 个实体");

                foreach (var entity in sceneEntities)
                {
                    RenderEntity(canvas, entity, camera);
                }
            }
        }

        private void RenderMainWorldEntities(SKCanvas canvas, Camera2D camera)
        {
            // 获取所有实体
            var entities = world.GetAllEntities();

            // 按层级排序（简单示例：背景 -> 其它实体 -> 玩家）
            var backgroundEntities = entities.Where(e => e != playerEntity && e != cameraEntity).ToList();

            // 渲染背景
            foreach (var entity in backgroundEntities)
            {
                RenderEntity(canvas, entity, camera);
            }

            // 渲染玩家
            RenderEntity(canvas, playerEntity, camera);
        }
        private void RenderEntity(SKCanvas canvas, Entity entity, Camera2D camera)
        {
            if (entity == null)
                return;

            var sprite = entity.GetComponent<Sprite>();
            var transform = entity.GetComponent<Transform2D>();

            if (sprite != null && transform != null && sprite.Texture != null)
            {
                // 保存当前状态
                canvas.Save();

                // 计算屏幕坐标
                Vector2 screenPos = camera.WorldToScreen(transform.Position);

                // 应用变换
                canvas.Translate(screenPos.X, screenPos.Y);
                canvas.RotateDegrees(transform.Rotation * MathHelper.RadToDeg);
                canvas.Scale(transform.Scale.X * sprite.Scale.X, transform.Scale.Y * sprite.Scale.Y);

                // 应用锚点偏移
                float originX = -sprite.Origin.X * sprite.Texture.Width;
                float originY = -sprite.Origin.Y * sprite.Texture.Height;
                canvas.Translate(originX, originY);

                // 绘制精灵
                using (SKPaint paint = new SKPaint { FilterQuality = SKFilterQuality.High })
                {
                    canvas.DrawBitmap(sprite.Texture, 0, 0, paint);
                }

                // 恢复状态
                canvas.Restore();
            }
        }

        private void DrawCameraBounds(SKCanvas canvas, Camera2D camera)
        {
            // 绘制相机视口边界
            using (SKPaint paint = new SKPaint
            {
                Color = SKColors.Yellow,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 2,
                PathEffect = SKPathEffect.CreateDash(new float[] { 10, 5 }, 0)
            })
            {
                float left = 20;
                float top = 20;
                float right = ClientSize.Width - 20;
                float bottom = ClientSize.Height - 20;

                canvas.DrawRect(left, top, right - left, bottom - top, paint);
            }
        }

        private void DrawDebugInfo(SKCanvas canvas)
        {
            // 创建支持中文字体的SKTypeface
            using (SKTypeface typeface = SKTypeface.FromFamilyName("Microsoft YaHei", SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright))
            using (SKPaint textPaint = new SKPaint
            {
                Color = SKColors.White,
                TextSize = 14,
                IsAntialias = true,
                Typeface = typeface  // 使用支持中文的字体
            })
            {
                // 玩家信息
                Transform2D playerTransform = playerEntity?.GetComponent<Transform2D>();
                string playerPos = playerTransform != null
                    ? $"({playerTransform.Position.X:F1}, {playerTransform.Position.Y:F1})"
                    : "未知";

                // 获取当前活动场景名称
                Scene activeScene = sceneManager.GetActiveScene();
                string currentSceneName = activeScene != null ? activeScene.Name : "无";

                // FPS和时间信息
                canvas.DrawText($"FPS: {Time.FramesPerSecond:F1}", 10, 20, textPaint);
                canvas.DrawText($"总时间: {Time.TotalTime:F1}秒", 10, 40, textPaint);
                canvas.DrawText($"玩家位置: {playerPos}", 10, 60, textPaint);
                canvas.DrawText($"实体总数: {world.GetAllEntities().Count}", 10, 80, textPaint);
                canvas.DrawText($"当前场景: {currentSceneName}", 10, 100, textPaint);

                // 当前测试
                textPaint.Color = SKColors.Yellow;
                canvas.DrawText($"当前测试: {testNames[currentTestIndex]}", 10, 120, textPaint);

                // 控制说明
                textPaint.Color = SKColors.LightGray;
                textPaint.TextSize = 12;
                canvas.DrawText("Tab键切换测试 | F1键切换调试信息 | Esc键退出", 10, 140, textPaint);

                // 根据当前测试显示特定说明
                string instructions = "";
                switch (currentTestIndex)
                {
                    case 0:
                        instructions = "WASD/方向键移动，鼠标控制朝向";
                        break;
                    case 1:
                        instructions = "观察实体动画效果";
                        break;
                    case 2:
                        instructions = "空格键切换场景";
                        break;
                    case 3:
                        instructions = "按住左键拖动玩家，空格键产生特效";
                        break;
                    case 4:
                        instructions = "WASD/方向键移动，相机会跟随玩家";
                        break;
                }
                canvas.DrawText(instructions, 10, 160, textPaint);
            }
        }

        #region 事件处理

        private void GameEngineTest_Paint(object sender, PaintEventArgs e)
        {
            if (renderBitmap == null || renderBitmap.IsEmpty)
                return;

            try
            {
                using (var image = SKImage.FromBitmap(renderBitmap))
                using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
                using (var stream = data.AsStream())
                using (var bitmap = new System.Drawing.Bitmap(stream))
                {
                    e.Graphics.DrawImage(bitmap, 0, 0);
                }
            }
            catch (Exception ex)
            {
                e.Graphics.Clear(System.Drawing.Color.Black);
                e.Graphics.DrawString($"渲染错误: {ex.Message}",
                    new System.Drawing.Font("Arial", 12),
                    System.Drawing.Brushes.Red, 10, 10);

                System.Diagnostics.Debug.WriteLine($"绘制错误: {ex.Message}");
            }
        }

        private void GameEngineTest_KeyDown(object sender, KeyEventArgs e)
        {
            KeyboardState keyboardState = inputManager.KeyboardState.Clone();
            keyboardState.SetKeyDown((Keys)e.KeyCode);
            inputManager.SetKeyboardState(keyboardState);
        }

        private void GameEngineTest_KeyUp(object sender, KeyEventArgs e)
        {
            KeyboardState keyboardState = inputManager.KeyboardState.Clone();
            keyboardState.SetKeyUp((Keys)e.KeyCode);
            inputManager.SetKeyboardState(keyboardState);
        }

        private void GameEngineTest_MouseMove(object sender, MouseEventArgs e)
        {
            MouseState mouseState = inputManager.MouseState.Clone();
            mouseState.Position = new Vector2(e.X, e.Y);
            inputManager.SetMouseState(mouseState);
        }

        private void GameEngineTest_MouseDown(object sender, MouseEventArgs e)
        {
            MouseState mouseState = inputManager.MouseState.Clone();
            mouseState.Position = new Vector2(e.X, e.Y);

            if (e.Button == System.Windows.Forms.MouseButtons.Left)
                mouseState.SetButtonDown(MouseButton.Left);
            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
                mouseState.SetButtonDown(MouseButton.Right);
            else if (e.Button == System.Windows.Forms.MouseButtons.Middle)
                mouseState.SetButtonDown(MouseButton.Middle);

            inputManager.SetMouseState(mouseState);
        }

        private void GameEngineTest_MouseUp(object sender, MouseEventArgs e)
        {
            MouseState mouseState = inputManager.MouseState.Clone();
            mouseState.Position = new Vector2(e.X, e.Y);

            if (e.Button == System.Windows.Forms.MouseButtons.Left)
                mouseState.SetButtonUp(MouseButton.Left);
            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
                mouseState.SetButtonUp(MouseButton.Right);
            else if (e.Button == System.Windows.Forms.MouseButtons.Middle)
                mouseState.SetButtonUp(MouseButton.Middle);

            inputManager.SetMouseState(mouseState);
        }

        private void GameEngineTest_Resize(object sender, EventArgs e)
        {
            // 窗体大小改变时，我们会在下一帧渲染时更新位图大小
        }

        private void GameEngineTest_FormClosed(object sender, FormClosedEventArgs e)
        {
            // 停止游戏循环并清理资源
            gameLoop.Stop();
            renderBitmap?.Dispose();

            System.Diagnostics.Debug.WriteLine("测试程序已关闭");
        }

        #endregion
    }
}