/*
 * File: RoachState.cs
 * Created: 28/05/2026, 11:59:26 AM
 * Author: Travis Reid
 * Copyright 2019 - 2026 Studio Tilia
 */

public partial class Roach
{
    // ------------------------------------------------------------------------
    // Types
    // ------------------------------------------------------------------------
    protected class RoachCinematicState : RoachState
    {
        public override void EnterState(Roach roach)
        {
            base.EnterState(roach);
            _roach._gun.SkipFirstReloadAudio();
            roach._activeCinematic.Play();
        }
    }
}