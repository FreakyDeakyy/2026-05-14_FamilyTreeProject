namespace FamilyTreeParser
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    internal class UnFormattedCSVCreator
    {
        public static void UnformattedCSVFileWriter(List<string> personInfo)
        {
            using (StreamWriter fileWriter = new StreamWriter("..\\..\\..\\..\\..\\..\\HuxterFamilyTree\\HuxterFamilyTreeUnFormatted.txt", true))
            {
                for (int i = 0; i < personInfo.Count(); i++)
                {
                    fileWriter.Write(personInfo[i]);
                }

                fileWriter.WriteLine();
            }
        }
    }
}
