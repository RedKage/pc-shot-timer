using PropertyChanged;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace PCShotTimer.Core
{
    /// <summary>
    ///     Container for the options.
    /// </summary>
    [XmlRoot("Options")]
    [ImplementPropertyChanged]
    public class OptionsData
    {
        #region Members

        /// <summary>
        ///     I need to know when I'm going inside the getters since
        ///     XmlSerializer does not call the setter for a collection.
        ///     Instead it calls the getter, and then adds items to the collection returned.
        ///     Sux donkey ballz eh?
        /// </summary>
        private bool _isDeserializing = true;

        private bool _soundPlayReadyStandby;

        /// <summary>Current selected BEEP sound file.</summary>
        private string _soundSelectedBeepFile;

        /// <summary>Current selected Ready/Standby sound files.</summary>
        private List<string> _soundSelectedReadyStandbyFiles = new List<string>();

        #endregion

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
        ///     Gets or sets the DetectorSensitivity which is actually the number of time (or samples)
        ///     a high audio spike is detected before being counted as a 'shot'.
        ///     So the lower, the more sensitive it is.
        /// </summary>
        [XmlElement("DetectorSensitivity")]
        public int DetectorSensitivity { get; set; }

        /// <summary>
        ///     Gets or sets the value of the spike to reach.
        ///     The louder the higher the number.
        ///     Here this is a percent value of the maximum input loudness.
        /// </summary>
        [XmlElement("DetectorLoudness")]
        public int DetectorLoudness { get; set; }

        /// <summary>Gets or sets whether there should be a speech before the beep.</summary>
        [XmlElement("SoundPlayReadyStandby")]
        public bool SoundPlayReadyStandby
        {
            get { return _soundPlayReadyStandby; }
            set
            {
                _soundPlayReadyStandby = value;
                if (!_soundPlayReadyStandby)
                    return;

                // Yeah that was a tough one:
                // Querying the collection triggers the GET which will return null since it doesn't fully exist yet.
                // So will rebuilding the stuff which fucks up the deserialization process I guess,
                // as the deserialization dude is building that property at the same time or something.
                if (_isDeserializing)
                    return;
                
                // If activated we check we have some sounds selected
                if (null == SoundSelectedReadyStandbyFiles || 0 == SoundSelectedReadyStandbyFiles.Count)
                {
                    // And if not we select all of the available ones
                    RebuildingSoundSelectedReadyStandbyFiles();
                }
            }
        }

        /// <summary>
        ///     Gets or sets the selected Beep sound.
        ///     If the sounds doesn't exist, or is null or empty, a new one will be
        ///     picked from the sound folder (the 1st one available)
        /// </summary>
        [XmlElement("SoundSelectedBeepFile")]
        public string SoundSelectedBeepFile
        {
            get
            {
                // Check if not empty
                if (!String.IsNullOrEmpty(_soundSelectedBeepFile)
                    && File.Exists(_soundSelectedBeepFile)) // Check if exist
                    return _soundSelectedBeepFile;

                // Something went not okay above, so we just grab from the available system file.
                // This can happen if the config file is new or fucked up.
                App.Info("Rebuilding SoundSelectedBeepFile");
                var firstOrDefault = AvailableBeepSounds.FirstOrDefault();
                _soundSelectedBeepFile =
                    firstOrDefault != null
                        ? firstOrDefault.FullName
                        : null;

                return _soundSelectedBeepFile;
            }
            set { _soundSelectedBeepFile = value; }
        }

        /// <summary>
        ///     Gets or sets the selected Ready/Standby sounds.
        ///     During deserialization will return a reference on a null collection.
        ///     It's to prevent the deserialization process to access the GET behavior fully.
        ///     Which is to select all the available sounds when something goes wrong,
        ///     like the collection is null, and cleaning up non-existing files.
        /// </summary>
        [XmlElement("SoundSelectedReadyStandbyFiles")]
        public List<string> SoundSelectedReadyStandbyFiles
        {
            get
            {
                // XmlSerializer does not call the setter for a collection.
                // Instead it calls the getter, and then adds items to the collection returned.
                // Sux donkey ballz eh?
                // Which means that it will call this get here which will have a null value and then
                // it will rebuild itself, losing all the SAVED data in the process goddamnit.
                if (_isDeserializing)
                    return _soundSelectedReadyStandbyFiles;

                // Check if not empty
                if (null != _soundSelectedReadyStandbyFiles)
                {
                    // Filter only existing files
                    _soundSelectedReadyStandbyFiles = _soundSelectedReadyStandbyFiles
                        .Where(File.Exists)
                        .Distinct()
                        .ToList();

                    return _soundSelectedReadyStandbyFiles;
                }

                // Something went not okay above, so we just grab from the available system file.
                // This can happen if the config file is new or fucked up.
                RebuildingSoundSelectedReadyStandbyFiles();

                return _soundSelectedReadyStandbyFiles;
            }
            set { _soundSelectedReadyStandbyFiles = value; }
        }

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
            Update(options);
        }

        /// <summary>
        ///     Initializes a new instance of OptionsData, loading options from a file.
        /// </summary>
        /// <param name="xmlPath">The XML file containing the options to put into that new instance.</param>
        public OptionsData(string xmlPath)
        {
            var options = Load(new StreamReader(xmlPath).BaseStream);
            Update(options);
        }

        /// <summary>
        ///     Initializes a new instance of OptionsData, loading options from a stream.
        /// </summary>
        /// <param name="xmlStream">The XML stream containing the options to put into that new instance.</param>
        public OptionsData(Stream xmlStream)
        {
            var options = Load(xmlStream);
            Update(options);
        }

        #endregion

        #region Internal methods

        /// <summary>
        ///     Load options from a XML stream.
        ///     Yeah it's not a static method like you'd want.
        ///     It's because I need to know when the deserialization process occurs
        ///     in order to protect my collection properties:
        ///     XmlSerializer does not call the setter for a collection.
        ///     Instead it calls the getter, and then adds items to the collection returned.
        ///     Sux donkey ballz eh?
        ///     Which means that it will call some getters that will have a null value and then
        ///     it will rebuild itself.
        ///     I can't let that happen right, I worked my ass off to
        ///     create a SAVED configuation...
        /// </summary>
        /// <param name="xmlStream">The XML file stream containing the options to load.</param>
        /// <returns>A OptionData object.</returns>
        protected OptionsData Load(Stream xmlStream)
        {
            var serializer = new XmlSerializer(typeof (OptionsData));
            var loadedOptionsData = (OptionsData) serializer.Deserialize(xmlStream);
            _isDeserializing = false;
            return loadedOptionsData;
        }

        /// <summary>
        ///     This adds to the SoundSelectedReadyStandbyFiles properties all the
        ///     available sounds file for the Ready/Standby speeches.
        /// </summary>
        protected void RebuildingSoundSelectedReadyStandbyFiles()
        {
            App.Debug("Rebuilding SoundSelectedReadyStandbyFiles");
            _soundSelectedReadyStandbyFiles =
                AvailableReadyStandbySounds
                    .Select(sound => sound.FullName)
                    .ToList();
        }

        #endregion

        #region Public methods

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
        ///     Update this instance with another OptionsData.
        /// </summary>
        /// <param name="optionsData">The options that will update that instance data.</param>
        /// <returns>The given options.</returns>
        public OptionsData Update(OptionsData optionsData)
        {
            foreach (var property in GetType().GetProperties())
                if (0 == property.GetCustomAttributes(typeof (XmlIgnoreAttribute), false).GetLength(0))
                    property.SetValue(this, property.GetValue(optionsData, null), null);

            _isDeserializing = false;
            return optionsData;
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

            rc.AppendFormat(" / Spikes: {0} / Loudness {1}%", DetectorSensitivity, DetectorLoudness);
            return rc.ToString();
        }

        #endregion
    }
}