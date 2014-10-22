using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using PropertyChanged;

namespace PCShotTimer.Core
{
    /// <summary>
    ///     Container for the options.
    /// </summary>
    [XmlRoot("Options")]
    [ImplementPropertyChanged]
    public class OptionsData
    {
        #region Properties

        /// <summary>Gets or sets wether the beep shall have a random delay after pressing Start.</summary>
        [XmlElement("GeneralRandomDelay")]
        public bool GeneralRandomDelay { get; set; }

        /// <summary>Gets or sets the beep minimum delay (secs).</summary>
        [XmlElement("GeneralRandomStartDelayMin")]
        public double GeneralRandomStartDelayMin { get; set; }

        /// <summary>Gets or sets the beep maximum delay (secs).</summary>
        [XmlElement("GeneralRandomStartDelayMax")]
        public double GeneralRandomStartDelayMax { get; set; }

        /// <summary>Gets or sets whether the shot timer shall loop until pressing Stop.</summary>
        [XmlElement("GeneralLoopTimer")]
        public bool GeneralLoopTimer { get; set; }

        /// <summary>Gets or sets the current selected input device ID.</summary>
        [XmlElement("InputDeviceId")]
        public int InputDeviceId { get; set; }

        /// <summary>Gets or sets the default sample rate for the mic input.</summary>
        [XmlElement("InputSampleRate")]
        public int InputSampleRate { get; set; }

        /// <summary>Gets or sets the default sample bits for the mic input.</summary>
        [XmlElement("InputSampleBits")]
        public int InputSampleBits { get; set; }

        /// <summary>Gets or sets the default channel number for the mic input (stereo/mono).</summary>
        [XmlElement("InputChannels")]
        public int InputChannels { get; set; }

        /// <summary>
        ///     Gets or sets the DetectorDuration which is actually the number of time (or samples)
        ///     a high audio spike is detected before being counted as a 'shot'.
        ///     So the lower, the more sensitive it is.
        /// </summary>
        [XmlElement("DetectorDuration")]
        public int DetectorDuration { get; set; }

        /// <summary>
        ///     Gets or sets the value of the spike to reach.
        ///     The louder the higher the number.
        ///     Here this is a percent value of the maximum input loudness.
        /// </summary>
        [XmlElement("DetectorLoudness")]
        public int DetectorLoudness { get; set; }

        /// <summary>Gets or sets whether there should be a speech before the beep.</summary>
        [XmlElement("SoundPlayReadyStandby")]
        public bool SoundPlayReadyStandby { get; set; }

        /// <summary>Gets or sets the selected Beep sound.</summary>
        [XmlElement("SoundSelectedBeepFile")]
        public string SoundSelectedBeepFile { get; set; }

        /// <summary>Gets or sets the selected Ready/Standby sounds.</summary>
        [XmlElement("SoundSelectedReadyStandbyFiles")]
        public List<string> SoundSelectedReadyStandbyFiles { get; set; }

        /// <summary>Gets the available beep sounds from the app subfolder.</summary>
        [XmlIgnore]
        public IEnumerable<FileInfo> AvailableBeepSounds
        {
            get
            {
                var sounds = Directory.GetFiles(string.Format(@"{0}\{1}", App.AppDirectory, App.SoundsDirecoryName));
                return sounds
                    .Select(sound => new FileInfo(sound))
                    .Where(file => file.Name.StartsWith(App.BeepSoundsPrefix))
                    .ToList();
            }
        }

        /// <summary>Gets the available ready/standby sounds from the app subfolder.</summary>
        [XmlIgnore]
        public IEnumerable<FileInfo> AvailableReadyStandbySounds
        {
            get
            {
                var sounds = Directory.GetFiles(string.Format(@"{0}\{1}", App.AppDirectory, App.SoundsDirecoryName));
                return sounds
                    .Select(sound => new FileInfo(sound))
                    .Where(file => file.Name.StartsWith(App.ReadyStandbySoundsPrefix))
                    .ToList();
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of OptionsData.
        /// </summary>
        public OptionsData()
        {
        }

        /// <summary>
        ///     Initializes a new instance of OptionsData, creating a copy of another OptionsData object.
        /// </summary>
        /// <param name="options">The OptionsData object data to copy into this new instance.</param>
        public OptionsData(OptionsData options)
        {
            CopyToInstance(options);
            CheckDataConsistency();
        }

        /// <summary>
        ///     Initializes a new instance of OptionsData, loading options from a file.
        /// </summary>
        /// <param name="xmlPath">The XML file containing the options to put into that new instance.</param>
        public OptionsData(string xmlPath)
            : this(Load(new StreamReader(xmlPath).BaseStream))
        {
        }

        /// <summary>
        ///     Initializes a new instance of OptionsData, loading options from a stream.
        /// </summary>
        /// <param name="xmlStream">The XML stream containing the options to put into that new instance.</param>
        public OptionsData(Stream xmlStream)
            : this(Load(xmlStream))
        {
        }

        #endregion

        #region Internal methods

        /// <summary>
        ///     Checks properties validity according to other related properties.
        ///     Like you wanna have that ready/standby speech but no sound file is available.
        ///     So in that case we will set the ready/standby speech option to false to
        ///     reflect the other data related to it.
        ///     This awesome method is the result of pretty much 24hours of struggling with
        ///     deserializing, and at the same time trying to fix invalid options data like
        ///     non-existing path, empty collection, etc.
        ///     The thing is: never put logic in the getter and setters. It's all shit.
        ///     So I moved all of the logic here, and it is called after the object is created.
        ///     So now it's all good.
        /// </summary>
        protected void CheckDataConsistency()
        {
            // No beep sound or file doesnt'exist
            if (String.IsNullOrEmpty(SoundSelectedBeepFile) || !File.Exists(SoundSelectedBeepFile))
            {
                // Grab first beep sound
                var firstOrDefault = AvailableBeepSounds.FirstOrDefault();
                SoundSelectedBeepFile =
                    firstOrDefault != null
                        ? firstOrDefault.FullName
                        : null;
            }

            // No ready/standby sounds? Take everything from filesystem
            if (null == SoundSelectedReadyStandbyFiles
                || (null != SoundSelectedReadyStandbyFiles
                    && 0 == SoundSelectedReadyStandbyFiles.Count))
            {
                SoundSelectedReadyStandbyFiles =
                    AvailableReadyStandbySounds
                        .Select(sound => sound.FullName)
                        .ToList();
            }

            // Cleanup the non-existing files from the ready/standby list
            SoundSelectedReadyStandbyFiles =
                SoundSelectedReadyStandbyFiles
                    .Where(File.Exists)
                    .Distinct()
                    .ToList();

            // If after all of that there is still no ready/standby sounds
            // We don't want to play no speech
            if (0 == SoundSelectedReadyStandbyFiles.Count)
                SoundPlayReadyStandby = false;

            // Zero percent loudness is possible but it makes no sense.
            // That would be like disabling loudness altogether
            if (0 >= DetectorLoudness)
                DetectorLoudness = 1;
            else if (100 < DetectorLoudness)
                DetectorLoudness = 100;

            // Check the duration
            if (0 >= DetectorDuration)
                DetectorDuration = 1;
            else if (100 < DetectorDuration)
                DetectorDuration = 100;
        }

        #endregion

        #region Public methods

        /// <summary>
        ///     Load options from a XML stream.
        /// </summary>
        /// <param name="xmlStream">The XML file stream containing the options to load.</param>
        /// <returns>A OptionData object.</returns>
        public static OptionsData Load(Stream xmlStream)
        {
            var serializer = new XmlSerializer(typeof (OptionsData));
            var loadedOptionsData = (OptionsData) serializer.Deserialize(xmlStream);
            return loadedOptionsData;
        }

        /// <summary>
        ///     Save the options to an XML file.
        /// </summary>
        /// <param name="savePath">XML file that will be created.</param>
        public void Save(string savePath)
        {
            var serializer = new XmlSerializer(typeof (OptionsData));
            using (var writer = new StreamWriter(savePath))
            {
                serializer.Serialize(writer, this);
            }
        }

        /// <summary>
        ///     CopyToInstance this instance with another OptionsData.
        /// </summary>
        /// <param name="optionsData">The options that will update that instance data.</param>
        public void CopyToInstance(OptionsData optionsData)
        {
            foreach (var property in GetType().GetProperties())
                if (0 == property.GetCustomAttributes(typeof (XmlIgnoreAttribute), false).GetLength(0))
                    property.SetValue(this, property.GetValue(optionsData, null), null);
        }

        /// <summary>
        ///     Create a copy of this OptionsData instance.
        /// </summary>
        /// <returns>A new OptionsData instance containing the same stuff.</returns>
        public OptionsData Clone()
        {
            var optionsCopy = new OptionsData(this);
            return optionsCopy;
        }

        /// <summary>
        ///     Text representation of this OptionsData object.
        /// </summary>
        /// <returns>
        ///     Hard to explain.
        ///     Only human readable delay and ready/standby sounds for now
        /// </returns>
        public override string ToString()
        {
            var rc = new StringBuilder();
            if (GeneralRandomDelay)
                rc.AppendFormat("Delay: {0}-{1}secs", GeneralRandomStartDelayMin, GeneralRandomStartDelayMax);
            else
                rc.AppendFormat("Delay: {0}secs", GeneralRandomStartDelayMin);

            rc.Append(SoundPlayReadyStandby
                ? " / Ready-standby: yes"
                : " / Ready-standby: no");

            rc.AppendFormat(" / Spikes: {0} / Loudness {1}%", DetectorDuration, DetectorLoudness);
            return rc.ToString();
        }

        #endregion
    }
}