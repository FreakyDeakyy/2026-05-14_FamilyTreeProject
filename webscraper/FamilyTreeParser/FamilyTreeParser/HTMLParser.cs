// <copyright file="HTMLParser.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace FamilyTreeParser
{
    using System.IO.Pipelines;
    using System.Runtime.CompilerServices;
    using System.Text.RegularExpressions;

    internal class HTMLParser
    {
        private static readonly string HuxterFamilyTreeFileInput = "..\\..\\..\\..\\..\\..\\HuxterFamilyTree\\HuxterFamilyTree.Html";
        private static readonly string HuxterFamilyTreeFileOutPut = "..\\..\\..\\..\\..\\..\\HuxterFamilyTree\\HuxterFamilyTreeOUTPUT.txt";

        public static async Task Main()
        {
            await Task.Run(() => HtmlFileParser());
        }

        public static bool ContainsSupOrStrong(string line)
        {
            ArgumentNullException.ThrowIfNull(line);
            if (line.Contains("<sup>") || line.Contains("<strong>"))
            {
                return true;
            }

            return false;
        }

        private static async Task HtmlFileParser()
        {
            using (StreamReader fileReader = new StreamReader(HuxterFamilyTreeFileInput))
            {
                using (StreamWriter fileWriter = new StreamWriter(HuxterFamilyTreeFileOutPut))
                {
                    string? line;
                    bool startOfTag = false;
                    try
                    {
                        while ((line = fileReader.ReadLine()?.Trim()) != null)
                        {
                            if (ContainsSupOrStrong(line))
                            {
                                fileWriter.WriteLine(line);
                                continue;
                            }

                            if (StartOfTagChecker(line))
                            {
                                startOfTag = true;
                            }
                            else if (EndOfTagChecker(line))
                            {
                                startOfTag = false;
                                continue;
                            }

                            if (startOfTag)
                            {
                                continue;
                            }

                            if (GenerationChecker(line))
                            {
                                fileWriter.WriteLine(line);
                                fileWriter.WriteLine();
                                continue;
                            }

                            fileWriter.WriteLine(line);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"An error occurred: {e.Message}");
                    }
                }
            }

            OutputParser.OutPutParser();
        }

        private static bool StartOfTagChecker(string line)
        {
            ArgumentNullException.ThrowIfNull(line);
            if (line.Contains('<'))
            {
                return true;
            }

            return false;
        }

        private static bool EndOfTagChecker(string line)
        {
            ArgumentNullException.ThrowIfNull(line);

            if (line.Contains('>'))
            {
                return true;
            }

            return false;
        }

        private static bool GenerationChecker(string line)
        {
            if (line.Contains("Generation"))
            {
                return true;
            }

            return false;
        }
    }
}