using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputManager : MonoBehaviour
{
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
    [SerializeField] private string dodge = "Dodge";

    private InputAction moveAction;
    private InputAction attackDirectionAction;
    private InputAction attackAction;
    private InputAction goLeftInventorySlotAction;
    private InputAction goRightInventorySlotAction;
    private InputAction useItemAction;
    private InputAction dashAction;
    private InputAction dodgeAction;

    public Vector2 MoveInput { get; private set; }
    public Vector2 AttackDirectionInput { get; private set; }
    public bool AttackInput { get; private set; }
    public bool GoLeftInventorySlotInput { get; private set; }
    public bool GoRightInventorySlotInput { get; private set; }
    public bool UseItemInput { get; private set; }
    public bool DashInput { get; private set; }
    public bool DodgeInput { get; private set; }

    private void Awake()
    {
        InputActionMap actionMap = inputActions.FindActionMap(actionMapName);

        moveAction = actionMap.FindAction(move);
        attackDirectionAction = actionMap.FindAction(attackDirection);
        attackAction = actionMap.FindAction(attack);
        goLeftInventorySlotAction = actionMap.FindAction(goLeftInventorySlot);
        goRightInventorySlotAction = actionMap.FindAction(goRightInventorySlot);
        useItemAction = actionMap.FindAction(useItem);
        dashAction = actionMap.FindAction(dash);
        dodgeAction = actionMap.FindAction(dodge);

        BindActions();
    }

    private void BindActions()
    {
        if (moveAction != null)
        {
            moveAction.performed += ctx => MoveInput = ctx.ReadValue<Vector2>();
            moveAction.canceled += ctx => MoveInput = Vector2.zero;
        }

        if (attackDirectionAction != null)
        {
            attackDirectionAction.performed += ctx => AttackDirectionInput = ctx.ReadValue<Vector2>();
            attackDirectionAction.canceled += ctx => AttackDirectionInput = Vector2.zero;
        }

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

        if (dodgeAction != null)
        {
            dodgeAction.performed += ctx => DodgeInput = true;
            dodgeAction.canceled += ctx => DodgeInput = false;
        }
    }

    public void ConsumeGoLeftInventorySlotInput() => GoLeftInventorySlotInput = false;
    public void ConsumeGoRightInventorySlotInput() => GoRightInventorySlotInput = false;
    public void ConsumeAttackInput() => AttackInput = false;
    public void ConsumeUseItemInput() => UseItemInput = false;
    public void ConsumeDashInput() => DashInput = false;
    public void ConsumeDodgeInput() => DodgeInput = false;
}