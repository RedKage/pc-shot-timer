using System;
using PCShotTimer.openshottimer;

namespace PCShotTimer
{
    /// <summary>
    ///     ShotEventArgs just extends EventArgs real quick so a ShotEvent array can be bundled with an Event.
    /// </summary>
    public class ShotEventArgs : EventArgs
    {
        /// <summary>
        ///     Initializes a new instance of ShotEventArgs.
        ///     Yeah well, I was like, this will be pretier using a constructor instead of new ShotEventArgs { Shots = stuffff }
        /// </summary>
        /// <param name="shots">The shots to bundle inside this EventArgs.</param>
        public ShotEventArgs(ShotEvent[] shots)
        {
            Shots = shots;
        }

        /// <summary>Gets or sets the detected Shots into this EventArg.</summary>
        public ShotEvent[] Shots { get; set; }
    }
}