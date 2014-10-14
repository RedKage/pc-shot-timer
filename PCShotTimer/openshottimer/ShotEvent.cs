/* This file is part of Open Shot Timer.
 * Copyright (C) 2009-10 Ariel Weisberg
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using System;

namespace PCShotTimer.openshottimer
{
    /// <summary>
    ///     A Shot event contains:
    ///     - a number (the shot number)
    ///     - a number of ms since the beginning for that shot
    ///     - a number of ms since the last shot
    ///     File converted to C# by
    ///     Tactical Freak, 2014
    ///     Original source can by found there https://code.google.com/p/openshottimer/
    /// </summary>
    public class ShotEvent : EventArgs
    {
        /// <summary>
        ///     Initializes a new instance of ShotEvent.
        /// </summary>
        /// <param name="shotNum">The number of this shot in the timeline.</param>
        /// <param name="time">Time shot occured in milliseconds since the beginning of the timeline.</param>
        /// <param name="prevTime">Previous time shot occured in milliseconds.</param>
        public ShotEvent(int shotNum, TimeSpan time, TimeSpan prevTime)
        {
            ShotNum = shotNum;
            Time = time;
            Split = prevTime;
        }

        /// <summary>
        ///     Initializes a new instance of ShotEvent.
        /// </summary>
        /// <param name="shotNum">The number of this shot in the timeline.</param>
        /// <param name="time">Time shot occured in milliseconds since the beginning of the timeline.</param>
        public ShotEvent(int shotNum, TimeSpan time)
            : this(shotNum, time, TimeSpan.Zero)
        {
        }

        /// <summary>Gets or sets the number/Id of this shot in the timeline.</summary>
        public int ShotNum { get; protected set; }

        /// <summary>Gets or sets the shot time that occured in milliseconds since the beginning of the timeline.</summary>
        public TimeSpan Time { get; protected set; }

        /// <summary>
        ///     Gets or sets the time between this shot and the last shot (or the beginning of the timeline for the first shot).
        ///     Also in milliseconds
        /// </summary>
        public TimeSpan Split { get; protected set; }
    }
}