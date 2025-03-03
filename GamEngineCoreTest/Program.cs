using System;
using System.Drawing;
using System.Net.NetworkInformation;
using System.Windows.Forms;
using GameEngine.Common;
using GameEngine.Core;
using static System.Formats.Asn1.AsnWriter;
using static System.Net.Mime.MediaTypeNames;

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
        private Graphics graphics;
        private BufferedGraphics bufferedGraphics;
        private BufferedGraphicsContext context;
        private Entity playerEntity;
        private Entity cameraEntity;

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
            Size = new Size(800, 600);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            DoubleBuffered = true;

            // 初始化游戏引擎
            InitializeGameEngine();

            // 创建测试场景
            CreateTestScene();

            // 绑定窗体事件
            Paint += GameEngineCoreTest_Paint;
            KeyDown += GameEngineCoreTest_KeyDown;
            KeyUp += GameEngineCoreTest_KeyUp;
            MouseMove += GameEngineCoreTest_MouseMove;
            MouseDown += GameEngineCoreTest_MouseDown;
            MouseUp += GameEngineCoreTest_MouseUp;
            FormClosed += GameEngineCoreTest_FormClosed;

            // 创建计时器用于重绘窗体
            Timer timer = new Timer();
            timer.Interval = 16; // 约60 FPS
            timer.Tick += Timer_Tick;
            timer.Start();

            // 启动游戏循环
            gameLoop.Start();
        }

        /// <summary>
        /// 初始化游戏引擎
        /// </summary>
        private void InitializeGameEngine()
        {
            // 创建游戏循环
            gameLoop = new GameLoop();
            gameLoop.TargetElapsedTime = 1.0f / 60.0f; // 60 FPS

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

            // 添加更新对象到游戏循环
            gameLoop.AddUpdateable(sceneManager);
            gameLoop.AddUpdateable(world);

            // 初始化时间系统
            Time.Initialize();
        }

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
            playerSprite.Scale = new Vector2(50, 50); // 50x50 像素大小
            playerEntity.AddComponent(playerSprite);

            // 创建随机彩色图片作为玩家精灵
            Bitmap playerBitmap = new Bitmap(50, 50);
            using (Graphics g = Graphics.FromImage(playerBitmap))
            {
                g.Clear(Color.Blue);
                g.FillEllipse(Brushes.Red, 10, 10, 30, 30);
            }
            playerSprite.Texture = playerBitmap;

            // 创建相机实体
            cameraEntity = world.CreateEntity("Camera");

            // 添加相机组件
            Camera2D camera = new Camera2D();
            camera.ViewportSize = new SizeF(ClientSize.Width, ClientSize.Height);
            camera.Target = playerEntity; // 相机跟随玩家
            camera.Damping = 0.1f; // 添加一些平滑
            cameraEntity.AddComponent(camera);

            // 设置渲染器的相机
            renderer.Camera = camera;

            // 创建一些背景元素
            for (int i = 0; i < 10; i++)
            {
                Entity backgroundEntity = world.CreateEntity($"Background_{i}");

                // 随机位置
                float x = (float)new Random().NextDouble() * 800;
                float y = (float)new Random().NextDouble() * 600;
                Transform2D transform = new Transform2D(new Vector2(x, y));
                backgroundEntity.AddComponent(transform);

                // 随机颜色的方块
                Sprite sprite = new Sprite();
                sprite.Origin = new Vector2(0.5f, 0.5f);
                sprite.Scale = new Vector2(20, 20);
                backgroundEntity.AddComponent(sprite);

                // 创建随机颜色图片
                Bitmap bitmap = new Bitmap(20, 20);
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    Color randomColor = Color.FromArgb(
                        new Random().Next(100, 255),
                        new Random().Next(100, 255),
                        new Random().Next(100, 255));
                    g.Clear(randomColor);
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
        private void GameUpdate(float deltaTime)
        {
            // 检查玩家输入
            HandlePlayerInput(deltaTime);
        }

        /// <summary>
        /// 处理玩家输入
        /// </summary>
        private void HandlePlayerInput(float deltaTime)
        {
            // 获取玩家变换组件
            Transform2D playerTransform = playerEntity.GetComponent<Transform2D>();
            if (playerTransform == null)
                return;

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

            // 应用移动
            playerTransform.Position += movement * moveSpeed * deltaTime;

            // 如果空格键被按下，旋转玩家
            if (inputManager.IsKeyDown(Keys.Space))
            {
                playerTransform.Rotation += 2.0f * deltaTime;
            }

            // 如果按下ESC键，退出程序
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
            if (graphics == null)
                return;

            // 确保缓冲图形正确初始化
            if (bufferedGraphics == null || context == null)
            {
                context = BufferedGraphicsManager.Current;
                context.MaximumBuffer = new Size(ClientSize.Width + 1, ClientSize.Height + 1);
                bufferedGraphics = context.Allocate(graphics, new Rectangle(0, 0, ClientSize.Width, ClientSize.Height));
            }

            // 设置渲染目标
            renderer.RenderTarget = bufferedGraphics.Graphics;
            renderer.ClearColor = Color.CornflowerBlue;

            // 渲染帧
            renderer.Render();

            // 添加一些调试信息
            DrawDebugInfo(bufferedGraphics.Graphics);

            // 呈现缓冲的图形
            bufferedGraphics.Render();
        }

        /// <summary>
        /// 绘制调试信息
        /// </summary>
        private void DrawDebugInfo(Graphics g)
        {
            // 设置文本格式
            Font font = new Font("Arial", 10);
            Brush brush = Brushes.White;

            // 获取玩家位置
            Transform2D playerTransform = playerEntity?.GetComponent<Transform2D>();
            string playerPos = playerTransform != null
                ? $"位置: ({playerTransform.Position.X:F1}, {playerTransform.Position.Y:F1})"
                : "未知";

            // 绘制FPS和玩家位置
            g.DrawString($"FPS: {Time.FramesPerSecond:F1}", font, brush, 10, 10);
            g.DrawString($"玩家{playerPos}", font, brush, 10, 30);
            g.DrawString("使用WASD或方向键移动，空格键旋转，ESC退出", font, brush, 10, 50);
            g.DrawString($"实体总数: {world.GetAllEntities().Count}", font, brush, 10, 70);
        }

        #region 窗体事件处理

        private void Timer_Tick(object sender, EventArgs e)
        {
            // 重绘窗体
            Invalidate();
        }

        private void GameEngineCoreTest_Paint(object sender, PaintEventArgs e)
        {
            // 保存图形对象以供渲染使用
            graphics = e.Graphics;
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
            if (bufferedGraphics != null)
            {
                bufferedGraphics.Dispose();
                bufferedGraphics = null;
            }
        }

        #endregion
    }
}