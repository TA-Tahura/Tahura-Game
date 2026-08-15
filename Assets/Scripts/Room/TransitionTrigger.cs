using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class TransitionTrigger : MonoBehaviour
{
    public enum TransitionDirection { Up, Down }

    [SerializeField] private Transform targetDestination;
    [SerializeField] private TransitionDirection direction;
    [SerializeField] private GameObject icon;

    private static bool isTransitioning = false;

    private Transform player;
    private CameraFollow mainCamera;
    private InputAction interactAction;
    private InputActionMap playerActionMap;
    private ScreenFader fader;

    void Start()
    {
        playerActionMap = InputSystem.actions.FindActionMap("Player");
        string actionName = direction == TransitionDirection.Up ? "Interact Up" : "Interact Down";
        interactAction = InputSystem.actions.FindAction(actionName);
        
        player = GameObject.FindWithTag("Player").transform;
        fader = ScreenFader.Instance;
        mainCamera = Camera.main.GetComponent<CameraFollow>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            icon.SetActive(true);
            interactAction.performed += InitTransition;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            icon.SetActive(false);
            interactAction.performed -= InitTransition;
        }
    }

    private void OnDisable()
    {
        if (interactAction != null)
        {
            interactAction.performed -= InitTransition;
        }
    }

    void InitTransition(InputAction.CallbackContext context)
    {
        if (isTransitioning || !PlayerState.CanControl) return;
        StartCoroutine(TransitionRoutine());
    }

    private IEnumerator TransitionRoutine()
    {
        isTransitioning = true;
        PlayerState.CanControl = false;

        icon.SetActive(false);
        interactAction.performed -= InitTransition;

        float prevCameraSmooth = mainCamera.smooth;


        yield return fader.FadeTo(Color.black, 1f, 0.5f);

        mainCamera.smooth = 999f;
        player.position = targetDestination.position;
        mainCamera.UpdateY();

        yield return new WaitForSeconds(0.1f);

        mainCamera.smooth = prevCameraSmooth;

        yield return fader.FadeTo(Color.black, 0f, 0.5f);

        PlayerState.CanControl = true;
        isTransitioning = false;
    }
}