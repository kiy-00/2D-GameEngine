using System;
using System.Windows.Forms;
using GameEngine.Common;
using GameEngine.Core;
using SkiaSharp;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
using Keys = GameEngine.Core.Keys;
using Time = GameEngine.Core.Time;
// 显式导入以避免混淆
using WinFormsTimer = System.Windows.Forms.Timer;
using GameTimer = GameEngine.Common.Timer;


namespace GameEngineTest
{
    /// <summary>
    /// 游戏引擎核心测试程序
    /// </summary>
    public class GameEngineCoreTest : Form
    {
        private GameLoop gameLoop;
        private World world;
        private Renderer renderer;
        private InputManager inputManager;
        private SceneManager sceneManager;
        private Scene testScene;
        private Entity playerEntity;
        private Entity cameraEntity;
        private SKBitmap renderBitmap;
        private bool isInitialized = false;

        /// <summary>
        /// 程序入口点
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new GameEngineCoreTest());
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public GameEngineCoreTest()
        {
            // 窗体设置
            Text = "GameEngine.Core 测试程序";
            Size = new System.Drawing.Size(800, 600);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            DoubleBuffered = true;

            // 初始化游戏引擎
            InitializeGameEngine();

            // 创建测试场景
            CreateTestScene();

            // 初始化渲染位图
            renderBitmap = new SKBitmap(ClientSize.Width, ClientSize.Height);

            // 绑定窗体事件
            Paint += GameEngineCoreTest_Paint;
            KeyDown += GameEngineCoreTest_KeyDown;
            KeyUp += GameEngineCoreTest_KeyUp;
            MouseMove += GameEngineCoreTest_MouseMove;
            MouseDown += GameEngineCoreTest_MouseDown;
            MouseUp += GameEngineCoreTest_MouseUp;
            FormClosed += GameEngineCoreTest_FormClosed;
            Resize += GameEngineCoreTest_Resize;

            // 创建计时器用于重绘窗体
            WinFormsTimer timer = new WinFormsTimer();
            timer.Interval = 16; // 约60 FPS
            timer.Tick += Timer_Tick;
            timer.Start();

            // 标记初始化完成
            isInitialized = true;

            // 启动游戏循环
            gameLoop.Start();
        }

        /// <summary>
        /// 初始化游戏引擎
        /// </summary>
        // 3. 在InitializeGameEngine中添加额外检查，确保GameLoop正确配置
        private void InitializeGameEngine()
        {
            // 创建游戏循环
            gameLoop = new GameLoop();
            gameLoop.TargetElapsedTime = 1.0f / 60.0f; // 60 FPS
            System.Diagnostics.Debug.WriteLine($"GameLoop目标帧时间: {gameLoop.TargetElapsedTime}");

            // 创建游戏世界
            world = new World("TestWorld");

            // 创建渲染器
            renderer = new Renderer();
            world.AddSystem(renderer);

            // 创建输入管理器
            inputManager = new InputManager();
            inputManager.Initialize();
            world.AddSystem(inputManager);

            // 创建场景管理器
            sceneManager = SceneManager.Instance;
            sceneManager.SetEnableUpdate(true);

            // 设置游戏循环回调
            gameLoop.UpdateCallback = GameUpdate;
            gameLoop.RenderCallback = GameRender;

            // 添加可更新对象到游戏循环
            gameLoop.AddUpdateable(sceneManager);
            gameLoop.AddUpdateable(world);

            // 初始化时间系统
            Time.Initialize();
            System.Diagnostics.Debug.WriteLine("时间系统已初始化");
        }

        /// <summary>
        /// 创建测试场景
        /// </summary>
        /// <summary>
        /// 创建测试场景
        /// </summary>
        private void CreateTestScene()
        {
            // 创建测试场景
            testScene = new Scene("TestScene");
            sceneManager.RegisterScene(testScene);

            // 创建玩家实体
            playerEntity = world.CreateEntity("Player");

            // 添加变换组件
            Transform2D playerTransform = new Transform2D(new Vector2(400, 300));
            playerEntity.AddComponent(playerTransform);

            // 添加精灵组件
            Sprite playerSprite = new Sprite();
            playerSprite.Origin = new Vector2(0.5f, 0.5f); // 中心点
            playerSprite.Scale = new Vector2(2, 2); // 增大到100x100像素大小
            playerEntity.AddComponent(playerSprite);

            // 创建更加醒目的玩家精灵
            SKBitmap playerBitmap = new SKBitmap(50, 50);
            using (SKCanvas canvas = new SKCanvas(playerBitmap))
            {
                canvas.Clear(SKColors.Yellow); // 改为黄色背景
                using (SKPaint paint = new SKPaint
                {
                    Color = SKColors.Black, // 改为黑色前景
                    Style = SKPaintStyle.
        Fill
                })
                {
                    canvas.DrawOval(new SKRect(5, 5, 45, 45), paint); // 扩大椭圆
                }
            }
            playerSprite.Texture = playerBitmap;

            // 创建相机实体
            cameraEntity = world.CreateEntity("Camera");

            // 添加相机组件
            Camera2D camera = new Camera2D();
            camera.ViewportSize = new SKSize(ClientSize.Width, ClientSize.Height);
            camera.Target = null; // 暂时不跟随玩家
            camera.Position = new Vector2(ClientSize.Width / 2, ClientSize.Height / 2); // 固定位置
            cameraEntity.AddComponent(camera);

            // 设置渲染器的相机
            renderer.Camera = camera;

            // 创建一些背景元素
            Random random = new Random();
            for (int i = 0; i < 10; i++)
            {
                Entity backgroundEntity = world.CreateEntity($"Background_{i}");

                // 随机位置
                float x = (float)random.NextDouble() * 800;
                float y = (float)random.NextDouble() * 600;
                Transform2D transform = new Transform2D(new Vector2(x, y));
                backgroundEntity.AddComponent(transform);

                // 随机颜色的方块
                Sprite sprite = new Sprite();
                sprite.Origin = new Vector2(0.5f, 0.5f);
                sprite.Scale = new Vector2(20, 20);
                backgroundEntity.AddComponent(sprite);

                // 创建随机颜色图片
                SKBitmap bitmap = new SKBitmap(20, 20);
                using (SKCanvas canvas = new SKCanvas(bitmap))
                {
                    SKColor randomColor = new SKColor(
                        (byte)random.Next(100, 255),
                        (byte)random.Next(100, 255),
                        (byte)random.Next(100, 255));
                    canvas.Clear(randomColor);
                }
                sprite.Texture = bitmap;
            }

            // 载入并激活场景
            testScene.Load();
            testScene.Activate();
            sceneManager.ChangeScene("TestScene", true);
        }

        /// <summary>
        /// 游戏更新回调
        /// </summary>
        // 游戏更新回调中添加时间更新
        // 1. 修改GameUpdate方法，添加更详细的时间调试信息
        private void GameUpdate(float deltaTime)
        {
            try
            {
                // 添加详细的deltaTime日志
                System.Diagnostics.Debug.WriteLine($"GameUpdate被调用，deltaTime={deltaTime}");

                // 检查Time类是否正确初始化
                System.Diagnostics.Debug.WriteLine($"更新前的Time.DeltaTime: {Time.DeltaTime}");

                // 更新时间系统
                Time.Update();

                // 检查Time类是否正确更新
                System.Diagnostics.Debug.WriteLine($"更新后的Time.DeltaTime: {Time.DeltaTime}");

                // 检查我们是否应该使用Time.DeltaTime而不是传入的参数
                float effectiveDeltaTime = deltaTime > 0 ? deltaTime : Time.DeltaTime;
                System.Diagnostics.Debug.WriteLine($"使用的有效deltaTime: {effectiveDeltaTime}");

                // 使用有效的delta时间检查玩家输入
                HandlePlayerInput(effectiveDeltaTime);

                // 调试输出
                System.Diagnostics.Debug.WriteLine($"玩家实体存在: {playerEntity != null}");
                System.Diagnostics.Debug.WriteLine($"玩家有transform组件: {playerEntity?.GetComponent<Transform2D>() != null}");
                System.Diagnostics.Debug.WriteLine($"玩家有sprite组件: {playerEntity?.GetComponent<Sprite>() != null}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"异常详情: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"堆栈跟踪: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// 处理玩家输入
        /// </summary>
        // 2. 修复HandlePlayerInput方法，移除重复的移动应用
        private void HandlePlayerInput(float deltaTime)
        {
            // 调试输出
            System.Diagnostics.Debug.WriteLine($"HandlePlayerInput: deltaTime = {deltaTime}");

            if (deltaTime <= 0)
            {
                System.Diagnostics.Debug.WriteLine("警告: deltaTime为零或负值，移动将被禁用!");
                return;  // 使用无效的deltaTime时不处理移动
            }

            // 获取玩家变换组件
            Transform2D playerTransform = playerEntity.GetComponent<Transform2D>();
            if (playerTransform == null)
            {
                System.Diagnostics.Debug.WriteLine("错误: 玩家transform为空!");
                return;
            }

            // 移动速度
            const float moveSpeed = 200.0f;

            // 根据键盘状态移动玩家
            Vector2 movement = Vector2.Zero;

            if (inputManager.IsKeyDown(Keys.W) || inputManager.IsKeyDown(Keys.Up))
                movement.Y -= 1;

            if (inputManager.IsKeyDown(Keys.S) || inputManager.IsKeyDown(Keys.Down))
                movement.Y += 1;

            if (inputManager.IsKeyDown(Keys.A) || inputManager.IsKeyDown(Keys.Left))
                movement.X -= 1;

            if (inputManager.IsKeyDown(Keys.D) || inputManager.IsKeyDown(Keys.Right))
                movement.X += 1;

            // 归一化向量以确保对角线移动速度相同
            if (movement.Length > 0)
                movement = movement.Normalized;

            // 调试键盘状态
            System.Diagnostics.Debug.WriteLine($"键W: {inputManager.IsKeyDown(Keys.W)}, 上键: {inputManager.IsKeyDown(Keys.Up)}");
            System.Diagnostics.Debug.WriteLine($"移动向量: {movement}, 缩放后: {movement * moveSpeed * deltaTime}");

            // 仅应用一次移动(移除了重复调用)
            Vector2 positionChange = movement * moveSpeed * deltaTime;
            playerTransform.Position += positionChange;

            // 输出更新后的位置
            System.Diagnostics.Debug.WriteLine($"新位置: {playerTransform.Position}");

            // 如果空格键被按下，旋转玩家
            if (inputManager.IsKeyDown(Keys.Space))
            {
                playerTransform.Rotation += 2.0f * deltaTime;
            }

            // 如果ESC键被按下，退出应用程序
            if (inputManager.IsKeyPressed(Keys.Escape))
            {
                Application.Exit();
            }
        }

        /// <summary>
        /// 游戏渲染回调
        /// </summary>
        private void GameRender(float deltaTime)
        {
            if (!isInitialized)
                return;

            try
            {
                // 确保位图大小正确
                if (renderBitmap == null || renderBitmap.Width != ClientSize.Width || renderBitmap.Height != ClientSize.Height)
                {
                    renderBitmap?.Dispose();
                    renderBitmap = new SKBitmap(Math.Max(1, ClientSize.Width), Math.Max(1, ClientSize.Height));

                    // 更新相机视口
                    Camera2D camera = cameraEntity?.GetComponent<Camera2D>();
                    if (camera != null)
                    {
                        camera.ViewportSize = new SKSize(ClientSize.Width, ClientSize.Height);
                    }
                }

                // 使用安全渲染替代直接使用渲染器
                SafeRender();

                // 触发窗体重绘
                Invalidate();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GameRender异常: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"堆栈跟踪: {ex.StackTrace}");
            }
        }
        /// <summary>
        /// 绘制调试信息
        /// </summary>
        // 更新DrawDebugInfo方法，添加更多调试信息
        private void DrawDebugInfo(SKCanvas canvas)
        {
            // 设置文本绘制的画笔
            using (SKPaint textPaint = new SKPaint
            {
                Color = SKColors.White,
                TextSize = 12,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName("Arial")
            })
            {
                // 获取玩家位置
                Transform2D playerTransform = playerEntity?.GetComponent<Transform2D>();
                string playerPos = playerTransform != null
                    ? $"位置: ({playerTransform.Position.X:F1}, {playerTransform.Position.Y:F1}), 旋转: {playerTransform.Rotation:F2}"
                    : "未知";

                // 绘制FPS和玩家位置
                canvas.DrawText($"FPS: {Time.FramesPerSecond:F1}", 10, 20, textPaint);
                canvas.DrawText($"Player{playerPos}", 10, 40, textPaint);
                canvas.DrawText("使用WASD或方向键移动，空格键旋转，ESC退出", 10, 60, textPaint);
                canvas.DrawText($"实体总数: {world.GetAllEntities().Count}", 10, 80, textPaint);
                canvas.DrawText($"屏幕中心坐标: ({ClientSize.Width / 2}, {ClientSize.Height / 2})", 10, 100, textPaint);

                // 添加键盘状态信息
                string keyState = $"W: {inputManager.IsKeyDown(Keys.W)}, A: {inputManager.IsKeyDown(Keys.A)}, " +
                                 $"S: {inputManager.IsKeyDown(Keys.S)}, D: {inputManager.IsKeyDown(Keys.D)}";
                canvas.DrawText(keyState, 10, 120, textPaint);
            }
        }

        #region 窗体事件处理

        private void GameEngineCoreTest_Paint(object sender, PaintEventArgs e)
        {
            if (renderBitmap != null && !renderBitmap.IsEmpty)
            {
                try
                {
                    // 显示 SkiaSharp 绘制内容
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
                    // 发生错误时使用 GDI+ 绘制错误信息
                    e.Graphics.Clear(System.Drawing.Color.Black);
                    e.Graphics.DrawString($"SkiaSharp 错误: {ex.Message}",
                        new System.Drawing.Font("Arial", 12),
                        System.Drawing.Brushes.Red, 10, 10);
                    System.Diagnostics.Debug.WriteLine($"Paint 错误: {ex.Message}");
                }
            }
            else
            {
                e.Graphics.Clear(System.Drawing.Color.Black);
                e.Graphics.DrawString("renderBitmap 为空",
                    new System.Drawing.Font("Arial", 12),
                    System.Drawing.Brushes.Yellow, 10, 10);
            }
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            // 不需要任何操作，GameRender会调用Invalidate触发重绘
        }

        private void GameEngineCoreTest_Resize(object sender, EventArgs e)
        {
            // 窗体大小改变时，渲染位图会在下一帧GameRender中调整大小
        }

        private void GameEngineCoreTest_KeyDown(object sender, KeyEventArgs e)
        {
            // 更新键盘状态
            KeyboardState keyboardState = inputManager.KeyboardState.Clone();
            keyboardState.SetKeyDown((Keys)e.KeyCode);
            inputManager.SetKeyboardState(keyboardState);
        }

        private void GameEngineCoreTest_KeyUp(object sender, KeyEventArgs e)
        {
            // 更新键盘状态
            KeyboardState keyboardState = inputManager.KeyboardState.Clone();
            keyboardState.SetKeyUp((Keys)e.KeyCode);
            inputManager.SetKeyboardState(keyboardState);
        }

        private void GameEngineCoreTest_MouseMove(object sender, MouseEventArgs e)
        {
            // 更新鼠标状态
            MouseState mouseState = inputManager.MouseState.Clone();
            mouseState.Position = new Vector2(e.X, e.Y);
            inputManager.SetMouseState(mouseState);
        }

        private void GameEngineCoreTest_MouseDown(object sender, MouseEventArgs e)
        {
            // 更新鼠标状态
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

        private void GameEngineCoreTest_MouseUp(object sender, MouseEventArgs e)
        {
            // 更新鼠标状态
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

        private void GameEngineCoreTest_FormClosed(object sender, FormClosedEventArgs e)
        {
            // 停止游戏循环
            gameLoop.Stop();

            // 释放资源
            renderBitmap?.Dispose();
        }

        #endregion

        // 在 GameEngineTest 类中添加一个安全的渲染方法
        // 修改后的安全渲染方法
        private void SafeRender()
        {
            try
            {
                if (renderer != null && renderBitmap != null)
                {
                    using (SKCanvas canvas = new SKCanvas(renderBitmap))
                    {
                        // 先手动清屏，避免使用渲染器的 Clear
                        canvas.Clear(SKColors.CornflowerBlue);

                        // 检查相机是否有效
                        Camera2D camera = renderer.Camera;
                        if (camera == null || cameraEntity == null)
                        {
                            // 没有相机，使用简单绘制
                            SimpleRenderWithoutCamera(canvas);
                            return;
                        }

                        // 手动绘制各层级内容
                        try
                        {
                            // 手动获取和使用实体数据
                            var entities = world.GetAllEntities();
                            var backgroundEntities = entities.Where(e => e != playerEntity && e != cameraEntity).ToList();

                            // 先绘制背景实体
                            foreach (var entity in backgroundEntities)
                            {
                                RenderEntity(canvas, entity);
                            }

                            // 最后绘制玩家实体以确保在顶层
                            RenderEntity(canvas, playerEntity);

                            // 在屏幕中心绘制参考标记
                            using (SKPaint paint = new SKPaint { Color = SKColors.White, Style = SKPaintStyle.Stroke, StrokeWidth = 2 })
                            {
                                canvas.DrawCircle(ClientSize.Width / 2, ClientSize.Height / 2, 10, paint);
                                canvas.DrawLine(ClientSize.Width / 2 - 15, ClientSize.Height / 2, ClientSize.Width / 2 + 15, ClientSize.Height / 2, paint);
                                canvas.DrawLine(ClientSize.Width / 2, ClientSize.Height / 2 - 15, ClientSize.Width / 2, ClientSize.Height / 2 + 15, paint);
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"实体渲染错误: {ex.Message}");
                        }

                        // 添加调试信息
                        DrawDebugInfo(canvas);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SafeRender异常: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"堆栈跟踪: {ex.StackTrace}");
            }
        }

        // 添加新的辅助方法用于渲染单个实体
        private void RenderEntity(SKCanvas canvas, Entity entity)
        {
            if (entity == null)
                return;

            var sprite = entity.GetComponent<Sprite>();
            var transform = entity.GetComponent<Transform2D>();

            if (sprite != null && transform != null && sprite.Texture != null)
            {
                // 保存当前矩阵
                canvas.Save();

                // 手动应用变换
                float x = transform.Position.X;
                float y = transform.Position.Y;
                float rotation = transform.Rotation;
                float scaleX = transform.Scale.X * sprite.Scale.X;
                float scaleY = transform.Scale.Y * sprite.Scale.Y;

                System.Diagnostics.Debug.WriteLine($"渲染实体 {entity.Name}: 位置=({x}, {y}), 旋转={rotation}, 缩放=({scaleX}, {scaleY})");

                // 移动到位置
                canvas.Translate(x, y);

                // 应用旋转
                canvas.RotateDegrees(rotation * MathHelper.RadToDeg);

                // 应用缩放
                canvas.Scale(scaleX, scaleY);

                // 应用原点偏移
                float originX = -sprite.Origin.X;
                float originY = -sprite.Origin.Y;
                canvas.Translate(originX, originY);

                // 绘制精灵
                using (SKPaint paint = new SKPaint { FilterQuality = SKFilterQuality.High })
                {
                    // 对于玩家实体，使用更高的不透明度
                    if (entity == playerEntity)
                    {
                        paint.Color = SKColors.White.WithAlpha(255); // 完全不透明
                    }
                    canvas.DrawBitmap(sprite.Texture, 0, 0, paint);
                }

                // 恢复矩阵
                canvas.Restore();
            }
        }

        // 简单渲染方法（无需相机）
        private void SimpleRenderWithoutCamera(SKCanvas canvas)
        {
            // 绘制测试元素
            using (SKPaint textPaint = new SKPaint { Color = SKColors.White, TextSize = 24, IsAntialias = true })
            {
                canvas.DrawText("简单渲染模式（无相机）", 50, 50, textPaint);
            }

            // 绘制玩家位置
            Transform2D playerTransform = playerEntity?.GetComponent<Transform2D>();
            if (playerTransform != null)
            {
                float x = playerTransform.Position.X;
                float y = playerTransform.Position.Y;

                // 绘制玩家指示器
                using (SKPaint paint = new SKPaint { Color = SKColors.Red, Style = SKPaintStyle.Fill })
                {
                    canvas.DrawCircle(x, y, 25, paint);
                }

                // 绘制玩家精灵（如果有）
                Sprite playerSprite = playerEntity.GetComponent<Sprite>();
                if (playerSprite != null && playerSprite.Texture != null)
                {
                    try
                    {
                        float width = playerSprite.Texture.Width * playerSprite.Scale.X;
                        float height = playerSprite.Texture.Height * playerSprite.Scale.Y;
                        float offsetX = width * playerSprite.Origin.X;
                        float offsetY = height * playerSprite.Origin.Y;

                        using (SKPaint paint = new SKPaint())
                        {
                            canvas.DrawBitmap(playerSprite.Texture, x - offsetX, y - offsetY, paint);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"玩家精灵绘制错误: {ex.Message}");
                    }
                }
            }

            // 添加调试信息
            DrawDebugInfo(canvas);
        }
    }
}