// <copyright file="OutputParser.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace FamilyTreeParser
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Net.NetworkInformation;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Text.RegularExpressions;
    using static System.Runtime.InteropServices.JavaScript.JSType;

    internal class OutputParser
    {
        private static readonly string HuxterFamilyTreeFileOutPut = "..\\..\\..\\..\\..\\..\\HuxterFamilyTree\\HuxterFamilyTreeOUTPUT.txt";
        private static string dateConcat = string.Empty;
        private static int personNumber;
        private static bool partialDeathDate = false;
        private static string spouseFamilyNumber = string.Empty;

        public static void OutPutParser()
        {
            using (StreamReader fileReader = new StreamReader(HuxterFamilyTreeFileOutPut))
            {
                File.Create("..\\..\\..\\..\\..\\..\\HuxterFamilyTree\\HuxterFamilyTreeUnFormatted.txt").Dispose();

                try
                    {
                    const int MAXIMUM_LINES_PER_FAMILY = 4;
                    string line;
                    int linePosition = 1;
                    bool decendantsCheckNessecary = true;
                    bool personCreationStarted = false;
                    List<string> personInfo = new List<string>();
                    while ((line = fileReader.ReadLine().Trim()) != null)
                    {
                        // This tells when William the 1st appears
                        if (decendantsCheckNessecary)
                        {
                            decendantsCheckNessecary = DecendantsCheck(line);
                        }

                        // Family paragraphs are supposted to be smaller than the max, not exactily magic number 
                        if (linePosition > MAXIMUM_LINES_PER_FAMILY)
                        {
                        personCreationStarted = false;
                        linePosition = 1;
                        continue;
                        }

                        // Only time colons appear so far is when the "children of" appear. So hopefully that stays the same
                        if (LineContainsColon(line))
                        {
                            continue;
                        }

                        if (LineContainsDeathAfterContinuingFamily(line))
                        {
                            continue;
                        }

                        if (personCreationStarted || IsNewFamily(line))
                            {
                            if (ContainsFamilyNumberChildren(line))
                            {


                                personCreationStarted = true;
                                NewFamilyParser(line, linePosition, personInfo);
                                linePosition++;
                            }
                            }
                            else if (!ContainsFamilyNumberChildren(line))
                            {
                                NonContinuingChildren(line, linePosition, personInfo);
                            }

                            if (fileReader.EndOfStream)
                            {
                                break;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($" An error occurred: {e.Message}");
                    }
                }
            }

        private static bool LineContainsColon(string line)
        {
            return line.Contains(":");
        }

        private static bool LineContainsDeathAfterContinuingFamily(string line)
        {
            return line.Contains("d.");
        }

        private static bool ContainsFamilyNumberChildren(string line)
        {
            return Regex.IsMatch(line, @"^\d+\s+\S+\s+\S.*");
        }

        private static void NonContinuingChildren(string line, int linePosition, List<string> personInfo)
        {

        }

        private static List<string> NewFamilyParser(string line, int linePosition, List<string> personInfo)
        {
            switch (linePosition)
            {
                case 1:
                    PersonInfoAdderFirstLineMarriage(line, personInfo);
                    personNumber++;
                    break;
                case 2:
                    PersonInfoAdderSecondLineMarriage(line, personInfo);
                    break;
                case 3:
                    PersonInfoAdderThirdLineMarriage(line, personInfo);
                    break;
                case 4:
                    PersonInfoAdderFourthLineMarriage(line, personInfo);
                    break;
                default:
                    throw new Exception("Line Position Out Of Range For Possible Family (Possible That Family Has Become Bigger)");
            }

            return personInfo;
        }

        private static void PersonInfoAdderFirstLineMarriage(string line, List<string> personInfo)
        {
            personInfo.Add(FamilyNumber(line));
            personInfo.Add(PersonNumberCreator(line));
            personInfo.Add(FirstNameParser(line));
        }

        private static void PersonInfoAdderSecondLineMarriage(string line, List<string> personInfo)
        {
            personInfo.Add(LastNameParser(line));
            personInfo.Add(BirthYearParser(line));
            personInfo.Add(BirthPlaceParser(line));
            if (IsDeathUnknown(line))
            {
                personInfo.Add(",,");
                return;
            }
            else if (IsPartialDeathDate(line))
            {
                partialDeathDate = true;
            }
        }

        private static void PersonInfoAdderThirdLineMarriage(string line,List<string> personInfo)
        {
            string unformattedDate = PartialDeathDateConcat(line);
            personInfo.Add(DateConverter(unformattedDate));
            personInfo.Add(DeathPlaceParser(line));
            CreatePerson(personInfo);
            personInfo.Clear();
            if (HasMarriageInLineCheck(line))
            {
                personInfo.Add(spouseFamilyNumber);
                personInfo.Add(PersonNumberCreator(line));
                personInfo.Add(SpouseNameParser(line));
            }
            else
            {
                return;
            }
        }

        private static void PersonInfoAdderFourthLineMarriage(string line, List<string> personInfo)
        {
            personInfo.Add(SpouseLastNameParser(line));
            personInfo.Add(BirthYearParser(line));
            personInfo.Add(BirthPlaceParser(line));
            if (HasMarriageInformation(line))
            {
                personInfo.Add(MarriageDateParser(line));
                personInfo.Add(MarriagePlaceParser(line));
            }

            CreatePerson(personInfo);
            personInfo.Clear();
        }

        private static void CreatePerson(List<string> person)
        {
            UnFormattedCSVCreator.UnformattedCSVFileWriter(person);
        }

        private static bool HasMarriageInformation(string line)
        {
            if (line.Contains(" on "))
            {
                return true;
            }

            return false;
        }

        private static string MarriageDateParser(string line)
        {
            string[] splitLine = line.Split("on",',');
            splitLine = splitLine[1].Split(',');
            line = (splitLine[0] + splitLine[1]).Trim();
            return ',' + DateConverter(line);
        }

        private static string MarriagePlaceParser(string line)
        {
            line = line.Split("on")[1].Trim();
            return ',' + line.Split(',')[2].Trim();

        }

        private static string SpouseNameParser(string line)
        {
           return ',' + line.Split("married")[1].Trim();
        }

        private static bool HasMarriageInLineCheck(string line)
        {
            if (line.Contains("married"))
            {
                return true;
            }

            return false;
        }

        private static string PartialDeathDateConcat(string line)
        {
            string[] splitLine = line.Split(',');
            return dateConcat + ' ' + splitLine[0] + splitLine[1];
        }

        private static string DeathPlaceParser(string line)
        {
            string[] splitLine = line.Split(',');
            return ',' + splitLine[2].Trim();
        }

        private static bool IsDeathUnknown(string line)
        {
            if (line.Contains("d. Unknown"))
            {
                return true;
            }

            return false;
        }

        private static bool IsPartialDeathDate(string line)
        {
            line = line.Split("d.")[1].Trim();

            if (Regex.IsMatch(line, @"^[a-zA-Z]+\s\d+,\s\d+$") || Regex.IsMatch(line, @"^[a-zA-Z]+\s\d+"))
            {
                return false;
            }

            dateConcat = string.Empty;
            dateConcat = line;

            return true;
        }

        private static bool IsNewFamily(string line)
        {
            return HTMLParser.ContainsSupOrStrong(line);
        }

        private static int MonthConverter(string month)
        {
            month = month.ToLower();
            switch (month)
            {
                case "jan":
                    return 1;
                case "feb":
                    return 2;
                case "mar":
                    return 3;
                case "apr":
                    return 4;
                case "may":
                    return 5;
                case "jun":
                    return 6;
                case "jul":
                    return 7;
                case "aug":
                    return 8;
                case "sep":
                    return 9;
                case "oct":
                    return 10;
                case "nov":
                    return 11;
                case "dec":
                    return 12;
                default:
                    throw new ArgumentException("Invalid month abbreviation");
            }
        }

        private static string PersonNumberCreator(string line)
        {
            return ',' + "[" + 'P' + personNumber.ToString().PadLeft(4, '0') + ']';
        }

        private static string DateConverter(string line)
        {
            string[] date = line.Split(' ');
            int month = MonthConverter(date[0]);

            if (month < 1 || month > 12)
            {
                throw new ArgumentException("Invalid month");
            }
            else
            {
                if (month < 10)
                {
                    date[0] = '0' + month.ToString();
                }
                else
                {
                    date[0] = month.ToString();
                }
            }

            return date[1] + '-' + date[0] + '-' + date[2];
        }

        private static string BirthYearParser(string line)
        {
            line = line.Split("b.")[1].Trim();
            if (ContainsUnknown(line))
            {
                return SeperatorValueReturn();
            }

            return ',' + line.Split(',')[0].Trim();
        }

        private static bool ContainsUnknown(string line)
        {
            if (line.Contains("Unknown"))
            {
                return true;
            }
            return false;
        }

        private static string SeperatorValueReturn()
        {
            return ",";
        }

        private static string BirthPlaceParser(string line)
        {
            if (ContainsUnknown(line))
            {
                return SeperatorValueReturn();
            }
            line = line.Split(",", 2)[1].Trim();
            return ',' + line.Split("d.")[0].Trim();
        }

        private static bool DecendantsCheck(string line)
        {
            if (line.Contains("Descendants of"))
            {
                return false;
            }

            return true;
        }

        private static string FamilyNumber(string line)
        {
            line = line.Split("</strong>")[0].Trim();
            line = line.Split("<strong>")[1].Trim();
            int.TryParse(line, out int familyNumber);
            return FamilyNumberCreator(familyNumber);
        }

        private static string FamilyNumberCreator(int familyNumber)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(familyNumber);
            spouseFamilyNumber = "[" + 'F' + familyNumber.ToString().PadLeft(4, '0') + ']';
            return spouseFamilyNumber;
        }

        private static string FirstNameParser(string line)
        {
            return ',' + line.Split('>', '>', '<')[4].Trim();
        }

        private static string LastNameParser(string line)
            {
            return ',' + line.Split(' ')[0].Trim();
        }

        private static string SpouseLastNameParser(string line)
        {
            return ',' + line.Split(' ')[0].Trim();
        }
    }
}