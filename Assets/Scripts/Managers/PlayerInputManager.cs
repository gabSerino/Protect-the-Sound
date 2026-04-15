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


    private InputAction moveAction;
    private InputAction attackAction;

    public Vector2 MoveInput { get; private set; }
    public bool AttackInput { get; private set; }

    private void Awake()
    {
        InputActionMap actionMap = inputActions.FindActionMap(actionMapName);
        moveAction = actionMap.FindAction(move);
        attackAction = actionMap.FindAction(attack);
        BindActions();
    }

    private void BindActions()
    {
        moveAction.performed += ctx => MoveInput = ctx.ReadValue<Vector2>();
        moveAction.canceled += ctx => MoveInput = Vector2.zero;
        attackAction.performed += ctx => AttackInput = true;
        attackAction.canceled += ctx => AttackInput = false;
    }
}
