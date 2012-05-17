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
#  Filename:        /src/MSIS/Simulation.cs
#  License:         GNU General Public License (GPL), version 2 or later
#
# *******************************************************************************
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;

namespace MSIS
{
    class Simulation
    {
        List<Spacecraft> spacecrafts = new List<Spacecraft>();
        public Spacecraft spacecraft = new Spacecraft();
        ArrayList _simulation_times = new ArrayList();
        static string _dir_exe = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        static string _dir_MSISF = Directory.GetParent(_dir_exe).FullName;
        string _pattern_repos = _dir_MSISF + "\\pattern-repository\\";
        string _pov_path = _dir_MSISF + "\\POV-Ray-3.7\\bin\\";
        string _output_path = Directory.GetParent(Directory.GetCurrentDirectory()).FullName + "\\output\\";
        double _FOV = 40;
        uint _width = 1024;
        uint _height = 1024;
        uint _res = 4;
        protected static double _moon_radius = 1737150;
        private int loopCount = 1;
        private uint gridH = 50;
        private uint gridV = 50;

        public void checkConfig()
        {
            if (!Directory.Exists(this.getOutputPath()))
            {
                throw new Exception("Output path is not set properly. Please use the -o option.\nPATH: " + this.getOutputPath());
            }
            if (!File.Exists(this.getPOVPath() + "pvengine64.exe"))
            {
                throw new Exception("pvengine64.exe could not be found. Please set a valid path via the --pov-path option.\nPATH: " + this.getPOVPath());
            }
            if (!Directory.Exists(this.getPatternRepoPath()))
            {
                throw new Exception("Path to LDEM pattern repository is not set properly. Please use the -p option.\nPATH: " + this.getOutputPath());
            }
        }

        public void setSimulationTime(double time)
        {
            this._simulation_times.Add(time);
        }

        public void setSimulationTime(ArrayList times)
        {
            foreach(double time in times)
            {
                this._simulation_times.Add(time);
            }
        }

        public ArrayList getSimulationTime()
        {
            return this._simulation_times;
        }

        public ArrayList getSimulationTimesAsUTCString()
        {
            ArrayList simulation_times_UTC = new ArrayList();

            for (int i = 0; i < this._simulation_times.Count; i++)
            {
                simulation_times_UTC.Add(tools.MJDtoUTC(Convert.ToDouble(this._simulation_times[i])).ToString("s"));
            }
            return simulation_times_UTC;
        }

        public void setPatternRepoPath(string path)
        {
            if (Directory.Exists(path))
            {
                if (path.Substring(path.Length-2, 1) != "\\")
                {
                    this._pattern_repos = path + "\\";
                }
                else
                {
                    this._pattern_repos = path;
                }
            }
            else
            {
                throw new Exception("Specified pattern repository could not be found.");
            }
        }

        public string getPatternRepoPath()
        {
            return this._pattern_repos;
        }

        public void setPOVPath(string path)
        {
            // remove quotes
            path = path.Replace("\"", "");

            string POVpath = "";
            if (Directory.Exists(path))
            {
                if (path.Substring(path.Length - 2, 1) != "\\")
                {
                    POVpath = path + "\\";
                }
                else
                {
                    POVpath = path;
                }
            }
            else
            {
                throw new Exception("Specified POV-Ray path \"" + path + "\" could not be found.");
            }

            string fullpath = POVpath + "pvengine64.exe";
            if (File.Exists(fullpath))
            {
                this._pov_path = POVpath;
            }
            else
            {
                throw new Exception("Specified POV-Ray path is invalid (pvengine64.exe could not be found):\n\"" + POVpath + "\"");
            }
        }
        
        public string getPOVPath()
        {
            return this._pov_path;
        }

        public void setOutputPath(string path)
        {
            if (Directory.Exists(path))
            {
                this._output_path = path + "\\";
            }
            else
            {
                throw new Exception("Specified output directory could not be found.");
            }
        }

        public string getOutputPath()
        {
            return this._output_path;
        }

        public void setFOV(double FOV)
        {
            this._FOV = FOV;
        }

        public double getFOV()
        {
            return this._FOV;
        }

        public void setWidth(uint width)
        {
            this._width = width;
        }

        public uint getWidth()
        {
            return this._width;
        }

        public void setHeight(uint height)
        {
            this._height = height;
        }

        public uint getHeight()
        {
            return this._height;
        }

        public void setRes(uint res)
        {
            this._res = res;
        }

        public uint getRes()
        {
            return this._res;
        }

        public void doCalculations()
        {
            if(Program.batchSet)
            {
                foreach(Spacecraft sc in this.spacecrafts)
                {
                    doCalculationsLogic(sc, sc.getFixedTime());
                }
            }
            else
            {
                foreach (double time in this.getSimulationTime())
                {
                    doCalculationsLogic(this.spacecraft, time);
                }
            }
        }

        public void doCalculationsLogic(Spacecraft sc, double time)
        {
            int step = sc.addSpacecraftState(time);
            SpacecraftState scState = sc.getSpacecraftState(step);

            double simTime = sc.getSpacecraftState(step).getTime();
            Vector3D position = sc.getSpacecraftState(step).getPosition();
            Vector3D sun_position = sc.getSpacecraftState(step).getSunPosition();
            Quaternion orientation = sc.getSpacecraftState(step).getOrientation();

            Console.WriteLine("  Simulation time point #" + this.loopCount.ToString("D5") + ": " + simTime + " MJD (" + tools.MJDtoUTC(simTime).ToString("s") + " UTC)");
            Console.WriteLine("  --------------------------------------------------------------------------------------------------------------");
            Console.WriteLine();
            Console.WriteLine("    Calculating spacecraft position at given simulation time...");
            Console.WriteLine("      Result:");
            Console.WriteLine("        x = " + position.x().ToString(Program.scientificFormat) + " m      (" + tools.m2AU(position.x()).ToString(Program.scientificFormat) + " AU)");
            Console.WriteLine("        y = " + position.y().ToString(Program.scientificFormat) + " m      (" + tools.m2AU(position.y()).ToString(Program.scientificFormat) + " AU)");
            Console.WriteLine("        z = " + position.z().ToString(Program.scientificFormat) + " m      (" + tools.m2AU(position.z()).ToString(Program.scientificFormat) + " AU)");
            Console.WriteLine();
            if (sc.isOrientationGiven())
            {
                Console.WriteLine("    Calculating spacecraft orientation at given simulation time...");
                Console.WriteLine("      Result (Quaternion):");
                Console.WriteLine("        q_0 = " + orientation.r().ToString(Program.scientificFormat));
                Console.WriteLine("        q_1 = " + orientation.v().x().ToString(Program.scientificFormat));
                Console.WriteLine("        q_2 = " + orientation.v().y().ToString(Program.scientificFormat));
                Console.WriteLine("        q_3 = " + orientation.v().z().ToString(Program.scientificFormat));
                Console.WriteLine();
            }
            Console.WriteLine("    Calculating Sun position at given simulation time...");
            Console.WriteLine("      Result:");
            Console.WriteLine("        x = " + sun_position.x().ToString(Program.scientificFormat) + " m      (" + tools.m2AU(sun_position.x()).ToString(Program.scientificFormat) + " AU)");
            Console.WriteLine("        y = " + sun_position.y().ToString(Program.scientificFormat) + " m      (" + tools.m2AU(sun_position.y()).ToString(Program.scientificFormat) + " AU)");
            Console.WriteLine("        z = " + sun_position.z().ToString(Program.scientificFormat) + " m      (" + tools.m2AU(sun_position.z()).ToString(Program.scientificFormat) + " AU)");
            Console.WriteLine();

            string filename = this.getOutputPath() + "MoonSurfIllumSim_step_" + this.loopCount.ToString("D5");
            Console.WriteLine("    Ray tracing scene (Dynamical Surface Pattern Selection Algorithm - DSPSA)...");
            this.generatePOVRayFile(sc.getSpacecraftState(step), filename);
            Console.WriteLine("    POV-Ray file written to \"" + filename + ".pov\"");
            Console.WriteLine("    Rendering using POV-Ray...");

            Process pov = new Process();
            pov.StartInfo.FileName = this._pov_path + "pvengine64.exe";
            pov.StartInfo.Arguments = "/EXIT Quality=11 Antialias_Depth=3 Antialias=On Antialias_Threshold=0.1 Jitter_Amount=0.5 Jitter=On Width=" + this._width + " Height=" + this._height + " Antialias=On Antialias_Threshold=0.3 /RENDER \"" + filename + ".pov\"";
            pov.Start();
            pov.WaitForExit();

            if (Program.writeAnnotation)
            {
                Console.WriteLine("    Regenerating rendering with annotations...");
                FileStream myStream = new FileStream(filename + ".png", FileMode.Open);
                Image myBitmap = Image.FromStream(myStream);
                myStream.Close();
                myStream.Dispose();
                Graphics g = Graphics.FromImage(myBitmap);
                //g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                float fontSize = (float)Convert.ToDouble(this._width) * 0.0095F;
                Font myFont = new Font("Consolas", fontSize);
                Quaternion rotQuat;

                if (sc.isOrientationGiven())
                {
                    rotQuat = scState.getOrientation();
                }
                else
                {
                    double phi = Math.Atan2(scState.getPosition().y(), scState.getPosition().x());
                    double theta = Math.Acos(scState.getPosition().z() / (scState.getPosition().norm()));
                    rotQuat = new RotationQuaternion(new Vector3D(0, 1, 0), theta) * new RotationQuaternion(new Vector3D(0, 0, 1), -phi);
                }

                // Local Solar Illumination Angle
                Pen pen = new Pen(Color.Red, 2);
                pen.StartCap = LineCap.SquareAnchor;
                pen.EndCap = LineCap.Round;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                SolidBrush redBrush = new SolidBrush(Color.Red);
                
                foreach (var pixel in scState.PixelInfo)
                {
                    double x1 = pixel.Key.x();
                    double y1 = pixel.Key.y();

                    Vector2D endPoint = new Vector2D(x1, y1) + new Vector2D(0, this.gridH/2).rotate(tools.deg2rad(pixel.Value.IlluminationAngle));

                    g.DrawLine(pen, new Point(Convert.ToInt32(x1), Convert.ToInt32(y1)), new Point(Convert.ToInt32(endPoint.x()), Convert.ToInt32(endPoint.y())));
                    g.FillEllipse(redBrush, (float)x1 - 3F,  (float)y1 - 3F, 6, 6);
                }

                // Annotations to be written
                g.DrawString("Simulation Timecode:        " + scState.getTime().ToString(Program.scientificFormat) + " MJD (" + tools.MJDtoUTC(simTime).ToString("s") + "Z UTC)", myFont, Brushes.White, new PointF(2 * fontSize, 1 * 1.8F * fontSize));
                g.DrawString("S/C Position:               [" + scState.getPosition().x().ToString(Program.scientificFormat) + "," + scState.getPosition().y().ToString(Program.scientificFormat) + "," + scState.getPosition().z().ToString(Program.scientificFormat) + "] m", myFont, Brushes.White, new PointF(2 * fontSize, 2 * 1.8F * fontSize));
                g.DrawString("S/C Orientation Quaternion: [" + rotQuat.v().x().ToString(Program.scientificFormat) + "," + rotQuat.v().y().ToString(Program.scientificFormat) + "," + rotQuat.v().z().ToString(Program.scientificFormat) + "," + rotQuat.r().ToString(Program.scientificFormat) + "]", myFont, Brushes.White, new PointF(2 * fontSize, 3 * 1.8F * fontSize));
                g.DrawString("Sun Position:               [" + scState.getSunPosition().x().ToString(Program.scientificFormat) + "," + scState.getSunPosition().y().ToString(Program.scientificFormat) + "," + scState.getSunPosition().z().ToString(Program.scientificFormat) + "] m", myFont, Brushes.White, new PointF(2 * fontSize, 4 * 1.8F * fontSize));
                g.DrawString("Flight Altitude over MMR:   " + (scState.getPosition().norm() - 1.73715E6).ToString(Program.scientificFormat) + " m", myFont, Brushes.White, new PointF(2 * fontSize, 5 * 1.8F * fontSize));
                g.DrawString("Surface Mesh Resolution:    " + this._res.ToString() + " px/deg", myFont, Brushes.White, new PointF(2 * fontSize, 6 * 1.8F * fontSize));
                g.DrawString("FOV:                        " + this._FOV.ToString() + " deg (hor. & vert.), optics: perspective camera", myFont, Brushes.White, new PointF(2 * fontSize, 7 * 1.8F * fontSize));
                g.DrawString("Moon Surface Illumination Simulation Framework (MSISF), v" + Program.VERSION, new Font("Tahoma", fontSize), Brushes.White, new PointF(2 * fontSize, (float)Convert.ToDouble(this._height) - 3 * 1.8F * fontSize));
                g.DrawString("B.Eng. René Schwarz (rene-schwarz.com), more Information: http://go.rene-schwarz.com/masters-thesis", new Font("Tahoma", fontSize), Brushes.White, new PointF(2 * fontSize, (float)Convert.ToDouble(this._height) - 2 * 1.8F * fontSize));

                myBitmap.Save(filename + ".annotated.png");
                myBitmap.Dispose();
            }

            Console.WriteLine();
            Console.WriteLine();

            this.loopCount++;
        }

        private void generatePOVRayFile(SpacecraftState sc, string filename)
        {
            ArrayList filetext = new ArrayList();
            Vector3D scPos = sc.getPosition();
            Vector3D sunPos = sc.getSunPosition();


            /* POV-Ray file */
            /* ============ */

                filetext.Add("#version 3.7;");
                filetext.Add("");
                filetext.Add("#declare Orange = rgb <1,0.5,0>;");
                filetext.Add("#declare Red = rgb <1,0,0>;");
                filetext.Add("#declare Yellow = rgb <1,1,0>;");
                filetext.Add("#declare moon = texture");
                filetext.Add("                {");
                filetext.Add("                  pigment { color rgb<0.8, 0.8, 0.8> }");
                filetext.Add("                  finish");
                filetext.Add("                  {");
                filetext.Add("                    ambient 0.0");
                filetext.Add("                    diffuse 0.8");
                filetext.Add("                  }");
                filetext.Add("                }");
                filetext.Add("");
                filetext.Add("global_settings");
                filetext.Add("{");
                filetext.Add("   charset utf8");
                filetext.Add("   assumed_gamma 1.0");
                filetext.Add("}");
                filetext.Add("");
                filetext.Add("camera");
                filetext.Add("{");
                filetext.Add("  perspective");
                filetext.Add("  location <" + scPos.x()/1000 + ", " + scPos.y()/1000 + ", " + scPos.z()/1000 + ">");
                filetext.Add("  right <" + sc.getPOVRight().x().ToString("0.0000000") + ", " + sc.getPOVRight().y().ToString("0.0000000") + ", " + sc.getPOVRight().z().ToString("0.0000000") + ">");
                filetext.Add("  up <" + sc.getPOVUp().x().ToString("0.0000000") + ", " + sc.getPOVUp().y().ToString("0.0000000") + ", " + sc.getPOVUp().z().ToString("0.0000000") + ">");
                filetext.Add("  direction <" + sc.getPOVDirection().x().ToString("0.0000000") + ", " + sc.getPOVDirection().y().ToString("0.0000000") + ", " + sc.getPOVDirection().z().ToString("0.0000000") + ">");
                filetext.Add("}");
                filetext.Add("");
                if (!Program.ignoreSun)
                {
                    filetext.Add("light_source");
                    filetext.Add("{");
                    filetext.Add("  <" + sunPos.x() / 1000 + ", " + sunPos.y() / 1000 + ", " + sunPos.z() / 1000 + ">");
                    filetext.Add("  color rgb<1, 1, 1>");
                    filetext.Add("  looks_like");
                    filetext.Add("  {");
                    filetext.Add("    sphere");
                    filetext.Add("    {");
                    filetext.Add("      0, 1000");
                    filetext.Add("      pigment { rgbt 1 }");
                    filetext.Add("      hollow");
                    filetext.Add("      interior");
                    filetext.Add("      {");
                    filetext.Add("        media");
                    filetext.Add("        {");
                    filetext.Add("          emission 1");
                    filetext.Add("          density");
                    filetext.Add("          {");
                    filetext.Add("            spherical");
                    filetext.Add("            density_map");
                    filetext.Add("            {");
                    filetext.Add("              [0 rgb 0]");
                    filetext.Add("              [60 Orange]");
                    filetext.Add("              [80 Red]");
                    filetext.Add("              [100 Yellow]");
                    filetext.Add("            }");
                    filetext.Add("            scale 1000");
                    filetext.Add("          }");
                    filetext.Add("        }");
                    filetext.Add("      }");
                    filetext.Add("    }");
                    filetext.Add("  }");
                    filetext.Add("}");
                }
                else
                {
                    filetext.Add("light_source");
                    filetext.Add("{");
                    filetext.Add("  <" + scPos.x() / 1000 + ", " + scPos.y() / 1000 + ", " + scPos.z() / 1000 + ">");
                    filetext.Add("  color rgb<1, 1, 1>");
                    filetext.Add("}");
                }
                filetext.Add("");
            
            
                // Dynamical Surface Pattern Selection Algorithm (DSPSA)
                ArrayList pattern = new ArrayList();
                for (double y = 1; y <= this._height; y++)
                {
                    for (double x = 1; x <= this._width; x++)
                    {
                        Vector3D rayDirection = sc.getPOVDirection() + (((1 + this._height - (2 * y)) / (2 * this._height)) * sc.getPOVUp()) - (((1 + this._width - (2 * x)) / (2 * this._width)) * sc.getPOVRight());
                        this.DSPSA(rayDirection, scPos, pattern);
                    }
                }

                foreach (string str in pattern)
                {
                    filetext.Add("#include \"" + this._pattern_repos + this._res + "\\pattern_LDEM_" + this._res + str + ".inc\"");
                }

                /*  FOR DEBUG: diplay three dots as axis marker
                filetext.Add("sphere { <1800,0,0>, 100 pigment { color rgb <1.0, 0.0, 0.0> } finish { ambient 1.0 diffuse 1.0 } no_shadow }");
                filetext.Add("sphere { <0,1800,0>, 100 pigment { color rgb <1.0, 1.0, 0.0> } finish { ambient 1.0 diffuse 1.0 } no_shadow }");
                filetext.Add("sphere { <0,0,1800>, 100 pigment { color rgb <0.0, 1.0, 0.0> } finish { ambient 1.0 diffuse 1.0 } no_shadow }");
                */
            
                filetext.Add("");
                filetext.Add("//-------------------------DEBUG INFORMATION-------------------------------------------------------------------------------------------------------");
                filetext.Add("// MSISF Version: " + Program.VERSION);
                filetext.Add("// Config:");
                filetext.Add("//   SimTime:              " + sc.getTime().ToString(Program.scientificFormat));
                filetext.Add("//   s/c position:         [" + scPos.x().ToString(Program.scientificFormat) + "," + scPos.y().ToString(Program.scientificFormat) + "," + scPos.z().ToString(Program.scientificFormat) + "]");
                filetext.Add("//   s/c orientation:      [" + sc.getOrientation().v().x().ToString(Program.scientificFormat) + "," + sc.getOrientation().v().y().ToString(Program.scientificFormat) + "," + sc.getOrientation().v().z().ToString(Program.scientificFormat) + "," + sc.getOrientation().r().ToString(Program.scientificFormat) + "]");
                filetext.Add("//-----------------------END DEBUG INFORMATION------------------------------------------------------------------------------------------------------");

                TextWriter file = new System.IO.StreamWriter(filename + ".pov", false);
                foreach (string line in filetext)
                {
                    file.WriteLine(line);
                }
                file.Close();

                string md5 = tools.md5File(filename + ".pov");
                file = new System.IO.StreamWriter(filename + ".pov", true);
                file.WriteLine("// " + md5);
                file.Close();


            /* META INFORMATION FILE (XML FILE) */
            /* ================================ */

                filetext = new ArrayList();
                filetext.Add("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?>");
                filetext.Add("<!DOCTYPE MSISRendering SYSTEM \"../lib/MSISRendering.dtd\">");
                filetext.Add("<MSISRendering>");

                // GENERAL INFORMATION
                filetext.Add("    <GeneralInformation>");
                filetext.Add("        <SimulationTime>");
                filetext.Add("            <MJD>" + sc.getTime().ToString(Program.scientificFormat) + "</MJD>");
                filetext.Add("            <UTC>" + tools.MJDtoUTC(sc.getTime()).ToString("s") + "Z</UTC>");
                filetext.Add("        </SimulationTime>");
                filetext.Add("        <CameraPosition unit=\"m\">");
                filetext.Add("            <Vector3D x=\"" + sc.getPosition().x().ToString(Program.scientificFormat) + "\" y=\"" + sc.getPosition().y().ToString(Program.scientificFormat) + "\" z=\"" + sc.getPosition().z().ToString(Program.scientificFormat) + "\" />");
                filetext.Add("        </CameraPosition>");
                filetext.Add("        <CameraOrientation>");
                filetext.Add("            <Quaternion r=\"" + sc.getOrientation().r().ToString(Program.scientificFormat) + "\" x=\"" + sc.getOrientation().v().x().ToString(Program.scientificFormat) + "\" y=\"" + sc.getOrientation().v().y().ToString(Program.scientificFormat) + "\" z=\"" + sc.getOrientation().v().z().ToString(Program.scientificFormat) + "\" />");
                filetext.Add("        </CameraOrientation>");
                filetext.Add("        <SunPosition unit=\"m\">");
                filetext.Add("            <Vector3D x=\"" + sc.getSunPosition().x().ToString(Program.scientificFormat) + "\" y=\"" + sc.getSunPosition().y().ToString(Program.scientificFormat) + "\" z=\"" + sc.getSunPosition().z().ToString(Program.scientificFormat) + "\" />");
                filetext.Add("        </SunPosition>");
                filetext.Add("        <FlightAltitude unit=\"m\">" + (sc.getPosition().norm() - 1.73715E6).ToString(Program.scientificFormat) + "</FlightAltitude>");
                filetext.Add("        <SurfaceResolution unit=\"px/deg\">" + this._res.ToString() + "</SurfaceResolution>");
                filetext.Add("        <FOV unit=\"deg\">" + this._FOV.ToString() + "</FOV>");
                filetext.Add("        <MSISVersion>" + Program.VERSION + "</MSISVersion>");
                filetext.Add("        <CommandLine>" + Program.commandLineString +"</CommandLine>");
                filetext.Add("    </GeneralInformation>");

                // PIXEL INFORMATION
                filetext.Add("    <PixelInformation>");
                for(uint y = this.gridV; y <= this._height; y += this.gridV)
                {
                    for(uint x = this.gridH; x <= this._width; x += this.gridH)
                    {
                        PixelInformation PixelMetaData = new PixelInformation();
                        // if getIlluminationDirectionPerRenderingPixel() returns false, the Moon's
                        // surface has not been hit (the respective pixel shows the space background)
                        PixelMetaData = getIlluminationDirectionPerRenderingPixel(sc, x, y);
                        if (PixelMetaData.exists)
                        {
                            filetext.Add("        <Pixel h=\"" + x.ToString() + "\" v=\"" + y.ToString() + "\">");
                            filetext.Add("            <SelenographicCoordinates lat=\"" + PixelMetaData.lat.ToString(Program.scientificFormat) + "\" lon=\"" + PixelMetaData.lon.ToString(Program.scientificFormat) + "\" units=\"deg\" />");
                            filetext.Add("            <IlluminationDirection unit=\"deg\">" + PixelMetaData.IlluminationAngle.ToString(Program.scientificFormat) + "</IlluminationDirection>");
                            filetext.Add("        </Pixel>");
                            sc.PixelInfo.Add(new Vector2D(Convert.ToDouble(x), Convert.ToDouble(y)), PixelMetaData);
                        }
                    }
                }
                filetext.Add("    </PixelInformation>");

                filetext.Add("</MSISRendering>");

                file = new System.IO.StreamWriter(filename + ".xml", false);
                foreach (string line in filetext)
                {
                    file.WriteLine(line);
                }
                file.Close();
        }

        private PixelInformation getIlluminationDirectionPerRenderingPixel(SpacecraftState sc, uint x, uint y)
        {
            PixelInformation PixelOut = new PixelInformation();
            Vector3D c_pos = sc.getPosition();

            // STEP 1: Visibility Test
            Vector3D rayDirection = (sc.getPOVDirection() + (((1 + Convert.ToDouble(this._height) - (2 * Convert.ToDouble(y))) / (2 * Convert.ToDouble(this._height))) * sc.getPOVUp()) - (((1 + Convert.ToDouble(this._width) - (2 * Convert.ToDouble(x))) / (2 * Convert.ToDouble(this._width))) * sc.getPOVRight())).unit();
            double discriminant = 4 * Math.Pow(c_pos * rayDirection, 2) - 4 * (rayDirection * rayDirection) * (c_pos * c_pos) + 4 * (rayDirection * rayDirection) * Math.Pow(_moon_radius, 2);

            bool encounter = false;
            Vector3D p_surf = new Vector3D();

            if (discriminant < 0)
            {
                // no encounter
            }
            else if (discriminant == 0)
            {
                // one intersection
                encounter = true;
                double t = (-2 * (c_pos * rayDirection)) / (2 * (rayDirection * rayDirection));

                p_surf = c_pos + rayDirection * t;
            }
            else
            {
                // two intersections
                encounter = true;
                double t1 = (-2 * (c_pos * rayDirection) + Math.Sqrt(discriminant)) / (2 * (rayDirection * rayDirection));
                double t2 = (-2 * (c_pos * rayDirection) - Math.Sqrt(discriminant)) / (2 * (rayDirection * rayDirection));

                Vector3D ray_point1 = c_pos + rayDirection * t1;
                Vector3D ray_point2 = c_pos + rayDirection * t2;
                double ray_norm1 = (ray_point1 - c_pos).norm();
                double ray_norm2 = (ray_point2 - c_pos).norm();

                if (ray_norm1 < ray_norm2)
                {
                    p_surf = ray_point1;
                }
                else
                {
                    p_surf = ray_point2;
                }
            }

            if (encounter)
            {
                /* 
                    Pixel shows the Moon's surface: Continue with executing steps 2 - 8. 
                */
                double x1 = Convert.ToDouble(x);
                double y1 = Convert.ToDouble(y);
                
                // STEP 2: Obtaining the Surface Hit Point
                PixelOut.lat = tools.rad2deg((Math.PI / 2) - Math.Acos(p_surf.z() / _moon_radius));
                PixelOut.lon = tools.rad2deg(Math.Atan2(p_surf.y(), p_surf.x()));

                // STEP 3: Calculation of the Sun's direction
                Vector3D hat_p_surf = p_surf.unit();
                Vector3D hat_d_sun = (sc.getSunPosition() - p_surf).unit();

                // STEP 4: Derivation of the Local Tangent Plane of the Surface Hit Point (nothing to do here)
                // STEP 5: Determination of a Subsurface Point of the Solar Illumination Direction on the Local Tangent Plane
                double lambda_5 = -(1000 * (hat_d_sun.x() * hat_p_surf.x() + hat_d_sun.y() * hat_p_surf.y() + hat_d_sun.z() * hat_p_surf.z())) / (hat_p_surf.x() * hat_p_surf.x() + hat_p_surf.y() * hat_p_surf.y() + hat_p_surf.z() * hat_p_surf.z());
                Vector3D p_local = p_surf + 1000 * hat_d_sun + lambda_5 * hat_p_surf;

                // STEP 6: Local Illumination Direction
                Vector3D hat_d_local = (p_local - c_pos).unit();

                /// STEP 7: Projection of the Local Illumination Point to the Image Plane
                Vector3D k = c_pos + sc.getPOVDirection();
                Vector3D c_up = sc.getPOVUp();
                Vector3D c_right = sc.getPOVRight();
                double w = Convert.ToDouble(this._width);
                double h = Convert.ToDouble(this._height);

                double x2 =
                    (
                        -2 * w * c_pos.y() * c_up.z() * hat_d_local.x() - c_right.y() * c_up.z() * hat_d_local.x() - w * c_right.y() * c_up.z() * hat_d_local.x()
                        + 2 * w * c_pos.x() * c_up.z() * hat_d_local.y() + c_right.x() * c_up.z() * hat_d_local.y() + w * c_right.x() * c_up.z() * hat_d_local.y()
                        + 2 * w * c_pos.z() * (c_up.y() * hat_d_local.x() - c_up.x() * hat_d_local.y()) 
                        + (1 + w) * c_right.z() * (c_up.y() * hat_d_local.x() - c_up.x() * hat_d_local.y())
                        + 2 * w * c_pos.y() * c_up.x() * hat_d_local.z() + c_right.y() * c_up.x() * hat_d_local.z() + w * c_right.y() * c_up.x() * hat_d_local.z()
                        - 2 * w * c_pos.x() * c_up.y() * hat_d_local.z() - c_right.x() * c_up.y() * hat_d_local.z() - w * c_right.x() * c_up.y() * hat_d_local.z()
                        - 2 * w * c_up.z() * hat_d_local.y() * k.x() + 2 * w * c_up.y() * hat_d_local.z() * k.x() + 2 * w * c_up.z() * hat_d_local.x() * k.y() - 2 * w * c_up.x() * hat_d_local.z() * k.y()
                        - 2 * w * c_up.y() * hat_d_local.x() * k.z() + 2 * w * c_up.x() * hat_d_local.y() * k.z()
                    )/(
                        2 * (c_right.z() * (c_up.y() * hat_d_local.x() - c_up.x() * hat_d_local.y()) + c_right.y() * (-c_up.z() * hat_d_local.x() + c_up.x() * hat_d_local.z()) + c_right.x() * (c_up.z() * hat_d_local.y() - c_up.y() * hat_d_local.z()))
                    );

                double y2 =
                    (
                        -c_right.z() * c_up.y() * hat_d_local.x() - h * c_right.z() * c_up.y() * hat_d_local.x() + c_right.y() * c_up.z() * hat_d_local.x()
                        + h * c_right.y() * c_up.z() * hat_d_local.x() - 2 * h * c_pos.x() * c_right.z() * hat_d_local.y() + c_right.z() * c_up.x() * hat_d_local.y()
                        + h * c_right.z() * c_up.x() * hat_d_local.y() - c_right.x() * c_up.z() * hat_d_local.y() - h * c_right.x() * c_up.z() * hat_d_local.y()
                        + c_pos.z() * (-2 * h * c_right.y() * hat_d_local.x() + 2 * h * c_right.x() * hat_d_local.y()) + 2 * h * c_pos.x() * c_right.y() * hat_d_local.z()
                        - c_right.y() * c_up.x() * hat_d_local.z() - h * c_right.y() * c_up.x() * hat_d_local.z() + c_right.x() * c_up.y() * hat_d_local.z()
                        + h * c_right.x() * c_up.y() * hat_d_local.z() + 2 * h * c_pos.y() * (c_right.z() * hat_d_local.x() - c_right.x() * hat_d_local.z())
                        + 2 * h * c_right.z() * hat_d_local.y() * k.x() - 2 * h * c_right.y() * hat_d_local.z() * k.x() - 2 * h * c_right.z() * hat_d_local.x() * k.y()
                        + 2 * h * c_right.x() * hat_d_local.z() * k.y() + 2 * h * c_right.y() * hat_d_local.x() * k.z() - 2 * h * c_right.x() * hat_d_local.y() * k.z()
                    )/(
                        2 * (c_right.z() * (-c_up.y() * hat_d_local.x() + c_up.x() * hat_d_local.y()) + c_right.y() * (c_up.z() * hat_d_local.x() - c_up.x() * hat_d_local.z()) + c_right.x() * (-c_up.z() * hat_d_local.y() + c_up.y() * hat_d_local.z()))
                    );

                // STEP 8: The Local Solar Illumination Angle
                Vector2D v1 = new Vector2D(0, 1000);
                Vector2D v2 = new Vector2D(x2-x1, y2-y1);

                if (v2.x() > 0)
                {
                    PixelOut.IlluminationAngle = tools.rad2deg(Math.Acos((v1 * v2) / (v1.norm() * v2.norm())));
                }
                else
                {
                    PixelOut.IlluminationAngle = tools.rad2deg(2 * Math.PI - Math.Acos((v1 * v2) / (v1.norm() * v2.norm())));
                }
                PixelOut.exists = true;

                return PixelOut;
            }
            else
            {
                // next grid sample point
                return PixelOut;
            }
        }

        public void setBatchSet(double time, Vector3D pos, Quaternion orientation)
        {
            Spacecraft sc = new Spacecraft();
            sc.setPosition(pos);
            if (orientation.Norm() > 0)
            {
                sc.setOrientation(orientation);
            }
            sc.setFixedTime(time);
            this.spacecrafts.Add(sc);
        }

        public void setBatchSet(double time, Vector3D pos)
        {
            Spacecraft sc = new Spacecraft();
            sc.setStateVectors(pos, new Vector3D(0, 0, 0));
            this.spacecrafts.Add(sc);
        }

        private void DSPSA(Vector3D rayDirection, Vector3D camPos, ArrayList patternList)
        {
            double discriminant = 4 * Math.Pow(camPos * rayDirection, 2) - 4 * (rayDirection * rayDirection) * (camPos * camPos) + 4 * (rayDirection * rayDirection) * Math.Pow(_moon_radius, 2);

            bool encounter = false;
            double lat = 0;
            double lon = 0;

            if (discriminant < 0)
            {
                // no encounter
            }
            else if (discriminant == 0)
            {
                // one intersection
                encounter = true;
                double t = (-2 * (camPos * rayDirection)) / (2 * (rayDirection * rayDirection));

                Vector3D ray_point = camPos + rayDirection * t;
                lat = tools.rad2deg((Math.PI / 2) - Math.Acos(ray_point.z() / _moon_radius));
                lon = tools.rad2deg(Math.Atan2(ray_point.y(), ray_point.x()));
            }
            else
            {
                // two intersections
                encounter = true;
                double t1 = (-2 * (camPos * rayDirection) + Math.Sqrt(discriminant)) / (2 * (rayDirection * rayDirection));
                double t2 = (-2 * (camPos * rayDirection) - Math.Sqrt(discriminant)) / (2 * (rayDirection * rayDirection));

                Vector3D ray_point1 = camPos + rayDirection * t1;
                Vector3D ray_point2 = camPos + rayDirection * t2;
                double ray_norm1 = (ray_point1 - camPos).norm();
                double ray_norm2 = (ray_point2 - camPos).norm();

                if (ray_norm1 < ray_norm2)
                {
                    lat = tools.rad2deg((Math.PI / 2) - Math.Acos(ray_point1.z() / _moon_radius));
                    lon = tools.rad2deg(Math.Atan2(ray_point1.y(), ray_point1.x()));
                }
                else
                {
                    lat = tools.rad2deg((Math.PI / 2) - Math.Acos(ray_point2.z() / _moon_radius));
                    lon = tools.rad2deg(Math.Atan2(ray_point2.y(), ray_point2.x()));
                }
            }

            if (lon < 0)
            {
                lon += 360;
            }

            if (encounter)
            {
                double lat_start = 0;
                double lat_end = 0;
                double lon_start = 0;
                double lon_end = 0;

                if (lat >= 0)
                {
                    lat_start = Math.Truncate(lat / 5) * 5;
                    lat_end = lat_start + 5;
                }
                else
                {
                    lat_end = Math.Truncate(lat / 5) * 5;
                    lat_start = lat_end - 5;
                }

                lon_start = Math.Truncate(lon / 5) * 5;
                lon_end = lon_start + 5;

                string temp = "_lat_" + lat_start + "_" + lat_end + "_lon_" + lon_start + "_" + lon_end;
                if (!patternList.Contains(temp))
                {
                    patternList.Add(temp);
                }
            }
        }

        public void setGrid(uint spacing)
        {
            if (spacing > 0)
            {
                this.gridH = spacing;
                this.gridV = spacing;
            }
        }

        public void setGridH(uint spacing)
        {
            if (spacing > 0)
            {
                this.gridH = spacing;
            }
        }

        public void setGridV(uint spacing)
        {
            if (spacing > 0)
            {
                this.gridV = spacing;
            }
        }

        private Vector3D getImagePlanePoint(SpacecraftState sc, uint x, uint y)
        {
            return sc.getPosition() + sc.getPOVDirection() + (((1 + this._height - (2 * y)) / (2 * this._height)) * sc.getPOVUp()) - (((1 + this._width - (2 * x)) / (2 * this._width)) * sc.getPOVRight());
        }
    }
}
