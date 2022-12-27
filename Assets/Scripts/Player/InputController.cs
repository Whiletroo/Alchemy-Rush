using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

public class InputController : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private PlayerController playerController;

    private InputAction _startAtMenuAction;
    private InputAction _startAtPlayerAction;

    private bool _hasSubscribedPlayerActions;
    private bool _hasSubscribedMenuActions;

    private const string ActionMapGameplay = "PlayerControls";
    private const string ActionMapMenu = "MenuControls";

    public delegate void StartPressed();
    public StartPressed OnStartPressedAtMenu;
    public StartPressed OnStartPressedAtPlayer;

    private void Awake()
    {
#if UNITY_EDITOR
        Assert.IsNotNull(playerInput);
        Assert.IsNotNull(playerController);
#endif
    }

    internal void EnableFirstPlayerController()
    {
        playerController.ActivatePlayer();
    }


    internal void DisableAllPlayerControllers()
    {
        playerController.DeactivatePlayer();
        UnsubscribePlayerActions();
    }

    private void TogglePlayerController()
    {
        EnableFirstPlayerController();
    }

    private void OnEnable()
    {
        SubscribeInputEvents();
    }

    private void OnDisable()
    {
        UnsubscribeInputEvents();
    }

    private void SubscribeInputEvents()
    {
        playerInput.onDeviceLost += HandleDeviceLost;
        playerInput.onDeviceRegained += HandleDeviceRegained;
        playerInput.onControlsChanged += HandleControlsChanged;
    }

    private void UnsubscribeInputEvents()
    {
        playerInput.onDeviceLost -= HandleDeviceLost;
        playerInput.onDeviceRegained -= HandleDeviceRegained;
        playerInput.onControlsChanged -= HandleControlsChanged;
    }

    private void SubscribePlayerActions()
    {
        if (_hasSubscribedPlayerActions) return;
        _hasSubscribedPlayerActions = true;
        _startAtPlayerAction = playerInput.currentActionMap["Start@Player"];
        _startAtPlayerAction.performed += HandleStartAtPLayer;
    }

    private void UnsubscribePlayerActions()
    {
        if (_hasSubscribedPlayerActions == false) return;
        _hasSubscribedPlayerActions = false;
        _startAtPlayerAction.performed -= HandleStartAtPLayer;
    }

    private void SubscribeMenuActions()
    {
        if (_hasSubscribedMenuActions) return;
        _hasSubscribedMenuActions = true;

        _startAtMenuAction = playerInput.currentActionMap["Start@Menu"];
        _startAtMenuAction.performed += HandleStartAtMenu;
    }

    private void UnsubscribeMenuActions()
    {
        if (_hasSubscribedMenuActions == false) return;
        _hasSubscribedMenuActions = false;
        _startAtMenuAction.performed -= HandleStartAtMenu;
    }

    private void HandleStartAtPLayer(InputAction.CallbackContext context)
    {
        OnStartPressedAtPlayer?.Invoke();
    }

    private void HandleStartAtMenu(InputAction.CallbackContext context)
    {
        OnStartPressedAtMenu?.Invoke();
    }

    private void HandleSwitchAvatar(InputAction.CallbackContext context)
    {
        TogglePlayerController();
    }

    private void HandleControlsChanged(PlayerInput _playerInput)
    {
        Debug.Log($"ControlsChanged {playerInput.currentControlScheme}");
        //TODO: Controls change dynamically
    }


    private void HandleDeviceLost(PlayerInput context)
    {
        Debug.Log("Device Lost");
        //TODO: pause game with a warning
    }

    private void HandleDeviceRegained(PlayerInput context)
    {
        Debug.Log("Device Regained");
        //TODO: notify player and keep it paused (we could resume after some countdown)
    }

    public void Action(InputAction.CallbackContext context)
    {
        Debug.Log("Action");
    }

    internal void EnableGameplayControls()
    {
        Debug.Log("[InputController] Enable GamePlayControls");
        UnsubscribeMenuActions();
        playerInput.currentActionMap.Disable();
        playerInput.SwitchCurrentActionMap(ActionMapGameplay);
        SubscribePlayerActions();
        playerInput.currentActionMap.Enable();
    }

    internal void EnableMenuControls()
    {
        Debug.Log("[InputController] Enable MenuControls");
        UnsubscribePlayerActions();
        playerInput.currentActionMap.Disable();
        playerInput.SwitchCurrentActionMap(ActionMapMenu);
        SubscribeMenuActions();
        playerInput.currentActionMap.Enable();
    }
}
