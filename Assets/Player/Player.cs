﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float moveSpeed = 10.0f;
    public float fireDelay = 0.2f;
    public float inheritedSpeedScale = 0.3f;
    public float accel = 100.0f;
    public float decel = 400.0f;
    public float angularAccel = 5.0f;

    public Vector3 lastInput = Vector3.zero;

    private Inventory inventory;

    public GameObject whatIsGoalPointer;
    private GameObject myGoalPointer;
    private POI myPOI;

    //private float fireCooldown = 0.0f;
    private CharacterController charController;
    protected GameManager gameManager;
    private float currentSpeed = 0.0f;
    private bool swapRightAxis = false;

    private Plane groundPlane;
    private AudioSource[] audioSources;

  

    // Use this for initialization
    void Start()
    {
        inventory = FindObjectOfType<Inventory>();
        charController = GetComponent<CharacterController>();
        myPOI = GetComponent<POI>();
        swapRightAxis = Application.platform == RuntimePlatform.WebGLPlayer;

        groundPlane = new Plane(Vector3.up, Vector3.zero);

        audioSources = GetComponents<AudioSource>();
        gameManager = FindObjectOfType<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        var currentGoal = gameManager.GetCurrentGoal();

        Transform CamTran = Camera.main.transform;
        Vector3 forward = CamTran.forward;
        forward.y = 0;
        forward.Normalize();
        Vector3 right = CamTran.right;
        right.y = 0;
        right.Normalize();
        Vector3 InputV =
            gameManager.GetGameState() == GameState.STARTED
                ? Input.GetAxis("Horizontal") * right + Input.GetAxis("Vertical") * forward
                : Vector3.zero;
        if (InputV.magnitude > 0.1f)
        {
            if (currentSpeed > 0.0f)
            {
                lastInput = Vector3.Slerp(lastInput, InputV, angularAccel);
            }
            else
            {
                lastInput = InputV;
            }
            currentSpeed = Mathf.Min(currentSpeed + accel * Time.deltaTime, moveSpeed);
        }
        else
        {
            currentSpeed = Mathf.Max(currentSpeed - decel * Time.deltaTime, 0.0f);
        }

        if (currentSpeed > -1.0f)
        {
            charController.SimpleMove(lastInput * currentSpeed);
            if (myPOI != null)
            {
                myPOI.forward = lastInput;
            }
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float enter = 0.0f;

        Debug.Log(Input.GetAxis("AimHorizontal"));
        Vector3 AimV = Vector3.zero;
        if (swapRightAxis)
        {
            AimV = Input.GetAxis("AimHorizontal") * forward + Input.GetAxis("AimVertical") * -right;
        }
        else
        {
            AimV = Input.GetAxis("AimHorizontal") * right + Input.GetAxis("AimVertical") * forward;
        }

        if (Input.GetButton("Fire1"))
        {
            if (groundPlane.Raycast(ray, out enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                AimV = (hitPoint - transform.position);
                AimV.y = 0.0f;
            }
        }

        if (gameManager.GetGameState() == GameState.STARTED)
        {
            HandleDropItemInput();

            myGoalPointer.transform.position = transform.position;
            if (currentGoal)
            {
                Vector3 dirToGoal = currentGoal.transform.position - transform.position;
                myGoalPointer.transform.rotation = Quaternion.LookRotation(dirToGoal.normalized);
                //Debug.DrawLine(currentGoal.transform.position, transform.position);
            }
        }
    }

    public void Init()
    {
        myGoalPointer = Instantiate(whatIsGoalPointer, transform.position, Quaternion.identity);
        myPOI = GetComponent<POI>();
    }

    public void RechargeInventory(int attractiveCount, int repulsiveCount)
    {
        this.inventory.AddAttractive(attractiveCount);
        this.inventory.AddRepulsive(repulsiveCount);
    }

    public int getAttractiveObjCount()
    {
        return this.inventory.attractiveObjCount;
    }

    public int getRepulsiveObjCount()
    {
        return this.inventory.repulsiveObjCount;
    }

    private void HandleDropItemInput()
    {
        if (Input.GetButtonDown("DropItem1"))
        {
            if (inventory.HasAttractive())
            {
                Instantiate(
                    inventory.DropAttractive(),
                    this.gameObject.GetComponent<Player>().transform.position,
                    Quaternion.identity
                );
            }
        }
        if (Input.GetButtonDown("DropItem2"))
        {
            if (inventory.HasRepulsive())
            {
                Instantiate(
                    inventory.DropRepulsive(),
                    this.gameObject.GetComponent<Player>().transform.position,
                    Quaternion.identity
                );
            }
        }
    }
}
