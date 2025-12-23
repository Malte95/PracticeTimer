using System;
using System.Threading;


namespace PracticeTimer
{
    class Program
    {
        static void Main(string[] args)
        {
            var session = ReadPhasesFromConsole();

            Console.WriteLine("\n--- Session Phases ---");

            foreach (var p in session.Phases)
            {
                Console.WriteLine($"{p.Name} - {p.DurationMinutes} min");
            }

            if (session.Phases.Count > 0)
            {
                Console.WriteLine("\nStarting session...");
                Console.WriteLine("Controls: [p] pause/resume, [q] quit\n");

                foreach (var current in session.Phases)
                {
                    int remainingSeconds = current.DurationMinutes * 60;
                    bool isPaused = false;

                    Console.WriteLine($"Starting: {current.Name} ({current.DurationMinutes} min)");

                    while (remainingSeconds > 0)
                    {
                        // Check for key press without blocking
                        if (Console.KeyAvailable)
                        {
                            var key = Console.ReadKey(true);

                            if (key.KeyChar == 'p')
                            {
                                isPaused = !isPaused;
                                Console.WriteLine();
                                Console.WriteLine(isPaused ? "Paused." : "Resumed.");
                            }
                            else if (key.KeyChar == 'q')
                            {
                                Console.WriteLine("Session aborted.");
                                return;
                            }
                        }

                        if (!isPaused)
                        {
                            int minutesLeft = remainingSeconds / 60;
                            int secondsLeft = remainingSeconds % 60;

                            Console.Write($"\rTime left: {minutesLeft:D2}:{secondsLeft:D2}   ");

                            remainingSeconds--;
                        }

                        Thread.Sleep(1000);
                    }

                    Console.WriteLine(); // Zeilenumbruch nach Countdown
                    Console.WriteLine($"Phase finished: {current.Name}\n");

                }

                Console.WriteLine("Session finished.");
            }
            else
            {
                Console.WriteLine("No phases entered.");
            }

        }

        static PracticeSession ReadPhasesFromConsole()
        {
            var session = new PracticeSession();

            while (true)
            {
                Console.Write("Phase name (or 'start'): ");
                string? name = Console.ReadLine();

                if (name == null)
                    continue;

                if (name.Trim().ToLower() == "start")
                    break;

                int minutes;
                while (true)
                {
                    Console.Write("Duration in minutes: ");
                    string? input = Console.ReadLine();

                    if (int.TryParse(input, out minutes) && minutes > 0)
                        break;

                    Console.WriteLine("Please enter a valid number greater than 0.");
                }

                var phase = new Phase
                {
                    Name = name,
                    DurationMinutes = minutes
                };

                session.AddPhase(phase);
                Console.WriteLine($"Phase added: {phase.Name}");
            }

            return session;
        }

    }
}
