using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public class PlayerInputManager : MonoBehaviour
{
    [Header("Cursor Settings")]
    [SerializeField] private bool cursorVisible = true;

    [Header("Input Action Asset")]
    [SerializeField] private InputActionAsset inputActions;

    [Header("Action Map Name")]
    [SerializeField] private string actionMapName = "Player";

    [Header("Action Names")]
    [SerializeField] private string move = "Move";
    [SerializeField] private string attackDirection = "Attack Direction"; 
    [SerializeField] private string attack = "Attack";
    [SerializeField] private string goLeftInventorySlot = "Go Left Inventory Slot"; 
    [SerializeField] private string goRightInventorySlot = "Go Right Inventory Slot"; 
    [SerializeField] private string useItem = "Use Item";
    [SerializeField] private string deleteItem = "Delete Item";
    [SerializeField] private string dash = "Dash";

    [SerializeField] private string songSwitch = "Song Switch";
    [SerializeField] private string songConfirm = "Song Confirm";

    private InputAction moveAction;
    private InputAction attackDirectionAction;
    private InputAction attackAction;
    private InputAction goLeftInventorySlotAction;
    private InputAction goRightInventorySlotAction;
    private InputAction useItemAction;
    private InputAction deleteItemAction;
    private InputAction dashAction;
    private InputAction songSwitchAction;
    private InputAction songConfirmAction;

    public Vector2 MoveInput { get; private set; }
    public Vector2 AttackDirectionInput { get; private set; }
    public bool AttackInput { get; private set; }
    public bool GoLeftInventorySlotInput { get; private set; }
    public bool GoRightInventorySlotInput { get; private set; }
    public bool UseItemInput { get; private set; }
    public bool DeleteItemInput { get; private set; }
    public bool DashInput { get; private set; }
    public bool SongSwitchInput { get; private set; }
    public bool SongConfirmInput { get; private set; }

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = cursorVisible;
        
        InputActionMap actionMap = inputActions.FindActionMap(actionMapName);

        moveAction = actionMap.FindAction(move);
        attackDirectionAction = actionMap.FindAction(attackDirection);
        attackAction = actionMap.FindAction(attack);
        goLeftInventorySlotAction = actionMap.FindAction(goLeftInventorySlot);
        goRightInventorySlotAction = actionMap.FindAction(goRightInventorySlot);
        useItemAction = actionMap.FindAction(useItem);
        deleteItemAction = actionMap.FindAction(deleteItem);
        dashAction = actionMap.FindAction(dash);
        songSwitchAction = actionMap.FindAction(songSwitch);
        songConfirmAction = actionMap.FindAction(songConfirm);

        BindActions();

        // Abilitiamo esplicitamente l'action map
        if (actionMap != null)
        {
            actionMap.Enable();
        }

        // Assicuriamoci che l'asset sia attivo
        inputActions.Enable();
    }

private void Update()
{
    // 1. Lettura standard dall'Action Asset
    if (moveAction != null && moveAction.enabled)
    {
        MoveInput = moveAction.ReadValue<Vector2>();
    }

    // 2. FALLBACK DEFINTIVO: Se l'azione restituisce zero ma l'utente sta premendo la tastiera
    if (MoveInput == Vector2.zero && Keyboard.current != null)
    {
        Vector2 keyboardFallback = Vector2.zero;

        if (Keyboard.current.wKey.isPressed) keyboardFallback.y += 1f;
        if (Keyboard.current.sKey.isPressed) keyboardFallback.y -= 1f;
        if (Keyboard.current.dKey.isPressed) keyboardFallback.x += 1f;
        if (Keyboard.current.aKey.isPressed) keyboardFallback.x -= 1f;

        // Se abbiamo rilevato movimento da tastiera, usiamo questo valore!
        if (keyboardFallback != Vector2.zero)
        {
            MoveInput = keyboardFallback.normalized;
        }
    }

    // Lettura direzione attacco
    if (attackDirectionAction != null && attackDirectionAction.enabled)
    {
        AttackDirectionInput = attackDirectionAction.ReadValue<Vector2>();
        
        // Applichiamo lo stesso fallback anche all'attacco se necessario (es. se usi le frecce direzionali)
        if (AttackDirectionInput == Vector2.zero && Keyboard.current != null)
        {
            Vector2 attackFallback = Vector2.zero;
            if (Keyboard.current.upArrowKey.isPressed) attackFallback.y += 1f;
            if (Keyboard.current.downArrowKey.isPressed) attackFallback.y -= 1f;
            if (Keyboard.current.rightArrowKey.isPressed) attackFallback.x += 1f;
            if (Keyboard.current.leftArrowKey.isPressed) attackFallback.x -= 1f;
            
            if (attackFallback != Vector2.zero)
            {
                AttackDirectionInput = attackFallback.normalized;
            }
        }
    }

    // Controllo UI (lasciato intatto)
    if (UnityEngine.EventSystems.EventSystem.current != null)
    {
        var selected = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        if (selected != null)
            Debug.Log($"UI Selected: {selected.name}");
    }
}

    private void BindActions()
    {
        // Move e AttackDirection non hanno più bisogno di performed/canceled:
        // vengono letti ogni frame in Update().

        if (attackAction != null)
        {
            attackAction.performed += ctx => AttackInput = true;
            attackAction.canceled += ctx => AttackInput = false;
        }

        if (goLeftInventorySlotAction != null)
        {
            goLeftInventorySlotAction.performed += ctx => GoLeftInventorySlotInput = true;
            goLeftInventorySlotAction.canceled += ctx => GoLeftInventorySlotInput = false;
        }

        if (goRightInventorySlotAction != null)
        {
            goRightInventorySlotAction.performed += ctx => GoRightInventorySlotInput = true;
            goRightInventorySlotAction.canceled += ctx => GoRightInventorySlotInput = false;
        }

        if (useItemAction != null)
        {
            useItemAction.performed += ctx => UseItemInput = true;
            useItemAction.canceled += ctx => UseItemInput = false;
        }

        if (deleteItemAction != null)
        {
            deleteItemAction.performed += ctx => DeleteItemInput = true;
            deleteItemAction.canceled += ctx => DeleteItemInput = false;
        }

        if (dashAction != null)
        {
            dashAction.performed += ctx => DashInput = true;
            dashAction.canceled += ctx => DashInput = false;
        }

        if (songSwitchAction != null)
        {
            songSwitchAction.performed += ctx => SongSwitchInput = true;
            songSwitchAction.canceled += ctx => SongSwitchInput = false;
        }

        if (songConfirmAction != null)
        {
            songConfirmAction.performed += ctx => SongConfirmInput = true;
            songConfirmAction.canceled += ctx => SongConfirmInput = false;
        }
    }

    public void ConsumeGoLeftInventorySlotInput() => GoLeftInventorySlotInput = false;
    public void ConsumeGoRightInventorySlotInput() => GoRightInventorySlotInput = false;
    public void ConsumeAttackInput() => AttackInput = false;
    public void ConsumeUseItemInput() => UseItemInput = false;
    public void ConsumeDeleteItemInput() => DeleteItemInput = false;
    public void ConsumeDashInput() => DashInput = false;
    public void ConsumeSongSwitchInput() => SongSwitchInput = false;
    public void ConsumeSongConfirmInput() => SongConfirmInput = false;

    public void DisableAllControls() => inputActions.Disable();
    public void EnableAllControls() => inputActions.Enable();


    private void OnEnable()
    {
        InputSystem.onActionChange += OnActionChange;
    }

    private void OnDisable()
    {
        InputSystem.onActionChange -= OnActionChange;
    }

    private void OnActionChange(object obj, InputActionChange change)
    {
        // Controlliamo quando un'azione viene eseguita/effettuata
        if (change == InputActionChange.ActionStarted || change == InputActionChange.ActionPerformed)
        {
            var action = (InputAction)obj;
            if (action.activeControl != null)
            {
                var device = action.activeControl.device;

                if (device is Gamepad)
                {
                    // Il giocatore sta usando il controller
                    // Es: UIPlayModeManager.SetControllerMode(true);
                }
                else if (device is Keyboard || device is Mouse)
                {
                    // Il giocatore sta usando tastiera/mouse
                    // Es: UIPlayModeManager.SetControllerMode(false);
                }
            }
        }
    }
}