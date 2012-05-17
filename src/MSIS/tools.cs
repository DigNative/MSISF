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
#  Filename:        /src/MSIS/tools.cs
#  License:         GNU General Public License (GPL), version 2 or later
#
# *******************************************************************************
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace MSIS
{
    static class tools
    {
        [DllImport("../lib/SPICEhelper.dll")]
        static extern void getSPICEVersion(StringBuilder version, ref int length);

        public static DateTime MJDtoUTC(double MJD)
        {
            decimal JD = Convert.ToDecimal(MJD) + 2400000.5m;
            decimal T = JD + 0.5m;
            decimal Z = Decimal.Truncate(T);
            decimal F = T - Z;
            decimal A;

            if (Z < 2299161m)
            {
                A = Z;
            }
            else
            {
                decimal alpha = Decimal.Truncate((Z - 1867216.25m) / 36524.25m);
                A = Z + 1 + alpha - Decimal.Truncate(alpha / 4);
            }

            decimal B = A + 1524m;
            decimal C = Decimal.Truncate((B - 122.1m) / 365.25m);
            decimal D = Decimal.Truncate(365.25m * C);
            decimal E = Decimal.Truncate((B - D) / 30.6001m);
            int day = Convert.ToInt32(Decimal.Truncate(B - D - Decimal.Truncate(30.6001m * E) + F));
            int month;

            if (E < 14m)
            {
                month = Convert.ToInt32(E - 1m);
            }
            else if (E == 14m || E == 15m)
            {
                month = Convert.ToInt32(E - 13m);
            }
            else
            {
                month = 0;
            }

            int year;

            if (month > 2m)
            {
                year = Convert.ToInt32(C - 4716m);
            }
            else if (month == 1m || month == 2m)
            {
                year = Convert.ToInt32(C - 4715m);
            }
            else
            {
                year = 0;
            }

            decimal h = F * 24m;
            int hour = Convert.ToInt32(Decimal.Truncate(h));
            decimal m = (h - Decimal.Truncate(h)) * 60m;
            int minute = Convert.ToInt32(Decimal.Truncate(m));
            decimal s = (m - Decimal.Truncate(m)) * 60m;
            int second = Convert.ToInt32(Decimal.Truncate(s));

            return new DateTime(year, month, day, hour, minute, second);
        }

        public static bool parse_numeric_list(string list, ArrayList result, int length = 0)
        {
            Regex rx = new Regex(@"^\{([0-9.,Ee\-\+]{1,})\}$");
            Match res = rx.Match(list);

            int i = 0;
            if (res.Success)
            {
                bool finished = false;
                list = res.Groups[1].ToString();
                while (!finished)
                {
                    if (list == "")
                    {
                        finished = true;
                        break;
                    }
                    else
                    {
                        if (length > 0 && i >= length)
                        {
                            return false;
                        }
                        string substr = "";
                        int pos = list.IndexOf(",");
                        if (pos == -1)
                        {
                            // no comma exists
                            substr = list;
                            list = "";
                        }
                        else
                        {
                            // comma exists
                            substr = list.Substring(0, pos);
                            list = list.Substring(pos + 1);
                        }

                        Regex rx2 = new Regex(@"^[\-\+]{0,1}[0-9]{1,}[.]{0,1}[0-9]{0,}[Ee]{0,1}[\-\+]{0,1}[0-9]{0,}$");
                        if (rx2.IsMatch(substr))
                        {
                            result.Add(Double.Parse(substr));
                            i++;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                return false;
            }

            if (length > 0 && i != length)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static string getNextArg(string[] args, uint i)
        {
            if (args.Length <= i + 1)
            {
                return "";
            }
            else
            {
                return args[i + 1];
            }
        }

        public static void parse_arguments(string[] args)
        {
            bool[] argOK = new bool[args.Length];
            for (int i = 0; i < argOK.Length; i++)
            {
                argOK[i] = false;
            }

            for (uint i = 0; i < args.Length; i++)
            {
                if (args[i] == "--help"
                    || args[i] == "/?")
                {
                    /* help was requested */
                    throw new Exception("");
                }
                else if (args[i] == "--time"
                        || args[i] == "-t")
                {
                    /* one single simulation time shall be set */
                    string arg = getNextArg(args, i);

                    Regex rx = new Regex("^[0-9]{1,}[.]{0,1}[0-9]{0,}$");
                    if (rx.IsMatch(arg) == true)
                    {
                        Program.sim.setSimulationTime(Double.Parse(arg));
                        argOK[i] = true;
                        argOK[i + 1] = true;
                        Program.checkTime = true;
                    }
                    else
                    {
                        throw new Exception("Specified simulation time is invalid.");
                    }
                }
                else if (args[i] == "--times"
                        || args[i] == "-tt")
                {
                    /* multiple simulation times shall be set */
                    string arg = getNextArg(args, i);
                    ArrayList simulation_times = new ArrayList();

                    if(parse_numeric_list(arg, simulation_times))
                    {
                        Program.sim.setSimulationTime(simulation_times);
                        argOK[i] = true;
                        argOK[i + 1] = true;
                        Program.checkTime = true;
                    }
                    else
                    {
                        throw new Exception("Syntax error using multiple simulation times.");
                    }

                }
                else if (args[i] == "--time-interval")
                {
                    /* a time interval for simulation shall be set */
                }
                else if (args[i] == "--epoch"
                        || args[i] == "-e")
                {
                    /* an epoch of validity shall be set */
                    string arg = getNextArg(args, i);

                    Regex rx = new Regex("^[0-9]{1,}[.]{0,1}[0-9]{0,}$");
                    if (rx.IsMatch(arg) == true)
                    {
                        Program.sim.spacecraft.setEpoch(Double.Parse(arg));
                        argOK[i] = true;
                        argOK[i + 1] = true;
                        Program.checkTimeT0 = true;
                    }
                    else
                    {
                        throw new Exception("Specified epoch t_0 is invalid.");
                    }
                }
                else if (args[i] == "--kepler-set"
                        || args[i] == "-k")
                {
                    /* a set of KEPLERian orbit elements shall be set */
                    string arg = getNextArg(args, i);
                    ArrayList kepler_set = new ArrayList();

                    if (Program.checkOrbit)
                    {
                        throw new Exception("S/C orbit has been given before (probably by using a set of KEPLERian elements).");
                    }

                    if (tools.parse_numeric_list(arg, kepler_set, 6))
                    {
                        Program.sim.spacecraft.setKeplerOrbitParameters(Convert.ToDouble(kepler_set[0]),
                                                                        Convert.ToDouble(kepler_set[1]),
                                                                        Convert.ToDouble(kepler_set[2]),
                                                                        Convert.ToDouble(kepler_set[3]),
                                                                        Convert.ToDouble(kepler_set[4]),
                                                                        Convert.ToDouble(kepler_set[5]));
                        argOK[i] = true;
                        argOK[i + 1] = true;
                        Program.checkOrbit = true;
                    }
                    else
                    {
                        throw new Exception("Syntax error specifying set of KEPLERian elements.");
                    }
                }
                else if (args[i] == "--state-vectors"
                        || args[i] == "-s")
                {
                    /* a set of state vectors shall be set */
                    string arg = getNextArg(args, i);
                    ArrayList state_vectors = new ArrayList();

                    if (Program.checkOrbit)
                    {
                        throw new Exception("S/C orbit has been given before (probably by using a set of state vectors).");
                    }

                    if(tools.parse_numeric_list(arg, state_vectors, 6))
                    {
                        argOK[i] = true;
                        argOK[i + 1] = true;
                        Program.sim.spacecraft.setStateVectors(new Vector3D(
                                                                    Convert.ToDouble(state_vectors[0]),
                                                                    Convert.ToDouble(state_vectors[1]),
                                                                    Convert.ToDouble(state_vectors[2])),
                                                               new Vector3D(
                                                                    Convert.ToDouble(state_vectors[3]),
                                                                    Convert.ToDouble(state_vectors[4]),
                                                                    Convert.ToDouble(state_vectors[5])));
                        Program.checkOrbit = true;
                    }
                    else
                    {
                        throw new Exception("Syntax error specifying set of state vectors.");
                    }
                }
                else if (args[i] == "--pattern-repos"
                        || args[i] == "-p")
                {
                    /* the path for the pattern repository shall be set */
                    string arg = getNextArg(args, i);

                    try
                    {
                        Program.sim.setPatternRepoPath(arg);
                        argOK[i] = true;
                        argOK[i + 1] = true;
                    }
                    catch(Exception e)
                    {
                        throw e;
                    }
                }
                else if (args[i] == "--pov-path")
                {
                    /* the path for POV-Ray executable shall be set */
                    string arg = getNextArg(args, i);

                    try
                    {
                        Program.sim.setPOVPath(arg);
                        argOK[i] = true;
                        argOK[i + 1] = true;
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                }
                else if (args[i] == "--output-dir"
                        || args[i] == "-o")
                {
                    /* the output dir shall be set */
                    string arg = getNextArg(args, i);

                   try
                    {
                       Program.sim.setOutputPath(arg);
                        argOK[i] = true;
                        argOK[i + 1] = true;
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                }
                else if (args[i] == "--fov"
                        || args[i] == "-f")
                {
                    /* the camera's FOV shall be set */
                    string arg = getNextArg(args, i);

                    Regex rx = new Regex("^[0-9]{1,}[.]{0,1}[0-9]{0,}$");
                    if (rx.IsMatch(arg))
                    {
                        Program.sim.setFOV(Double.Parse(arg));
                        argOK[i] = true;
                        argOK[i + 1] = true;
                    }
                    else
                    {
                        throw new Exception("Specified FOV is invalid.");
                    }
                }
                else if (args[i] == "--attitude"
                        || args[i] == "-a")
                {
                    /* the s/c attitude shall be set */
                    string arg = getNextArg(args, i);
                    ArrayList attitude = new ArrayList();

                    if(tools.parse_numeric_list(arg, attitude, 4))
                    {
                        Program.sim.spacecraft.setOrientation(new Quaternion(
                                                                Convert.ToDouble(attitude[3]),
                                                                Convert.ToDouble(attitude[0]),
                                                                Convert.ToDouble(attitude[1]),
                                                                Convert.ToDouble(attitude[2])));
                        argOK[i] = true;
                        argOK[i + 1] = true;
                    }
                    else
                    {
                        throw new Exception("Syntax error specifying s/c attitude quaternion.");
                    }
                }
                else if (args[i] == "--attitude-transition"
                        || args[i] == "-at")
                {
                    /* the s/c attitude transition shall be set */
                    string arg = getNextArg(args, i);
                    ArrayList attitude_transition = new ArrayList();

                    if(tools.parse_numeric_list(arg, attitude_transition, 3))
                    {
                        Program.sim.spacecraft.setOrientationTransition(new Vector3D(
                                                                        Convert.ToDouble(attitude_transition[0]),
                                                                        Convert.ToDouble(attitude_transition[1]),
                                                                        Convert.ToDouble(attitude_transition[2])));
                        argOK[i] = true;
                        argOK[i + 1] = true;
                    }
                    else
                    {
                        throw new Exception("Syntax error specifying the attitude transition vector.");
                    }
                }
                else if (args[i] == "--width"
                        || args[i] == "-w")
                {
                    /* the width of the rendered image shall be set */
                    string arg = getNextArg(args, i);

                    Regex rx = new Regex("^[0-9]{1,}$");
                    if (rx.IsMatch(arg))
                    {
                        Program.sim.setWidth(uint.Parse(arg));
                        argOK[i] = true;
                        argOK[i + 1] = true;
                    }
                    else
                    {
                        throw new Exception("Specified width is invalid.");
                    }
                }
                else if (args[i] == "--height"
                        || args[i] == "-h")
                {
                    /* the height of the rendered image shall be set */
                    string arg = getNextArg(args, i);

                    Regex rx = new Regex("^[0-9]{1,}$");
                    if (rx.IsMatch(arg))
                    {
                        Program.sim.setHeight(uint.Parse(arg));
                        argOK[i] = true;
                        argOK[i + 1] = true;
                    }
                    else
                    {
                        throw new Exception("Specified height is invalid.");
                    }
                }
                else if (args[i] == "--res"
                        || args[i] == "-r")
                {
                    /* the resolution of the used LDEM dataset shall be set */
                    string arg = getNextArg(args, i);

                    Regex rx = new Regex("^4|16|64$");
                    if (rx.IsMatch(arg))
                    {
                        Program.sim.setRes(uint.Parse(arg));
                        argOK[i] = true;
                        argOK[i + 1] = true;
                    }
                    else
                    {
                        throw new Exception("Specified NASA LRO LOLA LDEM resolution is invalid.");
                    }
                }
                else if (args[i] == "--batch"
                        || args[i] == "-b")
                {
                    /* batch mode turned on */
                    Program.batchmode = true;
                    argOK[i] = true;
                }
                else if (args[i] == "--ignore-sun")
                {
                    Program.ignoreSun = true;
                    argOK[i] = true;
                }
                else if (args[i] == "--rendering-annotation")
                {
                    Program.writeAnnotation = true;
                    argOK[i] = true;
                }
                else if (args[i] == "--fixed-state")
                {
                    if (Program.checkOrbit)
                    {
                        throw new Exception("S/C orbit has been given before, fixed state set can't be used.");
                    }

                    string arg = getNextArg(args, i);
                    ArrayList state = new ArrayList();

                    if (tools.parse_numeric_list(arg, state, 8))
                    {
                        Program.sim.setBatchSet
                        (
                                Double.Parse(state[0].ToString()),
                                new Vector3D
                                (
                                Double.Parse(state[1].ToString()),
                                Double.Parse(state[2].ToString()),
                                Double.Parse(state[3].ToString())
                                ),
                                new Quaternion
                                (
                                Double.Parse(state[7].ToString()),
                                Double.Parse(state[4].ToString()),
                                Double.Parse(state[5].ToString()),
                                Double.Parse(state[6].ToString())
                                )
                        );

                        Program.batchSet = true;
                        Program.checkOrbit = true;

                        argOK[i] = true;
                        argOK[i + 1] = true;
                    }
                    else
                    {
                        throw new Exception("Fixed state set is invalid.");
                    }
                }
                else if (args[i] == "--batch-file")
                {
                    if (Program.checkOrbit)
                    {
                        throw new Exception("S/C orbit has been given before, batch file can't be used.");
                    }

                    string arg = getNextArg(args, i);
                    Regex rx = new Regex(@"^([\-\+]?[0-9]+[.]?[0-9]*[Ee]?[\-\+]?[0-9]*)\s+([\-\+]?[0-9]+[.]?[0-9]*[Ee]?[\-\+]?[0-9]*)\s+([\-\+]?[0-9]+[.]?[0-9]*[Ee]?[\-\+]?[0-9]*)\s+([\-\+]?[0-9]+[.]?[0-9]*[Ee]?[\-\+]?[0-9]*)\s+([\-\+]?[0-9]+[.]?[0-9]*[Ee]?[\-\+]?[0-9]*)\s+([\-\+]?[0-9]+[.]?[0-9]*[Ee]?[\-\+]?[0-9]*)\s+([\-\+]?[0-9]+[.]?[0-9]*[Ee]?[\-\+]?[0-9]*)\s+([\-\+]?[0-9]+[.]?[0-9]*[Ee]?[\-\+]?[0-9]*)\s*$");

                    if (File.Exists(arg))
                    {
                        StreamReader file = new StreamReader(arg);
                        while (file.Peek() > -1)
                        {
                            string line = file.ReadLine();
                            if (rx.IsMatch(line))
                            {
                                Match res = rx.Match(line);
                                Program.sim.setBatchSet
                                (
                                     Double.Parse(res.Groups[1].ToString()),
                                     new Vector3D
                                     (
                                        Double.Parse(res.Groups[2].ToString()),
                                        Double.Parse(res.Groups[3].ToString()),
                                        Double.Parse(res.Groups[4].ToString())
                                     ),
                                     new Quaternion
                                     (
                                        Double.Parse(res.Groups[8].ToString()),
                                        Double.Parse(res.Groups[5].ToString()),
                                        Double.Parse(res.Groups[6].ToString()),
                                        Double.Parse(res.Groups[7].ToString())
                                     )
                                );
                            }
                            else
                            {
                                throw new Exception("Batch file is invalid.");
                            }
                        }
                        Program.batchSet = true;
                        Program.checkOrbit = true;
                    }
                    else
                    {
                        throw new Exception("Specified batch file could not be opened.");
                    }

                    argOK[i] = true;
                    argOK[i + 1] = true;
                }
                else if (args[i] == "--grid"
                        || args[i] == "-g")
                {
                    /* grid spacing shall be set (both horizontally/vertically) */
                    string arg = getNextArg(args, i);

                    Regex rx = new Regex("^[0-9]{1,}$");
                    if (rx.IsMatch(arg))
                    {
                        Program.sim.setGrid(uint.Parse(arg));
                        argOK[i] = true;
                        argOK[i + 1] = true;
                    }
                    else
                    {
                        throw new Exception("Specified grid spacing is invalid.");
                    }
                }
                else if (args[i] == "--gridH"
                        || args[i] == "-gH")
                {
                    /* horizontal grid spacing shall be set */
                    string arg = getNextArg(args, i);

                    Regex rx = new Regex("^[0-9]{1,}$");
                    if (rx.IsMatch(arg))
                    {
                        Program.sim.setGridH(uint.Parse(arg));
                        argOK[i] = true;
                        argOK[i + 1] = true;
                    }
                    else
                    {
                        throw new Exception("Specified horizontal grid spacing is invalid.");
                    }
                }
                else if (args[i] == "--gridV"
                        || args[i] == "-gV")
                {
                    /* vertical grid spacing shall be set */
                    string arg = getNextArg(args, i);

                    Regex rx = new Regex("^[0-9]{1,}$");
                    if (rx.IsMatch(arg))
                    {
                        Program.sim.setGridV(uint.Parse(arg));
                        argOK[i] = true;
                        argOK[i + 1] = true;
                    }
                    else
                    {
                        throw new Exception("Specified vertical grid spacing is invalid.");
                    }
                }
            }

            bool passed = true;
            for (int i = 0; i < argOK.Length; i++)
            {
                if (argOK[i] == false)
                {
                    passed = false;
                    break;
                }
            }

            if (!passed)
            {
                throw new Exception("Syntax error.");
            }

            if (!Program.checkOrbit)
            {
                throw new Exception("No orbit shape has been given.\nPlease enter either a set of KEPLERian elements or a set of state vectors.");
            }

            if (!Program.checkTime && !Program.batchSet)
            {
                throw new Exception("No simulation time(s) has/have been given.\nPlease enter at leat one simulation time.");
            }

            if (!Program.checkTimeT0 && !Program.batchSet)
            {
                throw new Exception("No epoch for the given orbit parameters have been given.");
            }
            return;
        }

        public static double rad2deg(double phi)
        {
            return phi * (180 / Math.PI);
        }

        public static double deg2rad(double phi)
        {
            return phi * (Math.PI / 180);
        }

        public static double m2AU(double meter)
        {
            return meter * 6.68458712267060E-12;
        }

        public static double normalizeAngle(double phi)
        {
            double phi_n = phi;

            while (phi_n < 0)
            {
                phi_n += (Math.PI * 2);
            }
            while (phi_n >= (Math.PI * 2))
            {
                phi_n -= (Math.PI * 2);
            }

            return phi_n;
        }

        public static void output_header()
        {
            Console.WriteLine();
            Console.WriteLine("================================================================================================================");
            Console.WriteLine("|                                MOON SURFACE ILLUMINATION SIMULATION FRAMEWORK                                |");
            Console.WriteLine("|                                               Version: " + Program.VERSION + "                                            |");
            Console.WriteLine("|                                                                                                              |");
            Console.WriteLine("|                                    M.Eng. Rene Schwarz (rene-schwarz.com)                                    |");
            Console.WriteLine("================================================================================================================");
            Console.WriteLine("|                                         German Aerospace Center (DLR)                                        |");
            Console.WriteLine("|                                   Merseburg University of Applied Sciences                                   |");
            Console.WriteLine("================================================================================================================");
            Console.WriteLine();
        }

        public static void output_configuration_overview()
        {
            Console.WriteLine("CONFIGURATION OVERVIEW");
            Console.WriteLine("----------------------");
            Console.WriteLine();
            Console.WriteLine("   (1) Spacecraft orbit (KEPLERian elements)");
            Console.WriteLine("   -----------------------------------------");
            Console.WriteLine();
            if (!Program.batchSet)
            {
                Console.WriteLine("      t_0     epoch of validity                      " + Program.sim.spacecraft.getEpoch() + " MJD (" + Program.sim.spacecraft.getEpochAsUTCString() + " UTC)");
                Console.WriteLine("      a       semi-major axis                        " + Program.sim.spacecraft.getSemiMajorAxis().ToString(Program.scientificFormat) + " m");
                Console.WriteLine("      e       eccentricity                           " + Program.sim.spacecraft.getEccentricity().ToString(Program.scientificFormat));
                Console.WriteLine("      omega   argument of periapsis                  " + tools.rad2deg(Program.sim.spacecraft.getArgumentOfPeriapsis()).ToString(Program.scientificFormat) + " deg");
                Console.WriteLine("      Omega   longitude of ascending node (LAN)      " + tools.rad2deg(Program.sim.spacecraft.getLongOfAscNode()).ToString(Program.scientificFormat) + " deg");
                Console.WriteLine("      i       inclination                            " + tools.rad2deg(Program.sim.spacecraft.getInclination()).ToString(Program.scientificFormat) + " deg");
                Console.WriteLine("      M_0     mean anomaly at epoch t_0              " + tools.rad2deg(Program.sim.spacecraft.getMeanAnomalyAtEpoch()).ToString(Program.scientificFormat) + " deg");
            }
            else
            {
                Console.WriteLine("      Multiple spacecraft positions given using batch file mode. There is no single orbit.");
            }
            Console.WriteLine();
            Console.WriteLine("   (2) Spacecraft orientation");
            Console.WriteLine("   -----------------------");
            Console.WriteLine();
            if (Program.sim.spacecraft.isOrientationGiven() && !Program.batchSet)
            {
                Console.WriteLine("      Orientation at epoch t_0 (represented as quaternion q = q_0 + q_1 i + q_2 j + q_3 k):");
                Console.WriteLine();
                Console.WriteLine("      q_0     scalar component                       " + Program.sim.spacecraft.getInitialOrientation().r().ToString(Program.scientificFormat));
                Console.WriteLine("      q_1     first vectorial component              " + Program.sim.spacecraft.getInitialOrientation().v().x().ToString(Program.scientificFormat));
                Console.WriteLine("      q_2     second component                       " + Program.sim.spacecraft.getInitialOrientation().v().y().ToString(Program.scientificFormat));
                Console.WriteLine("      q_3     third component                        " + Program.sim.spacecraft.getInitialOrientation().v().z().ToString(Program.scientificFormat));
                Console.WriteLine();
                Console.WriteLine("      Orientation transition (represented as vector of angular rotation rates):");
                Console.WriteLine();
                Console.WriteLine("      phi_x   ang. rot. rate about S/C's x-axis      " + tools.rad2deg(Program.sim.spacecraft.getOrientationTransition().x()).ToString(Program.scientificFormat) + " deg/s");
                Console.WriteLine("      phi_y   ang. rot. rate about S/C's y-axis      " + tools.rad2deg(Program.sim.spacecraft.getOrientationTransition().y()).ToString(Program.scientificFormat) + " deg/s");
                Console.WriteLine("      phi_z   ang. rot. rate about S/C's z-axis      " + tools.rad2deg(Program.sim.spacecraft.getOrientationTransition().z()).ToString(Program.scientificFormat) + " deg/s");
            }
            else if (Program.batchSet)
            {
                Console.WriteLine("      Multiple spacecraft orientations given using batch file mode. There is no single initial orientation.");
            }
            else
            {
                Console.WriteLine("      No spacecraft orientation given. Camera will look in the nadir.");
            }
            Console.WriteLine();
            Console.WriteLine("   (3) Simulation time(s)");
            Console.WriteLine("   ----------------------");
            Console.WriteLine();
            if (!Program.batchSet)
            {
                Console.WriteLine("      step            time (UTC)                     time (MJD)");

                ArrayList simulationTimes = Program.sim.getSimulationTime();
                ArrayList simulationTimesUTC = Program.sim.getSimulationTimesAsUTCString();
                for (int i = 0; i < simulationTimes.Count; i++)
                {
                    Console.WriteLine("      " + (i + 1).ToString("D5") + "           " + simulationTimesUTC[i] + "            " + simulationTimes[i]);
                }
            }
            else
            {
                Console.WriteLine("      Simulation times were specified using a batch file. Those values won't be displayed here.");
            }

            Console.WriteLine();
            Console.WriteLine("   (4) Paths");
            Console.WriteLine("   ---------");
            Console.WriteLine();
            Console.WriteLine("      Pattern Repository:                            " + Program.sim.getPatternRepoPath());
            Console.WriteLine("      POV-Ray binary:                                " + Program.sim.getPOVPath());
            Console.WriteLine("      Output directory:                              " + Program.sim.getOutputPath());
            Console.WriteLine();
            Console.WriteLine("   (5) Scene settings");
            Console.WriteLine("   ------------------");
            Console.WriteLine();
            Console.WriteLine("      S/C camera FOV:                                " + Program.sim.getFOV() + " deg");
            Console.WriteLine("      Width of rendered image:                       " + Program.sim.getWidth() + " px");
            Console.WriteLine("      Height of rendered image:                      " + Program.sim.getHeight() + " px");
            Console.WriteLine("      LDEM resolution to be used:                    " + Program.sim.getRes() + " px/deg");
            Console.WriteLine();
            Console.WriteLine("   (6) General information");
            Console.WriteLine("   -----------------------");
            Console.WriteLine();

            int length = 11;
            StringBuilder sb = new StringBuilder(length);
            getSPICEVersion(sb, ref length);
            Console.WriteLine("      Linked SPICE toolkit version is \"" + sb.ToString() + "\".");
            if (Program.batchmode)
            {
                Console.WriteLine("      Batch mode is set ON.");
            }
            else
            {
                Console.WriteLine("      Batch mode is set OFF.");
            }

            Console.WriteLine("\n\n");
            Console.WriteLine("STATUS LOG");
            Console.WriteLine("----------");
            Console.WriteLine();
        }

        public static void display_help()
        {
            Console.WriteLine("I.O.Y. a help message.");
        }

        public static double solveKeplerForE(double M, double e)
        {
            double tol = 1e-15;
            double E = 0;
            double Ej = M;

            bool ready = false;
            while (!ready)
            {
                E = Ej - ((Ej - e * Math.Sin(Ej) - M) / (1 - e * Math.Cos(Ej)));

                if (Math.Abs(E - Ej) < tol)
                {
                    ready = true;
                }

                Ej = E;
            }

            E = normalizeAngle(E);
            
            return E;
        }

        public static string md5File(string file)
        {
            System.IO.FileStream filestream = System.IO.File.OpenRead(file);
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] md5Hash = md5.ComputeHash(filestream);
            filestream.Close();
            return BitConverter.ToString(md5Hash).Replace("-", "").ToLower();
        }
    }
}
