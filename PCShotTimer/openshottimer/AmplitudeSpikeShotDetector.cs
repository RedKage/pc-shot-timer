/* This file is part of Open Shot Timer.
 * Copyright (C) 2009 Ariel Weisberg
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
using System.Collections.Generic;
using System.IO;

namespace PCShotTimer.openshottimer
{
    /// <summary>
    ///     This detector uses sound spikes to detect a gunshot.
    ///     File converted to C# by Tactical Freak, 2014
    ///     Original source can by found there https://code.google.com/p/openshottimer/
    ///     Original class comment:
    ///     With a .45 I found that it spikes at for 18 milliseconds with a dense number of samples and then drops off.
    ///     When the first big sample comes in count it as a shot and then stop listening for .12 seconds.
    /// </summary>
    public class AmplitudeSpikeShotDetector : ShotDetector
    {
        #region members

        /// <summary>
        /// Number of consecutive sample spikes required to consider a gunshot has occured.
        /// The lower the more sensitive.
        /// </summary>
        private readonly int _samplesAboveThresholdRequired;

        /// <summary>This is the spike value: pretty much how loud it should be.</summary>
        private readonly short _shotDetectionThreshold;

        /// <summary>Used to ignore samples until the buffer is large enough.</summary>
        private long _ignoreUntilSample = -1;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of AmplitudeSpikeShotDetector.
        ///     This shot detector detects noise spike.
        /// </summary>
        /// <param name="sampleRate">Format of the audio that will be provided to this detector.</param>
        /// <param name="sampleSizeInBits">Application config properties.</param>
        /// <param name="samplesAboveThresholdRequired">
        ///     Number of samples with a spike required to consider a gunshot has occured.
        ///     The lower the more sensitive.
        /// </param>
        /// <param name="shotDetectionLoudness">
        ///     This is the previous param's threshold value:
        ///     pretty much how loud it should be. Here it's a percent.
        /// </param>
        public AmplitudeSpikeShotDetector(int sampleRate, int sampleSizeInBits, int samplesAboveThresholdRequired, int shotDetectionLoudness)
            : base(sampleRate, sampleSizeInBits)
        {
            _shotDetectionThreshold = Convert.ToInt16(shotDetectionLoudness * short.MaxValue / 100);
            _samplesAboveThresholdRequired = samplesAboveThresholdRequired + 1;
            if (sampleSizeInBits != 16)
                throw new ArgumentException("Only support 16 bit samples");

            _ignoreUntilSample = Convert.ToInt64(_samplesPerMillisecond*1000);
        }

        #endregion

        #region Internal methods

        /// <summary>
        ///     Detects noise spike.
        /// </summary>
        /// <param name="buffer">The audio data.</param>
        /// <returns>Array of ShotEvent containing the shots found from this audio data.</returns>
        protected override ShotEvent[] p_processAudio(MemoryStream buffer)
        {
            var length = Convert.ToInt32(buffer.Length);
            var shotEvents = new List<ShotEvent>();
            var sampleMax = 0;

            // Ignoring samples until buffer is large enough
            // Or check if our buffer is not already in the 'ignored' range
            if (_ignoreUntilSample > 0)
            {
                var bufferLength = (length/_sampleSizeInBytes);
                if (_currentSample + bufferLength < _ignoreUntilSample)
                    return new ShotEvent[0];

                buffer.Position = _ignoreUntilSample - _currentSample;
                _ignoreUntilSample = -1;
            }

            // Reads the audio data from memory
            using (var reader = new BinaryReader(buffer))
            {
                var samplesAboveThreshold = 0;
                while (reader.BaseStream.Position != reader.BaseStream.Length)
                {
                    short rawSample;

                    // TODO I had a 2 crashes there while messing with the sample rate. Now can't reproduce.
                    // TODO Weird cuz Position was same as reader.BaseStream.Length in the debugger.
                    // TODO So yeah it was like impossible to crash there, shoulda exited the while.
                    // TODO Guess that the stream can get affected somehow from outside that loop then wtf?
                    try
                    {
                        rawSample = reader.ReadInt16();
                    }
                    catch (EndOfStreamException)
                    {
                        // TODO why the fuck should that happen anyway uh?
                        break;
                    }

                    // Can't Abs of -32768 because that will output +32768 which is outta range for a short:
                    // it's max value is +32767 (it's one short, yeah...)
                    var sample = rawSample == short.MinValue
                        ? short.MaxValue
                        : Math.Abs(rawSample);

                    // Get sample max spike so far
                    sampleMax = Math.Max(sample, sampleMax);
                    if (sample <= _shotDetectionThreshold)
                        continue;

                    // Found a spike that is above the threshold
                    samplesAboveThreshold++;
                    if (samplesAboveThreshold < _samplesAboveThresholdRequired)
                        continue;

                    // Number of samples above threshold reached
                    samplesAboveThreshold = 0;
                    var currentSample = _currentSample + buffer.Position;
                    _ignoreUntilSample = currentSample + Convert.ToInt64(_samplesPerMillisecond*120);

                    // Create the shot event
                    var shotTime = Convert.ToInt32(Math.Floor(currentSample/_samplesPerMillisecond));
                    var split = Convert.ToInt32(Math.Floor((currentSample - _lastShotSample)/_samplesPerMillisecond));
                    shotEvents.Add(new ShotEvent(++_shotCount, new TimeSpan(0, 0, 0, 0, shotTime),
                        new TimeSpan(0, 0, 0, 0, split)));
                    _lastShotSample = currentSample;

                    // Compute if we have reached the ignored range
                    var remaining = Convert.ToInt32(buffer.Length - buffer.Position);
                    var currentSamplePlusRemaining = currentSample + remaining;
                    if (currentSamplePlusRemaining <= _ignoreUntilSample)
                        break;

                    buffer.Position = (buffer.Position + (_ignoreUntilSample - currentSample));
                    _ignoreUntilSample = -1;
                }
            }
            return shotEvents.ToArray();
        }

        #endregion

        #region Public methods

        /// <summary>
        ///     Resets this Shot Detector.
        /// </summary>
        public override void Reset()
        {
            _currentSample = 0;
            _shotCount = 0;
            _lastShotSample = 0;
            _ignoreUntilSample = -1;
        }

        #endregion
    }
}