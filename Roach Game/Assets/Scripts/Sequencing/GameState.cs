/*
 * File: GameState.cs
 * Created: 06/06/2026, 2:54:39 PM
 * Author: Travis Reid
 * Copyright 2019 - 2026 Studio Tilia
 */

using UnityEngine;

public partial class SequenceController : MonoBehaviour
{
    // ------------------------------------------------------------------------
    // Types
    // ------------------------------------------------------------------------
    public class GameState
    {
        // --------------------------------------------------------------------
        // Properties
        // --------------------------------------------------------------------
        public virtual GameStateType StateType => GameStateType.Invalid;

        // --------------------------------------------------------------------
        // Methods
        // --------------------------------------------------------------------
        public virtual void EnterState(SequenceController controller) {}

        // --------------------------------------------------------------------
        public virtual void ExitState() {}

        // --------------------------------------------------------------------
        public virtual void RunState(float deltaTime) {}
    }

    // ------------------------------------------------------------------------
    public class GameActionState : GameState
    {
        public override GameStateType StateType => GameStateType.Action;

        public override void EnterState(SequenceController controller)
        {
            Cursor.lockState = CursorLockMode.Locked;
            controller._player.SetInputEnabled(true);
        }
    }

    // ------------------------------------------------------------------------
    public class GameCinematicState : GameState
    {
        public override GameStateType StateType => GameStateType.Cinematic;

        public override void EnterState(SequenceController controller)
        {
            Cursor.lockState = CursorLockMode.Locked;
            controller._player.SetInputEnabled(false);
        }
    }

    // ------------------------------------------------------------------------
    public class GameDialogueState : GameState
    {
        public override GameStateType StateType => GameStateType.Dialogue;

        public override void EnterState(SequenceController controller)
        {
            Cursor.lockState = CursorLockMode.None;
            controller._player.SetInputEnabled(false);
        }
    }

    // ------------------------------------------------------------------------
    public class GameMenuState : GameState
    {
        public override GameStateType StateType => GameStateType.Menu;

        public override void EnterState(SequenceController controller)
        {
            Cursor.lockState = CursorLockMode.None;
            controller._player.SetInputEnabled(false);
        }
    }
}