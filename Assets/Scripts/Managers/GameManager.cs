using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using DaltonLima.Core;
using UnityEngine.SceneManagement;
using Lean.Transition;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private LevelData MainLevel;
    [SerializeField] private InputController inputController;
    [SerializeField] private PlayerInput playerInput;

    [SerializeField] private AudioClip mainTheme;

    public static LevelData LevelData => Instance.MainLevel;

    private void Awake()
    {
#if UNITY_EDITOR
        Assert.IsNotNull(MainLevel);
        Assert.IsNotNull(inputController);
#endif
    }

    private async void Start()
    {
        await GameLoop();
    }

    private async Task GameLoop()
    {
        await StartLevelAsync(MainLevel);
    }

    private bool _userPressedStart;

    private async Task StartLevelAsync(LevelData levelData)
    {
        inputController.EnableGameplayControls();
        inputController.EnableFirstPlayerController();
        this.PlaySoundTransition(mainTheme);
    }
}