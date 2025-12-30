using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.IO;
using PracticeTimer.Core;
using Avalonia.Threading;
using NetCoreAudio;

namespace PracticeTimer.Gui.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel()
    {
        isInitializing = true;
        RefreshPresetList();
        isInitializing = false;
    }

    private bool isInitializing;

    private PracticeSession? session;
    private int currentIndex = -1;

    private TimeSpan remainingTime;
    private DispatcherTimer? timer;

    private readonly Player audioPlayer = new Player();

    /* =========================
       Observable State
       ========================= */

    [ObservableProperty]
    private bool isPaused;

    [ObservableProperty]
    private bool isSessionRunning;

    [ObservableProperty]
    private string statusText = "Ready.";

    [ObservableProperty]
    private string totalDurationText = "Total: 00:00:00";

    [ObservableProperty]
    private string currentPhaseName = "—";

    [ObservableProperty]
    private string phaseCounterText = "Phase: —";

    [ObservableProperty]
    private string remainingTimeText = "00:00";

    [ObservableProperty]
    private string pauseResumeText = "Pause";

    public ObservableCollection<string> PresetNames { get; } = new();

    [ObservableProperty]
    private string? selectedPresetName;

    public bool CanStartSession => !IsSessionRunning && Phases.Count > 0;

    public ObservableCollection<Phase> Phases { get; } = new();

    /* =========================
       Timer
       ========================= */

    private void Tick()
    {
        if (currentIndex < 0 || session == null)
            return;

        remainingTime -= TimeSpan.FromSeconds(1);

        if (remainingTime <= TimeSpan.Zero)
        {
            remainingTime = TimeSpan.Zero;
            RemainingTimeText = "00:00";
            NextPhase();
            return;
        }

        RemainingTimeText = remainingTime.ToString(@"mm\:ss");
    }

    /* =========================
       Commands
       ========================= */

    [RelayCommand]
    private void LoadPreset()
    {
        timer?.Stop();
        IsPaused = false;
        IsSessionRunning = false;
        PauseResumeText = "Pause";

        if (string.IsNullOrWhiteSpace(SelectedPresetName))
        {
            StatusText = "Select a preset first.";
            return;
        }

        var presetPath = Path.Combine(
            AppContext.BaseDirectory,
            "Presets",
            SelectedPresetName
        );

        if (!File.Exists(presetPath))
        {
            StatusText = $"Preset not found: {SelectedPresetName}";
            return;
        }

        var preset = PresetLoader.Load(presetPath);
        var loadedSession = PracticeSession.FromPreset(preset);

        Phases.Clear();
        foreach (var p in loadedSession.Phases)
            Phases.Add(p);

        session = loadedSession;
        currentIndex = -1;

        TotalDurationText = $"Total: {session.GetTotalDuration()}";
        CurrentPhaseName = "—";
        PhaseCounterText = $"Phase: 0/{session.Phases.Count}";
        RemainingTimeText = "00:00";
        StatusText = "Preset loaded. Ready to start.";

        OnPropertyChanged(nameof(CanStartSession));
    }

    [RelayCommand]
    private void StartSession()
    {
        if (session == null || session.Phases.Count == 0)
        {
            StatusText = "Load a preset first.";
            return;
        }

        PlayPhaseSound();

        IsPaused = false;
        IsSessionRunning = true;
        PauseResumeText = "Pause";
        OnPropertyChanged(nameof(CanStartSession));

        currentIndex = 0;
        CurrentPhaseName = session.Phases[currentIndex].Name;

        remainingTime = TimeSpan.FromMinutes(session.Phases[currentIndex].DurationMinutes);
        RemainingTimeText = remainingTime.ToString(@"mm\:ss");

        PhaseCounterText = $"Phase: {currentIndex + 1}/{session.Phases.Count}";
        StatusText = "Running.";

        timer?.Stop();
        timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        timer.Tick += (_, _) => Tick();
        timer.Start();
    }

    [RelayCommand]
    private void NextPhase()
    {
        if (session == null || currentIndex < 0)
            return;

        currentIndex++;
        PlayPhaseSound();

        if (currentIndex >= session.Phases.Count)
        {
            timer?.Stop();
            IsSessionRunning = false;

            CurrentPhaseName = "Done!";
            PhaseCounterText = $"Phase: {session.Phases.Count}/{session.Phases.Count}";
            RemainingTimeText = "00:00";
            StatusText = "Session finished.";

            OnPropertyChanged(nameof(CanStartSession));
            return;
        }

        CurrentPhaseName = session.Phases[currentIndex].Name;
        remainingTime = TimeSpan.FromMinutes(session.Phases[currentIndex].DurationMinutes);
        RemainingTimeText = remainingTime.ToString(@"mm\:ss");

        PhaseCounterText = $"Phase: {currentIndex + 1}/{session.Phases.Count}";
        StatusText = "Running.";
    }

    [RelayCommand]
    private void RestartExercise()
    {
        if (session == null || currentIndex < 0 || currentIndex >= session.Phases.Count)
            return;

        remainingTime = TimeSpan.FromMinutes(session.Phases[currentIndex].DurationMinutes);
        RemainingTimeText = remainingTime.ToString(@"mm\:ss");

        IsPaused = false;
        PauseResumeText = "Pause";

        timer?.Stop();
        timer?.Start();

        StatusText = "Running.";
        PlayPhaseSound();
    }

    [RelayCommand]
    private void StopSession()
    {
        timer?.Stop();

        IsSessionRunning = false;
        IsPaused = false;
        PauseResumeText = "Pause";

        currentIndex = -1;
        CurrentPhaseName = "—";
        PhaseCounterText = "Phase: —";
        RemainingTimeText = "00:00";
        StatusText = "Ready.";

        OnPropertyChanged(nameof(CanStartSession));
    }

    [RelayCommand]
    private void TogglePause()
    {
        if (!IsSessionRunning)
            return;

        if (IsPaused)
        {
            timer?.Start();
            IsPaused = false;
            PauseResumeText = "Pause";
            StatusText = "Running.";
        }
        else
        {
            timer?.Stop();
            IsPaused = true;
            PauseResumeText = "Resume";
            StatusText = "Paused.";
        }
    }

    /* =========================
       Sound
       ========================= */

    private async void PlayPhaseSound()
    {
        try
        {
            var path = Path.Combine(
                AppContext.BaseDirectory,
                "Assets",
                "Sounds",
                "phase.wav"
            );

            await audioPlayer.Play(path);
        }
        catch
        {
            // Sound playback failure is non-critical
        }
    }

    /* =========================
       Presets
       ========================= */

    private void RefreshPresetList()
    {
        PresetNames.Clear();

        var presetsDir = Path.Combine(AppContext.BaseDirectory, "Presets");
        if (!Directory.Exists(presetsDir))
            return;

        foreach (var file in Directory.GetFiles(presetsDir, "*.json"))
            PresetNames.Add(Path.GetFileName(file));

        // Start with no selection (user must pick)
        SelectedPresetName = null;
    }

    partial void OnSelectedPresetNameChanged(string? value)
    {
        if (isInitializing)
            return;

        if (string.IsNullOrWhiteSpace(value))
            return;

        LoadPreset();
    }
}




