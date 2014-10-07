using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;

namespace PCShotTimer
{
    /// <summary>
    ///     Container for the options.
    /// </summary>
    [XmlRoot("Options")]
    public class OptionsData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

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

        /// <summary>Default sample rate for the mic input.</summary>
        [XmlElement("InputSampleRate")]
        public int InputSampleRate { get; set; }

        /// <summary>Default sample bits for the mic input.</summary>
        [XmlElement("InputSampleBits")]
        public int InputSampleBits { get; set; }

        /// <summary>Default channel number for the mic input (stereo/mono).</summary>
        [XmlElement("InputChannels")]
        public int InputChannels { get; set; }

        /// <summary>
        ///     This is actually the number of time (or samples) a high audio spike is detected before being counted as a 'shot'.
        ///     So the lower, the more sensitive it is.
        /// </summary>
        [XmlElement("DetectorSensitivity")]
        public int DetectorSensitivity { get; set; }

        /// <summary>Defines if there should be a speech before the beep.</summary>
        [XmlElement("SoundPlayReadyStandby")]
        public bool SoundPlayReadyStandby { get; set; }

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
            var options = Load(xmlPath);
            Update(options);
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
        ///     Load options from a XML file.
        /// </summary>
        /// <param name="xmlPath">The XML file containing the options to load.</param>
        /// <returns>A OptionData object.</returns>
        public static OptionsData Load(string xmlPath)
        {
            OptionsData loadedOptionsData;
            var serializer = new XmlSerializer(typeof (OptionsData));
            using (var reader = new StreamReader(xmlPath))
            {
                loadedOptionsData = (OptionsData) serializer.Deserialize(reader);
            }
            return loadedOptionsData;
        }

        /// <summary>
        ///     Update this instance with another OptionsData.
        /// </summary>
        /// <param name="optionsData">The options that will update that instance data.</param>
        /// <returns>The given options.</returns>
        public OptionsData Update(OptionsData optionsData)
        {
            foreach (var property in GetType().GetProperties())
                if (property.GetCustomAttributes(typeof (XmlIgnoreAttribute), false).GetLength(0) == 0)
                    property.SetValue(this, property.GetValue(optionsData, null), null);
            return optionsData;
        }

        /// <summary>
        ///     Create a copy of this OptionsData instance.
        /// </summary>
        /// <returns>A new OptionsData instance containing the same stuff.</returns>
        public OptionsData Clone()
        {
            var optionsCopy = new OptionsData();
            optionsCopy.Update(this);
            return optionsCopy;
        }

        #endregion
    }
}