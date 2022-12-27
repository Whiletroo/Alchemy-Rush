using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lean.Transition;
using UnityEngine;
using UnityEngine.Assertions;
using Image = UnityEngine.UI.Image;
using Slider = UnityEngine.UI.Slider;

    [RequireComponent(typeof(Collider))]
    public class CookingPot : Interactable
    {
    [Header("UI")]
    [SerializeField] private Slider slider;
    [SerializeField] private List<Image> ingredientUISlots;
    [SerializeField] private Sprite plusIcon;

    // Timers
        private float _totalCookTime;
        private float _currentCookTime;

        private Coroutine _cookCoroutine;

        private const int MaxNumberIngredients = 2;

        private bool _isCooking;
        public bool IsCookFinished { get; private set; }
    public bool IsEmpty() =>Ingredients.Count == 0;
        public List<Ingredient> Ingredients { get; } = new List<Ingredient>(MaxNumberIngredients);
        
        [SerializeField] private Potion potionPrefab;


    public override bool TryToDropIntoSlot(IPickable pickableToDrop)
        {
            switch (pickableToDrop)
            {
                case Ingredient ingredient:
                    if (ingredient.Status != IngredientStatus.Processed)
                    {
                        return false;
                    }
                    if (ingredient.Type == IngredientType.Flower ||
                        ingredient.Type == IngredientType.Leg ||
                        ingredient.Type == IngredientType.Root ||
                        ingredient.Type == IngredientType.Gold ||
                        ingredient.Type == IngredientType.Silver)
                    {
                        return TryDrop(pickableToDrop);    
                    }
                    return false;

                default:
                    return false;
            }

        }

    public override IPickable TryToPickUpFromSlot(IPickable playerHoldPickable)
    {
        if (!IsCookFinished) return null;

        if (playerHoldPickable == null)
        {
            var output = Instantiate(potionPrefab, Slot.transform.position, Quaternion.identity);
            output.AddIngredients(Ingredients);
            EmptyPan();
            return output;

        }

        return null;

    }

        private bool TryAddIngredients(List<Ingredient> ingredients)
        {
            if (!IsEmpty()) return false;
            if (Potion.CheckSoupIngredients(ingredients) == false) return false;
            Ingredients.AddRange(ingredients);
            
            foreach (var ingredient in Ingredients)
            {
                ingredient.transform.SetParent(Slot);
                ingredient.transform.SetPositionAndRotation(Slot.transform.position, Quaternion.identity);
            }

            IsCookFinished = true;
        UpdateIngredientsUI();
        return true;
        }

        public void EmptyPan()
        {
            if (_cookCoroutine != null) StopCoroutine(_cookCoroutine);

            slider.gameObject.SetActive(false);
            Ingredients.Clear();

            _currentCookTime = 0f;
            _totalCookTime = 0f;
            IsCookFinished = false;
            _isCooking = false;

        UpdateIngredientsUI();
        }

        private void TryStopCook()
        {
            if (_cookCoroutine == null) return;
            
            StopCoroutine(_cookCoroutine);
            _isCooking = false;
        }
        
        private IEnumerator Cook()
        {
            _isCooking = true;
            slider.gameObject.SetActive(true);

            while (_currentCookTime < _totalCookTime)
            {
            slider.value = _currentCookTime / _totalCookTime;
            _currentCookTime += Time.deltaTime;
                yield return null;
            }

            _isCooking = false;
            
            if (Ingredients.Count == MaxNumberIngredients)
            {
                TriggerSuccessfulCook();
                yield break;
            }

        }

        private void TriggerSuccessfulCook()
        {
            IsCookFinished = true;
            _currentCookTime = 0f;
        }
        

        private bool TryDrop(IPickable pickable)
        {
            if (Ingredients.Count >= MaxNumberIngredients) return false;

            var ingredient = pickable as Ingredient;
            if (ingredient == null)
            {
                Debug.LogWarning("[CookingPot] Can only drop ingredients into CookingPot", this);
                return false;
            }
            
            Ingredients.Add(ingredient);

            _totalCookTime += ingredient.CookTime;
            
            ingredient.SetMeshRendererEnabled(false);
            ingredient.gameObject.transform.SetParent(Slot);

            if (!_isCooking)
            {
                _cookCoroutine = StartCoroutine(Cook());
            }

        UpdateIngredientsUI();
        return true;
        }


    private void UpdateIngredientsUI()
    {
        for (int i = 0; i < MaxNumberIngredients; i++)
        {
            if (i < Ingredients.Count)
            {
                ingredientUISlots[i].sprite = Ingredients[i] == null ? plusIcon : Ingredients[i].SpriteUI;
            }
            else
            {
                ingredientUISlots[i].sprite = plusIcon;
            }
        }
    }

} 
