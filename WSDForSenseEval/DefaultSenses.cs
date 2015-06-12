using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WSDForSenseEval
{
    public class DefaultSenses
    {
        Dictionary<string, string> SenseToDefaultMapping = new Dictionary<string, string>();

        public void GetDefaultSensesOfWord()
        {
            string[] lines = File.ReadAllLines(@"..\..\EnglishLS.train.key");

            Dictionary<string, int> senses = new Dictionary<string, int>();
            Dictionary<string, List<string>> senseDict = new Dictionary<string, List<string>>();
            if (File.Exists(@"..\..\DefaultSense.txt"))
            {
                File.Delete(@"..\..\DefaultSense.txt");
            }

            string prevWord = string.Empty;
            foreach (var line in lines)
            {
                string[] words = line.Split(' ');

                if (senseDict.ContainsKey(words[0]))
                {
                    for (int i = 2; i < words.Length; i++)
                    {
                        if (senses.ContainsKey(words[i]))
                            senses[words[i]] = senses[words[i]] + 1;
                        else
                        {
                            senses.Add(words[i], 1);
                            senseDict[words[0]].Add(words[i]);
                        }

                    }
                }
                else
                {
                    if (!string.Equals(prevWord, string.Empty))
                    {
                        GetMaxCountSenseId(senses, prevWord);
                        senses = new Dictionary<string, int>();
                    }
                    prevWord = words[0];
                    List<string> tempList = new List<string>();

                    for (int i = 2; i < words.Length; i++)
                    {
                        tempList.Add(words[i]);
                        if (senses.ContainsKey(words[i]))
                            senses[words[i]] = senses[words[i]] + 1;
                        else senses.Add(words[i], 1);
                    }
                    senseDict.Add(words[0], tempList);
                    

                }

            }
            GetMaxCountSenseId(senses, prevWord);
            senses = new Dictionary<string, int>();

        }

        public void GetMaxCountSenseId(Dictionary<string, int> senses, string word)
        {
            string defaultSenseFile = @"..\..\DefaultSense.txt";

            List<string> senseList = senses.Keys.ToList();
            int max = -1;
            string senseId = string.Empty;
            foreach (var item1 in senseList)
            {
                int cnt = senses[item1];
                if (cnt > max)
                {
                    max = cnt;
                    senseId = item1;
                }
            }
            FileData.WriteToFile(word + " : " + senseId, defaultSenseFile);

        }
    }

}
