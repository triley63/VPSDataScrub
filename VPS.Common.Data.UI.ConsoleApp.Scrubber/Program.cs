using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace VPS.Common.Data.UI.ConsoleApp.Scrubber
{
    class Program
    {
        static void Main(string[] args)
        {
            string textLine;

            //d:\projects\vpsdatascrub\teapartymembership.txt
            //Console.Write("Name of the file to scrub: ");
            //string fileName = Console.ReadLine();

            string fileName = @"d:\projects\vpsdatascrub\teapartymembership.txt";
            List<string> data = new List<string>();

            using (StreamReader sr = new StreamReader(fileName))
            {
                while ((textLine = sr.ReadLine()) != null)
                {
                    data.Add(textLine.Trim());
                }
            }

            data = RemoveExtraLines(data);
            using (StreamWriter sw = new StreamWriter(@"d:/projects/vpsdatascrub/text1.txt"))
            {
                for (int i = 0; i < data.Count; i++)
                {
                    sw.WriteLine(data[i]);
                }
            }

            data = RemoveInvalidCharacters(data, '_');
            using (StreamWriter sw = new StreamWriter(@"d:/projects/vpsdatascrub/text2.txt"))
            {
                for (int i = 0; i < data.Count; i++)
                {
                    sw.WriteLine(data[i]);
                }
            }

            data = RemoveDuplicateWhiteSpace(data);
            using (StreamWriter sw = new StreamWriter(@"d:/projects/vpsdatascrub/text3.txt"))
            {
                for (int i = 0; i < data.Count; i++)
                {
                    sw.WriteLine(data[i]);
                }
            }

            data = SplitDuplicateContacts(data);
            using (StreamWriter sw = new StreamWriter(@"d:/projects/vpsdatascrub/text4.txt"))
            {
                for (int i = 0; i < data.Count; i++)
                {
                    sw.WriteLine(data[i]);
                }
            }

            data = FormatLastName(data);
            using (StreamWriter sw = new StreamWriter(@"d:/projects/vpsdatascrub/text5.txt"))
            {
                for (int i = 0; i < data.Count; i++)
                {
                    sw.WriteLine(data[i]);
                }
            }

            data = FormatValue(data, @"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$");
            using (StreamWriter sw = new StreamWriter(@"d:/projects/vpsdatascrub/text6.txt"))
            {
                for (int i = 0; i < data.Count; i++)
                {
                    sw.WriteLine(data[i]);
                }
            }

            //manage other tea party membership
            //manage multiple emails or phone numbers
            //eliminate and manage special characters
            //manage no phone or email listed
            //manage upper case last name with special characters after (Christena CLEREK ???????)
            //remove page number listings
            //all emails should be lowercase
            //remove all non-printable characters
            //addresses that include places (Calico Junction New Beginnings Ranch)
            
            
            Console.WriteLine("Done");
            Console.Read();
        }

        private static List<string> RemoveExtraLines(List<string> data)
        {
            List<string> blanksRemoved = new List<string>();
            string previousLine;
            string currentLine;

            for (int i = 0; i < data.Count; i++)
            {
                previousLine = i > 0 ? data[i - 1].Trim() : null;
                currentLine = data[i].Trim();

                if (previousLine != currentLine)
                    blanksRemoved.Add(data[i]);
            }

            return blanksRemoved;

        }

        private static List<string> RemoveInvalidCharacters(List<string> data, char invalidCharacter)
        {
            List<string> removed = new List<string>();

            for (int i = 0; i < data.Count; i++)
            {
                string test = new string(invalidCharacter, data[i].Trim().Length);

                if (data[i] != test)
                {
                    StringBuilder text = new StringBuilder();
                    text.Append(data[i]);
                    removed.Add(text.Replace(invalidCharacter.ToString(), string.Empty).ToString());
                }
                else
                {
                    if (data[i].Trim().Length == 0)
                        removed.Add(data[i]);
                }
            }

            return removed;
        }

        private static List<string> RemoveDuplicateWhiteSpace(List<string> data)
        {
            List<string> returnData = new List<string>();
            for (int i = 0; i < data.Count; i++)
            {
                while (data[i].Contains("  "))
                {
                    data[i] = data[i].Replace("  ", " ");
                }

                returnData.Add(data[i]);
            }

            return returnData;
        }

        private static List<string> SplitDuplicateContacts(List<string> data)
        {
            List<string> returnData = new List<string>();

            for (int i = 0; i < data.Count; i++)
            {
                if (data[i].Contains("&") && i >= 1 && data[i - 1] == string.Empty)
                {
                    string lastName = data[i].Substring(data[i].LastIndexOf(' ')).Trim();
                    string[] contacts = data[i].Substring(0, data[i].LastIndexOf(' ')).ToString().Split('&');
                    List<string> contactInfo = new List<string>();

                    for (int a = i + 1; a < data.Count; a++)
                    { 
                        if (data[a] == string.Empty)
                            break;

                        contactInfo.Add(data[a]);
                    }

                    for (int b = 0; b < contacts.Length; b++)
                        {
                            returnData.Add(string.Format("{0} {1}", contacts[b].Trim(), lastName));
                            foreach (string s in contactInfo)
                            {
                                returnData.Add(s);
                            }
                            if (b < contacts.Length - 1)
                                returnData.Add(string.Empty);
                        }

                    i = i + contactInfo.Count;
                }
                else
                {
                    returnData.Add(data[i]);
                }
            }

            return returnData;
        }

        private static List<string> FormatLastName(List<string> data)
        {
            List<string> returnData = new List<string>();

            for (int i = 1; i < data.Count; i++)
            {
                if (data[i - 1] == string.Empty)
                {
                    string firstName = data[i].Substring(0, data[i].LastIndexOf(' ')).Trim();
                    string lastName = string.Format("{0}{1}",
                        data[i].Substring(data[i].LastIndexOf(' ')).Trim().Substring(0, 1).ToUpper(),
                        data[i].Substring(data[i].LastIndexOf(' ')).Trim().Substring(1).ToLower());

                    returnData.Add(string.Format("{0} {1}", firstName, lastName));
                }
                else
                {
                    returnData.Add(data[i]);
                }

            }

            return returnData;
        }

        private static List<string> FormatValue(List<string> data, string regExpression)
        {
            List<string> returnData = new List<string>();
            Regex rgx = new Regex(regExpression);

            for (int i = 0; i < data.Count; i++)
            {
                if (rgx.Match(data[i]).Success == true)
                {
                    data[i] = data[i].Replace("(", string.Empty).Replace(")", string.Empty);
                }

                returnData.Add(data[i]);
            }

            return returnData;
        }
    }
}
