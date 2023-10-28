using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.UI;
using UnityEngine.UIElements;

public class Player : MonoBehaviour {
   
    [SerializeField] private float moveSpeed;
    [SerializeField] private float accelerateMoveSpeed;
    [SerializeField] private float moveSpeedJumpWallratio;
    [SerializeField] private float rotateSpeed;
    [SerializeField] private float jumpSpeed;
    [SerializeField] private float gravityMaxSpeedWithFriction;
    [SerializeField] private float gravityMaxSpeed;
    [SerializeField] private float gravity;
    [SerializeField] private float buttonPressedWindow;
    [SerializeField] private GameInput gameInput;

    private bool isWalking = false;
    private bool isJumping = false;
    private bool canJump = false;
    private float buttonPressedTime;
    private float verticalVelocity;
    private CharacterController controller;

    public void Start() {
        moveSpeed = 5f;
        accelerateMoveSpeed = 7f;
        moveSpeedJumpWallratio = 5f;
        rotateSpeed = 10f;
        jumpSpeed = 25f;
        gravity = 60;
        gravityMaxSpeed = 20f;
        gravityMaxSpeedWithFriction = 5f;
        buttonPressedWindow = .3f;
        controller = GetComponent<CharacterController>();
    }

    private void Update() {
        HandleMovement();
        HandleJump();
        HandleFacement();
    }

    public bool IsWalking() {
        return isWalking;
    }

    private void HandleMovement() {
        // velocity = sqrt(JumpHeight * (-2) * gravity)
        Vector3 moveDir = gameInput.GetMovementVectorNormalized();
        float velocity = gameInput.AccelerateMove() ? accelerateMoveSpeed : moveSpeed;
        Vector3 moveVector = velocity * Time.deltaTime * moveDir;
        controller.Move(moveVector);
        isWalking = moveDir != Vector3.zero;
    }

    private void HandleJump() {
        Vector3 moveVector = Vector3.zero;
        bool isWall = CheckWall();
        canJump |= isWall;
        if (Input.GetKeyDown(KeyCode.Space) && canJump) {
            isJumping = true;
            canJump = false;
            buttonPressedTime = 0;
        }
        if (isJumping && isWall) {
            WallJump();
            return;
        }
        if (isJumping) {
            buttonPressedTime += Time.deltaTime;
            verticalVelocity = jumpSpeed;
            if (buttonPressedTime > buttonPressedWindow || Input.GetKeyUp(KeyCode.Space)) {
                isJumping = false;
                verticalVelocity = 0;
            }
        }  else if (controller.isGrounded) {
            isJumping = false;
            canJump = true;
            verticalVelocity = -1;
        } else {
            verticalVelocity -= gravity * Time.deltaTime;
        }

        verticalVelocity = Mathf.Clamp(verticalVelocity, 
                                        isWall ? -gravityMaxSpeedWithFriction : -gravityMaxSpeed, 
                                        float.PositiveInfinity);
        moveVector.y = verticalVelocity;
        controller.Move(moveVector * Time.deltaTime);
    }

    private void WallJump() {
        Vector3 moveVector = Vector3.zero;
        verticalVelocity = 20;
        Vector3 p1 = transform.position + controller.center + Vector3.up * -controller.height * 0.5F;
        Vector3 p2 = p1 + Vector3.up * controller.height;
        if (Physics.CapsuleCast(p1, p2, controller.radius, transform.forward, out RaycastHit hit, .2f) && hit.collider.CompareTag("Wall")) {
            moveVector = hit.normal * moveSpeed * moveSpeedJumpWallratio;
        }
        moveVector.y = verticalVelocity;
        Debug.Log(moveVector);
        controller.Move(moveVector * Time.deltaTime);
        isJumping = false;
    }

    private bool CheckWall() {
        Vector3 p1 = transform.position + controller.center + Vector3.up * -controller.height * 0.5F;
        Vector3 p2 = p1 + Vector3.up * controller.height;
        if (Physics.CapsuleCast(p1, p2, controller.radius, transform.forward, out RaycastHit hit, .2f) && hit.collider.CompareTag("Wall")) {
            return true;
        }
        return false;
    }

    private void HandleFacement() {
        Vector3 moveDir = gameInput.GetMovementVectorNormalized();
        transform.forward = Vector3.Slerp(transform.forward, moveDir, Time.deltaTime * rotateSpeed);
    }
}
