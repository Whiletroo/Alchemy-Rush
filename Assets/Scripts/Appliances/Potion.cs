using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class Potion : Interactable, IPickable
    {
        
        private const int MaxNumberIngredients = 2;
        
        private Rigidbody _rigidbody;
        private Collider _collider;
        private readonly List<Ingredient> _ingredients = new List<Ingredient>(MaxNumberIngredients);

        public bool IsClean { get; private set; } = true;
        public List<Ingredient> Ingredients => _ingredients;
        public bool IsEmpty() =>_ingredients.Count == 0;

        protected override void Awake()
        {
            base.Awake();
            _rigidbody = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();
            
            #if UNITY_EDITOR
                Assert.IsNotNull(_rigidbody);
                Assert.IsNotNull(_collider);
#endif
            
            Setup();
        }
        
        private void Setup()
        {
            _rigidbody.isKinematic = true;
            _collider.enabled = false;
        }

        public bool AddIngredients(List<Ingredient> ingredients)
        {
            if (!IsEmpty()) return false;
            _ingredients.AddRange(ingredients);
            
            foreach (var ingredient in _ingredients)
            {
                ingredient.transform.SetParent(Slot);
                ingredient.transform.SetPositionAndRotation(Slot.transform.position, Quaternion.identity);
            }
            return true;
        }
        
        public void RemoveAllIngredients()
        {
            _ingredients.Clear();
        }

        public static bool CheckSoupIngredients(IReadOnlyList<Ingredient> ingredients)
        {
            if (ingredients == null || ingredients.Count != 2)
            {
                return false;
            }

            if (ingredients[0].Type != IngredientType.Flower &&
                ingredients[0].Type != IngredientType.Leg &&
                ingredients[0].Type != IngredientType.Root)
            {
                Debug.Log("[Potion] Soup only must contain onion, tomato or mushroom");
                return false;
            }
            
            return true;
        }
        
        
        public void Pick()
        {
            _rigidbody.isKinematic = true;
            _collider.enabled = false;
        }

        public void Drop()
        {
            gameObject.transform.SetParent(null);
            _rigidbody.isKinematic = false;
            _collider.enabled = true;
        }
        
        public override bool TryToDropIntoSlot(IPickable pickableToDrop)
        {
            if (pickableToDrop == null) return false;
            
            //we can drop soup from plate to plate AND from plate to CookingPot (and vice-versa)
            switch (pickableToDrop)
            {
                case CookingPot cookingPot:
                    if (cookingPot.IsCookFinished &&
                        //cookingPot.IsBurned == false &&
                        CheckSoupIngredients(cookingPot.Ingredients))
                    {
                        AddIngredients(cookingPot.Ingredients);
                        cookingPot.EmptyPan();
                        return false;
                    }
                    break;
                case Ingredient ingredient:
                    Debug.Log("[Potion] Trying to dropping Ingredient into Potion! Not implemented");
                    break;
                case Potion plate:
                    //Debug.Log("[Potion] Trying to drop something from a plate into other plate! We basically swap contents");
                    if (this.IsEmpty() == false || this.IsClean == false) return false;
                    this.AddIngredients(plate.Ingredients);
                    plate.RemoveAllIngredients();
                    return false;
                default:
                    Debug.LogWarning("[Potion] Drop not recognized", this);
                    break;
            }
            return false;
        }

        public override IPickable TryToPickUpFromSlot(IPickable playerHoldPickable)
        {
            // We can pickup Ingredients from plates with other plates (effectively swapping content) or from Pans
            
            if (playerHoldPickable == null) return null;
            
            switch (playerHoldPickable)
            {
                // we just pick the soup ingredients, not the CookingPot itself
                case CookingPot cookingPot:
                    Debug.Log("[Potion] Trying to pick from a plate with a CookingPot", this);
                    break;
                case Ingredient ingredient:
                    //TODO: we can pickup some ingredients into plate, not all of them.
                    break;
                // swap plate ingredients
                case Potion plate:
                    if (plate.IsEmpty())
                    {
                        if (this.IsEmpty()) return null;
                        plate.AddIngredients(this._ingredients);
                    }
                    break;
            }
            return null;
        }
    }
