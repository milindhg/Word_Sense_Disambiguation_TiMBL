using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WSDForSenseEval
{
    public class FileData
    {
        public static string Train_File = @"..\..\EnglishLS.train";
        public static string Test_File = @"..\..\EnglishLS.test";
        public static string Key_File = @"..\..\EnglishLS.test.key";
        public static string Train_Output = @"..\..\MyTrain.txt";
        public static string Test_Output = @"..\..\MyTest.txt";
        public static string Key_Output = @"..\..\MyKey.xml";
        public static string Train_Keyword_Count = @"..\..\MyTrainCnt.txt";
        public static string Test_Keyword_Count = @"..\..\MyTestCnt.txt";
        

        public static void WriteToFile(String input, String fileName)
        {
            string path = fileName;
            // This text is added only once to the file. 
            if (!File.Exists(path))
            {
                // Create a file to write to. 
                using (StreamWriter sw = File.CreateText(path))
                {
                    sw.WriteLine(input);
                }
            }
            else
            {

                // This text is always added, making the file longer over time 
                // if it is not deleted. 
                using (StreamWriter sw = File.AppendText(path))
                {
                    sw.WriteLine(input);
                }
            }
        }

    }
}
