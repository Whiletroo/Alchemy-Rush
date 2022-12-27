using UnityEngine;

    [CreateAssetMenu(fileName = "LevelData", menuName = "LevelData", order = 1)]
    public class LevelData : ScriptableObject
    {
        public string levelName;
        [Tooltip("Level duration time in seconds")]
        public int durationTime;
        [Header("Star Ratings")]
        public int star1Score;
        public int star2Score;
        public int star3Score;
    }