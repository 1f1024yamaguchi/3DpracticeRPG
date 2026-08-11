@echo off
REM ============================================================
REM  Sakana Fugu launcher for Claude Code
REM  Usage: double-click this file, or run it in a terminal
REM  First-time setup (once): register your API key
REM    setx SAKANA_API_KEY "fish_YOUR_KEY"
REM    then open a NEW terminal so it takes effect
REM ============================================================

if "%SAKANA_API_KEY%"=="" (
  echo.
  echo [!] SAKANA_API_KEY is not set.
  echo     Run this once:  setx SAKANA_API_KEY "fish_YOUR_KEY"
  echo     Then open a new terminal and run this again.
  echo.
  pause
  exit /b 1
)

set ANTHROPIC_BASE_URL=https://api.sakana.ai
set ANTHROPIC_AUTH_TOKEN=%SAKANA_API_KEY%
set ANTHROPIC_DEFAULT_OPUS_MODEL=fugu-ultra[1m]
set ANTHROPIC_DEFAULT_SONNET_MODEL=fugu[1m]
set ANTHROPIC_DEFAULT_HAIKU_MODEL=fugu[1m]
set CLAUDE_CODE_SUBAGENT_MODEL=fugu[1m]

echo Starting Claude Code (Sakana Fugu)...
claude %*
