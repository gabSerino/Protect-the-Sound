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
    [SerializeField] private string attack = "Attack";
    [SerializeField] private string goLeftInventorySlot = "Go Left Inventory Slot";
    [SerializeField] private string goRightInventorySlot = "Go Right Inventory Slot";
    [SerializeField] private string useItem = "Use Item";


    private InputAction moveAction;
    private InputAction attackAction;
    private InputAction goLeftInventorySlotAction;
    private InputAction goRightInventorySlotAction;
    private InputAction useItemAction;

    public Vector2 MoveInput { get; private set; }
    public bool AttackInput { get; private set; }
    public bool GoLeftInventorySlotInput { get; private set; }
    public bool GoRightInventorySlotInput { get; private set; }
    public bool UseItemInput { get; private set; }

    private void Awake()
    {
        InputActionMap actionMap = inputActions.FindActionMap(actionMapName);
        moveAction = actionMap.FindAction(move);
        attackAction = actionMap.FindAction(attack);
        goLeftInventorySlotAction = actionMap.FindAction(goLeftInventorySlot);
        goRightInventorySlotAction = actionMap.FindAction(goRightInventorySlot);
        useItemAction = actionMap.FindAction(useItem);
        BindActions();
    }

    private void BindActions()
    {
        moveAction.performed += ctx => MoveInput = ctx.ReadValue<Vector2>();
        moveAction.canceled += ctx => MoveInput = Vector2.zero;

        attackAction.performed += ctx => AttackInput = true;
        attackAction.canceled += ctx => AttackInput = false;

        goLeftInventorySlotAction.performed += ctx => GoLeftInventorySlotInput = true;
        goLeftInventorySlotAction.canceled += ctx => GoLeftInventorySlotInput = false;

        goRightInventorySlotAction.performed += ctx => GoRightInventorySlotInput = true;
        goRightInventorySlotAction.canceled += ctx => GoRightInventorySlotInput = false;

        useItemAction.performed += ctx => UseItemInput = true;
        useItemAction.canceled += ctx => UseItemInput = false;
    }

    public void ConsumeGoLeftInventorySlotInput() => GoLeftInventorySlotInput = false;
    public void ConsumeGoRightInventorySlotInput() => GoRightInventorySlotInput = false;
    public void ConsumeAttackInput() => AttackInput = false;
    public void ConsumeUseItemInput() => UseItemInput = false;
}
