using UnityEngine;
using UnityEngine.InputSystem;

/// Gerak kiri-kanan (panah/WASD), Shift untuk lari, animasi frame sprite + SFX langkah.
public class PlayerController2D : MonoBehaviour
{
    public float walkSpeed = 2.6f;
    public float runSpeed = 5.2f;
    public float minX = -18.5f;
    public float maxX = 18.5f;

    [Header("Animation")]
    public Sprite[] walkFrames;
    public Sprite idleFrame;
    public float walkFps = 14f;
    public float runFps = 22f;

    [Header("Audio")]
    public AudioSource footstepSource;

    [HideInInspector] public bool canMove = true;

    public bool IsMoving { get; private set; }

    SpriteRenderer sr;
    float animTime;
    private InputAction moveAction;
    private InputAction runAction;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        runAction = InputSystem.actions.FindAction("Sprint");
    }

    void Update()
    {
        
        float input = 0f;
        bool run = false;

        bool paused = Time.timeScale < 0.01f;
        bool dialogOpen = DialogueManager.Instance != null && DialogueManager.Instance.IsActive;

        if (canMove && !paused && !dialogOpen && moveAction != null && PlayerState.CanControl) 
        {
            input = moveAction.ReadValue<Vector2>().x;

            if (runAction != null)
            {
                run = runAction.IsPressed();
            }
        }

        IsMoving = Mathf.Abs(input) > 0.01f;

        if (IsMoving)
        {
            float speed = run ? runSpeed : walkSpeed;
            var p = transform.position;
            p.x = Mathf.Clamp(p.x + input * speed * Time.deltaTime, minX, maxX);
            transform.position = p;

            // Flip: sprite asli menghadap kanan.
            var s = transform.localScale;
            s.x = Mathf.Abs(s.x) * (input < 0 ? -1f : 1f);
            transform.localScale = s;

            animTime += Time.deltaTime * (run ? runFps : walkFps);
            if (walkFrames != null && walkFrames.Length > 0)
                sr.sprite = walkFrames[Mathf.FloorToInt(animTime) % walkFrames.Length];
        }
        else
        {
            animTime = 0f;
            if (idleFrame != null) sr.sprite = idleFrame;
        }

        if (footstepSource != null)
        {
            if (IsMoving && !paused)
            {
                footstepSource.pitch = run ? 1.35f : 1f;
                if (!footstepSource.isPlaying) footstepSource.Play();
            }
            else if (footstepSource.isPlaying)
            {
                footstepSource.Pause();
            }
        }
    }
}
