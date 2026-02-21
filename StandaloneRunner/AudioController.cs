using System;
using System.IO;
using System.Linq;

namespace StandaloneRunner;

internal sealed class AudioController : IDisposable
{
    private readonly string bgmFolder;
    private readonly string sfxFolder;

    private readonly dynamic? bgmPlayer;
    private readonly dynamic? sfxPlayer;

    private string? bgmTrackPath;
    private string? jetpackTrackPath;

    public string BgmFolder => bgmFolder;
    public string SfxFolder => sfxFolder;

    public AudioController(string baseDirectory)
    {
        bgmFolder = Path.Combine(baseDirectory, "Audio", "BGM");
        sfxFolder = Path.Combine(baseDirectory, "Audio", "SFX");

        Directory.CreateDirectory(bgmFolder);
        Directory.CreateDirectory(sfxFolder);

        bgmPlayer = CreateWindowsMediaPlayer();
        sfxPlayer = CreateWindowsMediaPlayer();

        RefreshTracks();
    }

    public void RefreshTracks()
    {
        bgmTrackPath = FindFirstMp3(bgmFolder);
        jetpackTrackPath = FindFirstMp3(sfxFolder);
    }

    public void StartBackgroundLoop()
    {
        if (bgmPlayer == null)
        {
            return;
        }

        RefreshTracks();
        if (bgmTrackPath == null)
        {
            return;
        }

        PlayLoop(bgmPlayer, bgmTrackPath, 30);
    }

    public void StartJetpackLoop()
    {
        if (sfxPlayer == null)
        {
            return;
        }

        RefreshTracks();
        if (jetpackTrackPath == null)
        {
            return;
        }

        PlayLoop(sfxPlayer, jetpackTrackPath, 60);
    }

    public void StopJetpackLoop()
    {
        StopPlayer(sfxPlayer);
    }

    public void StopAll()
    {
        StopPlayer(bgmPlayer);
        StopPlayer(sfxPlayer);
    }

    private static dynamic? CreateWindowsMediaPlayer()
    {
        try
        {
            var t = Type.GetTypeFromProgID("WMPlayer.OCX");
            return t == null ? null : Activator.CreateInstance(t);
        }
        catch
        {
            return null;
        }
    }

    private static string? FindFirstMp3(string folder)
    {
        return Directory
            .GetFiles(folder, "*.mp3", SearchOption.TopDirectoryOnly)
            .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault();
    }

    private static void PlayLoop(dynamic player, string path, int volume)
    {
        try
        {
            player.URL = path;
            player.settings.volume = volume;
            player.settings.setMode("loop", true);
            player.controls.play();
        }
        catch
        {
            // Ignore audio errors to keep gameplay running.
        }
    }

    private static void StopPlayer(dynamic? player)
    {
        if (player == null)
        {
            return;
        }

        try
        {
            player.controls.stop();
        }
        catch
        {
            // Ignore audio errors to keep gameplay running.
        }
    }

    public void Dispose()
    {
        StopAll();
    }
}
