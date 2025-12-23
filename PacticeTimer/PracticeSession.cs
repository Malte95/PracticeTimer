using System.Collections.Generic;

public class PracticeSession
{
    private readonly List<Phase> _phases = new();

    public IReadOnlyList<Phase> Phases => _phases;

    public void AddPhase(Phase phase)
    {
        _phases.Add(phase);
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
}
