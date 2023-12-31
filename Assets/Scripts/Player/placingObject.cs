using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;
using Cinemachine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private float sensitive_rotate_camera;
    [SerializeField] private float sensitive_rotate_object;
    [SerializeField] private float sensitive_move;
    [SerializeField] private float sensitive_zoom;

    public GameObject transparentObject { get; private set; }
    // private StageController stageController;
    private GameObject playerObject;
    private Color invalidColor = new(1.0f, 0.0f, 0.0f, 0.05f);
    private Color validColor = new(0.0f, 1.0f, 0.0f, 0.05f);
    private Dictionary<string, GameObject> name2object;
    private InputActionMap placeObjectInputActionMap;
    private StageController stageController;
    private GameObject pauseMenu;
    private float rotationX = 0;
    private float rotationY = 0;
    private float diviateX = 0;
    private float diviateZ = 0;
    private float distance;
    private bool pressRotateHorizontal;
    private bool pressRotateVertical;
    private string gameMode;

    private const string FOLDERPATH = "Item";

    private const float MIN_SENSITIVE_ROTATE_CAMERA = 0.5f;
    private const float MAX_SENSITIVE_ROTATE_CAMERA = 3f;
    private const float MIN_SENSITIVE_ROTATE_OBJECT = 0.5f;
    private const float MAX_SENSITIVE_ROTATE_OBJECT = 3f;
    private const float MIN_SENSITIVE_MOVE = 0.3f;
    private const float MAX_SENSITIVE_MOVE = 3f;
    private const float MIN_SENSITIVE_ZOOM = 0.3f;
    private const float MAX_SENSITIVE_ZOOM = 3f;

    void Awake() {
        placeObjectInputActionMap = transform.parent.gameObject.GetComponent<Player>().GetPlaceObjectInputActionMap();
        pauseMenu = GameObject.Find("PauseCanvas").transform.Find("PauseMenu").gameObject;
    }

    void Start() {
        stageController = GameObject.Find("GameController").GetComponent<StageController>();
        playerObject = transform.parent.gameObject;
        transparentObject = null;
        sensitive_rotate_camera = 1.0f;
        sensitive_rotate_object = 1.0f;
        sensitive_move = 0.2f;
        sensitive_zoom = 0.7f;
        distance = 25.0f;
        pressRotateHorizontal = false;
        pressRotateVertical = false;
        gameMode = PlayerPrefs.GetString("GameMode", "Party");
        // load prefab for creating object
        name2object = new Dictionary<string, GameObject>();
        LoadAllPrefabsInFolder();
    }

    void Update() {
        MoveCamera();
        ZoomCamera();
        if (transparentObject == null) {
            TransparentObject();
        }
        AddingObject();
    }

    private void RotateCamera() {
        Vector2 inputVector = placeObjectInputActionMap.FindAction("RotateCamera").ReadValue<Vector2>().normalized;
        rotationX -= inputVector.y * sensitive_rotate_camera;
        rotationY += inputVector.x * sensitive_rotate_camera;
        rotationX = Mathf.Clamp(rotationX, -90, 90);
        transform.rotation = Quaternion.Euler(rotationX, rotationY, 0.0f);
    }

    private void MoveCamera() {
        float x1=-100, x2=100, y1=-20, y2=100, z1=-100, z2=100;
        Vector2 inputVector = placeObjectInputActionMap.FindAction("MoveCamera").ReadValue<Vector2>().normalized;
        if (inputVector.y > 0) {
            transform.position += sensitive_move * transform.forward;
        } else if (inputVector.y < 0) {
            transform.position -= sensitive_move * transform.forward;
        }
        if (inputVector.x > 0) {
            transform.position += sensitive_move * transform.right;
        } else if (inputVector.x < 0) {
            transform.position -= sensitive_move * transform.right;
        }
        float inputValue = placeObjectInputActionMap.FindAction("MoveCameraUpDown").ReadValue<float>();
        if (inputValue > 0) {
            transform.position += new Vector3(0, sensitive_move, 0);
        } else if (inputValue < 0) {
            transform.position -= new Vector3(0, sensitive_move, 0);
        }
        if (transform.position.x < x1) {
            transform.position = new Vector3(x1, transform.position.y, transform.position.z);
        }
        if (transform.position.x > x2) {
            transform.position = new Vector3(x2, transform.position.y, transform.position.z);
        }
        if (transform.position.z < z1) {
            transform.position = new Vector3(transform.position.x, transform.position.y, z1);
        }
        if (transform.position.z > z2) {
            transform.position = new Vector3(transform.position.x, transform.position.y, z2);
        }
        if (transform.position.y < y1) {
            transform.position = new Vector3(transform.position.x, y1, transform.position.z);
        }
        if (transform.position.y > y2) {
            transform.position = new Vector3(transform.position.x, y2, transform.position.z);
        }
    }

    private void ZoomCamera() {
        float scrollWheelInput = placeObjectInputActionMap.FindAction("ZoomCamera").ReadValue<float>();
        if (scrollWheelInput != 0) {
            if (scrollWheelInput > 0) {
                distance -= sensitive_zoom;
                distance = Mathf.Max(distance, 2.0f);
                if (distance > 2.0) {
                    transform.position = transform.position + transform.forward * sensitive_zoom;
                }
            } else if (scrollWheelInput < 0) {
                distance += sensitive_zoom;
                transform.position = transform.position - transform.forward * sensitive_zoom;
            }
        }
    }

    public void Enable() {
        placeObjectInputActionMap.Enable();
        placeObjectInputActionMap.FindAction("Place").started += PlaceObject;
        InputAction rotateObjectHorizontalAction = placeObjectInputActionMap.FindAction("rotateObjectHorizontal");
        rotateObjectHorizontalAction.performed += ctx => pressRotateHorizontal = true;
        rotateObjectHorizontalAction.canceled += ctx => pressRotateHorizontal = false;
        InputAction rotateObjectVerticalAction = placeObjectInputActionMap.FindAction("rotateObjectVertical");
        rotateObjectVerticalAction.performed += ctx => pressRotateVertical = true;
        rotateObjectVerticalAction.canceled += ctx => pressRotateVertical = false;
        InputAction pause = placeObjectInputActionMap.FindAction("Pause");
        pause.started += ctx => pauseMenu.GetComponent<PauseMenu>().Pause();
        enabled = true;
    }

    public void Disable() {
        placeObjectInputActionMap.Disable();
        enabled = false;
    }

    private void PlaceObject(InputAction.CallbackContext context) {
        if (PlacingIsValid()) {
            CreateObject(); 
        }
    }

    private void TransparentObject() {
        string name = playerObject.GetComponent<Player>().GetItemName();
        transparentObject = Instantiate(Resources.Load<GameObject>(FOLDERPATH + "/" + name));
        Transform zone = transparentObject.transform.Find("Zone");
        zone?.gameObject.SetActive(true);
        DisableCollidersRecursively(transparentObject);
    }

    private void DisableCollidersRecursively(GameObject obj) {
        Collider[] colliders = obj.GetComponents<Collider>();
        foreach (Collider collider in colliders) {
            collider.enabled = collider.isTrigger;
        }
        foreach (Transform child in obj.transform) {
            DisableCollidersRecursively(child.gameObject);
        }
    }

    private void CreateObject() {
        if (playerObject != null) {
            string name = playerObject.GetComponent<Player>().GetItemName();
            Transform zone = transparentObject.transform.Find("Zone");
            // if item is bomb, then destroy items
            zone?.GetComponent<DestroyDetecter>().DestroyItems();
            GameObject obj = Instantiate(name2object[name], transparentObject.transform.position, transparentObject.transform.rotation);
            obj.name = name;
            if (zone != null) {
                obj.transform.Find("Bomb").gameObject.SetActive(false);
                obj.transform.Find("Explode").gameObject.SetActive(true);
            }
            stageController.items.Add(obj);
            Destroy(transparentObject);
            transparentObject = null;
            if (gameMode == "Party") {
                playerObject.GetComponent<Player>().RemoveItem();
            }
        }
    }

    private bool PlacingIsValid() {
        // not finished
        /*
        if (transparentObject.transform.position.x > 0)
            return false;*/
        return true;
    }

    private void AddingObject(float x1=-100, float x2=100, float y1=-20, float y2=100, float z1=-100, float z2=100) {
        MoveObject(x1, x2, y1, y2, z1, z2);
        RotateObjectOrCamera();
        if (PlacingIsValid()) {
            if (transparentObject.TryGetComponent<Renderer>(out var renderer)) {
                renderer.material.color = validColor;
            }
        } else {
            if (transparentObject.TryGetComponent<Renderer>(out var renderer)) {
                renderer.material.color = invalidColor;
            }
        }
    }

    private void MoveObject(float x1=-100, float x2=100, float y1=-20, float y2=100, float z1=-100, float z2=100) {
        Transform transparentObjectTransform = transparentObject.transform;
        Vector3 forward = transform.forward;
        transparentObjectTransform.position = transform.position + distance * transform.forward;/*
        if (transparentObjectTransform.position.x < x1) {
            transparentObjectTransform.position -= Math.Abs((x1 - transparentObjectTransform.position.x) / forward.x) * forward;
        }
        if (transparentObjectTransform.position.x > x2) {
            transparentObjectTransform.position -= Math.Abs((x2 - transparentObjectTransform.position.x) / forward.x) * forward;
        }
        if (transparentObjectTransform.position.y < y1) {
            transparentObjectTransform.position -= Math.Abs((y1 - transparentObjectTransform.position.y) / forward.y) * forward;
        }
        if (transparentObjectTransform.position.y > y2) {
            transparentObjectTransform.position -= Math.Abs((y2 - transparentObjectTransform.position.y) / forward.y) * forward;
        }
        if (transparentObjectTransform.position.z < z1) {
            transparentObjectTransform.position -= Math.Abs((z1 - transparentObjectTransform.position.z) / forward.z) * forward;
        }
        if (transparentObjectTransform.position.z > z2) {
            transparentObjectTransform.position -= Math.Abs((z2 - transparentObjectTransform.position.z) / forward.z) * forward;
        }*/
    }

    private void RotateObjectOrCamera() {
        Vector2 inputVector = placeObjectInputActionMap.FindAction("RotateCamera").ReadValue<Vector2>().normalized;
        float mouseX = inputVector.x;
        float mouseY = inputVector.y;
        if (pressRotateHorizontal) {
            Vector3 cameraForward = -transform.right;

            float verticalRotationAngle = -mouseY * sensitive_rotate_object;
            float horizontalRotationAngle = mouseX * sensitive_rotate_object;

            Vector3 transparentObjectForward = transparentObject.transform.forward;
            transparentObjectForward.y = 0.0f;
            Vector3 rotatedForward = Quaternion.AngleAxis(horizontalRotationAngle, Vector3.up) * transparentObjectForward;
            Vector3 finalForward = Quaternion.AngleAxis(verticalRotationAngle, cameraForward) * rotatedForward;

            transparentObject.transform.rotation = Quaternion.LookRotation(finalForward, cameraForward);
            Vector3 newRotation = transparentObject.transform.rotation.eulerAngles;
            diviateX = newRotation.x;
            diviateZ = newRotation.z;
            newRotation.x -= diviateX;
            newRotation.z -= diviateZ;
            transparentObject.transform.rotation = Quaternion.Euler(newRotation);
        } else if (pressRotateVertical && !transparentObject.CompareTag("OnlyHorizontalRotate")) {
            mouseX = 0;
            Vector3 cameraForward = -transform.right;

            float verticalRotationAngle = -mouseY * sensitive_rotate_object;
            float horizontalRotationAngle = mouseX * sensitive_rotate_object;

            Vector3 transparentObjectForward = transparentObject.transform.forward;
            Vector3 rotatedForward = Quaternion.AngleAxis(horizontalRotationAngle, Vector3.up) * transparentObjectForward;
            Vector3 finalForward = Quaternion.AngleAxis(verticalRotationAngle, cameraForward) * rotatedForward;

            transparentObject.transform.rotation = Quaternion.LookRotation(finalForward, cameraForward);

            Vector3 newRotation = transparentObject.transform.rotation.eulerAngles;
            newRotation.x -= diviateX;
            newRotation.z -= diviateZ;
            transparentObject.transform.rotation = Quaternion.Euler(newRotation);

        } else {
            RotateCamera();
        }

        ItemVisible(transparentObject, PlacingIsValid());
    }

    private void ItemVisible(GameObject item, bool visible) {
        Transform parentTransform = item.transform;
        if (item.TryGetComponent<Renderer>(out var renderer)) {
            renderer.enabled = visible;
        }
        for (int i = 0; i < parentTransform.childCount; i++) {
            Transform childTransform = parentTransform.GetChild(i);
            GameObject childObject = childTransform.gameObject;
            ItemVisible(childObject, visible);
        }
    }

    private void LoadAllPrefabsInFolder() {
        UnityEngine.Object[] loadedObjects = Resources.LoadAll(FOLDERPATH);
        foreach (UnityEngine.Object obj in loadedObjects) {
            if (obj is GameObject) {
                name2object[obj.name] = obj as GameObject;
            }
        }
    }

    public void AdjustSensitiveRotateCamera(float ratio) {
        sensitive_rotate_camera = (1 - ratio) * MIN_SENSITIVE_ROTATE_CAMERA + ratio * MAX_SENSITIVE_ROTATE_CAMERA;
    }

    public void AdjustSensitiveRotateObject(float ratio) {
        sensitive_rotate_object = (1 - ratio) * MIN_SENSITIVE_ROTATE_OBJECT + ratio * MAX_SENSITIVE_ROTATE_OBJECT;
    }

    public void AdjustSensitiveZoomCamera(float ratio) {
        sensitive_zoom = (1 - ratio) * MIN_SENSITIVE_ZOOM + ratio * MAX_SENSITIVE_ZOOM;
    }

    public void AdjustSensitiveMoveCamera(float ratio) {
        sensitive_move = (1 - ratio) * MIN_SENSITIVE_MOVE + ratio * MAX_SENSITIVE_MOVE;
    }

    public void DestoryTransparentObject() {
        if (transparentObject != null) {
            Destroy(transparentObject);
            transparentObject = null;
        }
    }
}

