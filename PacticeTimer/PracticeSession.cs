using System.Collections.Generic;

public class PracticeSession
{
    private readonly List<Phase> _phases = new();

    public IReadOnlyList<Phase> Phases => _phases;

    public void AddPhase(Phase phase)
    {
        _phases.Add(phase);
    }

    public void RemovePhaseAt(int index)
    {
        _phases.RemoveAt(index);
    }

    public void UpdatePhaseAt(int index, string name, int durationMinutes)
    {
        _phases[index].Name = name;
        _phases[index].DurationMinutes = durationMinutes;
    }

    public System.TimeSpan GetTotalDuration()
    {
        int totalSeconds = 0;

        foreach(var phase in _phases)
        {
            totalSeconds += phase.DurationMinutes * 60;
        }

        return System.TimeSpan.FromSeconds(totalSeconds);

    }

    public static PracticeSession FromPreset(Preset preset)
    {
        var session = new PracticeSession();

        foreach (var phase in preset.Phases)
        {
            session.AddPhase(new Phase
            {
                Name = phase.Name,
                DurationMinutes = phase.DurationMinutes
            });
        }

        return session;
    }
}
