using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.IO;
using PracticeTimer.Core;
using Avalonia.Threading;

namespace PracticeTimer.Gui.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private PracticeSession? session;
    private int currentIndex = -1;
    
    private TimeSpan remainingTime;
    private DispatcherTimer? timer;
    
    private void Tick()
{
    if (currentIndex < 0 || session == null)
        return;

    remainingTime = remainingTime - TimeSpan.FromSeconds(1);

    if (remainingTime <= TimeSpan.Zero)
    {
        remainingTime = TimeSpan.Zero;
        RemainingTimeText = "00:00";
        NextPhase(); // wechselt Phase und setzt remainingTime neu
        return;
    }

    RemainingTimeText = remainingTime.ToString(@"mm\:ss");
}



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

    public ObservableCollection<Phase> Phases { get; } = new();

    [RelayCommand]
    private void LoadPreset()
    {
        var presetPath = Path.Combine(
            AppContext.BaseDirectory,
            "Presets",
            "Warmup.json"
        );

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
    }

    [RelayCommand]
    private void StartSession()
    {
        if (session == null || session.Phases.Count == 0)
        {
            StatusText = "Load a preset first.";
            return;
        }

        currentIndex = 0;
        CurrentPhaseName = session.Phases[currentIndex].Name;

       var minutes = session.Phases[currentIndex].DurationMinutes;
remainingTime = TimeSpan.FromMinutes(minutes);
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
        if (session == null || session.Phases.Count == 0)
        {
            StatusText = "Load a preset first.";
            return;
        }

        if (currentIndex < 0)
        {
            StatusText = "Press Start first.";
            return;
        }

        currentIndex++;

        if (currentIndex >= session.Phases.Count)
        {
            CurrentPhaseName = "Done!";
            PhaseCounterText = $"Phase: {session.Phases.Count}/{session.Phases.Count}";
            RemainingTimeText = "00:00";
            StatusText = "Session finished.";
            timer?.Stop();
            return;
        }

        CurrentPhaseName = session.Phases[currentIndex].Name;

        var minutes = session.Phases[currentIndex].DurationMinutes;
remainingTime = TimeSpan.FromMinutes(minutes);
RemainingTimeText = remainingTime.ToString(@"mm\:ss");


        PhaseCounterText = $"Phase: {currentIndex + 1}/{session.Phases.Count}";
        StatusText = "Running.";
    }

    [RelayCommand]
    private void StopSession()
    {
    timer?.Stop();

        currentIndex = -1;
        CurrentPhaseName = "—";
        PhaseCounterText = "Phase: —";
        RemainingTimeText = "00:00";
        StatusText = "Ready.";
    }
}



