<video src="assets/practice-timer-demo.mov"
       controls
       width="800">
</video>

# Practice Timer (MVP)

A simple desktop practice timer built with **C#**, **Avalonia UI**, and **MVVM**.

This project focuses on a clean **MVP (Minimum Viable Product)** to keep the core logic easy to understand and maintain.

---

## Features

- Add practice exercises (name + duration in minutes)
- Remove exercises (only when the session is not running)
- Start a practice session
- Automatic countdown timer
- Automatically switch to the next exercise
- Stop the session at any time

---

## Why MVP?

During development, the project was intentionally reduced to an MVP.
This helped to:

- focus on the core session flow
- avoid unnecessary complexity
- keep the codebase readable and explainable

---

## Architecture

- **MVVM pattern**
- `Phases` are used for editing exercises (Add / Remove)
- A `PracticeSession` is built from the UI list when the session starts
- The running session is completely separated from edit state

---

## Tech Stack

- .NET
- C#
- Avalonia UI
- CommunityToolkit.MVVM

---

## How to run

```bash
dotnet run --project PracticeTimer.Gui

