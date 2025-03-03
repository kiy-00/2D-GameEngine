using System;
using System.Windows.Forms;
using GameEngine.Common;
using GameEngine.Core;
using SkiaSharp;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
using Keys = GameEngine.Core.Keys;
using Time = GameEngine.Core.Time;
// ��ʽ�����Ա������
using WinFormsTimer = System.Windows.Forms.Timer;
using GameTimer = GameEngine.Common.Timer;


namespace GameEngineTest
{
    /// <summary>
    /// ��Ϸ������Ĳ��Գ���
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
        /// ������ڵ�
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new GameEngineCoreTest());
        }

        /// <summary>
        /// ���캯��
        /// </summary>
        public GameEngineCoreTest()
        {
            // ��������
            Text = "GameEngine.Core ���Գ���";
            Size = new System.Drawing.Size(800, 600);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            DoubleBuffered = true;

            // ��ʼ����Ϸ����
            InitializeGameEngine();

            // �������Գ���
            CreateTestScene();

            // ��ʼ����Ⱦλͼ
            renderBitmap = new SKBitmap(ClientSize.Width, ClientSize.Height);

            // �󶨴����¼�
            Paint += GameEngineCoreTest_Paint;
            KeyDown += GameEngineCoreTest_KeyDown;
            KeyUp += GameEngineCoreTest_KeyUp;
            MouseMove += GameEngineCoreTest_MouseMove;
            MouseDown += GameEngineCoreTest_MouseDown;
            MouseUp += GameEngineCoreTest_MouseUp;
            FormClosed += GameEngineCoreTest_FormClosed;
            Resize += GameEngineCoreTest_Resize;

            // ������ʱ�������ػ洰��
            WinFormsTimer timer = new WinFormsTimer();
            timer.Interval = 16; // Լ60 FPS
            timer.Tick += Timer_Tick;
            timer.Start();

            // ��ǳ�ʼ�����
            isInitialized = true;

            // ������Ϸѭ��
            gameLoop.Start();
        }

        /// <summary>
        /// ��ʼ����Ϸ����
        /// </summary>
        // 3. ��InitializeGameEngine����Ӷ����飬ȷ��GameLoop��ȷ����
        private void InitializeGameEngine()
        {
            // ������Ϸѭ��
            gameLoop = new GameLoop();
            gameLoop.TargetElapsedTime = 1.0f / 60.0f; // 60 FPS
            System.Diagnostics.Debug.WriteLine($"GameLoopĿ��֡ʱ��: {gameLoop.TargetElapsedTime}");

            // ������Ϸ����
            world = new World("TestWorld");

            // ������Ⱦ��
            renderer = new Renderer();
            world.AddSystem(renderer);

            // �������������
            inputManager = new InputManager();
            inputManager.Initialize();
            world.AddSystem(inputManager);

            // ��������������
            sceneManager = SceneManager.Instance;
            sceneManager.SetEnableUpdate(true);

            // ������Ϸѭ���ص�
            gameLoop.UpdateCallback = GameUpdate;
            gameLoop.RenderCallback = GameRender;

            // ��ӿɸ��¶�����Ϸѭ��
            gameLoop.AddUpdateable(sceneManager);
            gameLoop.AddUpdateable(world);

            // ��ʼ��ʱ��ϵͳ
            Time.Initialize();
            System.Diagnostics.Debug.WriteLine("ʱ��ϵͳ�ѳ�ʼ��");
        }

        /// <summary>
        /// �������Գ���
        /// </summary>
        /// <summary>
        /// �������Գ���
        /// </summary>
        private void CreateTestScene()
        {
            // �������Գ���
            testScene = new Scene("TestScene");
            sceneManager.RegisterScene(testScene);

            // �������ʵ��
            playerEntity = world.CreateEntity("Player");

            // ��ӱ任���
            Transform2D playerTransform = new Transform2D(new Vector2(400, 300));
            playerEntity.AddComponent(playerTransform);

            // ��Ӿ������
            Sprite playerSprite = new Sprite();
            playerSprite.Origin = new Vector2(0.5f, 0.5f); // ���ĵ�
            playerSprite.Scale = new Vector2(2, 2); // ����100x100���ش�С
            playerEntity.AddComponent(playerSprite);

            // ����������Ŀ����Ҿ���
            SKBitmap playerBitmap = new SKBitmap(50, 50);
            using (SKCanvas canvas = new SKCanvas(playerBitmap))
            {
                canvas.Clear(SKColors.Yellow); // ��Ϊ��ɫ����
                using (SKPaint paint = new SKPaint
                {
                    Color = SKColors.Black, // ��Ϊ��ɫǰ��
                    Style = SKPaintStyle.
        Fill
                })
                {
                    canvas.DrawOval(new SKRect(5, 5, 45, 45), paint); // ������Բ
                }
            }
            playerSprite.Texture = playerBitmap;

            // �������ʵ��
            cameraEntity = world.CreateEntity("Camera");

            // ���������
            Camera2D camera = new Camera2D();
            camera.ViewportSize = new SKSize(ClientSize.Width, ClientSize.Height);
            camera.Target = null; // ��ʱ���������
            camera.Position = new Vector2(ClientSize.Width / 2, ClientSize.Height / 2); // �̶�λ��
            cameraEntity.AddComponent(camera);

            // ������Ⱦ�������
            renderer.Camera = camera;

            // ����һЩ����Ԫ��
            Random random = new Random();
            for (int i = 0; i < 10; i++)
            {
                Entity backgroundEntity = world.CreateEntity($"Background_{i}");

                // ���λ��
                float x = (float)random.NextDouble() * 800;
                float y = (float)random.NextDouble() * 600;
                Transform2D transform = new Transform2D(new Vector2(x, y));
                backgroundEntity.AddComponent(transform);

                // �����ɫ�ķ���
                Sprite sprite = new Sprite();
                sprite.Origin = new Vector2(0.5f, 0.5f);
                sprite.Scale = new Vector2(20, 20);
                backgroundEntity.AddComponent(sprite);

                // ���������ɫͼƬ
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

            // ���벢�����
            testScene.Load();
            testScene.Activate();
            sceneManager.ChangeScene("TestScene", true);
        }

        /// <summary>
        /// ��Ϸ���»ص�
        /// </summary>
        // ��Ϸ���»ص������ʱ�����
        // 1. �޸�GameUpdate��������Ӹ���ϸ��ʱ�������Ϣ
        private void GameUpdate(float deltaTime)
        {
            try
            {
                // �����ϸ��deltaTime��־
                System.Diagnostics.Debug.WriteLine($"GameUpdate�����ã�deltaTime={deltaTime}");

                // ���Time���Ƿ���ȷ��ʼ��
                System.Diagnostics.Debug.WriteLine($"����ǰ��Time.DeltaTime: {Time.DeltaTime}");

                // ����ʱ��ϵͳ
                Time.Update();

                // ���Time���Ƿ���ȷ����
                System.Diagnostics.Debug.WriteLine($"���º��Time.DeltaTime: {Time.DeltaTime}");

                // ��������Ƿ�Ӧ��ʹ��Time.DeltaTime�����Ǵ���Ĳ���
                float effectiveDeltaTime = deltaTime > 0 ? deltaTime : Time.DeltaTime;
                System.Diagnostics.Debug.WriteLine($"ʹ�õ���ЧdeltaTime: {effectiveDeltaTime}");

                // ʹ����Ч��deltaʱ�����������
                HandlePlayerInput(effectiveDeltaTime);

                // �������
                System.Diagnostics.Debug.WriteLine($"���ʵ�����: {playerEntity != null}");
                System.Diagnostics.Debug.WriteLine($"�����transform���: {playerEntity?.GetComponent<Transform2D>() != null}");
                System.Diagnostics.Debug.WriteLine($"�����sprite���: {playerEntity?.GetComponent<Sprite>() != null}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"�쳣����: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"��ջ����: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// �����������
        /// </summary>
        // 2. �޸�HandlePlayerInput�������Ƴ��ظ����ƶ�Ӧ��
        private void HandlePlayerInput(float deltaTime)
        {
            // �������
            System.Diagnostics.Debug.WriteLine($"HandlePlayerInput: deltaTime = {deltaTime}");

            if (deltaTime <= 0)
            {
                System.Diagnostics.Debug.WriteLine("����: deltaTimeΪ���ֵ���ƶ���������!");
                return;  // ʹ����Ч��deltaTimeʱ�������ƶ�
            }

            // ��ȡ��ұ任���
            Transform2D playerTransform = playerEntity.GetComponent<Transform2D>();
            if (playerTransform == null)
            {
                System.Diagnostics.Debug.WriteLine("����: ���transformΪ��!");
                return;
            }

            // �ƶ��ٶ�
            const float moveSpeed = 200.0f;

            // ���ݼ���״̬�ƶ����
            Vector2 movement = Vector2.Zero;

            if (inputManager.IsKeyDown(Keys.W) || inputManager.IsKeyDown(Keys.Up))
                movement.Y -= 1;

            if (inputManager.IsKeyDown(Keys.S) || inputManager.IsKeyDown(Keys.Down))
                movement.Y += 1;

            if (inputManager.IsKeyDown(Keys.A) || inputManager.IsKeyDown(Keys.Left))
                movement.X -= 1;

            if (inputManager.IsKeyDown(Keys.D) || inputManager.IsKeyDown(Keys.Right))
                movement.X += 1;

            // ��һ��������ȷ���Խ����ƶ��ٶ���ͬ
            if (movement.Length > 0)
                movement = movement.Normalized;

            // ���Լ���״̬
            System.Diagnostics.Debug.WriteLine($"��W: {inputManager.IsKeyDown(Keys.W)}, �ϼ�: {inputManager.IsKeyDown(Keys.Up)}");
            System.Diagnostics.Debug.WriteLine($"�ƶ�����: {movement}, ���ź�: {movement * moveSpeed * deltaTime}");

            // ��Ӧ��һ���ƶ�(�Ƴ����ظ�����)
            Vector2 positionChange = movement * moveSpeed * deltaTime;
            playerTransform.Position += positionChange;

            // ������º��λ��
            System.Diagnostics.Debug.WriteLine($"��λ��: {playerTransform.Position}");

            // ����ո�������£���ת���
            if (inputManager.IsKeyDown(Keys.Space))
            {
                playerTransform.Rotation += 2.0f * deltaTime;
            }

            // ���ESC�������£��˳�Ӧ�ó���
            if (inputManager.IsKeyPressed(Keys.Escape))
            {
                Application.Exit();
            }
        }

        /// <summary>
        /// ��Ϸ��Ⱦ�ص�
        /// </summary>
        private void GameRender(float deltaTime)
        {
            if (!isInitialized)
                return;

            try
            {
                // ȷ��λͼ��С��ȷ
                if (renderBitmap == null || renderBitmap.Width != ClientSize.Width || renderBitmap.Height != ClientSize.Height)
                {
                    renderBitmap?.Dispose();
                    renderBitmap = new SKBitmap(Math.Max(1, ClientSize.Width), Math.Max(1, ClientSize.Height));

                    // ��������ӿ�
                    Camera2D camera = cameraEntity?.GetComponent<Camera2D>();
                    if (camera != null)
                    {
                        camera.ViewportSize = new SKSize(ClientSize.Width, ClientSize.Height);
                    }
                }

                // ʹ�ð�ȫ��Ⱦ���ֱ��ʹ����Ⱦ��
                SafeRender();

                // ���������ػ�
                Invalidate();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GameRender�쳣: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"��ջ����: {ex.StackTrace}");
            }
        }
        /// <summary>
        /// ���Ƶ�����Ϣ
        /// </summary>
        // ����DrawDebugInfo��������Ӹ��������Ϣ
        private void DrawDebugInfo(SKCanvas canvas)
        {
            // �����ı����ƵĻ���
            using (SKPaint textPaint = new SKPaint
            {
                Color = SKColors.White,
                TextSize = 12,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName("Arial")
            })
            {
                // ��ȡ���λ��
                Transform2D playerTransform = playerEntity?.GetComponent<Transform2D>();
                string playerPos = playerTransform != null
                    ? $"λ��: ({playerTransform.Position.X:F1}, {playerTransform.Position.Y:F1}), ��ת: {playerTransform.Rotation:F2}"
                    : "δ֪";

                // ����FPS�����λ��
                canvas.DrawText($"FPS: {Time.FramesPerSecond:F1}", 10, 20, textPaint);
                canvas.DrawText($"Player{playerPos}", 10, 40, textPaint);
                canvas.DrawText("ʹ��WASD������ƶ����ո����ת��ESC�˳�", 10, 60, textPaint);
                canvas.DrawText($"ʵ������: {world.GetAllEntities().Count}", 10, 80, textPaint);
                canvas.DrawText($"��Ļ��������: ({ClientSize.Width / 2}, {ClientSize.Height / 2})", 10, 100, textPaint);

                // ��Ӽ���״̬��Ϣ
                string keyState = $"W: {inputManager.IsKeyDown(Keys.W)}, A: {inputManager.IsKeyDown(Keys.A)}, " +
                                 $"S: {inputManager.IsKeyDown(Keys.S)}, D: {inputManager.IsKeyDown(Keys.D)}";
                canvas.DrawText(keyState, 10, 120, textPaint);
            }
        }

        #region �����¼�����

        private void GameEngineCoreTest_Paint(object sender, PaintEventArgs e)
        {
            if (renderBitmap != null && !renderBitmap.IsEmpty)
            {
                try
                {
                    // ��ʾ SkiaSharp ��������
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
                    // ��������ʱʹ�� GDI+ ���ƴ�����Ϣ
                    e.Graphics.Clear(System.Drawing.Color.Black);
                    e.Graphics.DrawString($"SkiaSharp ����: {ex.Message}",
                        new System.Drawing.Font("Arial", 12),
                        System.Drawing.Brushes.Red, 10, 10);
                    System.Diagnostics.Debug.WriteLine($"Paint ����: {ex.Message}");
                }
            }
            else
            {
                e.Graphics.Clear(System.Drawing.Color.Black);
                e.Graphics.DrawString("renderBitmap Ϊ��",
                    new System.Drawing.Font("Arial", 12),
                    System.Drawing.Brushes.Yellow, 10, 10);
            }
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            // ����Ҫ�κβ�����GameRender�����Invalidate�����ػ�
        }

        private void GameEngineCoreTest_Resize(object sender, EventArgs e)
        {
            // �����С�ı�ʱ����Ⱦλͼ������һ֡GameRender�е�����С
        }

        private void GameEngineCoreTest_KeyDown(object sender, KeyEventArgs e)
        {
            // ���¼���״̬
            KeyboardState keyboardState = inputManager.KeyboardState.Clone();
            keyboardState.SetKeyDown((Keys)e.KeyCode);
            inputManager.SetKeyboardState(keyboardState);
        }

        private void GameEngineCoreTest_KeyUp(object sender, KeyEventArgs e)
        {
            // ���¼���״̬
            KeyboardState keyboardState = inputManager.KeyboardState.Clone();
            keyboardState.SetKeyUp((Keys)e.KeyCode);
            inputManager.SetKeyboardState(keyboardState);
        }

        private void GameEngineCoreTest_MouseMove(object sender, MouseEventArgs e)
        {
            // �������״̬
            MouseState mouseState = inputManager.MouseState.Clone();
            mouseState.Position = new Vector2(e.X, e.Y);
            inputManager.SetMouseState(mouseState);
        }

        private void GameEngineCoreTest_MouseDown(object sender, MouseEventArgs e)
        {
            // �������״̬
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
            // �������״̬
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
            // ֹͣ��Ϸѭ��
            gameLoop.Stop();

            // �ͷ���Դ
            renderBitmap?.Dispose();
        }

        #endregion

        // �� GameEngineTest �������һ����ȫ����Ⱦ����
        // �޸ĺ�İ�ȫ��Ⱦ����
        private void SafeRender()
        {
            try
            {
                if (renderer != null && renderBitmap != null)
                {
                    using (SKCanvas canvas = new SKCanvas(renderBitmap))
                    {
                        // ���ֶ�����������ʹ����Ⱦ���� Clear
                        canvas.Clear(SKColors.CornflowerBlue);

                        // �������Ƿ���Ч
                        Camera2D camera = renderer.Camera;
                        if (camera == null || cameraEntity == null)
                        {
                            // û�������ʹ�ü򵥻���
                            SimpleRenderWithoutCamera(canvas);
                            return;
                        }

                        // �ֶ����Ƹ��㼶����
                        try
                        {
                            // �ֶ���ȡ��ʹ��ʵ������
                            var entities = world.GetAllEntities();
                            var backgroundEntities = entities.Where(e => e != playerEntity && e != cameraEntity).ToList();

                            // �Ȼ��Ʊ���ʵ��
                            foreach (var entity in backgroundEntities)
                            {
                                RenderEntity(canvas, entity);
                            }

                            // ���������ʵ����ȷ���ڶ���
                            RenderEntity(canvas, playerEntity);

                            // ����Ļ���Ļ��Ʋο����
                            using (SKPaint paint = new SKPaint { Color = SKColors.White, Style = SKPaintStyle.Stroke, StrokeWidth = 2 })
                            {
                                canvas.DrawCircle(ClientSize.Width / 2, ClientSize.Height / 2, 10, paint);
                                canvas.DrawLine(ClientSize.Width / 2 - 15, ClientSize.Height / 2, ClientSize.Width / 2 + 15, ClientSize.Height / 2, paint);
                                canvas.DrawLine(ClientSize.Width / 2, ClientSize.Height / 2 - 15, ClientSize.Width / 2, ClientSize.Height / 2 + 15, paint);
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"ʵ����Ⱦ����: {ex.Message}");
                        }

                        // ��ӵ�����Ϣ
                        DrawDebugInfo(canvas);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SafeRender�쳣: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"��ջ����: {ex.StackTrace}");
            }
        }

        // ����µĸ�������������Ⱦ����ʵ��
        private void RenderEntity(SKCanvas canvas, Entity entity)
        {
            if (entity == null)
                return;

            var sprite = entity.GetComponent<Sprite>();
            var transform = entity.GetComponent<Transform2D>();

            if (sprite != null && transform != null && sprite.Texture != null)
            {
                // ���浱ǰ����
                canvas.Save();

                // �ֶ�Ӧ�ñ任
                float x = transform.Position.X;
                float y = transform.Position.Y;
                float rotation = transform.Rotation;
                float scaleX = transform.Scale.X * sprite.Scale.X;
                float scaleY = transform.Scale.Y * sprite.Scale.Y;

                System.Diagnostics.Debug.WriteLine($"��Ⱦʵ�� {entity.Name}: λ��=({x}, {y}), ��ת={rotation}, ����=({scaleX}, {scaleY})");

                // �ƶ���λ��
                canvas.Translate(x, y);

                // Ӧ����ת
                canvas.RotateDegrees(rotation * MathHelper.RadToDeg);

                // Ӧ������
                canvas.Scale(scaleX, scaleY);

                // Ӧ��ԭ��ƫ��
                float originX = -sprite.Origin.X;
                float originY = -sprite.Origin.Y;
                canvas.Translate(originX, originY);

                // ���ƾ���
                using (SKPaint paint = new SKPaint { FilterQuality = SKFilterQuality.High })
                {
                    // �������ʵ�壬ʹ�ø��ߵĲ�͸����
                    if (entity == playerEntity)
                    {
                        paint.Color = SKColors.White.WithAlpha(255); // ��ȫ��͸��
                    }
                    canvas.DrawBitmap(sprite.Texture, 0, 0, paint);
                }

                // �ָ�����
                canvas.Restore();
            }
        }

        // ����Ⱦ���������������
        private void SimpleRenderWithoutCamera(SKCanvas canvas)
        {
            // ���Ʋ���Ԫ��
            using (SKPaint textPaint = new SKPaint { Color = SKColors.White, TextSize = 24, IsAntialias = true })
            {
                canvas.DrawText("����Ⱦģʽ���������", 50, 50, textPaint);
            }

            // �������λ��
            Transform2D playerTransform = playerEntity?.GetComponent<Transform2D>();
            if (playerTransform != null)
            {
                float x = playerTransform.Position.X;
                float y = playerTransform.Position.Y;

                // �������ָʾ��
                using (SKPaint paint = new SKPaint { Color = SKColors.Red, Style = SKPaintStyle.Fill })
                {
                    canvas.DrawCircle(x, y, 25, paint);
                }

                // ������Ҿ��飨����У�
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
                        System.Diagnostics.Debug.WriteLine($"��Ҿ�����ƴ���: {ex.Message}");
                    }
                }
            }

            // ��ӵ�����Ϣ
            DrawDebugInfo(canvas);
        }
    }
}