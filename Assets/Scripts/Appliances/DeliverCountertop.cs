using System;
using Lean.Transition;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;


    public class DeliverCountertop : Interactable
    {

        [SerializeField] private TextMeshProUGUI scoreCountText;
        public delegate void PotionDropped(Potion potion);
        public static event PotionDropped OnPotionDropped;
        public delegate void PotionMissing();

        public int score;


        public override bool TryToDropIntoSlot(IPickable pickableToDrop)
        {
            if (pickableToDrop == null) return false;
            
            switch (pickableToDrop)
            {
                case Potion potion:
                    potion.transform.SetParent(null);
                    potion.transform.SetPositionAndRotation(Slot.position, Quaternion.identity);
                    OnPotionDropped?.Invoke(potion);
                    score = Int32.Parse(scoreCountText.text) + 1;
                scoreCountText.text = score.ToString();
                    potion.transform.position = new Vector3(10000f, 10000f, 10000f);
                    return true;
                case Ingredient ingredient:
                    return false;
            }
            return false;
        }

        public override IPickable TryToPickUpFromSlot(IPickable playerHoldPickable) => null;
        
    }
