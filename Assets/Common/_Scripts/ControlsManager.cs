using UnityEngine;

public enum ActionMap
{
    Player,
    UI
}

public class ControlsManager : MonoBehaviour
{
    private static ControlsManager _instance;
    private InputSystem_Actions _inputActions;
    private ActionMap _currentActionMap;

    [SerializeField] private bool _cursorLockedHiddenOnAwake = false;

    public static ControlsManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<ControlsManager>();
            }
            if (_instance == null)
            {
                _instance = new GameObject("ControlsManager").AddComponent<ControlsManager>();
            }
            return _instance;
        }
        private set
        {
            _instance = value;
        }
    }
    public InputSystem_Actions InputActions
    {
        get
        {
            if (_inputActions == null)
            {
                _inputActions = new InputSystem_Actions();
            }
            return _inputActions;
        }
        private set
        {
            _inputActions = value;
        }
    }

    private void Awake()
    {
        Instance = this;

        if (InputActions == null)
        {
            InputActions = new();
        }

        SetCursorLock(_cursorLockedHiddenOnAwake);
    }

    private void OnEnable()
    {
        InputActions.Player.Enable();
        InputActions.UI.Enable();
    }

    /// <summary>
    /// Locks and hides cursor if true, unlocks and unhides if false
    /// </summary>
    /// <param name="toggle"></param>
    private void SetCursorLock(bool toggle)
    {
        Cursor.lockState = toggle ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !toggle;
    }

    public void SetActiveActionMap(ActionMap currentActionMap)
    {
        _currentActionMap = currentActionMap;
        switch (_currentActionMap)
        {
            case ActionMap.Player:
                InputActions.Player.Enable();
                InputActions.UI.Disable();
                SetCursorLock(true);
                break;
            case ActionMap.UI:
                InputActions.UI.Enable();
                InputActions.Player.Disable();
                SetCursorLock(false);
                break;
            default:
                break;
        }
    }

    private void OnDisable()
    {
        //InputActions.Disable();
        InputActions.Player.Disable();
        InputActions.UI.Disable();
    }
}
