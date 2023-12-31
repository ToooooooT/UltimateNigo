using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    private PlayerInputManager playerManager;
    private StageController stageController;
    private GameObject pauseCanvas;
    private GameObject blackCanvas;

    private static readonly int[] slidersPosX = new int[] { 880, 500, 350, 300 };
    private static readonly int[] slidersGridSpace = new int[] { 300, 700, 500, 330 };
    private static readonly int[] slidersCellSizeX = new int[] { 300, 300, 200, 150 };

    private Material[] playerMaterials = new Material[4];
    private Sprite[] playerCursors = new Sprite[4];

    public List<PlayerInput> playerList = new();
    public event System.Action<PlayerInput> PlayerJoinedGame;
    public event System.Action<PlayerInput> PlayerLeftGame;

    [SerializeField] InputAction joinAction;
    [SerializeField] InputAction leaveAction;

    private const string PLAYER_MATERIALS_FOLDER = "Materials";
    private const string PLAYER_CURSORS_FOLDER = "Cursors";

    private AudioManager audioManager;

    private void Awake() {
        playerManager = GetComponent<PlayerInputManager>();
        stageController = GetComponent<StageController>();
        pauseCanvas = GameObject.Find("PauseCanvas");
        blackCanvas = GameObject.Find("BlackCanvas");

        joinAction.Enable();
        joinAction.performed += context => JoinAction(context);

        leaveAction.Enable();
        leaveAction.performed += context => LeaveAction(context);

        playerMaterials[0] = Resources.Load<Material>(PLAYER_MATERIALS_FOLDER + "/PlayerBody");
        playerMaterials[1] = Resources.Load<Material>(PLAYER_MATERIALS_FOLDER + "/PlayerBody_Blue");
        playerMaterials[2] = Resources.Load<Material>(PLAYER_MATERIALS_FOLDER + "/PlayerBody_Red");
        playerMaterials[3] = Resources.Load<Material>(PLAYER_MATERIALS_FOLDER + "/PlayerBody_Green");

        playerCursors[0] = Resources.Load<Sprite>(PLAYER_CURSORS_FOLDER + "/YellowCursor");
        playerCursors[1] = Resources.Load<Sprite>(PLAYER_CURSORS_FOLDER + "/BlueCursor");
        playerCursors[2] = Resources.Load<Sprite>(PLAYER_CURSORS_FOLDER + "/RedCursor");
        playerCursors[3] = Resources.Load<Sprite>(PLAYER_CURSORS_FOLDER + "/GreenCursor");
    }

    void Start() {
        audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
    }

    void Update() {

    }

    void OnPlayerJoined(PlayerInput playerInput) {
        playerList.Add(playerInput);
        PlayerJoinedGame?.Invoke(playerInput);
        GameObject player = playerInput.gameObject;
        stageController.playerObjects.Add(player);
        audioManager.PlaySE("summon1");
        // set player materials
        List<Material> materialList = new()
        {
            playerMaterials[playerList.Count - 1]
        };
        player.transform.Find("PlayerVisual").Find("Head").GetComponent<MeshRenderer>().SetMaterials(materialList);
        player.transform.Find("PlayerVisual").Find("Body").GetComponent<MeshRenderer>().SetMaterials(materialList);
        // set player cursors 
        player.transform.Find("Canvas").Find("Cursor").GetComponent<Image>().sprite = playerCursors[playerList.Count - 1];
        // modify sliders layout when adding player
        GameObject sliders = pauseCanvas.transform.Find("SettingMenu").Find("Sliders").gameObject;
        int n = playerList.Count;
        Vector3 pos = sliders.GetComponent<RectTransform>().position;
        pos.x = slidersPosX[n - 1];
        sliders.GetComponent<RectTransform>().position = pos;
        Vector2 spacing = sliders.GetComponent<GridLayoutGroup>().spacing;
        spacing.x = slidersGridSpace[n - 1];
        sliders.GetComponent<GridLayoutGroup>().spacing = spacing;
        // attach new slider to sliders
        GameObject newSlider = Instantiate(Resources.Load<GameObject>("Canvas/Slider"));
        newSlider.transform.SetParent(sliders.transform);
        Vector2 cellSize = newSlider.GetComponent<GridLayoutGroup>().cellSize;
        cellSize.x = slidersCellSizeX[n - 1];
        newSlider.GetComponent<GridLayoutGroup>().cellSize = cellSize;
        newSlider.GetComponent<CameraSlider>().SetCamera(playerInput.gameObject);
        // add black curtain if only three players
        if (playerList.Count == 3) {
            blackCanvas.transform.Find("Black").gameObject.SetActive(true);
        } else if (playerList.Count == 4) {
            blackCanvas.transform.Find("Black").gameObject.SetActive(false);
        }
    }

    void OnPlayerLeft(PlayerInput playerInput) {

    }

    void JoinAction(InputAction.CallbackContext context) {
        playerManager.JoinPlayerFromActionIfNotAlreadyJoined(context);
    }

    void LeaveAction(InputAction.CallbackContext context) {
        if (playerList.Count > 1) {
            foreach (var player in playerList) {
                foreach (var device in player.devices) {
                    if (device != null && context.control.device == device) {
                        Unregisterplayer(player);
                        return;
                    }
                }
            }
        }
    }

    public void Unregisterplayer(PlayerInput playerInput) {
        playerList.Remove(playerInput);
        stageController.playerObjects.Remove(playerInput.gameObject);
        CameraMovement virtualCamera = playerInput.gameObject.transform.Find("Camera").GetComponent<CameraMovement>();
        if (virtualCamera.transparentObject != null) {
            Destroy(virtualCamera.transparentObject);
        }
        virtualCamera.Disable();
        virtualCamera.enabled = false;
        playerInput.gameObject.transform.Find("Camera").GetComponent<MouseControlFollowCamera>().enabled = false;
        playerInput.gameObject.GetComponent<Player>().Disable(Player.State.STOP);
        Destroy(playerInput.gameObject);
    }

    public void DisableJoinAction() {
        joinAction.Disable();
    }

    public void EnableJoinAction() {
        joinAction.Enable();
    }
}
