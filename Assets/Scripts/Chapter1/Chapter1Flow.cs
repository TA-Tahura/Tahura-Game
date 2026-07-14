using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Alur cerita Chapter 1 "Beneath the Silence" sesuai video referensi:
/// bangun tidur -> gerbang Tahura -> arahan guru & Pak Dadang -> izin ke toilet ->
/// menemukan kompas glitch -> mini game kompas -> boker -> ketemu Rika -> masuk Tahura (BG2).
public class Chapter1Flow : MonoBehaviour
{
    [Header("Scene refs")]
    public PlayerController2D player;
    public CameraFollow cameraFollow;
    public ScreenFader fader;
    public DialogueManager dialogue;
    public GameObject introPanel;
    public CanvasGroup tutorialOverlay;
    public GameObject miniGameRoot;
    public CompassMiniGame miniGame;
    public Interactable kumpulSpot;   // titik "!" di pagar kiri gerbang (arahan guru, stage 1)
    public Interactable dadangNPC;
    public Interactable kompasSpot;
    public Interactable toiletSpot;
    public Interactable rikaNPC;
    public AudioSource bgm;

    [Header("Portraits")]
    public Sprite asepHappy;
    public Sprite asepAwkward;
    public Sprite asepKaget;
    public Sprite asepPenasaran1;
    public Sprite asepPenasaran2;
    public Sprite guruKelas;
    public Sprite dadangPortrait;
    public Sprite rikaBingung;
    public Sprite rikaBukaMulut;

    [Header("Layout")]
    public float bg2PlayerSpawnX = 42f;
    public float bg2PlayerMinX = 41.5f;
    public float bg2PlayerMaxX = 78.5f;
    public float bg2CamMinX = 50.4f;
    public float bg2CamMaxX = 69.6f;
    public float afterToiletPlayerX = -16.9f;

    int stage;
    bool busy;

    const string StageKey = "Chapter1_Stage";
    const string PlayerXKey = "Chapter1_PlayerX";

    void Start()
    {
        Time.timeScale = 1f;

        kumpulSpot.player = player.transform;
        dadangNPC.player = player.transform;
        kompasSpot.player = player.transform;
        toiletSpot.player = player.transform;
        rikaNPC.player = player.transform;

        kumpulSpot.onInteract += OnKumpulInteract;
        dadangNPC.onInteract += OnDadangInteract;
        kompasSpot.onInteract += OnKompasInteract;
        toiletSpot.onInteract += OnToiletInteract;
        rikaNPC.onInteract += OnRikaInteract;

        miniGame.onWrongPlacement += OnMiniGameWrong;
        miniGame.onComplete += OnMiniGameComplete;

        kumpulSpot.Available = false;
        dadangNPC.Available = false;
        kompasSpot.Available = false;
        toiletSpot.Available = false;
        rikaNPC.Available = false;
        rikaNPC.gameObject.SetActive(false);
        miniGameRoot.SetActive(false);
        tutorialOverlay.alpha = 0f;
        tutorialOverlay.gameObject.SetActive(false);

        if (bgm != null && !bgm.isPlaying) bgm.Play();

        bool continueGame = PlayerPrefs.GetInt("LoadSave", 0) == 1 && PlayerPrefs.GetInt(StageKey, 0) > 0;
        if (continueGame)
        {
            introPanel.SetActive(false);
            LoadStage(PlayerPrefs.GetInt(StageKey, 1), PlayerPrefs.GetFloat(PlayerXKey, player.transform.position.x));
            fader.FadeTo(Color.black, 0f, 0.8f);
        }
        else
        {
            StartCoroutine(IntroSequence());
        }
    }

    // ===== SAVE / LOAD =====

    public void SaveProgress()
    {
        PlayerPrefs.SetInt(StageKey, Mathf.Max(1, stage));
        PlayerPrefs.SetFloat(PlayerXKey, player.transform.position.x);
        PlayerPrefs.SetInt("LoadSave", 1);
    }

    void LoadStage(int s, float playerX)
    {
        stage = Mathf.Clamp(s, 1, 6);
        var p = player.transform.position;
        p.x = playerX;
        player.transform.position = p;

        kumpulSpot.Available = stage == 1;
        dadangNPC.Available = stage == 2;
        dadangNPC.gameObject.SetActive(stage < 5); // hilang setelah rombongan masuk
        kompasSpot.Available = stage == 3;
        toiletSpot.Available = stage == 4;
        rikaNPC.gameObject.SetActive(stage >= 5);
        rikaNPC.Available = stage == 5;

        if (stage >= 6) EnterBg2Bounds(playerX < bg2PlayerMinX ? bg2PlayerSpawnX : playerX);
        player.canMove = true;
        cameraFollow.SnapTo(player.transform.position.x);
    }

    // ===== INTRO =====

    IEnumerator IntroSequence()
    {
        player.canMove = false;
        introPanel.SetActive(true);
        yield return fader.FadeTo(Color.black, 0f, 1.2f);
        yield return new WaitForSeconds(0.6f);

        yield return Say(
            A("Oh! Sudah jam segini!", asepKaget),
            A("Otw TahuraAAAA", asepHappy));

        yield return fader.FadeTo(Color.white, 1f, 0.8f);
        introPanel.SetActive(false);
        yield return fader.FadeTo(Color.white, 0f, 0.8f);

        stage = 1;
        kumpulSpot.Available = true;
        player.canMove = true;
        StartCoroutine(TutorialRoutine());
    }

    IEnumerator TutorialRoutine()
    {
        tutorialOverlay.gameObject.SetActive(true);
        for (float t = 0; t < 0.5f; t += Time.deltaTime) { tutorialOverlay.alpha = t / 0.5f; yield return null; }
        tutorialOverlay.alpha = 1f;
        yield return new WaitForSeconds(9f);
        for (float t = 0; t < 1f; t += Time.deltaTime) { tutorialOverlay.alpha = 1f - t; yield return null; }
        tutorialOverlay.gameObject.SetActive(false);
    }

    // ===== INTERAKSI =====

    void OnKumpulInteract()
    {
        if (busy || stage != 1) return;
        StartCoroutine(DadangStage1());
    }

    void OnDadangInteract()
    {
        if (busy || stage != 2) return;
        StartCoroutine(DadangStage2());
    }

    IEnumerator DadangStage1()
    {
        busy = true;
        yield return Say(
            A("PAAGIIII~ Yang cerah!!!", asepHappy),
            A("Pagi yang bikin moodku bagus!", asepHappy),
            A("Lama ga berkunjung ke Tahura Ir.H.Djuanda", asepPenasaran1),
            L("GURU KELAS", "Anak-anak, kumpul sebentar!", guruKelas, true),
            L("GURU KELAS", "Sebentar lagi akan ada arahan terkait kegiatan kita hari ini", guruKelas, true),
            A("Oh? Okey", asepAwkward));
        stage = 2;
        kumpulSpot.Available = false;
        dadangNPC.Available = true;   // "!" pindah ke Pak Dadang (sesuai video)
        busy = false;
    }

    IEnumerator DadangStage2()
    {
        busy = true;
        yield return Say(
            L("PAK DADANG", "Halo semuanya, saya Dadang selaku pemandu wisata tahura", dadangPortrait, true),
            L("PAK DADANG", "Sebentar lagi kegiatan kunjungan kita akan dimulai", dadangPortrait, true),
            L("PAK DADANG", "Persiapkan barang-barangnya dan jangan sampai ada yang tertinggal", dadangPortrait, true),
            A("(Aduh, kebelet boker, aku izin dulu ke guru kelas deh)", asepAwkward),
            A("Pak, saya izin ke toileeeetttt!!!", asepKaget),
            L("GURU KELAS", "Baik, silakan, tapi segera menyusul yah", guruKelas, true));
        stage = 3;
        dadangNPC.Available = false;
        kompasSpot.Available = true;
        busy = false;
    }

    void OnKompasInteract()
    {
        if (busy || stage != 3) return;
        StartCoroutine(KompasSequence());
    }

    IEnumerator KompasSequence()
    {
        busy = true;
        kompasSpot.Available = false;
        yield return Say(
            A("Huh, bentar... Benda apa itu yang ada di tanah?", asepPenasaran1),
            A("Aku ambil aja deh, sepertinya menarik", asepPenasaran2));

        player.canMove = false;
        yield return fader.FadeTo(Color.black, 1f, 0.7f);
        miniGameRoot.SetActive(true);
        yield return fader.FadeTo(Color.black, 0f, 0.7f);

        yield return Say(
            A("Ini... Euh... Kompas???", asepKaget),
            A("Kompas apa ini? Jujur janggal", asepPenasaran1),
            A("Kenapa beda dari kompas yang lain? Duh, rusak kek gini.....", asepPenasaran2),
            A("Aku benerin deh kalau gitu", asepHappy));

        miniGame.InputEnabled = true;
        busy = false;
    }

    void OnMiniGameWrong()
    {
        if (dialogue.IsActive) return;
        dialogue.Show(new List<DialogueLine> {
            A("Lah kok posisinya balik lagi? Euh sepertinya salah...", asepAwkward)
        }, null);
    }

    void OnMiniGameComplete()
    {
        StartCoroutine(MiniGameCompleteSequence());
    }

    IEnumerator MiniGameCompleteSequence()
    {
        busy = true;
        yield return Say(A("Wah akhirnya......", asepHappy));

        // Flash putih: kompas glitch berubah jadi kompas sempurna.
        yield return fader.FadeTo(Color.white, 1f, 0.35f);
        miniGame.ShowPerfectCompass();
        yield return fader.FadeTo(Color.white, 0f, 0.35f);
        yield return new WaitForSeconds(1.1f);

        yield return Say(A("Eh...??? Anjay...", asepKaget));

        yield return fader.FadeTo(Color.black, 1f, 0.7f);
        miniGameRoot.SetActive(false);
        yield return fader.FadeTo(Color.black, 0f, 0.7f);

        yield return Say(
            A("Eh...??? Anjay...", asepKaget),
            A("Huh? Wait... Sekarang udah normal?", asepPenasaran1),
            A("....., Ah mungkin karena aku mau boker...", asepAwkward),
            A("Oiya, MAU BOKER!!!", asepKaget));

        stage = 4;
        toiletSpot.Available = true;
        player.canMove = true;
        busy = false;
    }

    void OnToiletInteract()
    {
        if (busy || stage != 4) return;
        StartCoroutine(ToiletSequence());
    }

    IEnumerator ToiletSequence()
    {
        busy = true;
        toiletSpot.Available = false;
        player.canMove = false;
        yield return fader.FadeTo(Color.black, 1f, 0.8f);
        yield return new WaitForSeconds(1.2f);

        var p = player.transform.position;
        p.x = afterToiletPlayerX;
        player.transform.position = p;
        dadangNPC.gameObject.SetActive(false); // rombongan sudah masuk Tahura
        rikaNPC.gameObject.SetActive(true);
        rikaNPC.Available = true;
        stage = 5;

        yield return fader.FadeTo(Color.black, 0f, 0.8f);
        player.canMove = true;
        busy = false;
    }

    void OnRikaInteract()
    {
        if (busy || stage != 5) return;
        StartCoroutine(RikaSequence());
    }

    IEnumerator RikaSequence()
    {
        busy = true;
        rikaNPC.Available = false;
        yield return Say(
            A("Rika? Ngapain kamu di sini?", asepPenasaran1),
            L("RIKA", "Huft... Nungguin kamu lah...", rikaBingung, true),
            A("Aaa... Thank you Rika", asepHappy),
            L("RIKA", "Dah, ayok jalan. Dah ketinggalan weh", rikaBukaMulut, true));

        // Masuk ke area Tahura (background 2).
        player.canMove = false;
        yield return fader.FadeTo(Color.black, 1f, 1f);
        rikaNPC.gameObject.SetActive(false);
        EnterBg2Bounds(bg2PlayerSpawnX);
        stage = 6;
        yield return fader.FadeTo(Color.black, 0f, 1f);
        player.canMove = true;
        busy = false;
    }

    void EnterBg2Bounds(float playerX)
    {
        var p = player.transform.position;
        p.x = playerX;
        player.transform.position = p;
        player.minX = bg2PlayerMinX;
        player.maxX = bg2PlayerMaxX;
        cameraFollow.minX = bg2CamMinX;
        cameraFollow.maxX = bg2CamMaxX;
        cameraFollow.SnapTo(playerX);
    }

    // ===== HELPERS =====

    DialogueLine A(string text, Sprite portrait) => new DialogueLine("ASEP", text, portrait, false);
    DialogueLine L(string speaker, string text, Sprite portrait, bool right) => new DialogueLine(speaker, text, portrait, right);

    IEnumerator Say(params DialogueLine[] lines)
    {
        bool done = false;
        dialogue.Show(lines, () => done = true);
        while (!done) yield return null;
    }
}
