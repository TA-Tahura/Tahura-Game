#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// Menjalankan builder satu kali secara otomatis setelah kompilasi selesai,
/// dipicu oleh file penanda "claude-pending-rebuild.marker" di root project.
/// Dipakai untuk menerapkan perbaikan layout Mini Game 1 dan panel How to Play
/// tanpa harus mengklik menu Tools secara manual.
[InitializeOnLoad]
public static class PendingBuildRunner
{
    const string Marker = "claude-pending-rebuild.marker";

    static double nextCheck;

    static PendingBuildRunner()
    {
        // Cek berkala (bukan sekali saja): kalau marker dibuat/tertunda saat
        // Play mode, runner tetap menjalankannya begitu keadaan aman.
        EditorApplication.update += Tick;
    }

    static void Tick()
    {
        if (EditorApplication.timeSinceStartup < nextCheck) return;
        nextCheck = EditorApplication.timeSinceStartup + 2.0;
        Run();
    }

    static void Run()
    {
        if (!File.Exists(Marker)) return;
        // Belum aman dieksekusi — biarkan marker, coba lagi nanti.
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;

        File.Delete(Marker);

        // Simpan scene yang sedang terbuka supaya tidak ada pekerjaan yang hilang.
        EditorSceneManager.SaveOpenScenes();

        Chapter1Builder.Build();          // rebuild GameScene (fix kompas mini game 1)
        MainMenuBuilder.BuildHowToPlay(); // tambah panel How to Play ke MainMenu

        Debug.Log("[PendingBuildRunner] Rebuild otomatis selesai: GameScene + panel How To Play.");
    }
}
#endif
