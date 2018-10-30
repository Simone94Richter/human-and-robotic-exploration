using System.Collections;
using UnityEngine;

namespace Polimi.GameCollective.Connectivity {

    /// <summary>
    /// Attached to a GameObject, allows to send and retrieve data from the server. Use the
    /// parameters to specify the full url to the server (including the directory), the encryption 
    /// key, the encryption initialization vector (IV) and the salt. Only one send/retrive request
    /// can be submitted at a time. Check IsRequestPending to know if you can submit a new one, and 
    /// IsResultReady for knowing when your data is ready, in case of retrieval (check them for 
    /// example in the Update() method of another component). Retrieved data is in the Result field 
    /// once it arrives. The Result field is overwritten with the empty string when a new retrieval 
    /// request is sent.
    /// </summary>
    public class RemoteDataManager : SceneSingleton<RemoteDataManager> {
        private static readonly string COMMAND_SAVE = "save|";
        private static readonly string COMMAND_LOAD_LAST = "load|last";
        private static readonly string COMMAND_LOAD_N = "load|";
        private static readonly string COMMAND_LOAD_ALL = "load|all";

        // Specify the full URL with all directories.
        private static readonly string ServerUrl = "https://data.polimigamecollective.org/mancala/";

        // 256 bit = 32 ASCII characters * 8 bits.
        private static readonly string EncryptionKey = "lkirwf897+22#bbtrm8814z5qq=498j5";

        // 256 bit = 32 ASCII characters * 8 bits.
        private static readonly string EncryptionIV = "741952hheeyy66#cs!9hjv887mxx7@8y";

        // Used for hashing purposes.
        private static readonly string ServerSalt = "DKud_64$D:ZUg=c i}d1XG|_?KkE o(S(V!U)*d c_tJNp_]>+`CT2:sd>]&W}~d";

        private ServerConnection connection;

        public bool IsRequestPending { get; private set; }

        public bool IsResultReady { get; private set; }

        public string Result { get; private set; }

        private void Awake() {
            connection = new ServerConnection(ServerUrl, ServerSalt, EncryptionKey, EncryptionIV);
        }

        /// <summary>
        /// Saves the data on the server.
        /// </summary>
        /// <param name="entry">Bundled data to save.</param>
        public void SaveData(Entry entry) {
            SaveData(entry.Label, entry.Data, entry.Comment);
        }

        /// <summary>
        /// Saves the data on the server.
        /// </summary>
        /// <param name="label">Identifier for the entry or the experiment.</param>
        /// <param name="data">The actual data saved (anything that can be represented as a string).
        /// </param>
        /// <param name="comment">Any free comment needed.</param>
        public void SaveData(string label, string data, string comment) {
            Post(COMMAND_SAVE + label + "|" + data + "|" + comment);
        }

        /// <summary>
        /// Retrieves the last entry from the server.
        /// </summary>
        public void GetLastEntry(DoneCallback callback = null) {
            Get(COMMAND_LOAD_LAST, callback);
        }

        /// <summary>
        /// Retrieves the last n entry from the server.
        /// </summary>
        /// <param name="n">How many entries to retrieve.</param>
        public void GetLastEntries(int n, DoneCallback callback = null) {
            Get(COMMAND_LOAD_N + n, callback);
        }

        /// <summary>
        /// Retrieves all entries from the server.
        /// </summary>
        public void GetAllEntries(DoneCallback callback = null) {
            Get(COMMAND_LOAD_ALL, callback);
        }

        private void Post(string message, DoneCallback callback = null) {
            // Handles only one request at a time.
            if (IsRequestPending) {
                return;
            }
            IsRequestPending = true;
            string postUrl = connection.GeneratePostURL(message);
            StartCoroutine(ContactServer(postUrl, callback));
        }

        private void Get(string message, DoneCallback callback = null) {
            // Handles only one request at a time.
            if (IsRequestPending) {
                return;
            }
            IsRequestPending = true;
            Result = "";
            string getUrl = connection.GenerateGetURL(message);
            StartCoroutine(ContactServer(getUrl, callback));
        }

        private IEnumerator ContactServer(string url, DoneCallback callback = null) {
            IsResultReady = false;
            WWW www = new WWW(url);
            yield return www;

            // Computation resumes automatically after www arrives.
            Result = www.text;
            IsResultReady = true;
            IsRequestPending = false;

            if (callback != null) {
                callback();
            }
        }

        public delegate void DoneCallback();
    }

}