using System;
using System.Threading;


namespace PracticeTimer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Load preset? (y/n): ");
            var presetChoice = Console.ReadKey(true).KeyChar;


            PracticeSession session;

            if (presetChoice == 'y')
            {
                var presetPath = System.IO.Path.Combine(
                    AppContext.BaseDirectory,
                    "Presets",
                    "Warmup.json"
                );

                var preset = PresetLoader.Load(presetPath);

                session = PracticeSession.FromPreset(preset);
            }
            else
            {
                session = ReadPhasesFromConsole();
            }

            PrintPhases(session);
            Console.WriteLine($"\nTotal duration: {session.GetTotalDuration()}");

            EditSession(session);
            PrintPhases(session);
            Console.WriteLine($"\nTotal duration: {session.GetTotalDuration()}");
            RunSession(session);
           
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

        static void PrintPhases(PracticeSession session)
        {
            Console.WriteLine("\n--- Session Phases ---");

            foreach (var p in session.Phases)
            {
                Console.WriteLine($"{p.Name} - {p.DurationMinutes} min");
            }
        }

        static void EditSession(PracticeSession session)
        {
            while(true)
            {
                Console.Write("\nCommand (list/add/remove/edit/start/help): ");
                string? input = Console.ReadLine();

                if (input == null)
                    continue;

                input = input.Trim().ToLower();

                if (input == "help")
                {
                    Console.WriteLine("Commands:");
                    Console.WriteLine("  list            - show phases");
                    Console.WriteLine("  add             - add a new phase");
                    Console.WriteLine("  remove <number> - remove phase by number");
                    Console.WriteLine("  edit <number>   - edit phase by number");
                    Console.WriteLine("  start           - start the session");
                    continue;
                }


                if (input == "list")
                {
                    PrintPhases(session);
                }
                else if (input == "add")
                {
                    Console.Write("Phase name: ");
                    string? name = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        Console.WriteLine("Name cannot be empty.");
                        continue;
                    }

                    int minutes;
                    while (true)
                    {
                        Console.Write("Duration in minutes: ");
                        string? durationInput = Console.ReadLine();

                        if (int.TryParse(durationInput, out minutes) && minutes > 0)
                            break;

                        Console.WriteLine("Please enter a valid number greater than 0.");
                    }

                    session.AddPhase(new Phase { Name = name.Trim(), DurationMinutes = minutes });

                    PrintPhases(session);
                    Console.WriteLine($"\nTotal duration: {session.GetTotalDuration()}");
                }

                else if (input.StartsWith("remove "))
                {
                    var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                    if (parts.Length != 2 || !int.TryParse(parts[1], out int number))
                    {
                        Console.WriteLine("Usage: remove <number>");
                        continue;
                    }

                    int index = number - 1;

                    if (index < 0 || index >= session.Phases.Count)
                    {
                        Console.WriteLine("Invalid phase number.");
                        continue;
                    }

                    session.RemovePhaseAt(index);

                    PrintPhases(session);
                    Console.WriteLine($"\nTotal duration: {session.GetTotalDuration()}");
                }

                else if (input.StartsWith("edit "))
                {
                    var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                    if (parts.Length != 2 || !int.TryParse(parts[1], out int number))
                    {
                        Console.WriteLine("Usage: edit <number>");
                        continue;
                    }

                    int index = number - 1;

                    if (index < 0 || index >= session.Phases.Count)
                    {
                        Console.WriteLine("Invalid phase number.");
                        continue;
                    }

                    var phase = session.Phases[index];

                    Console.Write($"New name (current: {phase.Name}): ");
                    string? newName = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(newName))
                        phase.Name = newName.Trim();

                    Console.Write($"New duration in minutes (current: {phase.DurationMinutes}): ");
                    string? durationInput = Console.ReadLine();
                    if (int.TryParse(durationInput, out int newMinutes) && newMinutes > 0)
                        phase.DurationMinutes = newMinutes;

                    PrintPhases(session);
                    Console.WriteLine($"\nTotal duration: {session.GetTotalDuration()}");
                }


                else if (input == "start") 
                {
                    break;
                }
                else
                {
                    Console.WriteLine("Unknown command. Try 'list' or 'start'.");
                }
            }
        }

        static void RunSession(PracticeSession session)
        {
            if (session.Phases.Count == 0)
            {
                Console.WriteLine("No phases entered.");
                return;
            }

            Console.WriteLine("\nStarting session...");
            Console.WriteLine("Controls: [p] pause/resume, [r] restart phase, [q] quit\n");


            for (int i = 0; i < session.Phases.Count; i++)

            {
                var current = session.Phases[i];
                int remainingSeconds = current.DurationMinutes * 60;
                bool isPaused = false;

                Console.WriteLine($"Phase {i + 1}/{session.Phases.Count}");

                // Audio feedback on phase change (terminal bell, platform-dependent)
                Console.Write("\a");

                Console.WriteLine($"Starting: {current.Name} ({current.DurationMinutes} min)");

                if(i < session.Phases.Count -1)
                {
                    var next = session.Phases[i + 1];
                    Console.WriteLine($"Next: {next.Name} ({next.DurationMinutes} min)");

                }

                while (remainingSeconds > 0)
                {
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
                        else if (key.KeyChar == 'r')
                        {
                            Console.WriteLine();
                            Console.WriteLine("Restart current session ? (y/n)");

                            var confirmKey = Console.ReadKey(true);

                            if(confirmKey.KeyChar == 'y')
                            {
                                remainingSeconds = current.DurationMinutes * 60;
                                Console.WriteLine("Phase restarted.");
                            }
                            else
                            {
                                Console.WriteLine("Restart cancelled.");
                            }
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

                Console.WriteLine();

                // Audio feedback on phase change (terminal bell, platform-dependent)
                Console.Write("\a");

                Console.WriteLine($"Phase finished: {current.Name}\n");
            }

            Console.WriteLine("Session finished.");
        }
    }

}

