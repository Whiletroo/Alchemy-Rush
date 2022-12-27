using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using Slider = UnityEngine.UI.Slider;

    public class MortarBoard : Interactable
    {
        [SerializeField] private Slider slider;

    private float _finalProcessTime;
        private float _currentProcessTime;
        private Coroutine _grindCoroutine;
        private Ingredient _ingredient;
        private bool _isGrinding;

        public delegate void GrindingStatus(PlayerController playerController);
        public static event GrindingStatus OnGrindingStart;
        public static event GrindingStatus OnGrindingStop;

        protected override void Awake()
        {
#if UNITY_EDITOR
        Assert.IsNotNull(slider);
        Assert.IsNotNull(slider);
#endif

        base.Awake();
        slider.gameObject.SetActive(false);
    }

        public override void Interact(PlayerController playerController)
        {
            LastPlayerControllerInteracting = playerController;
            base.Interact(playerController);
            if (CurrentPickable == null ||
                _ingredient == null ||
                _ingredient.Status != IngredientStatus.Raw) return;

            if (_grindCoroutine == null)
            {
                _finalProcessTime = _ingredient.ProcessTime;
                _currentProcessTime = 0f;
                slider.value = 0f;
                slider.gameObject.SetActive(true);
                StartGrindCoroutine();
                return;
            }

            if (_isGrinding == false)
            {
                StartGrindCoroutine();
            }
        }

        private void StartGrindCoroutine()
        {
            OnGrindingStart?.Invoke(LastPlayerControllerInteracting);
            _grindCoroutine = StartCoroutine(Grind());
        }

        private void StopGrindCoroutine()
        {
            OnGrindingStop?.Invoke(LastPlayerControllerInteracting);
            _isGrinding = false;
            if (_grindCoroutine != null) StopCoroutine(_grindCoroutine);
        }

        public override void ToggleHighlightOff()
        {
            base.ToggleHighlightOff();
            StopGrindCoroutine();
        }
        
        private IEnumerator Grind()
        {
            _isGrinding = true;
            while (_currentProcessTime < _finalProcessTime)
            {
                slider.value = _currentProcessTime / _finalProcessTime;
                _currentProcessTime += Time.deltaTime;
                yield return null;
            }

            _ingredient.ChangeToProcessed();
        slider.gameObject.SetActive(false);
        _isGrinding = false;
            _grindCoroutine = null;
            OnGrindingStop?.Invoke(LastPlayerControllerInteracting);
        }
        
        public override bool TryToDropIntoSlot(IPickable pickableToDrop)
        {
            if (pickableToDrop is Ingredient)
            {
                return TryDropIfNotOccupied(pickableToDrop);
            }
            return false;
        }

        public override IPickable TryToPickUpFromSlot(IPickable playerHoldPickable)
        {
            if (CurrentPickable == null) return null;
            if (_grindCoroutine != null) return null;
            
            var output = CurrentPickable;
            _ingredient = null;
            var interactable = CurrentPickable as Interactable;
            interactable?.ToggleHighlightOff();
            CurrentPickable = null;
            return output;
        }
        
        private bool TryDropIfNotOccupied(IPickable pickable)
        {
            if (CurrentPickable != null) return false;
            CurrentPickable = pickable;
            _ingredient = pickable as Ingredient;
            if (_ingredient == null) return false;

            _finalProcessTime = _ingredient.ProcessTime;
            
            CurrentPickable.gameObject.transform.SetParent(Slot);
            CurrentPickable.gameObject.transform.SetPositionAndRotation(Slot.position, Quaternion.identity);
            return true;
        }
    }

