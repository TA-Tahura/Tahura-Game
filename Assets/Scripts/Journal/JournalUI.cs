using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class JournalUI : MonoBehaviour
{
    public static JournalUI Instance { get; private set; }

    [Header("Content")]
    [SerializeField, TextArea(6, 15)] private string[] chapters;

    [Header("UI Elements")]
    [SerializeField] private GameObject journalUI;
    [SerializeField] private TMP_Text leftPageText;
    [SerializeField] private TMP_Text rightPageText;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button prevButton;

    [Header("Animation Settings")]
    [SerializeField] private float textTypeSpeed = 0.02f;
    [SerializeField] private float pageTurnDelay = 0.4f;
    [SerializeField] private int rightPageCharDelay = 15; // Characters typed on left before right starts

    private int unlockedCount = 0;
    private int totalPages = 1;
    private int currentLeftPage = 1;
    private bool isOpen = false;
    private bool isPlayingSequence = false;

    private InputAction journalAction;
    private Coroutine activeSequence;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        journalAction = InputSystem.actions.FindAction("Journal");
        RebuildBookContent(out _);
        UpdateButtons();
    }

    private void OnEnable() => journalAction.performed += OnJournalAction;
    private void OnDisable() => journalAction.performed -= OnJournalAction;

    private void OnJournalAction(InputAction.CallbackContext context)
    {
        if (isPlayingSequence) return;
        if (!isOpen && PlayerState.IsAnyUIOpen) return;
        isOpen = !isOpen;
        if (isOpen) OpenMenu(); else CloseMenu();
    }

    #region Unlock Logic

    /// <summary>
    /// Standard unlock method. Can trigger the animation or unlock instantly.
    /// </summary>
    public void UnlockNextChapter(bool playSequence = true)
    {
        if (playSequence)
        {
            UnlockNextChapterSequence();
        }
        else
        {
            if (unlockedCount >= chapters.Length) return;
            unlockedCount++;
            RebuildBookContent(out _);
        }
    }

    /// <summary>
    /// Unlocks the next chapter and starts the animation sequence, returning the Coroutine handle.
    /// </summary>
    public Coroutine UnlockNextChapterSequence()
    {
        if (unlockedCount >= chapters.Length) return null;

        unlockedCount++;

        if (activeSequence != null) StopCoroutine(activeSequence);
        activeSequence = StartCoroutine(UnlockSequenceRoutine());
        return activeSequence;
    }

    private void RebuildBookContent(out int lastChapterStartChar)
    {
        lastChapterStartChar = 0;
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < unlockedCount; i++)
        {
            if (i > 0) sb.Append("<page>");

            if (i == unlockedCount - 1)
            {
                lastChapterStartChar = sb.Length;
            }

            sb.Append(chapters[i]);
        }

        string fullText = sb.ToString();
        leftPageText.text = fullText;
        rightPageText.text = fullText;

        leftPageText.ForceMeshUpdate();
        rightPageText.ForceMeshUpdate();

        totalPages = Mathf.Max(1, leftPageText.textInfo.pageCount);
    }

    #endregion

    #region Cutscene Sequence

    private IEnumerator UnlockSequenceRoutine()
    {
        isPlayingSequence = true;

        if (!isOpen)
        {
            isOpen = true;
            journalUI.SetActive(true);
            yield return new WaitForSeconds(pageTurnDelay);
        }

        RebuildBookContent(out int newChapterCharIndex);

        int charIndex = Mathf.Clamp(newChapterCharIndex, 0, Mathf.Max(0, leftPageText.textInfo.characterCount - 1));
        int startPage = leftPageText.textInfo.characterCount > 0
            ? leftPageText.textInfo.characterInfo[charIndex].pageNumber + 1
            : 1;

        bool startsOnLeft = (startPage % 2 != 0);

        currentLeftPage = startsOnLeft ? startPage : startPage - 1;
        UpdatePageDisplay();

        if (startsOnLeft)
        {
            yield return AnimateSpreadSimultaneous(currentLeftPage - 1, currentLeftPage);
        }
        else
        {
            leftPageText.maxVisibleCharacters = int.MaxValue;
            yield return AnimateSinglePage(rightPageText, startPage - 1);
        }

        // Restore full visibility
        leftPageText.maxVisibleCharacters = int.MaxValue;
        rightPageText.maxVisibleCharacters = int.MaxValue;

        isPlayingSequence = false;
        activeSequence = null;
        UpdateButtons();
    }

    private IEnumerator AnimateSpreadSimultaneous(int leftPageIndex, int rightPageIndex)
    {
        TMP_PageInfo leftInfo = leftPageText.textInfo.pageInfo[leftPageIndex];
        bool hasRightPage = rightPageIndex < totalPages;

        int leftStart = leftInfo.firstCharacterIndex;
        int leftEnd = leftInfo.lastCharacterIndex + 1;
        int leftTotal = leftEnd - leftStart;

        int rightStart = 0, rightEnd = 0, rightTotal = 0;
        if (hasRightPage)
        {
            TMP_PageInfo rightInfo = rightPageText.textInfo.pageInfo[rightPageIndex];
            rightStart = rightInfo.firstCharacterIndex;
            rightEnd = rightInfo.lastCharacterIndex + 1;
            rightTotal = rightEnd - rightStart;
        }

        // Hide text before starting
        leftPageText.maxVisibleCharacters = leftStart;
        rightPageText.maxVisibleCharacters = hasRightPage ? rightStart : int.MaxValue;

        // Account for delay in total steps so right page finishes completely
        int maxSteps = Mathf.Max(leftTotal, hasRightPage ? rightTotal + rightPageCharDelay : 0);

        for (int step = 0; step <= maxSteps; step++)
        {
            leftPageText.maxVisibleCharacters = Mathf.Min(leftStart + step, leftEnd);

            if (hasRightPage)
            {
                int rightStep = step - rightPageCharDelay;
                if (rightStep > 0)
                {
                    rightPageText.maxVisibleCharacters = Mathf.Min(rightStart + rightStep, rightEnd);
                }
            }

            yield return new WaitForSeconds(textTypeSpeed);
        }
    }

    private IEnumerator AnimateSinglePage(TMP_Text textComponent, int pageIndex)
    {
        if (pageIndex < 0 || pageIndex >= textComponent.textInfo.pageCount) yield break;

        TMP_PageInfo info = textComponent.textInfo.pageInfo[pageIndex];
        textComponent.maxVisibleCharacters = info.firstCharacterIndex;

        for (int i = info.firstCharacterIndex; i <= info.lastCharacterIndex + 1; i++)
        {
            textComponent.maxVisibleCharacters = i;
            yield return new WaitForSeconds(textTypeSpeed);
        }
    }

    #endregion

    #region Navigation

    public void OpenMenu()
    {
        PlayerState.IsAnyUIOpen = true;
        PlayerState.CanControl = false;
        isOpen = true;
        journalUI.SetActive(true);
        leftPageText.maxVisibleCharacters = int.MaxValue;
        rightPageText.maxVisibleCharacters = int.MaxValue;
        UpdatePageDisplay();
    }

    public void CloseMenu()
    {
        isOpen = false;
        journalUI.SetActive(false);
        PlayerState.CanControl = true;
        PlayerState.IsAnyUIOpen = false;
    }

    public void NextSpread()
    {
        if (isPlayingSequence || currentLeftPage + 2 > totalPages) return;
        currentLeftPage += 2;
        UpdatePageDisplay();
    }

    public void PreviousSpread()
    {
        if (isPlayingSequence || currentLeftPage - 2 < 1) return;
        currentLeftPage -= 2;
        UpdatePageDisplay();
    }

    private void UpdatePageDisplay()
    {
        leftPageText.pageToDisplay = currentLeftPage;
        rightPageText.pageToDisplay = currentLeftPage + 1;
        UpdateButtons();
    }

    private void UpdateButtons()
    {
        if (prevButton != null) prevButton.interactable = !isPlayingSequence && (currentLeftPage > 1);
        if (nextButton != null) nextButton.interactable = !isPlayingSequence && (currentLeftPage + 1 < totalPages);
    }

    #endregion
}