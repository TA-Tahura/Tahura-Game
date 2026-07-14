#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

/// Menyusun GameScene (Chapter 1) lengkap sesuai video referensi.
/// Jalankan: Tools > Beneath the Silence > Build Chapter 1 Scene
public static class Chapter1Builder
{
    const string Root = "Assets/Chapter1/";
    const string ScenePath = "Assets/Scenes/GameScene.unity";

    // Layout dunia (PPU 100, BG 3840x1080 -> 38.4 x 10.8 unit)
    const float GroundY = -3.1f;
    const float Bg2CenterX = 60f;

    [MenuItem("Tools/Beneath the Silence/Build Chapter 1 Scene")]
    public static void Build()
    {
        ConfigureImporters();

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // ===== CAMERA =====
        var camGO = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener), typeof(CameraFollow));
        camGO.tag = "MainCamera";
        var cam = camGO.GetComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5.4f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.07f, 0.09f, 0.1f);
        camGO.transform.position = new Vector3(-9.6f, 0f, -10f);
        var camFollow = camGO.GetComponent<CameraFollow>();
        camFollow.minX = -9.6f;
        camFollow.maxX = 9.6f;

        // ===== WORLD =====
        var world = new GameObject("World");

        CreateWorldSprite("BG1_PintuDepanTahura", Root + "Backgrounds/BG1_PintuDepanTahura.png",
            new Vector3(0, 0, 0), 0, world.transform);
        CreateWorldSprite("BG2_AfterPintuTahura", Root + "Backgrounds/BG2_AfterPintuTahura.png",
            new Vector3(Bg2CenterX, 0, 0), 0, world.transform);

        // --- Player ---
        var playerGO = new GameObject("Player", typeof(SpriteRenderer), typeof(PlayerController2D), typeof(AudioSource));
        playerGO.transform.SetParent(world.transform);
        playerGO.transform.position = new Vector3(-7f, GroundY, 0);
        var playerSR = playerGO.GetComponent<SpriteRenderer>();
        playerSR.sortingOrder = 10;

        var walkFrames = new System.Collections.Generic.List<Sprite>();
        for (int i = 1; i <= 38; i++)
        {
            var s = AssetDatabase.LoadAssetAtPath<Sprite>(Root + $"Sprites/AsepWalk/walk_{i:00}.png");
            if (s != null) walkFrames.Add(s);
        }
        var player = playerGO.GetComponent<PlayerController2D>();
        player.walkFrames = walkFrames.ToArray();
        player.idleFrame = walkFrames.Count > 0 ? walkFrames[0] : null;
        playerSR.sprite = player.idleFrame;
        player.minX = -18.5f;
        player.maxX = 18.5f;

        var footstep = playerGO.GetComponent<AudioSource>();
        footstep.clip = AssetDatabase.LoadAssetAtPath<AudioClip>(Root + "Audio/SFX_Jalan.mp3");
        footstep.loop = true;
        footstep.playOnAwake = false;
        footstep.volume = 0.55f;
        player.footstepSource = footstep;

        camFollow.target = playerGO.transform;

        // --- NPC & titik interaksi (posisi diukur dari video referensi) ---
        var kumpulSpot = CreateInteractPoint("KumpulSpot", new Vector3(-2.7f, GroundY, 0), world.transform, 2.6f, 1.7f);
        var dadang = CreateNpc("PakDadang", Root + "Sprites/NPC/PakDadang.png",
            new Vector3(3.8f, GroundY, 0), world.transform, 5.2f, 2.8f);
        var kompasSpot = CreateInteractPoint("KompasSpot", new Vector3(-13.3f, GroundY, 0), world.transform, 0.7f, 1.7f);
        var toiletSpot = CreateInteractPoint("ToiletSpot", new Vector3(-16.9f, GroundY, 0), world.transform, 2.2f, 3.2f);
        var rika = CreateNpc("Rika", Root + "Sprites/NPC/RikaWorld.png",
            new Vector3(0f, GroundY, 0), world.transform, 4.9f, 2.4f);

        // ===== AUDIO =====
        var bgmGO = new GameObject("BGM", typeof(AudioSource));
        var bgm = bgmGO.GetComponent<AudioSource>();
        bgm.clip = AssetDatabase.LoadAssetAtPath<AudioClip>(Root + "Audio/BGM_Chapter1.mp3");
        bgm.loop = true;
        bgm.playOnAwake = false;
        bgm.volume = 0.8f;

        // ===== EVENT SYSTEM =====
        var es = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

        // ===== UI: INTRO (bangun tidur) =====
        var introCanvas = CreateCanvas("IntroUI", 20);
        var introPanel = CreateUIImage("IntroPanel", introCanvas.transform,
            LoadSprite(Root + "Backgrounds/SceneBangunTidur.png"));
        StretchAll(introPanel.rectTransform);
        introPanel.preserveAspect = false;

        // ===== UI: TUTORIAL OVERLAY =====
        var gameplayCanvas = CreateCanvas("GameplayUI", 10);
        var tutorial = new GameObject("TutorialOverlay", typeof(RectTransform), typeof(CanvasGroup));
        tutorial.transform.SetParent(gameplayCanvas.transform, false);
        var tutRT = tutorial.GetComponent<RectTransform>();
        tutRT.anchorMin = tutRT.anchorMax = new Vector2(0.5f, 1f);
        tutRT.pivot = new Vector2(0.5f, 1f);
        tutRT.anchoredPosition = new Vector2(0, -50);
        tutRT.sizeDelta = new Vector2(860, 230);
        var tutBg = CreateUIImage("Bg", tutorial.transform, BuiltinSprite("UISprite.psd"));
        tutBg.type = Image.Type.Sliced;
        tutBg.color = new Color(1f, 1f, 1f, 0.88f);
        StretchAll(tutBg.rectTransform);
        var tutText = CreateTMP("Text", tutorial.transform, "← ↑ ↓ →  + Shift = Run\nEnter = Interact\nEsc = Pause",
            40, new Color(0.12f, 0.12f, 0.12f), TextAlignmentOptions.Center, FontStyles.Bold);
        StretchAll(tutText.rectTransform);

        // ===== UI: MINI GAME =====
        var miniCanvas = CreateCanvas("MiniGameUI", 15);
        var miniRoot = new GameObject("MiniGameRoot", typeof(RectTransform));
        miniRoot.transform.SetParent(miniCanvas.transform, false);
        StretchAll(miniRoot.GetComponent<RectTransform>());

        var miniBg = CreateUIImage("Bg", miniRoot.transform, LoadSprite(Root + "MiniGame1/BG_MiniGame1.png"));
        StretchAll(miniBg.rectTransform);
        miniBg.preserveAspect = false;

        // ArtRoot mengunci rasio 16:9 art minigame. Tanpa ini, gambar kompas ikut
        // melar mengikuti aspek Game view (kompas jadi terlalu lebar). Semua art
        // (kompas + tile) hidup di dalam ArtRoot dengan koordinat art 1920x1080.
        var artRoot = new GameObject("ArtRoot", typeof(RectTransform), typeof(AspectRatioFitter));
        artRoot.transform.SetParent(miniRoot.transform, false);
        var artFitter = artRoot.GetComponent<AspectRatioFitter>();
        artFitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        artFitter.aspectRatio = 1920f / 1080f;

        var kompasGlitch = CreateUIImage("KompasGlitch", artRoot.transform, LoadSprite(Root + "MiniGame1/KompasGlitch.png"));
        StretchAll(kompasGlitch.rectTransform);
        kompasGlitch.preserveAspect = false;
        kompasGlitch.raycastTarget = false;

        var kompasSempurna = CreateUIImage("KompasSempurna", artRoot.transform, LoadSprite(Root + "MiniGame1/KompasSempurna.png"));
        StretchAll(kompasSempurna.rectTransform);
        kompasSempurna.preserveAspect = false;
        kompasSempurna.raycastTarget = false;
        kompasSempurna.gameObject.SetActive(false);

        // Slot target dijahit ke gambar kompas dengan anchor proporsional.
        // Kompas pada art 1920x1080: tengah (483,528), slot U ~y330, slot S ~y718.
        var zoneU = CreateZone("ZoneU", kompasGlitch.transform, new Vector2(483f / 1920f, 1f - 330f / 1080f));
        var zoneS = CreateZone("ZoneS", kompasGlitch.transform, new Vector2(483f / 1920f, 1f - 718f / 1080f));

        // Posisi awal tile (koordinat art 1920x1080, origin kiri-bawah) sesuai video:
        // TG kiri-atas dekat kompas, S kanan-atas, U tengah-kanan.
        var tileU = CreateTile("TileU", artRoot.transform, Root + "MiniGame1/Tile_Utara.png", new Vector2(1207, 502));
        var tileS = CreateTile("TileS", artRoot.transform, Root + "MiniGame1/Tile_Selatan.png", new Vector2(1438, 857));
        var tileTG = CreateTile("TileTG", artRoot.transform, Root + "MiniGame1/Tile_Tenggara.png", new Vector2(917, 800));
        tileU.target = zoneU;
        tileS.target = zoneS;
        tileTG.target = null; // pengecoh

        var miniGame = miniRoot.AddComponent<CompassMiniGame>();
        miniGame.kompasGlitch = kompasGlitch;
        miniGame.kompasSempurna = kompasSempurna;
        miniGame.tiles = new[] { tileU, tileS, tileTG };

        // ===== UI: DIALOGUE =====
        var dialogueCanvas = CreateCanvas("DialogueUI", 30);
        var dlgRoot = new GameObject("DialoguePanel", typeof(RectTransform));
        dlgRoot.transform.SetParent(dialogueCanvas.transform, false);
        StretchAll(dlgRoot.GetComponent<RectTransform>());

        var portraitLeft = CreateUIImage("PortraitLeft", dlgRoot.transform, null);
        var plRT = portraitLeft.rectTransform;
        plRT.anchorMin = plRT.anchorMax = new Vector2(0f, 0f);
        plRT.pivot = new Vector2(0f, 0f);
        plRT.anchoredPosition = new Vector2(15, 0);
        plRT.sizeDelta = new Vector2(520, 900);
        portraitLeft.preserveAspect = true;
        portraitLeft.raycastTarget = false;

        var portraitRight = CreateUIImage("PortraitRight", dlgRoot.transform, null);
        var prRT = portraitRight.rectTransform;
        prRT.anchorMin = prRT.anchorMax = new Vector2(1f, 0f);
        prRT.pivot = new Vector2(1f, 0f);
        prRT.anchoredPosition = new Vector2(-15, 0);
        prRT.sizeDelta = new Vector2(520, 900);
        portraitRight.preserveAspect = true;
        portraitRight.raycastTarget = false;

        var boxSprite = LoadSprite(Root + "UI/DialogueBox.png");
        var box = CreateUIImage("Box", dlgRoot.transform, boxSprite);
        var boxRT = box.rectTransform;
        boxRT.anchorMin = boxRT.anchorMax = new Vector2(0.5f, 0f);
        boxRT.pivot = new Vector2(0.5f, 0f);
        boxRT.anchoredPosition = new Vector2(60, 15);
        boxRT.sizeDelta = new Vector2(1150, 530);
        box.preserveAspect = true;

        var nameText = CreateTMP("NameText", box.transform, "ASEP", 34,
            new Color(0.42f, 0.12f, 0.12f), TextAlignmentOptions.Center, FontStyles.Bold);
        var nameRT = nameText.rectTransform;
        nameRT.anchorMin = nameRT.anchorMax = new Vector2(0.37f, 0.81f);
        nameRT.sizeDelta = new Vector2(420, 60);

        var bodyText = CreateTMP("BodyText", box.transform, "", 30,
            new Color(0.38f, 0.12f, 0.06f), TextAlignmentOptions.Center, FontStyles.Normal);
        var bodyRT = bodyText.rectTransform;
        bodyRT.anchorMin = new Vector2(0.12f, 0.12f);
        bodyRT.anchorMax = new Vector2(0.88f, 0.58f);
        bodyRT.offsetMin = Vector2.zero;
        bodyRT.offsetMax = Vector2.zero;

        var nextHint = new GameObject("NextHint", typeof(RectTransform));
        nextHint.transform.SetParent(box.transform, false);
        var nhRT = nextHint.GetComponent<RectTransform>();
        nhRT.anchorMin = nhRT.anchorMax = new Vector2(0.88f, 0.14f);
        nhRT.sizeDelta = new Vector2(160, 44);
        var hintArrow = CreateUIImage("Arrow", nextHint.transform, LoadSprite(Root + "UI/NextDialog.png"));
        var haRT = hintArrow.rectTransform;
        haRT.anchorMin = haRT.anchorMax = new Vector2(0.5f, 0.5f);
        haRT.sizeDelta = new Vector2(140, 42);
        hintArrow.preserveAspect = true;
        hintArrow.raycastTarget = false;

        var dlgManager = dialogueCanvas.gameObject.AddComponent<DialogueManager>();
        dlgManager.panelRoot = dlgRoot;
        dlgManager.nameText = nameText;
        dlgManager.bodyText = bodyText;
        dlgManager.portraitLeft = portraitLeft;
        dlgManager.portraitRight = portraitRight;
        dlgManager.nextHint = nextHint;

        // ===== UI: PAUSE =====
        var pauseCanvas = CreateCanvas("PauseUI", 40);
        var pausePanel = new GameObject("PausePanel", typeof(RectTransform));
        pausePanel.transform.SetParent(pauseCanvas.transform, false);
        StretchAll(pausePanel.GetComponent<RectTransform>());

        var dim = CreateUIImage("Dim", pausePanel.transform, null);
        dim.color = new Color(0, 0, 0, 0.45f);
        StretchAll(dim.rectTransform);

        var pBox = CreateUIImage("Box", pausePanel.transform, BuiltinSprite("UISprite.psd"));
        pBox.type = Image.Type.Sliced;
        pBox.color = new Color(0.30f, 0.33f, 0.22f, 0.97f);
        var pBoxRT = pBox.rectTransform;
        pBoxRT.anchorMin = pBoxRT.anchorMax = new Vector2(0.5f, 0.5f);
        pBoxRT.sizeDelta = new Vector2(1000, 640);

        var pauseTitle = CreateTMP("Title", pBox.transform, "Pause", 52, Color.white, TextAlignmentOptions.Center, FontStyles.Bold);
        var ptRT = pauseTitle.rectTransform;
        ptRT.anchorMin = ptRT.anchorMax = new Vector2(0.5f, 1f);
        ptRT.anchoredPosition = new Vector2(0, -70);
        ptRT.sizeDelta = new Vector2(400, 70);

        var btnSave = CreatePauseButton(pBox.transform, "Save", new Vector2(90, -160));
        var btnSaveMenu = CreatePauseButton(pBox.transform, "Save and back to menu", new Vector2(90, -230));
        var btnOptions = CreatePauseButton(pBox.transform, "Options", new Vector2(90, -300));
        var btnBackMenu = CreatePauseButton(pBox.transform, "Back to menu", new Vector2(90, -370));

        var btnBack = CreateTMP("BtnBack", pBox.transform, "Back", 36, Color.white, TextAlignmentOptions.Right, FontStyles.Bold);
        var bbRT = btnBack.rectTransform;
        bbRT.anchorMin = bbRT.anchorMax = new Vector2(1f, 0f);
        bbRT.pivot = new Vector2(1f, 0f);
        bbRT.anchoredPosition = new Vector2(-60, 40);
        bbRT.sizeDelta = new Vector2(200, 50);
        btnBack.raycastTarget = true;
        var backButton = btnBack.gameObject.AddComponent<Button>();
        backButton.targetGraphic = btnBack;

        // Options sub-panel (slider BGM & SFX)
        var optPanel = new GameObject("OptionsSubPanel", typeof(RectTransform));
        optPanel.transform.SetParent(pBox.transform, false);
        var opRT = optPanel.GetComponent<RectTransform>();
        opRT.anchorMin = opRT.anchorMax = new Vector2(1f, 0.5f);
        opRT.pivot = new Vector2(1f, 0.5f);
        opRT.anchoredPosition = new Vector2(-50, 20);
        opRT.sizeDelta = new Vector2(420, 260);

        var bgmLabel = CreateTMP("BGMLabel", optPanel.transform, "Background music", 26, Color.white, TextAlignmentOptions.Left, FontStyles.Normal);
        var blRT = bgmLabel.rectTransform;
        blRT.anchorMin = blRT.anchorMax = new Vector2(0f, 1f);
        blRT.pivot = new Vector2(0f, 1f);
        blRT.anchoredPosition = new Vector2(0, 0);
        blRT.sizeDelta = new Vector2(300, 36);
        var bgmSlider = CreateSlider("BGMSlider", optPanel.transform, new Vector2(0, -46), new Vector2(400, 28));

        var sfxLabel = CreateTMP("SFXLabel", optPanel.transform, "Sound effect", 26, Color.white, TextAlignmentOptions.Left, FontStyles.Normal);
        var slRT = sfxLabel.rectTransform;
        slRT.anchorMin = slRT.anchorMax = new Vector2(0f, 1f);
        slRT.pivot = new Vector2(0f, 1f);
        slRT.anchoredPosition = new Vector2(0, -110);
        slRT.sizeDelta = new Vector2(300, 36);
        var sfxSlider = CreateSlider("SFXSlider", optPanel.transform, new Vector2(0, -156), new Vector2(400, 28));

        var feedback = CreateTMP("Feedback", pBox.transform, "", 26, new Color(1f, 0.95f, 0.6f), TextAlignmentOptions.Left, FontStyles.Italic);
        var fbRT = feedback.rectTransform;
        fbRT.anchorMin = fbRT.anchorMax = new Vector2(0f, 0f);
        fbRT.pivot = new Vector2(0f, 0f);
        fbRT.anchoredPosition = new Vector2(60, 40);
        fbRT.sizeDelta = new Vector2(500, 40);

        var pauseMenu = pauseCanvas.gameObject.AddComponent<PauseMenu>();
        pauseMenu.panel = pausePanel;
        pauseMenu.optionsSubPanel = optPanel;
        pauseMenu.bgmSlider = bgmSlider;
        pauseMenu.sfxSlider = sfxSlider;
        pauseMenu.bgmSource = bgm;
        pauseMenu.sfxSource = footstep;
        pauseMenu.feedbackText = feedback;
        pauseMenu.menuSceneName = "MainMenu";

        UnityEventTools.AddPersistentListener(btnSave.onClick, pauseMenu.OnSave);
        UnityEventTools.AddPersistentListener(btnSaveMenu.onClick, pauseMenu.OnSaveAndMenu);
        UnityEventTools.AddPersistentListener(btnOptions.onClick, pauseMenu.OnOptions);
        UnityEventTools.AddPersistentListener(btnBackMenu.onClick, pauseMenu.OnBackToMenu);
        UnityEventTools.AddPersistentListener(backButton.onClick, pauseMenu.Resume);

        // ===== UI: FADER =====
        var faderCanvas = CreateCanvas("FaderUI", 100);
        var faderImg = CreateUIImage("Overlay", faderCanvas.transform, null);
        faderImg.color = Color.black;
        StretchAll(faderImg.rectTransform);
        var fader = faderCanvas.gameObject.AddComponent<ScreenFader>();
        fader.overlay = faderImg;

        // ===== FLOW =====
        var flowGO = new GameObject("GameFlow", typeof(Chapter1Flow));
        var flow = flowGO.GetComponent<Chapter1Flow>();
        flow.player = player;
        flow.cameraFollow = camFollow;
        flow.fader = fader;
        flow.dialogue = dlgManager;
        flow.introPanel = introPanel.gameObject;
        flow.tutorialOverlay = tutorial.GetComponent<CanvasGroup>();
        flow.miniGameRoot = miniRoot;
        flow.miniGame = miniGame;
        flow.kumpulSpot = kumpulSpot;
        flow.dadangNPC = dadang;
        flow.kompasSpot = kompasSpot;
        flow.toiletSpot = toiletSpot;
        flow.rikaNPC = rika;
        flow.bgm = bgm;

        flow.asepHappy = LoadSprite(Root + "Portraits/Asep_Happy.png");
        flow.asepAwkward = LoadSprite(Root + "Portraits/Asep_Awkward.png");
        flow.asepKaget = LoadSprite(Root + "Portraits/Asep_Kaget.png");
        flow.asepPenasaran1 = LoadSprite(Root + "Portraits/Asep_Penasaran1.png");
        flow.asepPenasaran2 = LoadSprite(Root + "Portraits/Asep_Penasaran2.png");
        flow.guruKelas = LoadSprite(Root + "Portraits/GuruKelas.png");
        flow.dadangPortrait = LoadSprite(Root + "Portraits/PakDadang.png");
        flow.rikaBingung = LoadSprite(Root + "Portraits/Rika_Bingung.png");
        flow.rikaBukaMulut = LoadSprite(Root + "Portraits/Rika_BukaMulut.png");

        pauseMenu.flow = flow;

        // ===== SAVE =====
        EditorSceneManager.SaveScene(scene, ScenePath);
        AddScenesToBuild();

        Debug.Log("[Chapter1Builder] GameScene berhasil dibangun & disimpan di " + ScenePath);
    }

    [MenuItem("Tools/Beneath the Silence/Add Main Menu BGM")]
    public static void AddMenuBgm()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity", OpenSceneMode.Single);
        var existing = GameObject.Find("MenuBGM");
        if (existing == null)
        {
            var go = new GameObject("MenuBGM", typeof(AudioSource));
            var src = go.GetComponent<AudioSource>();
            src.clip = AssetDatabase.LoadAssetAtPath<AudioClip>(Root + "Audio/BGM_MainMenu.mp3");
            src.loop = true;
            src.playOnAwake = true;
            src.volume = 0.8f;
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[Chapter1Builder] MenuBGM ditambahkan ke MainMenu.");
        }
        else Debug.Log("[Chapter1Builder] MenuBGM sudah ada.");
    }

    // ================= HELPERS =================

    static void ConfigureImporters()
    {
        // Background & art besar
        EnsureSprite(Root + "Backgrounds/BG1_PintuDepanTahura.png", 100, null, 4096);
        EnsureSprite(Root + "Backgrounds/BG2_AfterPintuTahura.png", 100, null, 4096);
        EnsureSprite(Root + "Backgrounds/SceneBangunTidur.png", 100, null, 2048);
        EnsureSprite(Root + "MiniGame1/BG_MiniGame1.png", 100, null, 2048);
        EnsureSprite(Root + "MiniGame1/KompasGlitch.png", 100, null, 2048);
        EnsureSprite(Root + "MiniGame1/KompasSempurna.png", 100, null, 2048);
        EnsureSprite(Root + "MiniGame1/Tile_Utara.png", 100, null, 1024);
        EnsureSprite(Root + "MiniGame1/Tile_Selatan.png", 100, null, 1024);
        EnsureSprite(Root + "MiniGame1/Tile_Tenggara.png", 100, null, 1024);
        EnsureSprite(Root + "UI/DialogueBox.png", 100, null, 2048);
        EnsureSprite(Root + "UI/NextDialog.png", 100, null, 1024);

        // Portraits
        foreach (var n in new[] { "Asep_Happy", "Asep_Awkward", "Asep_Kaget", "Asep_Penasaran1",
                                  "Asep_Penasaran2", "GuruKelas", "PakDadang", "Rika_Bingung", "Rika_BukaMulut", "Rika_TutupMulut" })
            EnsureSprite(Root + "Portraits/" + n + ".png", 100, null, 2048);

        // Player: pivot di kaki
        for (int i = 1; i <= 38; i++)
            EnsureSprite(Root + $"Sprites/AsepWalk/walk_{i:00}.png", 150, new Vector2(0.5f, 0.10f), 1024);

        // NPC (art "Stand char" kanvas penuh 1080x1920): pivot tepat di kaki,
        // PPU dihitung dari tinggi konten agar proporsional dengan player
        // (Asep 711px @150 = 4.7u; Dadang 1144px @220 = 5.2u; Rika 1403px @310 = 4.5u).
        EnsureSprite(Root + "Sprites/NPC/PakDadang.png", 220, new Vector2(0.5f, 0.148f), 1024);
        EnsureSprite(Root + "Sprites/NPC/RikaWorld.png", 310, new Vector2(0.5f, 0.154f), 1024);

        AssetDatabase.Refresh();
    }

    static void EnsureSprite(string path, float ppu, Vector2? pivot, int maxSize)
    {
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
        {
            Debug.LogWarning("[Chapter1Builder] Asset tidak ditemukan: " + path);
            return;
        }
        bool dirty = false;
        if (importer.textureType != TextureImporterType.Sprite) { importer.textureType = TextureImporterType.Sprite; dirty = true; }
        if (Mathf.Abs(importer.spritePixelsPerUnit - ppu) > 0.01f) { importer.spritePixelsPerUnit = ppu; dirty = true; }
        if (importer.maxTextureSize != maxSize) { importer.maxTextureSize = maxSize; dirty = true; }
        if (importer.mipmapEnabled) { importer.mipmapEnabled = false; dirty = true; }

        // Single wajib: meta warisan bermode Multiple menyimpan slice untuk
        // gambar lama sehingga sprite baru bisa ter-render dari area transparan.
        if (importer.spriteImportMode != SpriteImportMode.Single)
        {
            importer.spriteImportMode = SpriteImportMode.Single;
            dirty = true;
        }

        var settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        if (settings.spriteMeshType != SpriteMeshType.FullRect)
        {
            settings.spriteMeshType = SpriteMeshType.FullRect;
            dirty = true;
        }
        if (pivot.HasValue)
        {
            if (settings.spriteAlignment != (int)SpriteAlignment.Custom ||
                Vector2.Distance(settings.spritePivot, pivot.Value) > 0.001f)
            {
                settings.spriteAlignment = (int)SpriteAlignment.Custom;
                settings.spritePivot = pivot.Value;
                dirty = true;
            }
        }
        if (dirty)
        {
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }
    }

    static Sprite LoadSprite(string path)
    {
        var s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (s == null) Debug.LogWarning("[Chapter1Builder] Sprite tidak ketemu: " + path);
        return s;
    }

    static Sprite BuiltinSprite(string name)
    {
        return AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/" + name);
    }

    static SpriteRenderer CreateWorldSprite(string name, string path, Vector3 pos, int order, Transform parent)
    {
        var go = new GameObject(name, typeof(SpriteRenderer));
        go.transform.SetParent(parent);
        go.transform.position = pos;
        var sr = go.GetComponent<SpriteRenderer>();
        sr.sprite = LoadSprite(path);
        sr.sortingOrder = order;
        return sr;
    }

    static Interactable CreateNpc(string name, string spritePath, Vector3 pos, Transform parent,
        float exclamationY, float labelY)
    {
        var sr = CreateWorldSprite(name, spritePath, pos, 5, parent);
        return AddInteraction(sr.gameObject, exclamationY, labelY);
    }

    static Interactable CreateInteractPoint(string name, Vector3 pos, Transform parent,
        float exclamationY, float labelY)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.position = pos;
        return AddInteraction(go, exclamationY, labelY);
    }

    static Interactable AddInteraction(GameObject go, float exclamationY, float labelY)
    {
        var it = go.AddComponent<Interactable>();

        // Tanda seru merah (TextMeshPro world-space)
        var exGO = new GameObject("Exclamation", typeof(TextMeshPro));
        exGO.transform.SetParent(go.transform, false);
        exGO.transform.localPosition = new Vector3(0, exclamationY, 0);
        var ex = exGO.GetComponent<TextMeshPro>();
        ex.text = "!";
        ex.fontSize = 13;
        ex.fontStyle = FontStyles.Bold;
        ex.color = new Color(0.85f, 0.07f, 0.07f);
        ex.alignment = TextAlignmentOptions.Center;
        ex.rectTransform.sizeDelta = new Vector2(2, 2);
        var exRenderer = exGO.GetComponent<MeshRenderer>();
        exRenderer.sortingOrder = 20;

        // Label "Interact" (world-space canvas)
        var lblCanvasGO = new GameObject("InteractLabel", typeof(Canvas));
        lblCanvasGO.transform.SetParent(go.transform, false);
        var lblCanvas = lblCanvasGO.GetComponent<Canvas>();
        lblCanvas.renderMode = RenderMode.WorldSpace;
        lblCanvas.sortingOrder = 25;
        var lblRT = lblCanvasGO.GetComponent<RectTransform>();
        lblRT.sizeDelta = new Vector2(240, 66);
        lblRT.localScale = Vector3.one * 0.011f;
        lblRT.localPosition = new Vector3(0, labelY, 0);

        var lblBg = CreateUIImage("Bg", lblCanvasGO.transform, null);
        lblBg.color = new Color(1f, 1f, 1f, 0.95f);
        StretchAll(lblBg.rectTransform);
        var lblText = CreateTMP("Text", lblCanvasGO.transform, "Interact", 34,
            new Color(0.1f, 0.1f, 0.1f), TextAlignmentOptions.Center, FontStyles.Bold);
        StretchAll(lblText.rectTransform);

        it.exclamation = exGO;
        it.interactLabel = lblCanvasGO;
        return it;
    }

    static Canvas CreateCanvas(string name, int sortOrder)
    {
        var go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortOrder;
        var scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        return canvas;
    }

    static Image CreateUIImage(string name, Transform parent, Sprite sprite)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.sprite = sprite;
        return img;
    }

    static TextMeshProUGUI CreateTMP(string name, Transform parent, string text, float size,
        Color color, TextAlignmentOptions align, FontStyles style)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.alignment = align;
        tmp.fontStyle = style;
        tmp.raycastTarget = false;
        return tmp;
    }

    static Button CreatePauseButton(Transform parent, string label, Vector2 pos)
    {
        var tmp = CreateTMP("Btn_" + label.Replace(" ", ""), parent, label, 34, Color.white,
            TextAlignmentOptions.Left, FontStyles.Normal);
        var rt = tmp.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(520, 52);
        tmp.raycastTarget = true;
        var btn = tmp.gameObject.AddComponent<Button>();
        btn.targetGraphic = tmp;
        var colors = btn.colors;
        colors.highlightedColor = new Color(1f, 0.95f, 0.6f);
        colors.pressedColor = new Color(0.9f, 0.8f, 0.4f);
        btn.colors = colors;
        return btn;
    }

    static Slider CreateSlider(string name, Transform parent, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Slider));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        var bg = CreateUIImage("Background", go.transform, BuiltinSprite("UISprite.psd"));
        bg.type = Image.Type.Sliced;
        bg.color = new Color(1f, 1f, 1f, 0.35f);
        StretchAll(bg.rectTransform);

        var fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(go.transform, false);
        var faRT = fillArea.GetComponent<RectTransform>();
        faRT.anchorMin = Vector2.zero;
        faRT.anchorMax = Vector2.one;
        faRT.offsetMin = new Vector2(6, 4);
        faRT.offsetMax = new Vector2(-6, -4);

        var fill = CreateUIImage("Fill", fillArea.transform, BuiltinSprite("UISprite.psd"));
        fill.type = Image.Type.Sliced;
        fill.color = new Color(0.95f, 0.9f, 0.65f);
        var fillRT = fill.rectTransform;
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = Vector2.one;
        fillRT.offsetMin = Vector2.zero;
        fillRT.offsetMax = Vector2.zero;

        var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
        handleArea.transform.SetParent(go.transform, false);
        var haRT = handleArea.GetComponent<RectTransform>();
        haRT.anchorMin = Vector2.zero;
        haRT.anchorMax = Vector2.one;
        haRT.offsetMin = new Vector2(10, 0);
        haRT.offsetMax = new Vector2(-10, 0);

        var handle = CreateUIImage("Handle", handleArea.transform, BuiltinSprite("Knob.psd"));
        handle.color = Color.white;
        var hRT = handle.rectTransform;
        hRT.sizeDelta = new Vector2(30, 30);

        var slider = go.GetComponent<Slider>();
        slider.fillRect = fillRT;
        slider.handleRect = hRT;
        slider.targetGraphic = handle;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;
        return slider;
    }

    static RectTransform CreateZone(string name, Transform parent, Vector2 normalizedAnchor)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = normalizedAnchor;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(100, 100);
        return rt;
    }

    /// Tile dianchor proporsional pada ArtRoot (koordinat art 1920x1080, origin
    /// kiri-bawah) sehingga posisi & ukurannya selalu ikut skala art kompas.
    static CompassTile CreateTile(string name, Transform parent, string spritePath, Vector2 artPos)
    {
        const float size = 125f;
        var img = CreateUIImage(name, parent, LoadSprite(spritePath));
        var rt = img.rectTransform;
        rt.anchorMin = new Vector2((artPos.x - size / 2f) / 1920f, (artPos.y - size / 2f) / 1080f);
        rt.anchorMax = new Vector2((artPos.x + size / 2f) / 1920f, (artPos.y + size / 2f) / 1080f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        img.preserveAspect = true;
        img.raycastTarget = true;
        return img.gameObject.AddComponent<CompassTile>();
    }

    static void StretchAll(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static void AddScenesToBuild()
    {
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>
        {
            new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true),
            new EditorBuildSettingsScene(ScenePath, true)
        };
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
#endif
