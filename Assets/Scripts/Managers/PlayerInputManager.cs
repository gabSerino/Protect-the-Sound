using UnityEngine;
using UnityEngine.InputSystem;

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
    [SerializeField] private string dash = "Dash";

    [SerializeField] private string songSwitch = "Song Switch";
    [SerializeField] private string songConfirm = "Song Confirm";

    private InputAction moveAction;
    private InputAction attackDirectionAction;
    private InputAction attackAction;
    private InputAction goLeftInventorySlotAction;
    private InputAction goRightInventorySlotAction;
    private InputAction useItemAction;
    private InputAction dashAction;
    private InputAction songSwitchAction;
    private InputAction songConfirmAction;

    public Vector2 MoveInput { get; private set; }
    public Vector2 AttackDirectionInput { get; private set; }
    public bool AttackInput { get; private set; }
    public bool GoLeftInventorySlotInput { get; private set; }
    public bool GoRightInventorySlotInput { get; private set; }
    public bool UseItemInput { get; private set; }
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
        dashAction = actionMap.FindAction(dash);
        songSwitchAction = actionMap.FindAction(songSwitch);
        songConfirmAction = actionMap.FindAction(songConfirm);

        BindActions();

        // Assicuriamoci che l'asset sia attivo fin da subito.
        inputActions.Enable();
    }

    private void Update()
    {
        // Move e AttackDirection sono valori Vector2 continui: li leggiamo
        // ogni frame direttamente, invece di fidarci solo di performed/canceled,
        // per evitare che un ricalcolo del composite WASD lasci il valore a zero.
        if (moveAction != null)
            MoveInput = moveAction.ReadValue<Vector2>();

        if (attackDirectionAction != null)
            AttackDirectionInput = attackDirectionAction.ReadValue<Vector2>();
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
    public void ConsumeDashInput() => DashInput = false;
    public void ConsumeSongSwitchInput() => SongSwitchInput = false;
    public void ConsumeSongConfirmInput() => SongConfirmInput = false;

    public void DisableAllControls() => inputActions.Disable();
    public void EnableAllControls() => inputActions.Enable();
}