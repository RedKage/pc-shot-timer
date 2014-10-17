using PCShotTimer.openshottimer;
using System;

namespace PCShotTimer.Core
{
    /// <summary>
    ///     ShotEventArgs just extends EventArgs real quick so a ShotEvent array can be bundled with an Event.
    /// </summary>
    public class ShotEventArgs : EventArgs
    {
        #region Properties

        /// <summary>Gets or sets the detected Shots into this EventArg.</summary>
        public ShotEvent[] Shots { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of ShotEventArgs.
        ///     Yeah well, I was like, this will be pretier using a constructor instead of new ShotEventArgs { Shots = stuffff }
        /// </summary>
        /// <param name="shots">The shots to bundle inside this EventArgs.</param>
        public ShotEventArgs(ShotEvent[] shots)
        {
            Shots = shots;
        }

        #endregion
    }
}