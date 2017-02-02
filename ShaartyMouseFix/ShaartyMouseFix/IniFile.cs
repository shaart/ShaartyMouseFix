// Thanks to: "C#, способы хранения настроек программы" https://habrahabr.ru/post/271483/
using System;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace ShaartyMouseFix
{
    /// <summary>
    /// Work with configuration files (.ini)
    /// </summary>
    class IniFile
    {
        private string _path; // ini-file name

        public string Path { get { return _path; } }

        // Import kernel32.dll
        [DllImport("kernel32")] 
        static extern long WritePrivateProfileString(string Section, string Key, string Value, string FilePath);
        [DllImport("kernel32")]
        static extern int GetPrivateProfileString(string Section, string Key, string Default, StringBuilder RetVal, int Size, string FilePath);
        
        /// <summary>
        /// Constructor. Writes file path and name
        /// </summary>
        /// <param name="IniPath">Path to configuration ini-file</param>
        public IniFile(string IniPath)
        {
            _path = new FileInfo(IniPath).FullName.ToString();
        }
        
        /// <summary>
        /// Read Key's Value from Section of ini-file
        /// </summary>
        /// <param name="Section">Section of seeking key</param>
        /// <param name="Key">What we are seeking for</param>
        /// <returns>Found Value</returns>
        public string ReadINI(string Section, string Key)
        {
            var RetVal = new StringBuilder(255);
            GetPrivateProfileString(Section, Key, "", RetVal, 255, _path);
            return RetVal.ToString();
        }

        /// <summary>
        /// Write to Section of ini-file "Key=Value" pair
        /// </summary>
        /// <param name="Section">Section of writing key</param>
        /// <param name="Key">Key writing to file</param>
        /// <param name="Value">Value writing to file</param>
        public void Write(string Section, string Key, string Value)
        {
            WritePrivateProfileString(Section, Key, Value, _path);
        }

        /// <summary>
        /// Delete key from Section
        /// </summary>
        /// <param name="Key">Deleting Key</param>
        /// <param name="Section">Section in which seek the key</param>
        public void DeleteKey(string Key, string Section = null)
        {
            Write(Section, Key, null);
        }

        /// <summary>
        /// Delete section from ini-file
        /// </summary>
        /// <param name="Section">Deleting Section</param>
        public void DeleteSection(string Section = null)
        {
            Write(Section, null, null);
        }

        /// <summary>
        /// Check key existance
        /// </summary>
        /// <param name="Key">Seeking Key</param>
        /// <param name="Section">Section of Key</param>
        /// <returns></returns>
        public bool KeyExists(string Key, string Section = null)
        {
            return ReadINI(Section, Key).Length > 0;
        }
    }
}
