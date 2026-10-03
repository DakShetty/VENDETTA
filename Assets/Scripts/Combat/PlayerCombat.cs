using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public float attackDamage = 25f;
    public float attackRange = 2.5f;
    public GameObject sword;
    public float swingDuration = 0.25f;

    private bool attacking;
    private float swingTimer;
    private int comboStep;
    private bool queuedAttack;

    private bool hasHit;
    private Vector3 startPosition;
    private Vector3 endPosition;
    private Quaternion startRotation;
    private Quaternion endRotation;
    private bool downwardAttack;
    private bool grounded;

    void Start()
    {
        if (sword != null)
            sword.SetActive(false);
    }

    void Update()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.IsGameOver())
            return;

        grounded = Physics.Raycast(transform.position, Vector3.down, 1.2f);

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.F))
        {
            if (!grounded)
            {
                StartDownwardAttack();
            }
            else if (attacking)
            {
                queuedAttack = true;
            }
            else
            {
                StartAttack();
            }
        }

        if (!attacking)
            return;

        swingTimer += Time.deltaTime;

        float progress = Mathf.Clamp01(swingTimer / swingDuration);

        float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

        if (smoothProgress >= 0.45f && !hasHit)
        {
            AttackHit();
            hasHit = true;
        }

        sword.transform.localPosition =
            Vector3.Lerp(startPosition, endPosition, smoothProgress);

        sword.transform.localRotation =
            Quaternion.Slerp(startRotation, endRotation, smoothProgress);

        if (progress >= 1f)
            EndAttack();
    }

    void FaceCameraDirection()
    {
        if (Camera.main == null) return;
        Transform cam = Camera.main.transform;

        Vector3 direction = cam.forward;
        direction.y = 0;

        if (direction.sqrMagnitude > 0.01f)
        {
            transform.forward = direction.normalized;
        }
    }

    void StartAttack()
    {
        FaceCameraDirection();
        VendettaAudioManager.Instance?.PlaySwordSwing();

        attacking = true;
        queuedAttack = false;
        swingTimer = 0f;

        if (sword == null)
            return;

        sword.SetActive(true);

        if (comboStep == 0)
        {
            // Right -> left slash
            startPosition = new Vector3(0.7f, 0.2f, 1.0f);
            endPosition = new Vector3(-0.2f, 0.2f, 1.1f);

            startRotation = Quaternion.Euler(0, 0, -60);
            endRotation = Quaternion.Euler(0, 0, 60);
        }
        else
        {
            // Left -> right slash
            startPosition = new Vector3(-0.2f, 0.2f, 1.1f);
            endPosition = new Vector3(0.7f, 0.2f, 1.0f);

            startRotation = Quaternion.Euler(0, 0, 60);
            endRotation = Quaternion.Euler(0, 0, -60);
        }

        sword.transform.localPosition = startPosition;
        sword.transform.localRotation = startRotation;

        hasHit = false;

        comboStep++;

        if (comboStep >= 2)
            comboStep = 0;
    }

    void StartDownwardAttack()
    {
        if (attacking)
            return;

        FaceCameraDirection();
        VendettaAudioManager.Instance?.PlaySwordSwing();

        attacking = true;
        downwardAttack = true;
        queuedAttack = false;
        swingTimer = 0f;
        hasHit = false;

        if (sword != null)
        {
            sword.SetActive(true);

            startPosition = new Vector3(0.5f, 0.3f, 0.8f);
            endPosition = new Vector3(0.2f, -0.5f, 0.8f);

            startRotation = Quaternion.Euler(-30, 0, -30);
            endRotation = Quaternion.Euler(90, 0, 0);

            sword.transform.localPosition = startPosition;
            sword.transform.localRotation = startRotation;
        }
    }

    void AttackHit()
    {
        Vector3 attackCenter =
            transform.position + transform.forward * 1.5f;

        Collider[] hits = Physics.OverlapSphere(
            attackCenter,
            attackRange
        );

        foreach (Collider hit in hits)
        {
            IDamageable target =
                hit.GetComponentInParent<IDamageable>();

            if (target != null &&
                hit.transform.root != transform.root)
            {
                target.TakeDamage(attackDamage);
                VendettaAudioManager.Instance?.PlaySwordHit();
                Debug.Log("Sword hit: " + hit.name);
            }
        }
    }

    void EndAttack()
    {
        attacking = false;

        if (sword != null)
            sword.SetActive(false);

        if (downwardAttack)
        {
            downwardAttack = false;
            return;
        }

        if (queuedAttack)
            StartAttack();
    }
}
