using System.IO;
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

        /// <summary>Gets or sets the current selected input device ID.</summary>
        public int SelectedDeviceId { get; set; }

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
            return Load(new StreamReader(xmlPath).BaseStream);
        }

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

        /// <summary>
        /// Text representation of this OptionsData object.
        /// </summary>
        /// <returns>
        /// Hard to explain.
        /// Only human readable delay and ready/standby sounds for now
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

            return rc.ToString();
        }

        #endregion
    }
}