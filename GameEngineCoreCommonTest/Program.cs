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
            "�����ƶ�����",
            "������Ⱦ����",
            "�����л�����",
            "������Ӧ����",
            "����������"
        };

        // ��Ӵ������Լ�����˸
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000; // WS_EX_COMPOSITED ��־
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
            // ��������
            Text = "GameEngine �ۺϲ��Գ���";
            Size = new System.Drawing.Size(1024, 768);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            DoubleBuffered = true;

            // ��ʼ������
            InitializeEngine();

            // �������Գ���
            CreateMainScene();

            // ������Ⱦλͼ
            renderBitmap = new SKBitmap(ClientSize.Width, ClientSize.Height);

            // ���¼�
            Paint += GameEngineTest_Paint;
            KeyDown += GameEngineTest_KeyDown;
            KeyUp += GameEngineTest_KeyUp;
            MouseMove += GameEngineTest_MouseMove;
            MouseDown += GameEngineTest_MouseDown;
            MouseUp += GameEngineTest_MouseUp;
            FormClosed += GameEngineTest_FormClosed;
            Resize += GameEngineTest_Resize;

            // ��������ӵ��Դ���
            KeyDown += (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine($"���̰���: {e.KeyCode}");
            };

            // �����ػ��ʱ��
            WinFormsTimer timer = new WinFormsTimer();
            timer.Interval = 16; // Լ60 FPS
            timer.Tick += (s, e) =>
            {
                // ֻ����Ҫʱ�ػ�
                if (renderBitmap != null && !renderBitmap.IsEmpty)
                    Invalidate(false); // false ������ʾ��������֤�ͻ�����
            };
            timer.Start();

            isInitialized = true;
            gameLoop.Start();

            // ��¼����
            System.Diagnostics.Debug.WriteLine("���Գ���������");
        }

        private void InitializeEngine()
        {
            // ��Ϸѭ��
            gameLoop = new GameLoop();
            gameLoop.TargetElapsedTime = 1.0f / 60.0f;

            // ��Ϸ����
            world = new World("TestWorld");

            // ��Ⱦ��
            renderer = new Renderer();
            world.AddSystem(renderer);

            // ���������
            inputManager = new InputManager();
            inputManager.Initialize();
            world.AddSystem(inputManager);

            // ����������
            // ����������
            sceneManager = SceneManager.Instance;
            sceneManager.SetEnableUpdate(true);
          

            // ���ûص�
            gameLoop.UpdateCallback = GameUpdate;
            gameLoop.RenderCallback = GameRender;

            // ��ӿɸ��¶���
            gameLoop.AddUpdateable(sceneManager);
            gameLoop.AddUpdateable(world);

            // ʱ��ϵͳ
            Time.Initialize();

            System.Diagnostics.Debug.WriteLine("�����ʼ�����");
        }

        private void CreateMainScene()
        {
            mainScene = new Scene("MainScene");
            sceneManager.RegisterScene(mainScene);

            // �������
            cameraEntity = world.CreateEntity("MainCamera");
            var camera = new Camera2D();
            camera.ViewportSize = new SKSize(ClientSize.Width, ClientSize.Height);
            camera.Position = new Vector2(ClientSize.Width / 2, ClientSize.Height / 2);
            cameraEntity.AddComponent(camera);
            renderer.Camera = camera;

            // �������
            playerEntity = CreatePlayerEntity();

            // ��������ʵ��
            CreateTestEntities();

            // �����
            mainScene.Load();
            mainScene.Activate();
            sceneManager.ChangeScene("MainScene", true);
            // ��CreateMainScene������ĩβ���
            //mainScene.World.AddEntity(cameraEntity);
            //mainScene.World.AddEntity(playerEntity);

            System.Diagnostics.Debug.WriteLine("�������������");
        }

        private Entity CreatePlayerEntity()
        {
            var entity = world.CreateEntity("Player");

            // �任���
            var transform = new Transform2D(new Vector2(ClientSize.Width / 2, ClientSize.Height / 2));
            entity.AddComponent(transform);

            // �������
            var sprite = new Sprite();
            sprite.Origin = new Vector2(0.5f, 0.5f);
            sprite.Scale = new Vector2(1, 1);
            entity.AddComponent(sprite);

            // �������ͼ��
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

                // ��ӷ���ָʾ
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

            // ����ɵĲ���ʵ��
            foreach (var entity in testEntities)
            {
                world.DestroyEntity(entity);
            }
            testEntities.Clear();

            // ����Χ����ҵĶ������ʵ��
            for (int i = 0; i < 20; i++)
            {
                var entity = world.CreateEntity($"TestEntity_{i}");

                // ���λ��
                float angle = (float)i / 20 * MathHelper.TwoPi;
                float distance = 200 + random.Next(0, 100);
                float x = ClientSize.Width / 2 + MathF.Cos(angle) * distance;
                float y = ClientSize.Height / 2 + MathF.Sin(angle) * distance;

                // �任���
                var transform = new Transform2D(new Vector2(x, y));
                transform.Rotation = angle;
                entity.AddComponent(transform);

                // �������
                var sprite = new Sprite();
                sprite.Origin = new Vector2(0.5f, 0.5f);
                sprite.Scale = new Vector2(1, 1);
                entity.AddComponent(sprite);

                // ���������ɫ��ͼ��
                byte r = (byte)random.Next(100, 255);
                byte g = (byte)random.Next(100, 255);
                byte b = (byte)random.Next(100, 255);

                SKBitmap bitmap = new SKBitmap(30, 30);
                using (SKCanvas canvas = new SKCanvas(bitmap))
                {
                    canvas.Clear(new SKColor(r, g, b));

                    // ��ӱ߿�
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

            System.Diagnostics.Debug.WriteLine($"������ {testEntities.Count} ������ʵ��");
        }

        private void GameUpdate(float deltaTime)
        {
            if (!isInitialized)
                return;

            try
            {
                // ����ʱ��
                Time.Update();

                // ʹ�ú��ʵ�ʱ�䲽��
                float effectiveDeltaTime = deltaTime > 0 ? deltaTime : Time.DeltaTime;

                // ��������
                HandleInput(effectiveDeltaTime);

                // ���ݵ�ǰ���Գ���ִ���ض��߼�
                ExecuteTestLogic(effectiveDeltaTime);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"���´���: {ex.Message}");
            }
        }

        private void HandleInput(float deltaTime)
        {
            // �л����Գ���
            if (inputManager.IsKeyPressed(Keys.Tab))
            {
                currentTestIndex = (currentTestIndex + 1) % testNames.Length;
                System.Diagnostics.Debug.WriteLine($"�л�������: {testNames[currentTestIndex]}");

                // Ϊ�²���׼������
                if (currentTestIndex == 2) // �����л�����
                {
                    PrepareSceneSwitchingTest();
                }
                else if (currentTestIndex == 4) // ����������
                {
                    PrepareFollowCameraTest();
                }
                else
                {
                    // �������
                    Camera2D camera = cameraEntity.GetComponent<Camera2D>();
                    if (camera != null)
                    {
                        camera.Position = new Vector2(ClientSize.Width / 2, ClientSize.Height / 2);
                        camera.Target = null;
                    }
                }
            }

            // �л�������Ϣ��ʾ
            if (inputManager.IsKeyPressed(Keys.F1))
            {
                showDebugInfo = !showDebugInfo;
            }

            // �����ƶ�����
            if (currentTestIndex == 0)
            {
                MovePlayer(deltaTime);
            }

            // ������Ӧ����
            if (currentTestIndex == 3)
            {
                HandleInputResponseTest(deltaTime);
            }

            // ESC�˳�Ӧ��
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

            // �����ƶ�
            if (inputManager.IsKeyDown(Keys.W) || inputManager.IsKeyDown(Keys.Up))
                movement.Y -= 1;

            if (inputManager.IsKeyDown(Keys.S) || inputManager.IsKeyDown(Keys.Down))
                movement.Y += 1;

            if (inputManager.IsKeyDown(Keys.A) || inputManager.IsKeyDown(Keys.Left))
                movement.X -= 1;

            if (inputManager.IsKeyDown(Keys.D) || inputManager.IsKeyDown(Keys.Right))
                movement.X += 1;

            // �淶����Ӧ���ƶ�
            if (movement.Length > 0)
            {
                movement = movement.Normalized;
                transform.Position += movement * moveSpeed * deltaTime;
            }

            // ��ת (�������λ��)
            Vector2 mousePos = inputManager.MouseState.Position;
            Vector2 direction = mousePos - transform.Position;

            if (direction.Length > 0)
            {
                transform.Rotation = MathF.Atan2(direction.Y, direction.X);
            }
        }

        private void PrepareFollowCameraTest()
        {
            // ��������������
            Camera2D camera = cameraEntity.GetComponent<Camera2D>();
            if (camera != null)
            {
                camera.Target = playerEntity;
                camera.LerpFactor = 0.1f; // ƽ������
                System.Diagnostics.Debug.WriteLine("�������Ϊ�������");
            }
        }

        private void PrepareSceneSwitchingTest()
        {
            // ���SecondScene�Ƿ��Ѵ���
            Scene existingScene = sceneManager.GetScene("SecondScene");
            if (existingScene != null)
            {
                System.Diagnostics.Debug.WriteLine("�ڶ������Ѵ��ڣ��������´���");
                return;
            }
            System.Diagnostics.Debug.WriteLine("��ʼ�����ڶ�����");

            // �����ڶ�������
            var secondScene = new Scene("SecondScene");
            sceneManager.RegisterScene(secondScene);
            System.Diagnostics.Debug.WriteLine("�ڶ�������ע�ᵽ����������");

            // ֱ��ʹ�ó�����World����ʵ�壨����ʵ���Ѿ��Զ��볡����World������
            var backgroundEntity = secondScene.World.CreateEntity("Background");

            // ������
            var bgTransform = new Transform2D(new Vector2(ClientSize.Width / 2, ClientSize.Height / 2));
            backgroundEntity.AddComponent(bgTransform);

            var bgSprite = new Sprite();
            bgSprite.Origin = new Vector2(0.5f, 0.5f);
            bgSprite.Scale = new Vector2(1, 1);
            backgroundEntity.AddComponent(bgSprite);

            // �������䱳��
            SKBitmap bgBitmap = new SKBitmap(ClientSize.Width, ClientSize.Height);
            using (SKCanvas canvas = new SKCanvas(bgBitmap))
            {
                // ԭ�еĽ��䱳����������...
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

                // ���һЩ����
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

                // ������� - ʹ��֧�����ĵ�����
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
                    canvas.DrawText("�ڶ�����", ClientSize.Width / 2, ClientSize.Height / 2, paint);
                }
            }
            bgSprite.Texture = bgBitmap;

            // ȷ����������ȷ���غͼ���
            secondScene.Load();
            System.Diagnostics.Debug.WriteLine("�ڶ������Ѽ���");
        }
        private void HandleInputResponseTest(float deltaTime)
        {
            // ��ȡ���λ�ú�״̬
            Vector2 mousePos = inputManager.MouseState.Position;
            bool leftButtonDown = inputManager.MouseState.IsButtonDown(MouseButton.Left);

            // �������λ�õ����λ��
            if (leftButtonDown)
            {
                Transform2D transform = playerEntity.GetComponent<Transform2D>();
                if (transform != null)
                {
                    // ʹ�ò�ֵƽ���ƶ�
                    transform.Position = Vector2.Lerp(transform.Position, mousePos, 5.0f * deltaTime);
                }
            }

            // ������̰���Ч��
            if (inputManager.IsKeyDown(Keys.Space))
            {
                // �ո������ʱ�ı�������
                Sprite sprite = playerEntity.GetComponent<Sprite>();
                if (sprite != null && sprite.Texture != null)
                {
                    // ����Ч��
                    float pulseFactor = 1.0f + 0.2f * MathF.Sin((float)Time.TotalTime * 10.0f);
                    sprite.Scale = new Vector2(pulseFactor, pulseFactor);
                }
            }
            else
            {
                // �ָ�������С
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
                case 0: // �����ƶ�����
                    // �����ƶ���HandleInput�д���
                    break;

                case 1: // ������Ⱦ����
                    AnimateTestEntities(deltaTime);
                    break;

                case 2: // �����л�����
                        // ��ӵ��������ȷ�Ͽո��������
                    if (inputManager.IsKeyPressed(Keys.Space))
                    {
                        System.Diagnostics.Debug.WriteLine("�ո�������£������л�����");

                        // ��ȡ�����
                        Scene activeScene = sceneManager.GetActiveScene();
                        if (activeScene != null)
                        {
                            string currentSceneName = activeScene.Name;
                            string targetSceneName = currentSceneName == "MainScene" ? "SecondScene" : "MainScene";

                            System.Diagnostics.Debug.WriteLine($"��ǰ����: {currentSceneName}, Ŀ�곡��: {targetSceneName}");

                            // ȷ��Ŀ�곡������
                            Scene targetScene = sceneManager.GetScene(targetSceneName);
                            if (targetScene == null)
                            {
                                System.Diagnostics.Debug.WriteLine($"����: Ŀ�곡�� {targetSceneName} ������");
                                if (targetSceneName == "SecondScene")
                                {
                                    // ����ڶ����������ڣ����Դ�����
                                    PrepareSceneSwitchingTest();
                                }
                                return;
                            }

                            // �����л�����
                            bool success = sceneManager.ChangeScene(targetSceneName, true);
                            System.Diagnostics.Debug.WriteLine($"�����л� {(success ? "�ɹ�" : "ʧ��")}: {targetSceneName}");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("����: û�л����");
                        }
                    }
                    break;

                case 3: // ������Ӧ����
                    // ������Ӧ��HandleInput�д���
                    break;

                case 4: // ����������
                    // ���������PrepareFollowCameraTest�����ã���������ֻ��ȷ����ҿ����ƶ�
                    MovePlayer(deltaTime);

                    // ���һЩ����Ч������ת����ʵ��
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
            // ʹ����ʵ��Χ�������ת
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
                    // ���㵱ǰ�ǶȺ;���
                    Vector2 relativePos = transform.Position - playerPos;
                    float distance = relativePos.Length;
                    float angle = MathF.Atan2(relativePos.Y, relativePos.X);

                    // ��������������ת�ٶ�
                    float rotationSpeed = baseRotationSpeed * (1.0f + (float)i / testEntities.Count);

                    // ���½Ƕ�
                    angle += rotationSpeed * deltaTime;

                    // ����λ��
                    float newX = playerPos.X + MathF.Cos(angle) * distance;
                    float newY = playerPos.Y + MathF.Sin(angle) * distance;
                    transform.Position = new Vector2(newX, newY);

                    // ������ת
                    transform.Rotation = angle;

                    // ����Ч��
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
                // ȷ��λͼ�ߴ���ȷ
                if (renderBitmap.Width != ClientSize.Width || renderBitmap.Height != ClientSize.Height)
                {
                    renderBitmap?.Dispose();
                    renderBitmap = new SKBitmap(
                        Math.Max(1, ClientSize.Width),
                        Math.Max(1, ClientSize.Height));

                    // ��������ӿ�
                    Camera2D camera = cameraEntity?.GetComponent<Camera2D>();
                    if (camera != null)
                    {
                        camera.ViewportSize = new SKSize(ClientSize.Width, ClientSize.Height);
                    }
                }

                // ��Ⱦ��λͼ
                using (SKCanvas canvas = new SKCanvas(renderBitmap))
                {
                    // �������
                    canvas.Clear(SKColors.Black);

                    // ��Ⱦ����ʵ��
                    RenderEntities(canvas);

                    // ���Ƶ�����Ϣ
                    if (showDebugInfo)
                    {
                        DrawDebugInfo(canvas);
                    }
                }

                // ��UI���������д������¼���������˸
                Application.DoEvents();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"��Ⱦ����: {ex.Message}");
            }
        }

        private void RenderEntities(SKCanvas canvas)
        {
            // ��ȡ���
            Camera2D camera = cameraEntity?.GetComponent<Camera2D>();
            if (camera == null)
                return;

            // ��ȡ��ǰ�����
            Scene activeScene = sceneManager.GetActiveScene();

            // ������ʹ����World��ʵ��
            if (activeScene == null || activeScene.Name == "MainScene")
            {
                RenderMainWorldEntities(canvas, camera);
            }
            // ��������ʹ�ø��Գ���World��ʵ��
            else
            {
                var sceneEntities = activeScene.World.GetAllEntities();
                System.Diagnostics.Debug.WriteLine($"��Ⱦ���� {activeScene.Name} �е� {sceneEntities.Count} ��ʵ��");

                foreach (var entity in sceneEntities)
                {
                    RenderEntity(canvas, entity, camera);
                }
            }
        }

        private void RenderMainWorldEntities(SKCanvas canvas, Camera2D camera)
        {
            // ��ȡ����ʵ��
            var entities = world.GetAllEntities();

            // ���㼶���򣨼�ʾ�������� -> ����ʵ�� -> ��ң�
            var backgroundEntities = entities.Where(e => e != playerEntity && e != cameraEntity).ToList();

            // ��Ⱦ����
            foreach (var entity in backgroundEntities)
            {
                RenderEntity(canvas, entity, camera);
            }

            // ��Ⱦ���
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
                // ���浱ǰ״̬
                canvas.Save();

                // ������Ļ����
                Vector2 screenPos = camera.WorldToScreen(transform.Position);

                // Ӧ�ñ任
                canvas.Translate(screenPos.X, screenPos.Y);
                canvas.RotateDegrees(transform.Rotation * MathHelper.RadToDeg);
                canvas.Scale(transform.Scale.X * sprite.Scale.X, transform.Scale.Y * sprite.Scale.Y);

                // Ӧ��ê��ƫ��
                float originX = -sprite.Origin.X * sprite.Texture.Width;
                float originY = -sprite.Origin.Y * sprite.Texture.Height;
                canvas.Translate(originX, originY);

                // ���ƾ���
                using (SKPaint paint = new SKPaint { FilterQuality = SKFilterQuality.High })
                {
                    canvas.DrawBitmap(sprite.Texture, 0, 0, paint);
                }

                // �ָ�״̬
                canvas.Restore();
            }
        }

        private void DrawCameraBounds(SKCanvas canvas, Camera2D camera)
        {
            // ��������ӿڱ߽�
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
            // ����֧�����������SKTypeface
            using (SKTypeface typeface = SKTypeface.FromFamilyName("Microsoft YaHei", SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright))
            using (SKPaint textPaint = new SKPaint
            {
                Color = SKColors.White,
                TextSize = 14,
                IsAntialias = true,
                Typeface = typeface  // ʹ��֧�����ĵ�����
            })
            {
                // �����Ϣ
                Transform2D playerTransform = playerEntity?.GetComponent<Transform2D>();
                string playerPos = playerTransform != null
                    ? $"({playerTransform.Position.X:F1}, {playerTransform.Position.Y:F1})"
                    : "δ֪";

                // ��ȡ��ǰ���������
                Scene activeScene = sceneManager.GetActiveScene();
                string currentSceneName = activeScene != null ? activeScene.Name : "��";

                // FPS��ʱ����Ϣ
                canvas.DrawText($"FPS: {Time.FramesPerSecond:F1}", 10, 20, textPaint);
                canvas.DrawText($"��ʱ��: {Time.TotalTime:F1}��", 10, 40, textPaint);
                canvas.DrawText($"���λ��: {playerPos}", 10, 60, textPaint);
                canvas.DrawText($"ʵ������: {world.GetAllEntities().Count}", 10, 80, textPaint);
                canvas.DrawText($"��ǰ����: {currentSceneName}", 10, 100, textPaint);

                // ��ǰ����
                textPaint.Color = SKColors.Yellow;
                canvas.DrawText($"��ǰ����: {testNames[currentTestIndex]}", 10, 120, textPaint);

                // ����˵��
                textPaint.Color = SKColors.LightGray;
                textPaint.TextSize = 12;
                canvas.DrawText("Tab���л����� | F1���л�������Ϣ | Esc���˳�", 10, 140, textPaint);

                // ���ݵ�ǰ������ʾ�ض�˵��
                string instructions = "";
                switch (currentTestIndex)
                {
                    case 0:
                        instructions = "WASD/������ƶ��������Ƴ���";
                        break;
                    case 1:
                        instructions = "�۲�ʵ�嶯��Ч��";
                        break;
                    case 2:
                        instructions = "�ո���л�����";
                        break;
                    case 3:
                        instructions = "��ס����϶���ң��ո��������Ч";
                        break;
                    case 4:
                        instructions = "WASD/������ƶ��������������";
                        break;
                }
                canvas.DrawText(instructions, 10, 160, textPaint);
            }
        }

        #region �¼�����

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
                e.Graphics.DrawString($"��Ⱦ����: {ex.Message}",
                    new System.Drawing.Font("Arial", 12),
                    System.Drawing.Brushes.Red, 10, 10);

                System.Diagnostics.Debug.WriteLine($"���ƴ���: {ex.Message}");
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
            // �����С�ı�ʱ�����ǻ�����һ֡��Ⱦʱ����λͼ��С
        }

        private void GameEngineTest_FormClosed(object sender, FormClosedEventArgs e)
        {
            // ֹͣ��Ϸѭ����������Դ
            gameLoop.Stop();
            renderBitmap?.Dispose();

            System.Diagnostics.Debug.WriteLine("���Գ����ѹر�");
        }

        #endregion
    }
}