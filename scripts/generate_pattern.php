<?php
# *******************************************************************************
#                                          _                           
#                                         | |                          
#           _ __ ___ _ __   ___   ___  ___| |____      ____ _ _ __ ____
#          | '__/ _ \ '_ \ / _ \ / __|/ __| '_ \ \ /\ / / _` | '__|_  /
#          | | |  __/ | | |  __/ \__ \ (__| | | \ V  V / (_| | |   / / 
#          |_|  \___|_| |_|\___| |___/\___|_| |_|\_/\_/ \__,_|_|  /___|
#                                                            rene-schwarz.com
#
# *******************************************************************************
#                    MSISF SURFACE PATTERN GENERATION SCRIPT
# *******************************************************************************
#
#  Author:		    M.Eng. René Schwarz 
#						mail: <mail@rene-schwarz.com>
#						web:  http://www.rene-schwarz.com
#                   on behalf of the German Aerospace Center (DLR)
#  Date:			2012/05/17
#  Filename:		generate_pattern.php
#  Version:			1.0
#  License:         GNU General Public License (GPL), version 2 or later
#
# *******************************************************************************
/**
 * @mainpage MSISF Surface Pattern Generation Script
 * @brief
 *   MSISF Surface Pattern Generation Script
 * 
 *   This script generates the MSISF-required surface patterns out of a spatial
 *   MySQL database for the Moon's surface (made out of NASA LRO LOLA LDEM files).
 *
 *   Pattern generation have only to be done once per resolution or after an
 *   modification of the database data for a certain resolution, since all 
 *   generated patterns will be placed in a central pattern repository, being
 *   available for the MSISF.
 *
 *   Please be aware that the pattern generation process for resolutions greater
 *   than LDEM_16 can require a considerable amount of time and resources, 
 *   depending on your hardware configuration. Generation times in the order of
 *   weeks are not unusual.
 *
 *   For more information, see my master's thesis,
 *   available at <http://go.rene-schwarz.com/masters-thesis>.
 *
 *
 *   Copyright (c) 2011 B.Eng. René Schwarz <mail@rene-schwarz.com>
 *   Licensed under <a href="http://creativecommons.org/licenses/by-sa/3.0/de/">
 *          Creative Commoms - Attribution-Share Alike 3.0 Germany</a>
 *
 *
 *   Requirements:
 *   - MySQL database loaded with lunar topographic data as specified in [1]
 *   - delaunay2D.exe, as distributed with this script
 *   - Matlab R2011b or MATLAB Compiler Runtime (MCR) v7.16
 *   - PHP >= version 5.3.9
 *
 *
 *   References:
 *   [1] Schwarz, René: Development of an illumination simulation software for
 *       the Moon’s surface: An approach to illumination direction estimation
 *       on pictures of planetary solid surfaces with a significant number of
 *       craters. Master’s Thesis, Merseburg University of Applied Sciences,
 *       German Aerospace Center (DLR). Books on Demand, Norderstedt, Germany,
 *       2012.
 *
 *
 * @author B.Eng. René Schwarz <mail@rene-schwarz.com>
 * @version 1.1
 * @date 2012/03/20
 */
 

# Raise PHP's memory limit to 1 GB of RAM to ensure data handling (enough up to LDEM_64). If you are trying to import LDEMs greater than 64 px/deg in resolution, increase the memory limit by testing.
ini_set('memory_limit', '1G');


# =======================================================
# Configuration of the surface pattern generation process
# =======================================================

# LDEM resolution (integer) to be used (preconfigured for LDEM_4, LDEM_16 and LDEM_64).
$LDEM = 64;

# Path to pattern repository, where all generated patterns will be placed and where all existing patterns are located (w/o trailing slash) 
$path_patternDB = "S:/svn/Masterthesis/Dev/MoonIllumSim/pattern-repository";

# Path to a temporary directory (w/o trailing slash)
$path_tempdir = "S:/svn/Masterthesis/Dev/MSISF Pattern Generation";

# Path to the supplied delaunay2D.exe file (w/o trailing slash)
$path_delaunayHelper = "S:/svn/Masterthesis/Dev/MSISF Pattern Generation";

# MySQL connection parameters
$mysql_host = "localhost";      # MySQL server, e.g. "localhost" or "localhost:3306"
$mysql_db = "masterthesis";     # MySQL database
$mysql_user = "masterthesis";   # MySQL username for specified database
$mysql_pwd = "masterthesis";    # MySQL password for specified username

# Surface pattern offset (preconfigured for LDEM_4, LDEM_16 and LDEM_64)
# This offset specifies the latitude and longitude, which will be added to 5°x5° as overlap area between the single surface patterns to ensure a closed 3D surface during rendering/raytracing. For resolutions greater than 64 px/deg, 0.1° should be sufficient. All values of $off are given in degrees latitude/longitude.
switch($LDEM)
{
    case 4:
        $off = 0.5;
        break;
    case 16:
        $off = 0.5;
        break;
    case 64:
        $off = 0.1;
        break;
    default:
        die("No valid LDEM dataset selected.");
        break;
}

# =======================================================
#                    END CONFIGURATION
#
#               DON'T EDIT BEYOND THIS LINE!
# =======================================================



function cline($str = "")
{
    print("{$str}\r\n");
}


$mtime = microtime();
$mtime = explode(" ",$mtime);
$mtime = $mtime[1] + $mtime[0];
$starttime = $mtime;

cline("Connecting to MySQL database...");
$db_link = mysql_connect($mysql_host, $mysql_user, $mysql_pwd) OR die(mysql_error($db_link));
mysql_select_db($mysql_db, $db_link) OR die(mysql_error($db_link));
cline("Connection to MySQL database established.");

for($lat = -90; $lat <= 85; $lat += 5)
{
    $lat_s = $lat;
    $lat_e = $lat + 5;
    $lat_start = $lat - $off;
    $lat_end = $lat + 5 + $off;
    
    for($lon = 0; $lon <= 355; $lon += 5)
    {
        $mtime = explode(" ",microtime());
        $mtime = $mtime[1] + $mtime[0];
        $starttime1 = $mtime;
        
        $lon_s = $lon;
        $lon_e = $lon + 5;
        $lon_start = $lon - $off;
        $lon_end = $lon + 5 + $off;
           
        $pattern = "LDEM_{$LDEM}_lat_{$lat_s}_{$lat_e}_lon_{$lon_s}_{$lon_e}";
        $special = FALSE;
        
        if(file_exists("{$path_patternDB}/pattern_{$pattern}.inc"))
        {
            cline("Pattern {$pattern}: Pattern already exists - SKIPPING.");
        }
        else
        {
            cline("Pattern {$pattern}: Requesting data for pattern from database...");
            
            if($lon_start < 0)
            {
                $special = TRUE;
                $lon_extra = $lon_start + 360;
                $sql = "SELECT lat, lon, x, y, z 
                        FROM LDEM_{$LDEM}
                        WHERE MBRContains(GeomFromText('POLYGON(({$lat_start} 0, {$lat_start} {$lon_end}, {$lat_end} {$lon_end}, {$lat_end} 0, {$lat_start} 0))'), point)
                            OR MBRContains(GeomFromText('POLYGON(({$lat_start} {$lon_extra}, {$lat_start} 360, {$lat_end} 360, {$lat_end} {$lon_extra}, {$lat_start} {$lon_extra}))'), point)
                        ORDER BY point_id ASC;";
            }
            elseif($lon_end > 360)
            {
                $special = TRUE;
                $lon_extra = $lon_end - 360;
                $sql = "SELECT lat, lon, x, y, z 
                        FROM LDEM_{$LDEM}
                        WHERE MBRContains(GeomFromText('POLYGON(({$lat_start} {$lon_start}, {$lat_start} 360, {$lat_end} 360, {$lat_end} {$lon_start}, {$lat_start} {$lon_start}))'), point)
                            OR MBRContains(GeomFromText('POLYGON(({$lat_start} 0, {$lat_start} {$lon_extra}, {$lat_end} {$lon_extra}, {$lat_end} 0, {$lat_start} 0))'), point)
                        ORDER BY point_id ASC;";
            }
            else
            {
                $sql = "SELECT lat, lon, x, y, z 
                        FROM LDEM_{$LDEM}
                        WHERE MBRContains(GeomFromText('POLYGON(({$lat_start} {$lon_start}, {$lat_start} {$lon_end}, {$lat_end} {$lon_end}, {$lat_end} {$lon_start}, {$lat_start} {$lon_start}))'), point)
                        ORDER BY point_id ASC;";
            }
           
            $res = mysql_query($sql, $db_link) OR die(mysql_error($db_link));
            
            cline("Pattern {$pattern}: Creating data array...");
            $points = array();
            $csv = array();
            $point_id = 0;
            while($row = mysql_fetch_assoc($res))
            {
                $point_id++;
                $points[$point_id] = array("x" => $row["x"]/1000,
                                           "y" => $row["y"]/1000,
                                           "z" => $row["z"]/1000);
               $csv[] = "{$row["lat"]}, {$row["lon"]}\r\n";
            }
            unset($point_id);
            
            file_put_contents("{$path_tempdir}/pattern_{$pattern}.csv", $csv);
            cline("Pattern {$pattern}: Pattern (lat, lon) written to file.");
            unset($csv);
            
            if($lat_start < -90 OR $lat_end > 90)
            {
                $special = TRUE;
            }
            
            if($special)
            {
                cline("Pattern {$pattern}: Running Delaunay Triangulation...");
                exec("\"{$path_tempdir}/delaunay2D.exe\" pattern_{$pattern} 2>&1");
                cline("Pattern {$pattern}: Delaunay Triangulation finished.");
            
                $delaunay_template = file("{$path_tempdir}/pattern_{$pattern}_delaunay.csv");
            }
            else
            {
                if(file_exists("{$path_tempdir}/delaunay_template_LDEM_{$LDEM}.csv"))
                {
                    $delaunay_template = file("{$path_tempdir}/delaunay_template_LDEM_{$LDEM}.csv");
                    cline("Pattern {$pattern}: Delaunay Triangulation not necessary: Suitable template found.");
                }
                else
                {
                    cline("Pattern {$pattern}: No Delaunay template for standard case found. Running Delaunay Triangulation...");
                    exec("\"{$path_tempdir}/delaunay2D.exe\" pattern_{$pattern} 2>&1");
                    cline("Pattern {$pattern}: Delaunay Triangulation finished and template saved.");
                    
                    rename("{$path_tempdir}/pattern_{$pattern}_delaunay.csv", "{$path_tempdir}/delaunay_template_LDEM_{$LDEM}.csv");
            
                    $delaunay_template = file("{$path_tempdir}/delaunay_template_LDEM_{$LDEM}.csv");
                }
            }
            
            cline("Pattern {$pattern}: Creating POV-Ray file using given Delaunay Triangulation template...");
            $povrayfile = array(    "// MSISF Pattern {$pattern}\r\n",
                                    "// generated using ./scripts/generate_pattern.php version 1.1\r\n",
                                    "// Moon Surface Illumination Simulation Framework (MSISF)\r\n",
                                    "// http://go.rene-schwarz.com/masters-thesis\r\n",
                                    "\r\n",
                                    "mesh\r\n",
                                    "{\r\n"     );
            for($i = 0; $i < count($delaunay_template); $i++)
            {
                if(preg_match("/^([0-9]{1,}),([0-9]{1,}),([0-9]{1,})[\\s]{0,}$/", $delaunay_template[$i], $matches))
                {
                    $point_id_triang_1 = $matches[1];
                    $point_id_triang_2 = $matches[2];
                    $point_id_triang_3 = $matches[3];
                    
                    $povrayfile[] = "   triangle\r\n";
                    $povrayfile[] = "   {\r\n";
                    $povrayfile[] = "       <{$points[$point_id_triang_1]["x"]}, {$points[$point_id_triang_1]["y"]}, {$points[$point_id_triang_1]["z"]}>, <{$points[$point_id_triang_2]["x"]}, {$points[$point_id_triang_2]["y"]}, {$points[$point_id_triang_2]["z"]}>, <{$points[$point_id_triang_3]["x"]}, {$points[$point_id_triang_3]["y"]}, {$points[$point_id_triang_3]["z"]}>\r\n";
                    $povrayfile[] = "       texture { moon }\r\n";
                    $povrayfile[] = "   }\r\n";
                }
                else
                {
                    die("Error: RegEx search in Delaunay template on line {$i} didn't succeeed.");
                }
            }
            $povrayfile[] =         "}\r\n";
            
            file_put_contents("{$path_patternDB}/{$LDEM}/pattern_{$pattern}.inc", $povrayfile, LOCK_EX);
            unlink("{$path_tempdir}/pattern_{$pattern}.csv");
            if(file_exists("{$path_tempdir}/pattern_{$pattern}_delaunay.csv"))
            {
                unlink("{$path_tempdir}/pattern_{$pattern}_delaunay.csv");
            }
            cline("Pattern {$pattern}: Pattern written to POV-Ray file.");
            
            unset($povrayfile);
            unset($delaunay_template);
            unset($points);
        }
        $mtime = explode(" ",microtime());
        $mtime = $mtime[1] + $mtime[0];
        $endtime1 = $mtime;
        $totaltime1 = ($endtime1 - $starttime1);
        cline("Pattern {$pattern}: Calculation took {$totaltime1} sec.");
        cline();
    }
}

cline();
cline();
mysql_close($db_link) OR die(mysql_error($db_link));
cline("MySQL connection closed.");


   $mtime = microtime();
   $mtime = explode(" ",$mtime);
   $mtime = $mtime[1] + $mtime[0];
   $endtime = $mtime;
   $totaltime = ($endtime - $starttime);

cline();
cline("Operation has finished.");
cline("Peak memory needed: " . (memory_get_peak_usage()/(1024*1024)) . " MiB, total execution time: {$totaltime} sec.");

?>