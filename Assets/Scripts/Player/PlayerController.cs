using System.Collections;
using Lean.Transition;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
    {
        [Header("Physics")]
        [SerializeField] private Rigidbody playerRigidbody;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    private readonly int _isGrindingHash = Animator.StringToHash("isGrinding");
    private readonly int _hasPickupHash = Animator.StringToHash("hasPickup");
    private readonly int _velocityHash = Animator.StringToHash("velocity");

    [Header("Input")]
        [SerializeField] private PlayerInput playerInput;
        private InputAction _moveAction;
        private InputAction _pickUpAction;
        private InputAction _interactAction;
        private InputAction _startAtPlayerAction;
        
        [Header("Movement Settings")]
        [SerializeField] private float movementSpeed = 5f;

        private InteractableController _interactableController;
        private bool _isActive;
        private IPickable _currentPickable;
        private Vector3 _inputDirection;
        private bool _hasSubscribedControllerEvents;

    [SerializeField] private Transform slot;

    [Header("Audio")]
    [SerializeField] private AudioClip pickupAudio;
    [SerializeField] private AudioClip dropAudio;

    private void Awake()
        {
            _moveAction = playerInput.currentActionMap["Move"];
            _pickUpAction = playerInput.currentActionMap["PickUp"];
            _interactAction = playerInput.currentActionMap["Interact"];
            _startAtPlayerAction = playerInput.currentActionMap["Start@Player"];

            _interactableController = GetComponentInChildren<InteractableController>();

        }

    public void ActivatePlayer()
        {
            _isActive = true;
            SubscribeControllerEvents();
        }
    
        public void DeactivatePlayer()
        {
            _isActive = false;
            UnsubscribeControllerEvents();
            animator.SetFloat(_velocityHash, 0f);
    }


        private void SubscribeControllerEvents()
        {
            if (_hasSubscribedControllerEvents) return;
            _hasSubscribedControllerEvents = true;
            _moveAction.performed += HandleMove;
            _pickUpAction.performed += HandlePickUp;
            _interactAction.performed += HandleInteract;
        }

        private void UnsubscribeControllerEvents()
        {
            if (_hasSubscribedControllerEvents == false) return;

            _hasSubscribedControllerEvents = false;
            _moveAction.performed -= HandleMove;
            _pickUpAction.performed -= HandlePickUp;
            _interactAction.performed -= HandleInteract;
        }


        private void HandlePickUp(InputAction.CallbackContext context)
        {
            var interactable = _interactableController.CurrentInteractable;

    
            if (_currentPickable == null)
            {
                _currentPickable = interactable as IPickable;
                if (_currentPickable != null)
                {
                 animator.SetBool(_hasPickupHash, true);
                this.PlaySoundTransition(pickupAudio);
                _currentPickable.Pick();
                    _interactableController.Remove(_currentPickable as Interactable);
                    _currentPickable.gameObject.transform.SetPositionAndRotation(slot.transform.position,
                        Quaternion.identity);
                    _currentPickable.gameObject.transform.SetParent(slot);
                    return;
                }

                _currentPickable = interactable?.TryToPickUpFromSlot(_currentPickable);
            if (_currentPickable != null)
            {
                animator.SetBool(_hasPickupHash, true);
                this.PlaySoundTransition(pickupAudio);
            }

            _currentPickable?.gameObject.transform.SetPositionAndRotation(
                    slot.position, Quaternion.identity);
                _currentPickable?.gameObject.transform.SetParent(slot);
                return;
            }

            if (interactable == null || interactable is IPickable)
            {
             animator.SetBool(_hasPickupHash, false);
            this.PlaySoundTransition(dropAudio);
            _currentPickable.Drop();
                _currentPickable = null;
                return;
            }

            bool dropSuccess = interactable.TryToDropIntoSlot(_currentPickable);
            if (!dropSuccess) return;

        animator.SetBool(_hasPickupHash, false);
        this.PlaySoundTransition(dropAudio);
        _currentPickable = null;
        }

        private void HandleMove(InputAction.CallbackContext context)
        {
            Vector2 inputMovement = context.ReadValue<Vector2>();
            if (inputMovement.x > 0.3f)
            {
                inputMovement.x = 1f;
            }
            else if (inputMovement.x < -0.3)
            {
                inputMovement.x = -1f;
            }
            else
            {
                inputMovement.x = 0f;
            }

            if (inputMovement.y > 0.3f)
            {
                inputMovement.y = 1f;
            }
            else if (inputMovement.y < -0.3f)
            {
                inputMovement.y = -1f;
            }
            else
            {
                inputMovement.y = 0f;
            }

            _inputDirection = new Vector3(inputMovement.x, 0, inputMovement.y);
        }

        private void HandleInteract(InputAction.CallbackContext context)
        {
            _interactableController.CurrentInteractable?.Interact(this);
        }

        private void Update()
        {
            if (!_isActive) return;
            CalculateInputDirection();
        }

        private void FixedUpdate()
        {
            if (!_isActive) return;
            MoveThePlayer();
            AnimatePlayerMovement();
            TurnThePlayer();
        }

        private void MoveThePlayer()
        {
            playerRigidbody.velocity = _inputDirection.normalized * movementSpeed;
        }

        private void CalculateInputDirection()
        {
            var inputMovement = _moveAction.ReadValue<Vector2>();
            if (inputMovement.x > 0.3f)
            {
                inputMovement.x = 1f;
            }
            else if (inputMovement.x < -0.3)
            {
                inputMovement.x = -1f;
            }
            else
            {
                inputMovement.x = 0f;
            }

            if (inputMovement.y > 0.3f)
            {
                inputMovement.y = 1f;
            }
            else if (inputMovement.y < -0.3f)
            {
                inputMovement.y = -1f;
            }
            else
            {
                inputMovement.y = 0f;
            }

            _inputDirection = new Vector3(inputMovement.x, 0f, inputMovement.y);
        }

        private void TurnThePlayer()
        {
            if (!(playerRigidbody.velocity.magnitude > 0.1f) || _inputDirection == Vector3.zero) return;

            Quaternion newRotation = Quaternion.LookRotation(_inputDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, Time.deltaTime * 15f);
        }

    private void AnimatePlayerMovement()
    {
        animator.SetFloat(_velocityHash, _inputDirection.sqrMagnitude);
    }
}