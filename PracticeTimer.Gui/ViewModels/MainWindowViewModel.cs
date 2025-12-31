using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using PracticeTimer.Core;
using Avalonia.Threading;
using NetCoreAudio;

namespace PracticeTimer.Gui.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel()
    {
        UpdateTotals();
    }

    private PracticeSession? session;
    private int currentIndex = -1;

    private TimeSpan remainingTime;
    private DispatcherTimer? timer;

    private readonly Player audioPlayer = new Player();

    /* =========================
       Observable State
       ========================= */

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
    [NotifyCanExecuteChangedFor(nameof(RemoveExerciseCommand))]
    private Phase? selectedExercise;

    /* =========================
       Add Exercise (Input State)
       ========================= */

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddExerciseCommand))]
    private string newExerciseName = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddExerciseCommand))]
    private string newExerciseMinutesText = string.Empty;

    public ObservableCollection<Phase> Phases { get; } = new();

    public bool CanStartSession => !IsSessionRunning && Phases.Count > 0;

    /* =========================
       Timer
       ========================= */

    private void Tick()
    {
        if (session == null || currentIndex < 0)
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
    private void StartSession()
    {
        if (Phases.Count == 0)
        {
            StatusText = "Add at least one exercise first.";
            return;
        }

        // Build a fresh session from the current UI list
        var newSession = new PracticeSession();
        foreach (var p in Phases)
        {
            newSession.AddPhase(new Phase
            {
                Name = p.Name,
                DurationMinutes = p.DurationMinutes
            });
        }

        session = newSession;
        currentIndex = 0;

        PlayPhaseSound();

        IsSessionRunning = true;
        OnPropertyChanged(nameof(CanStartSession));

        CurrentPhaseName = session.Phases[currentIndex].Name;
        remainingTime = TimeSpan.FromMinutes(session.Phases[currentIndex].DurationMinutes);
        RemainingTimeText = remainingTime.ToString(@"mm\:ss");

        PhaseCounterText = $"Phase: {currentIndex + 1}/{session.Phases.Count}";
        StatusText = "Running.";

        timer?.Stop();
        timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
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
            FinishSession();
            return;
        }

        CurrentPhaseName = session.Phases[currentIndex].Name;
        remainingTime = TimeSpan.FromMinutes(session.Phases[currentIndex].DurationMinutes);
        RemainingTimeText = remainingTime.ToString(@"mm\:ss");

        PhaseCounterText = $"Phase: {currentIndex + 1}/{session.Phases.Count}";
        StatusText = "Running.";
    }

    [RelayCommand]
    private void StopSession()
    {
        timer?.Stop();
        timer = null;

        session = null;
        currentIndex = -1;

        IsSessionRunning = false;

        CurrentPhaseName = "—";
        PhaseCounterText = "Phase: —";
        RemainingTimeText = "00:00";
        StatusText = "Ready.";

        OnPropertyChanged(nameof(CanStartSession));
        UpdateTotals();
    }

    private bool CanAddExercise()
    {
        if (IsSessionRunning)
            return false;

        if (string.IsNullOrWhiteSpace(NewExerciseName))
            return false;

        return int.TryParse(NewExerciseMinutesText, out var minutes) && minutes > 0;
    }

    [RelayCommand(CanExecute = nameof(CanAddExercise))]
    private void AddExercise()
    {
        var name = NewExerciseName.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            StatusText = "Exercise name must not be empty.";
            return;
        }

        if (!int.TryParse(NewExerciseMinutesText, out var minutes) || minutes <= 0)
        {
            StatusText = "Minutes must be a number greater than 0.";
            return;
        }

        Phases.Add(new Phase
        {
            Name = name,
            DurationMinutes = minutes
        });

        // Editing invalidates any previous built session
        session = null;
        currentIndex = -1;

        NewExerciseName = string.Empty;
        NewExerciseMinutesText = string.Empty;

        UpdateTotals();
        RemainingTimeText = "00:00";
        CurrentPhaseName = "—";

        StatusText = $"Exercise added: {name} ({minutes} min)";
        OnPropertyChanged(nameof(CanStartSession));
    }

    private bool CanRemoveExercise()
    {
        return !IsSessionRunning && SelectedExercise != null;
    }

    [RelayCommand(CanExecute = nameof(CanRemoveExercise))]
    private void RemoveExercise()
    {
        if (SelectedExercise == null)
            return;

        var removedName = SelectedExercise.Name;

        Phases.Remove(SelectedExercise);
        SelectedExercise = null;

        session = null;
        currentIndex = -1;

        CurrentPhaseName = "—";
        RemainingTimeText = "00:00";

        UpdateTotals();
        OnPropertyChanged(nameof(CanStartSession));

        StatusText = $"Removed: {removedName}";
    }

    /* =========================
       Helpers
       ========================= */

    private void FinishSession()
    {
        timer?.Stop();

        IsSessionRunning = false;

        CurrentPhaseName = "Done!";
        RemainingTimeText = "00:00";

        if (session != null)
            PhaseCounterText = $"Phase: {session.Phases.Count}/{session.Phases.Count}";
        else
            PhaseCounterText = "Phase: —";

        StatusText = "Session finished.";

        session = null;
        currentIndex = -1;

        OnPropertyChanged(nameof(CanStartSession));
    }

    private void UpdateTotals()
    {
        var totalMinutes = 0;
        foreach (var p in Phases)
            totalMinutes += p.DurationMinutes;

        TotalDurationText = $"Total: {TimeSpan.FromMinutes(totalMinutes):hh\\:mm\\:ss}";
        PhaseCounterText = $"Phase: 0/{Phases.Count}";
    }

    private async void PlayPhaseSound()
    {
        try
        {
            var path = System.IO.Path.Combine(
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
}




