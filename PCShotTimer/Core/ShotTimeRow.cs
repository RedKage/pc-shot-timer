namespace PCShotTimer.Core
{
    /// <summary>
    ///     Represents the data contained in a row from the big ass ListView showing the shot times, etc.
    /// </summary>
    public class ShotTimeRow
    {
        /// <summary>Gets or sets that shot ID (0.0).</summary>
        public string Id { get; set; }

        /// <summary>Gets or sets that shot time (00:00:000)</summary>
        public string Time { get; set; }

        /// <summary>Gets or sets that shot split time (00:00:000)</summary>
        public string Split { get; set; }
    }
}