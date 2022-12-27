using UnityEngine;
using UnityEngine.Assertions;
public class IngredientCrate : Interactable
    { 
        [SerializeField] private Ingredient ingredientPrefab;
        private static readonly int OpenHash = Animator.StringToHash("Open");

        protected override void Awake()
        {
            base.Awake();
            
            #if UNITY_EDITOR
                Assert.IsNotNull(ingredientPrefab);
            #endif
        }

        public override bool TryToDropIntoSlot(IPickable pickableToDrop)
        {
            if (CurrentPickable != null) return false;
            
            CurrentPickable = pickableToDrop;
            CurrentPickable.gameObject.transform.SetParent(Slot);
            pickableToDrop.gameObject.transform.SetPositionAndRotation(Slot.position, Quaternion.identity);
            return true;
        }

        public override IPickable TryToPickUpFromSlot(IPickable playerHoldPickable)
        {
            if (CurrentPickable == null)
            {
                return Instantiate(ingredientPrefab, Slot.transform.position, Quaternion.identity);
            }

            var output = CurrentPickable;
            var interactable = CurrentPickable as Interactable;
            interactable?.ToggleHighlightOff();
            CurrentPickable = null;
            return output;
        }
    }
