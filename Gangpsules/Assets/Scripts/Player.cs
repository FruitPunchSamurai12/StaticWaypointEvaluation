using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, IMover
{
    [SerializeField] Explosion deathEffect;
    [SerializeField] MeshRenderer[] renderers;
    [SerializeField] Collider col;
    PlayerAnimator playerAnimator;
    CharacterController controller;
    GameObject playerGFX;
    WeaponUser weaponUser;
    InteractBox interactBox;
    RotateTowardsMouse rotateTowardsMouse;
    PlayerHealth health;
   

    public float speed = 6f;
    public Vector3 Direction { get; private set; }
    public float Speed => speed;



    //gravity stuff
    public Transform groundCheck;
    public float groundDistance = 0.667f;
    public LayerMask groundMask;
    Vector3 fallVelocity;
    bool isGrounded;



    //roll stuff
    public bool rolling = false;
    public float rollSpeed = 5f;
    public float rollDuration = 0.4f;
    public Vector3 rollDirection;

    bool isDead = false;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        weaponUser = GetComponent<WeaponUser>();
        playerAnimator = GetComponent<PlayerAnimator>();
        interactBox = GetComponent<InteractBox>();
        rotateTowardsMouse = GetComponent<RotateTowardsMouse>();
        weaponUser.onEquip += () => UIManager.Instance.EquipedAGun(weaponUser.weapon.weaponAmmo);
        weaponUser.onNeedReload += () => UIManager.Instance.StartReload(weaponUser.weapon.reloadTime);
        weaponUser.onReloadComplete+=()=> UIManager.Instance.UpdateAmmo(weaponUser.weapon.weaponAmmo);
        weaponUser.onAmmoPickup += () => UIManager.Instance.UpdateAmmo(weaponUser.weapon.weaponAmmo);
        health = GetComponent<PlayerHealth>();
        health.onPlayerTookDamage += HandleDamageTaken;
        health.onPlayerRecoveredHealth += HandleHealthRecover;
    }


    void Update()
    {
        if (isDead) return;
        MovementInput();

        Dash();

        if (Input.GetButton("Fire") && weaponUser.weapon!=null)
        {
            Vector2 lookDirection = rotateTowardsMouse.GetLookDirection(weaponUser.weapon.LineOfSightStart.position);
            weaponUser.UseWeapon(lookDirection);
            UIManager.Instance.UpdateAmmo(weaponUser.weapon.weaponAmmo);
        }

        InteractWithObjectInFront();

        Movement();
        playerAnimator.MovementAnimation(Direction);

        //gravity
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        ApplyGravity();
    }

    private void MovementInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Direction = new Vector3(horizontal, 0, vertical).normalized;
    }

    private void Dash()
    {
        if (isGrounded && !rolling && Input.GetButtonDown("Roll"))
        {
            if (Direction.magnitude < 0.1f)
            {
                rollDirection = transform.forward;
            }
            else
            {
                rollDirection = Direction;
            }
            playerAnimator.DashAnimation(Direction);
            rolling = true;
            StartCoroutine("Rolling");
        }
        if (rolling)
        {
            controller.Move(rollDirection * rollSpeed * Time.deltaTime);
        }
    }

    private void FixedUpdate()
    {
        if (isDead) return;
        interactBox.LookForInteractables();
    }

    private void Movement()
    {
        if (Direction.magnitude >= 0.1f && !rolling)
        {
            controller.Move(Direction * speed * Time.deltaTime);
        }
    }

    void InteractWithObjectInFront()
    {
        if (!rolling && Input.GetKeyDown(KeyCode.E))
        {
            interactBox.Interact(this);
        }
    }


    void ApplyGravity()
    {
        if (isGrounded && fallVelocity.y < 0.1f)
        {
            fallVelocity.y = -2f;
        }
        fallVelocity.y += Physics.gravity.y * Time.deltaTime;
        controller.Move(fallVelocity * Time.deltaTime);
    }

    IEnumerator Rolling()
    {
        yield return new WaitForSeconds(rollDuration);
        rolling = false;
    }

    public void EquipWeapon(Weapon weapon)
    {
        weaponUser.EquipWeapon(weapon);
    }

    void HandleDamageTaken(int currentHealth)
    {
        if (isDead) return;
        UIManager.Instance.UpdatePlayerHealth(currentHealth);
        if(currentHealth<=0)
        {
            isDead = true;
            deathEffect.Get<Explosion>(new Vector3(transform.position.x,1,transform.position.z), Quaternion.identity);
            foreach (var rend in renderers)
            {
                rend.enabled = false;
            }
            weaponUser.weapon.gameObject.SetActive(false);
            col.enabled = false;
            controller.enabled = false;
            GameManager.Instance.GameOver();
        }
    }

    public void Reset()
    {
        isDead = false;
        foreach (var rend in renderers)
        {
            rend.enabled = true;
        }
        weaponUser.weapon.gameObject.SetActive(true);
        col.enabled = true;
        controller.enabled = true;
        health.Reset();
        UIManager.Instance.UpdatePlayerHealth(health.currentHealth);
        UIManager.Instance.UpdateAmmo(weaponUser.weapon.weaponAmmo);
    }

    void HandleHealthRecover(int currentHealth)
    {
        if (isDead) return;
        UIManager.Instance.UpdatePlayerHealth(currentHealth);
    }
}

