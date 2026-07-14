using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// Mini game 1: pasang label arah (U & S) ke lengan kompas yang glitch. TG adalah pengecoh.
public class CompassMiniGame : MonoBehaviour
{
    public Image kompasGlitch;
    public Image kompasSempurna;
    public CompassTile[] tiles;

    public event Action onWrongPlacement;
    public event Action onComplete;

    int placedCount;
    int requiredCount;
    bool finished;

    public bool InputEnabled { get; set; }

    void Awake()
    {
        foreach (var t in tiles)
        {
            t.game = this;
            if (t.target != null) requiredCount++;
        }
        if (kompasSempurna != null) kompasSempurna.gameObject.SetActive(false);
    }

    public void NotifyWrong()
    {
        if (!finished) onWrongPlacement?.Invoke();
    }

    public void NotifyPlaced()
    {
        placedCount++;
        if (placedCount >= requiredCount && !finished)
        {
            finished = true;
            InputEnabled = false;
            StartCoroutine(FinishRoutine());
        }
    }

    IEnumerator FinishRoutine()
    {
        yield return new WaitForSeconds(0.4f);
        onComplete?.Invoke();
    }

    /// Ganti tampilan ke kompas sempurna (dipanggil flow saat flash putih).
    public void ShowPerfectCompass()
    {
        if (kompasGlitch != null) kompasGlitch.gameObject.SetActive(false);
        foreach (var t in tiles) t.gameObject.SetActive(false);
        if (kompasSempurna != null) kompasSempurna.gameObject.SetActive(true);
    }
}

