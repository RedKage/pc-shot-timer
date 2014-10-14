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
using System.IO;

namespace PCShotTimer.openshottimer
{
    /// <summary>
    ///     Base class for different shot detection algorithms.
    /// 
    /// File converted to C# by
    /// Tactical Freak, 2014
    /// Original source can by found there https://code.google.com/p/openshottimer/
    /// </summary>
    public abstract class ShotDetector
    {
        /// <summary>Number of samples per second</summary>
        protected readonly int _sampleRate;

        /// <summary>Size of the audio samples in bits</summary>
        protected readonly int _sampleSizeInBits;

        /// <summary>Size of the audio samples in bytes</summary>
        protected readonly int _sampleSizeInBytes;

        /// <summary>Number of audio samples per millisecond at the current sample rate</summary>
        protected readonly double _samplesPerMillisecond;

        /// <summary>Current sample in the timeline that was last received from the capturer.</summary>
        protected long _currentSample = 0;

        /// <summary>Indicates the last shot time that occured.</summary>
        protected long _lastShotSample = 0;

        /// <summary>Number of shots detected to far.</summary>
        protected int _shotCount = 0;

        /// <summary>A stopwatch that will be used to know when shots occured.</summary>
        protected ShotTimer _shotTimer;

        /// <summary>
        ///     Initializes a new instance of ShotDetector.
        /// </summary>
        /// <param name="sampleRate">Format of the audio that will be provided to this detector.</param>
        /// <param name="sampleSizeInBits">Application config properties.</param>
        protected ShotDetector(int sampleRate, int sampleSizeInBits)
        {
            _sampleRate = sampleRate;
            _sampleSizeInBits = sampleSizeInBits;
            _sampleSizeInBytes = _sampleSizeInBits/8;
            _samplesPerMillisecond = Convert.ToInt32(_sampleRate/1000);
        }

        /// <summary>
        ///     Process the audio to find shot sounds.
        /// </summary>
        /// <param name="audioData">The audio data to analyze</param>
        /// <returns>Detected shots in a ShotEvent array</returns>
        public ShotEvent[] ProcessAudio(MemoryStream audioData)
        {
            var lenght = audioData.Length;
            var events = p_processAudio(audioData);
            _currentSample += (lenght/_sampleSizeInBytes);
            return events;
        }

        /// <summary>
        ///     Process the audio to find shot sounds. This method is used by p_processAudio()
        ///     Implement your own shot detection here.
        /// </summary>
        /// <param name="audioData">The audio data to analyze</param>
        /// <returns>Detected shots in a ShotEvent array</returns>
        protected abstract ShotEvent[] p_processAudio(MemoryStream audioData);

        /// <summary>
        ///     Resets this Shot Detector.
        /// </summary>
        public abstract void Reset();
    }
}