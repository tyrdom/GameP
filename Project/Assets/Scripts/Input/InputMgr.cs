using QFramework;

namespace Input
{
    public class InputMgr : Singleton<InputMgr>
    {
        private static InputSystem_Actions _actions;

        public static InputSystem_Actions Actions
        {
            get
            {
                if (_actions != null) return _actions;
                _actions = new InputSystem_Actions();
                _actions.Enable();
                return _actions;
            }
        }
    }
}