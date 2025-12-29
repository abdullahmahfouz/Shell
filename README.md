# My Shell 🐚

A simple shell I built in C#. It works like bash or zsh, but simpler!

## What Can It Do?

**Run commands:**
```
$ echo hello world
hello world

$ pwd
/Users/Abdullah

$ cd /tmp
```

**Tab completion** - Type part of a command, press Tab:
```
$ ech[TAB]
$ echo 
```

**History** - Press Up/Down arrows to see old commands:
```
$ [UP ARROW]
$ echo hello    ← your last command appears
```

**Pipes** - Send output from one command to another:
```
$ echo hello | cat
hello
```

**Redirects** - Save output to a file:
```
$ echo hello > file.txt
```

## Built-in Commands

| Command | What it does |
|---------|-------------|
| `echo text` | Prints text |
| `pwd` | Shows current folder |
| `cd folder` | Go to folder |
| `cat file` | Show file contents |
| `type cmd` | Is it builtin or external? |
| `history` | Show all past commands |
| `history 5` | Show last 5 commands |
| `history -r file` | Load history from file |
| `history -w file` | Save history to file |
| `history -a file` | Add new commands to file |
| `exit` | Close the shell |

## How to Run

```bash
dotnet run
```

## Project Files

```
src/
├── Builtins/        ← echo, cd, pwd, history, etc.
├── Execution/       ← Running programs and pipes
├── Parsing/         ← Understanding what you typed
├── Shell/           ← Main loop and keyboard input
└── Utils/           ← Helper stuff
```

## Built With

- C# / .NET 9.0

