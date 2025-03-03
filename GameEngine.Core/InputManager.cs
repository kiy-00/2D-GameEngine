using System;
using System.Collections.Generic;
using GameEngine.Common;

namespace GameEngine.Core
{
    /// <summary>
    /// 输入管理器，管理键盘和鼠标输入
    /// </summary>
    public class InputManager : System
    {
        private KeyboardState currentKeyboardState = new KeyboardState();
        private KeyboardState previousKeyboardState = new KeyboardState();
        private MouseState currentMouseState = new MouseState();
        private MouseState previousMouseState = new MouseState();
        private Dictionary<string, InputAction> inputActions = new Dictionary<string, InputAction>();
        private bool isInitialized = false;

        /// <summary>
        /// 获取当前键盘状态
        /// </summary>
        public KeyboardState KeyboardState => currentKeyboardState;

        /// <summary>
        /// 获取前一帧的键盘状态
        /// </summary>
        public KeyboardState PreviousKeyboardState => previousKeyboardState;

        /// <summary>
        /// 获取当前鼠标状态
        /// </summary>
        public MouseState MouseState => currentMouseState;

        /// <summary>
        /// 获取前一帧的鼠标状态
        /// </summary>
        public MouseState PreviousMouseState => previousMouseState;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="updateOrder">更新优先级</param>
        public InputManager(int updateOrder = -1000) : base(updateOrder) // 高优先级，确保输入先于其他系统更新
        {
        }

        /// <summary>
        /// 初始化输入管理器
        /// </summary>
        public void Initialize()
        {
            if (!isInitialized)
            {
                // 注册系统相关的输入源
                RegisterInputSources();
                isInitialized = true;
            }
        }

        /// <summary>
        /// 注册输入源
        /// </summary>
        private void RegisterInputSources()
        {
            // 这里可以根据平台注册不同的输入源实现
            // 例如在Windows下可以使用WinForms或WPF的输入事件
            // 在其他平台可以使用特定平台的输入API
        }

        /// <summary>
        /// 初始化需要的组件类型
        /// </summary>
        protected override void InitializeRequiredComponents()
        {
            // 输入管理器是一个全局系统，不需要特定的组件
        }

        /// <summary>
        /// 更新输入状态
        /// </summary>
        /// <param name="deltaTime">时间间隔</param>
        protected override void UpdateSystem(float deltaTime)
        {
            // 保存上一帧的状态
            previousKeyboardState = currentKeyboardState.Clone();
            previousMouseState = currentMouseState.Clone();

            // 更新当前状态
            // 在实际应用中，这里的逻辑会根据输入源的实现而变化
            // 例如，可能会从平台特定的输入API中获取最新状态

            // 更新所有输入动作
            foreach (var action in inputActions.Values)
            {
                action.Update(this);
            }
        }

        /// <summary>
        /// 注册输入动作
        /// </summary>
        /// <param name="name">动作名称</param>
        /// <param name="action">输入动作</param>
        public void RegisterAction(string name, InputAction action)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Action name cannot be null or empty", nameof(name));

            if (action == null)
                throw new ArgumentNullException(nameof(action));

            inputActions[name] = action;
        }

        /// <summary>
        /// 移除输入动作
        /// </summary>
        /// <param name="name">动作名称</param>
        /// <returns>是否成功移除</returns>
        public bool RemoveAction(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;

            return inputActions.Remove(name);
        }

        /// <summary>
        /// 获取输入动作
        /// </summary>
        /// <param name="name">动作名称</param>
        /// <returns>输入动作，如果不存在则返回null</returns>
        public InputAction GetAction(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            if (inputActions.TryGetValue(name, out InputAction action))
                return action;

            return null;
        }

        /// <summary>
        /// 检查键是否正在按下
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>如果键正在按下则返回true，否则返回false</returns>
        public bool IsKeyDown(Keys key)
        {
            return currentKeyboardState.IsKeyDown(key);
        }

        /// <summary>
        /// 检查键是否刚刚按下
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>如果键刚刚按下则返回true，否则返回false</returns>
        public bool IsKeyPressed(Keys key)
        {
            return currentKeyboardState.IsKeyDown(key) && !previousKeyboardState.IsKeyDown(key);
        }

        /// <summary>
        /// 检查键是否刚刚释放
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>如果键刚刚释放则返回true，否则返回false</returns>
        public bool IsKeyReleased(Keys key)
        {
            return !currentKeyboardState.IsKeyDown(key) && previousKeyboardState.IsKeyDown(key);
        }

        /// <summary>
        /// 检查鼠标按钮是否正在按下
        /// </summary>
        /// <param name="button">鼠标按钮</param>
        /// <returns>如果鼠标按钮正在按下则返回true，否则返回false</returns>
        public bool IsMouseButtonDown(MouseButton button)
        {
            return currentMouseState.IsButtonDown(button);
        }

        /// <summary>
        /// 检查鼠标按钮是否刚刚按下
        /// </summary>
        /// <param name="button">鼠标按钮</param>
        /// <returns>如果鼠标按钮刚刚按下则返回true，否则返回false</returns>
        public bool IsMouseButtonPressed(MouseButton button)
        {
            return currentMouseState.IsButtonDown(button) && !previousMouseState.IsButtonDown(button);
        }

        /// <summary>
        /// 检查鼠标按钮是否刚刚释放
        /// </summary>
        /// <param name="button">鼠标按钮</param>
        /// <returns>如果鼠标按钮刚刚释放则返回true，否则返回false</returns>
        public bool IsMouseButtonReleased(MouseButton button)
        {
            return !currentMouseState.IsButtonDown(button) && previousMouseState.IsButtonDown(button);
        }

        /// <summary>
        /// 获取鼠标位置
        /// </summary>
        /// <returns>鼠标位置</returns>
        public Vector2 GetMousePosition()
        {
            return currentMouseState.Position;
        }

        /// <summary>
        /// 获取鼠标移动增量
        /// </summary>
        /// <returns>鼠标移动增量</returns>
        public Vector2 GetMouseDelta()
        {
            return currentMouseState.Position - previousMouseState.Position;
        }

        /// <summary>
        /// 获取鼠标滚轮增量
        /// </summary>
        /// <returns>鼠标滚轮增量</returns>
        public float GetMouseWheelDelta()
        {
            return currentMouseState.ScrollWheelDelta;
        }

        /// <summary>
        /// 设置键盘状态
        /// </summary>
        /// <param name="state">键盘状态</param>
        public void SetKeyboardState(KeyboardState state)
        {
            currentKeyboardState = state;
        }

        /// <summary>
        /// 设置鼠标状态
        /// </summary>
        /// <param name="state">鼠标状态</param>
        public void SetMouseState(MouseState state)
        {
            currentMouseState = state;
        }
    }

    /// <summary>
    /// 表示一个输入动作，组合了多个输入条件
    /// </summary>
    public class InputAction
    {
        private List<Func<InputManager, bool>> conditions = new List<Func<InputManager, bool>>();
        private bool isActive = false;
        private bool wasActive = false;

        /// <summary>
        /// 获取动作是否处于活动状态
        /// </summary>
        public bool IsActive => isActive;

        /// <summary>
        /// 获取动作是否刚刚变为活动状态
        /// </summary>
        public bool IsPressed => isActive && !wasActive;

        /// <summary>
        /// 获取动作是否刚刚变为非活动状态
        /// </summary>
        public bool IsReleased => !isActive && wasActive;

        /// <summary>
        /// 添加键盘按键条件
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>当前输入动作实例</returns>
        public InputAction AddKeyCondition(Keys key)
        {
            conditions.Add(input => input.IsKeyDown(key));
            return this;
        }

        /// <summary>
        /// 添加鼠标按钮条件
        /// </summary>
        /// <param name="button">鼠标按钮</param>
        /// <returns>当前输入动作实例</returns>
        public InputAction AddMouseButtonCondition(MouseButton button)
        {
            conditions.Add(input => input.IsMouseButtonDown(button));
            return this;
        }

        /// <summary>
        /// 添加自定义条件
        /// </summary>
        /// <param name="condition">条件函数</param>
        /// <returns>当前输入动作实例</returns>
        public InputAction AddCustomCondition(Func<InputManager, bool> condition)
        {
            if (condition != null)
            {
                conditions.Add(condition);
            }
            return this;
        }

        /// <summary>
        /// 更新动作状态
        /// </summary>
        /// <param name="inputManager">输入管理器</param>
        internal void Update(InputManager inputManager)
        {
            wasActive = isActive;

            if (conditions.Count == 0)
            {
                isActive = false;
                return;
            }

            // 检查所有条件，如果有一个条件满足，则动作激活
            isActive = false;
            foreach (var condition in conditions)
            {
                if (condition(inputManager))
                {
                    isActive = true;
                    break;
                }
            }
        }
    }
}