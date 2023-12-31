using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pixace : BaseItem
{
    private enum State {placing, gaming};
    private State state;
    private float rotationSpeed;
    private float angular;
    private float clockwise;
    private Vector3 initRight;
    private float initRotateY;
    private AudioManager audioManager;


    void Awake() {
        state = State.placing;
        initRight = new Vector3(0, 0, 0);
    }

    void Start() {
        rotationSpeed = 30;
        angular = -0.1f;
        clockwise = 1f;
        audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
    }

    void Update() {
        if (state == State.gaming) {
            Rotate();
        }
    }

    void Rotate() {
        if (angular <= 0) {
            if (clockwise == -1f) {
                audioManager.PlaySE("swing");
            }
            clockwise = 1f;
            initRight = transform.right;
        } else if (angular >= 2 * Mathf.PI) {
            clockwise = -1f;
        }
        angular += clockwise * Time.deltaTime * Mathf.Max(Mathf.Pow(rotationSpeed * (1 - Mathf.Cos(angular)), 0.5f), 0.01f);
        transform.up = new Vector3(0, 1, 0) * Mathf.Cos(angular) + new Vector3(1, 0, 0) * Mathf.Sin(angular);
        Vector3 newRotation = transform.eulerAngles;
        newRotation.y += initRotateY;
        transform.eulerAngles = newRotation;
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            if (angular > Mathf.PI / 2 && angular < 3 * Mathf.PI /2) {
                other.GetComponent<Player>().SetDead();
            }
        }
    }

    public override void Initialize() {
        initRotateY = transform.eulerAngles.y;
        state = State.gaming;
    }

    public override void Reset() {
        state = State.placing;
    }
}
