using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.IO;
using PracticeTimer.Core;

namespace PracticeTimer.Gui.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private PracticeSession? session;
    private int currentIndex = -1;

    [ObservableProperty]
    private string statusText = "Ready.";

    [ObservableProperty]
    private string totalDurationText = "Total: 00:00:00";

    [ObservableProperty]
    private string currentPhaseName = "—";

    [ObservableProperty]
    private string phaseCounterText = "Phase: —";

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
        PhaseCounterText = $"Phase: {currentIndex + 1}/{session.Phases.Count}";
        StatusText = "Running.";
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
        StatusText = "Session finished.";
        return;
    }

    CurrentPhaseName = session.Phases[currentIndex].Name;
    PhaseCounterText = $"Phase: {currentIndex + 1}/{session.Phases.Count}";
    StatusText = "Running.";
}

}


