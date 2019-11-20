using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using System.Xml;

namespace M06_UF1_PersistenciaFitxers
{
    abstract class AbstractTool
    {
        /// <summary>
        /// Returns the name of a file given.
        /// </summary>
        /// <param name="file">String: File to analyze</param>
        /// <returns></returns>
        public static string GetFileName(string file)
        {
            return Path.GetFileNameWithoutExtension(file);
        }


        /// <summary>
        /// Returns the extension of a file given.
        /// </summary>
        /// <param name="file">String: File to analyze</param>
        /// <returns></returns>
        public static string GetFileExtension(string file)
        {
            return Path.GetExtension(file);
        }


        /// <summary>
        /// Returns the creation date of a file given.
        /// </summary>
        /// <param name="file">String: File to analyze</param>
        /// <returns></returns>
        public static string GetCreationDate(string file)
        {
            DateTime creation = File.GetCreationTime(file);
            return creation.ToString("dd/MM/yy HH:mm:ss");
        }


        /// <summary>
        /// Returns the last modification date of a file given.
        /// </summary>
        /// <param name="file">String: File to analyze</param>
        /// <returns></returns>
        public static string GetLastModificationDate(string file)
        {
            DateTime lastModified = File.GetLastWriteTime(file);
            return lastModified.ToString("dd/MM/yy HH:mm:ss");
        }


        /// <summary>
        /// Counts how many words has a file given excluding some delimiters.
        /// </summary>
        /// <param name="sr">StreamReader: File to analyze</param>
        public static int GetWordsInAFile(StreamReader sr)
        {
            int wordsCounter = 0;
            string delimiters = " ,.;:¿?¡!";
            string[] fields = null;
            string lines = null;

            while (!sr.EndOfStream)
            {
                lines = sr.ReadLine();
                lines.Trim();
                fields = lines.Split(delimiters.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                wordsCounter += fields.Length; 
            }

            return wordsCounter;
        }


        /// <summary>
        /// Reads the content of a file and returns the 5 most repeated words in order to detect the theme of the file.
        /// </summary>
        /// <param name="file">String: Path of the file to analyze</param>
        /// <param name="filename">String: Name of the file given</param>
        /// <returns></returns>
        public static string GetFileTheme(string file, string filename)
        {
            Dictionary<string, int> dictionary = new Dictionary<string, int>();
            string delimiters = " ,.;:¿?¡!'";
            string[] fields = null;
            string lines = null;
            StreamReader sr = new StreamReader(file);
            string invalidWordsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "AbstractTool", "InvalidWords.txt");
            string excludingWords = File.ReadAllText(invalidWordsPath);

            while (!sr.EndOfStream)
            {
                lines = sr.ReadLine();
                lines.Trim();
                fields = lines.Split(delimiters.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < fields.Length; i++)
                {
                    fields[i] = fields[i].ToLower();

                    if (dictionary.ContainsKey(fields[i]))
                    {
                        dictionary[fields[i]]++;
                    }
                    else
                    {

                        if(!excludingWords.Contains(fields[i]))
                        {
                            dictionary[fields[i]] = 1;
                        }
                    }
                }
            }

            // Uncomment this in order to show the occurrences of the words
            //foreach (KeyValuePair<string, int> kvp in dictionary)
            //{
            //    Console.WriteLine("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
            //}
            //Console.WriteLine();

            var MaxValuesDictionary = (from entry in dictionary orderby entry.Value descending select entry)
               .ToDictionary(pair => pair.Key, pair => pair.Value).Take(5);

            string moreRepeatedWords = "";
            int counterRepeatedWords = 1;
            foreach(KeyValuePair<string, int> max in MaxValuesDictionary)
            {
                if(counterRepeatedWords == 5)
                {
                    moreRepeatedWords += max.Key + ".";
                }
                else
                {
                    moreRepeatedWords += max.Key + ", ";
                }
                counterRepeatedWords++;
            }

            CreateXMLWords(dictionary, filename);

            return moreRepeatedWords;
        }

        /// <summary>
        /// Creates a file named as the original + "_info" where it saves the information of the file.
        /// </summary>
        /// <param name="fileName">String: Name of the file obtained by GetFileName()</param>
        /// <param name="fileExtension">String: Extension of the file obtained by GetFileExtension()</param>
        /// <param name="creationDate">String: Creation date of the file obtained by GetCreationDate()</param>
        /// <param name="modificationDate">String: Last modification date of a file obtained by GetLastModificationDate</param>
        /// <param name="words">int: Number of words of a file obtained by GetWordsInAFile()</param>
        /// <param name="theme">String: Top 5 words more repeated in a file obtained by GetFileTheme()</param>
        public static void SaveInfoInAFile(string fileName, string fileExtension, string creationDate, string modificationDate, int words, string theme)
        {
            string infoFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "AbstractTool", fileName + "_info.txt");
            string infoFileContent = "Information of the file " + fileName + fileExtension + Environment.NewLine + Environment.NewLine +"Name: " + fileName + Environment.NewLine + "Extension: " + fileExtension + Environment.NewLine + "Creation: " + creationDate +
                Environment.NewLine + "Last Modification: " + modificationDate + Environment.NewLine + "Words: " + words + Environment.NewLine + "Theme: " + theme;
            File.WriteAllText(infoFilePath, infoFileContent);
        }


        /// <summary>
        /// Creates a XML document to save all the words from a file and its occurrences.
        /// </summary>
        /// <param name="dictionary">Dictionary: Container of the words from a file and its occurrences</param>
        /// <param name="filename">String: Name of the file to analyze</param>
        public static void CreateXMLWords(Dictionary<string, int> dictionary, string filename)
        {
            XmlDocument xml = new XmlDocument();
            XmlNode rootNode = xml.CreateElement("words");
            xml.AppendChild(rootNode);

            foreach (KeyValuePair<string, int> entry in dictionary)
            {
                XmlNode userNode = xml.CreateElement(entry.Key);
                XmlAttribute attribute = xml.CreateAttribute("occurrences");
                attribute.Value = entry.Value.ToString();
                userNode.Attributes.Append(attribute);
                userNode.InnerText = entry.Key;
                rootNode.AppendChild(userNode);
            }

            string xmlName = filename + "_words.xml";
            string xmlPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "AbstractTool", xmlName);

            xml.Save(xmlPath);
        }


        /// <summary>
        /// Checks wheter the file requested exists or not. In case it does, the function provides the 
        /// information of the file (name, extension, last modification date, number of words and the theme of the file).
        /// </summary>
        /// <param name="filename">Name of the file to analyze with the tool</param>
        public static void FileInformation(string filename)
        {
            string file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "AbstractTool", filename);

            if(File.Exists(file))
            {
                using(StreamReader sr = new StreamReader(file))
                {
                    string fileName = GetFileName(filename);
                    string fileExtension = GetFileExtension(filename);
                    string creationDate = GetCreationDate(file);
                    string modificationDate = GetLastModificationDate(file);
                    int words = GetWordsInAFile(sr);
                    string theme = GetFileTheme(file, fileName);

                    SaveInfoInAFile(fileName, fileExtension, creationDate, modificationDate, words, theme);

                    Console.WriteLine("Name: " + fileName);
                    Console.WriteLine("Extension: " + fileExtension);
                    Console.WriteLine("Creation: " + creationDate);
                    Console.WriteLine("Last Modification: " + modificationDate);
                    Console.WriteLine("Words: " + words);
                    Console.WriteLine("Theme: " + theme);
                    Console.WriteLine(Environment.NewLine + Environment.NewLine);
                    Console.WriteLine("Information saved on a file named " + fileName + "_info" + fileExtension);
                    Console.WriteLine("All the occurences of the words have been saved in a XML document named " + fileName + "_words.xml" + Environment.NewLine);
                    Console.ReadLine();
                }
            } else if(!File.Exists(file))
            {
                Console.WriteLine("Cannot find the file or it doesn't exist.");
                Console.ReadLine();
            } else
            {
                Console.WriteLine("Unknown error!");
                Console.ReadLine();
            }
        }


        /// <summary>
        /// Checks if the folder "AbstractTool" exists or not. If doesn't exist, it is created by the application.
        /// </summary>
        public static bool FileChecker()
        {
            string folderAbstractTool = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "AbstractTool");

            if(Directory.Exists(folderAbstractTool))
            {
                Console.WriteLine("Folder AbstractTool already exists on your desktop.");
                Console.ReadLine();
                return true;
            } else if (!Directory.Exists(folderAbstractTool))
            {
                Directory.CreateDirectory(folderAbstractTool);
                Console.WriteLine("AbstractTool folder has been created on your desktop. Use it in order to save files that you will like to analyze.");
                Console.ReadLine();
                return false;
            } else
            {
                Console.WriteLine("Unknown error!");
                Console.ReadLine();
                return false;
            }
        }
    }
}
