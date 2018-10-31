namespace Polimi.GameCollective.Connectivity {

    /// <summary>
    /// Server entries must obey this structure.
    /// </summary>
    public struct Entry {
        public string Label;
        public string Data;
        public string Comment;

        /// <summary>
        /// Initializes a new instance of the <see cref="Polimi.GameCollective.Connectivity.Entry"/>
        /// struct.
        /// </summary>
        /// <param name="label">Identifier for the entry or the experiment.</param>
        /// <param name="data">The actual data saved (anything that can be represented as a string).
        /// </param>
        /// <param name="comment">Any free comment needed.</param>
        public Entry(string label, string data, string comment) {
            this.Label = label;
            this.Data = data;
            this.Comment = comment;
        }

        public override string ToString() {
            return Label + "|" + Data + "|" + Comment;
        }

        /// <summary>
        /// Builds an Entry from a properly formatted string.
        /// </summary>
        /// <returns>The Entry object.</returns>
        /// <param name="entry">The properly formatted string.</param>
        public static Entry FromString(string entry) {
            int first = entry.IndexOf("|");
            int last = entry.LastIndexOf("|");
            string label = entry.Substring(0, first);
            string data = entry.Substring(first + 1, last - first - 1);
            string comment = entry.Substring(last + 1, entry.Length - last - 1);
            return new Entry(label, data, comment);
        }
    }

}