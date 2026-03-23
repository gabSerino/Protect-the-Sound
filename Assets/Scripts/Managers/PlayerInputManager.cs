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
    [SerializeField] private string jump = "Jump";
    [SerializeField] private string attack = "Attack";


    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction attackAction;

    public Vector2 MoveInput { get; private set; }
    public bool JumpInput { get; private set; }
    public bool AttackInput { get; private set; }

    private void Awake()
    {
        InputActionMap actionMap = inputActions.FindActionMap(actionMapName);
        moveAction = actionMap.FindAction(move);
        jumpAction = actionMap.FindAction(jump);
        attackAction = actionMap.FindAction(attack);
        BindActions();
    }

    private void BindActions()
    {
        moveAction.performed += ctx => MoveInput = ctx.ReadValue<Vector2>();
        moveAction.canceled += ctx => MoveInput = Vector2.zero;
        jumpAction.performed += ctx => JumpInput = true;
        jumpAction.canceled += ctx => JumpInput = false;
        attackAction.performed += ctx => AttackInput = true;
        attackAction.canceled += ctx => AttackInput = false;
    }
}
