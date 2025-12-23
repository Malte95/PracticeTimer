using System.Collections.Generic;

public class PracticeSession
{
    private readonly List<Phase> _phases = new();

    public IReadOnlyList<Phase> Phases => _phases;

    public void AddPhase(Phase phase)
    {
        _phases.Add(phase);
    }
}
