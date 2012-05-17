/********************************************************************************
#                                          _                           
#                                         | |                          
#           _ __ ___ _ __   ___   ___  ___| |____      ____ _ _ __ ____
#          | '__/ _ \ '_ \ / _ \ / __|/ __| '_ \ \ /\ / / _` | '__|_  /
#          | | |  __/ | | |  __/ \__ \ (__| | | \ V  V / (_| | |   / / 
#          |_|  \___|_| |_|\___| |___/\___|_| |_|\_/\_/ \__,_|_|  /___|
#                                                            rene-schwarz.com
#
# *******************************************************************************
#           MOON SURFACE ILLUMINATION SIMULATION FRAMEWORK (MSISF)
# *******************************************************************************
#
#  Author:          M.Eng. René Schwarz 
#                       mail: <mail@rene-schwarz.com>
#                       web:  http://www.rene-schwarz.com
#                   on behalf of the German Aerospace Center (DLR)
#  Date:            2012/05/17
#  Filename:        /src/MSIS/Program.cs
#  License:         GNU General Public License (GPL), version 2 or later
#
# *******************************************************************************
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using System.Threading;

namespace MSIS
{
    class Program
    {
        public static string VERSION = new DateTime(2012,05,17).ToString(@"yyyy/MM/dd", DateTimeFormatInfo.InvariantInfo);
        public static bool batchmode = false;
        public static Simulation sim = new Simulation();
        public static bool checkTime = false;
        public static bool checkTimeT0 = false;
        public static bool checkOrbit = false;
        public static bool batchSet = false;
        public static bool ignoreSun = false;
        public static string scientificFormat = "+0.000000000000000E+000;-0.000000000000000E+000";
        public static bool writeAnnotation = false;
        public static string commandLineString = "";

        static void Main(string[] args)
        {
            try
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");
                foreach (string arg in args)
                {
                    commandLineString += arg + " ";
                }
                commandLineString = commandLineString.Trim();
                tools.output_header();
                tools.parse_arguments(args);
                sim.checkConfig();
            }
            catch (Exception e)
            {
                if (e.Message == "")
                {
                    tools.display_help();
                }
                else
                {
                    Console.WriteLine("ERROR:");
                    Console.WriteLine(e.Message);
                    Console.WriteLine();
                    Console.WriteLine("Display help to your support:\n");
                    tools.display_help();
                }
                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey();
                return;
            }

            tools.output_configuration_overview();

            try
            {
                sim.doCalculations();
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR:");
                Console.WriteLine(e.Message);
            }

            if (!batchmode)
            {
                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey();
            }
            return;
        }
    }

    public class PixelInformation
    {
        public bool exists = false;
        public double lat = 0;
        public double lon = 0;
        public double IlluminationAngle = 0;
    }
}
