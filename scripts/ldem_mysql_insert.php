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
#        MSISF LRO LOLA LUNAR DIGITAL ELEVATION MODEL MYSQL IMPORT SCRIPT
# *******************************************************************************
#
#  Author:          M.Eng. René Schwarz 
#                       mail: <mail@rene-schwarz.com>
#                       web:  http://www.rene-schwarz.com
#                   on behalf of the German Aerospace Center (DLR)
#  Date:            2012/05/17
#  Filename:        /scripts/ldem_mysql_insert.php
#  License:         GNU General Public License (GPL), version 2 or later
#
# *******************************************************************************
/**
 * @mainpage MSISF LRO LOLA Lunar Digital Elevation Model MySQL Import Script
 * @brief
 *   MSISF LRO LOLA Lunar Digital Elevation Model MySQL Import Script
 * 
 *   This script imports NASA LRO LOLA LDEM datasets into a spatial MySQL
 *   database. This database is required by the MSISF for the generation of
 *   the surface pattern repository.
 *
 *   LDEM import has only to be done once per resolution or after an update of
 *   NASA files for a certain resolution. Afterwards all surface patterns of this
 *   specific resolution have to be re-generated using the generate_pattern.php
 *   script. If a pattern repository already exists with actual data, neither a
 *   MySQL import nor a (re-)generation of surface patterns is necessary.
 *
 *   Please be aware that the LDEM data import process for resolutions greater
 *   than LDEM_16 can require a considerable amount of time and resources, 
 *   depending on your hardware configuration. Import times in the order of
 *   hours are not unusual.
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
 *   - MySQL database with tables prepared for the lunar topographic data as
 *     specified in [1]
 *   - PHP >= version 5.3.9
 *   - corresponding NASA LRO LOLA LDEM files, available from
 *     <http://imbrium.mit.edu/DATA/LOLA_GDR/CYLINDRICAL/IMG/>, which shall be
 *     imported
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
 * @version 1.0
 * @date 2012/01/30
 */

 
 
 
# =======================================================
# Configuration of the MySQL import process
# =======================================================

# LDEM resolution (integer) to be used (preconfigured for LDEM_4, LDEM_16,
# LDEM_64, LDEM_128, LDEM_256, LDEM_512 and LDEM_1024).
$LDEM = 1024;

# Path to LDEM directory, where all LDEM files (*.img) to be imported are placed
# (w/o trailing slash) 
$path_LDEM = "S:/Wissenschaft & Forschung/[2011 Master] Masterthesis/LOLA";

# MySQL connection parameters
$mysql_host = "localhost";      # MySQL server, e.g. "localhost" or "localhost:3306"
$mysql_db = "masterthesis";  # MySQL database
$mysql_user = "masterthesis";   # MySQL username for specified database
$mysql_pwd = "masterthesis";    # MySQL password for specified username

# LDEM-specific options
# These parameters can be found in the corresponding *.lbl files for each *.img file.
# Preconfigured for LDEM_4, LDEM_16, LDEM_64, LDEM_128, LDEM_256, LDEM_512 and LDEM_1024.
switch($LDEM)
{
    case 4:
    default:
    /* LDEM_4 */
        $c_MAP_RESOLUTION = 4;
        $c_LINE_SAMPLES = 1440;
        $c_LINE_LAST_PIXEL = 720;
        $c_LINE_PROJECTION_OFFSET = 359.5;
        $c_SAMPLE_PROJECTION_OFFSET = 719.5;
        break;
        
    case 16:
    /* LDEM_16 */
        $c_MAP_RESOLUTION = 16;
        $c_LINE_SAMPLES = 5760;
        $c_LINE_LAST_PIXEL = 2880;
        $c_LINE_PROJECTION_OFFSET = 1439.5;
        $c_SAMPLE_PROJECTION_OFFSET = 2879.5;
        break;
        
    case 64:
    /* LDEM_64 */
        $c_MAP_RESOLUTION = 64;
        $c_LINE_SAMPLES = 23040;
        $c_LINE_LAST_PIXEL = 11520;
        $c_LINE_PROJECTION_OFFSET = 5759.5;
        $c_SAMPLE_PROJECTION_OFFSET = 11519.5;
        break;
        
    case 128:
    /* LDEM_128 */
        $c_MAP_RESOLUTION = 128;
        $c_LINE_SAMPLES = 46080;
        $c_LINE_LAST_PIXEL = 23040;
        $c_LINE_PROJECTION_OFFSET = 11519.5;
        $c_SAMPLE_PROJECTION_OFFSET = 23039.5;
        break;
        
    case 256:
    /* LDEM_256 */
        $c_MAP_RESOLUTION = 256;
        $c_LINE_SAMPLES = 46080;
        $c_LINE_LAST_PIXEL = 23040;
        break;
        
    case 512:
    /* LDEM_512 */
        $c_MAP_RESOLUTION = 512;
        $c_LINE_SAMPLES = 46080;
        $c_LINE_LAST_PIXEL = 23040;
        break;
        
    case 1024:
    /* LDEM_1024 */
        $c_MAP_RESOLUTION = 1024;
        $c_LINE_SAMPLES = 30720;
        $c_LINE_LAST_PIXEL = 15360;
        break;
}

# =======================================================
#                    END CONFIGURATION
#
#               DON'T EDIT BEYOND THIS LINE!
# =======================================================






$mtime = microtime();
$mtime = explode(" ",$mtime);
$mtime = $mtime[1] + $mtime[0];
$starttime = $mtime;

function cline($str = "")
{
    print("{$str}\r\n");
}

$c_SAMPLE_BITS = 16;
$c_RECORD_BYTES = ($c_LINE_SAMPLES * $c_SAMPLE_BITS)/8;
$c_CENTER_LATITUDE = 0;
$c_CENTER_LONGITUDE = 180;
$c_SCALING_FACTOR = 0.5;
$c_OFFSET = 1737400;

if($LDEM > 128)
{
    if(!isset($argv[1]))
    {
        die("For LDEM resolutions greater than 128 px/deg files have been\r\n" .
            "splitted by NASA. Additional command-line arguments are necessary\r\n" .
            "for the specification of the certain LDEM part. The values can\r\n" .
            "be found in the corresponding *.lbl file.\r\n\r\n" .
            "USAGE:\r\n" .
            "php ldem_mysql_insert.php additionalFilenamePart LINE_PROJECTION_OFFSET SAMPLE_PROJECTION_OFFSET\r\n\r\n" .
            "EXAMPLE:\r\n" .
            "php ldem_mysql_insert.php 00N_15N_330_360 15359.5 -153600.5\r\n");
    }
    
    $filename = "{$path_LDEM}/LDEM_{$c_MAP_RESOLUTION}_{$argv[1]}.img";
    $c_LINE_PROJECTION_OFFSET = $argv[2];
    $c_SAMPLE_PROJECTION_OFFSET = $argv[3];
}
else
{
    $filename = "{$path_LDEM}/LDEM_{$c_MAP_RESOLUTION}.img";
}

if($handle = fopen($filename, "r"))
{
    cline("NASA LOLA data file \"{$filename}\" opened.");
}
else
{
    die("NASA LOLA data file \"{$filename}\" could not be opened.");
}

cline("Connecting to MySQL database...");
$mysqli = new mysqli($mysql_host, $mysql_user, $mysql_pwd, $mysql_db);
cline("Connection to MySQL database established.");

for($line = 1; $line <= $c_LINE_LAST_PIXEL; $line++)
{
    $mtime = microtime();
    $mtime = explode(" ",$mtime);
    $mtime = $mtime[1] + $mtime[0];
    $lstarttime = $mtime;

    fseek($handle, ($line - 1) * $c_RECORD_BYTES);
    $line_content = unpack("s*", fread($handle, $c_RECORD_BYTES));

    #$sql = "INSERT INTO LDEM_{$c_MAP_RESOLUTION} (line, sample, lat, lon, height, planetary_radius, x, y, z, point) VALUES ";
    $sql = "INSERT INTO LDEM_{$c_MAP_RESOLUTION} (lat, lon, planetary_radius, x, y, z, point) VALUES ";
    
    $j = 1;
    for($sample = 1; $sample <= $c_LINE_SAMPLES; $sample++)
    {
        $dn = $line_content[$sample];
        
        $point["lat"] = $c_CENTER_LATITUDE - ($line - $c_LINE_PROJECTION_OFFSET - 1) / $c_MAP_RESOLUTION;
        $point["lon"] = $c_CENTER_LONGITUDE + ($sample - $c_SAMPLE_PROJECTION_OFFSET - 1) / $c_MAP_RESOLUTION;
        $point["height"] = ($dn * $c_SCALING_FACTOR);
        $point["planetary_radius"] = $point["height"] + $c_OFFSET;
        $point["x"] = $point["planetary_radius"] * cos(deg2rad($point["lat"])) * cos(deg2rad($point["lon"]));
        $point["y"] = $point["planetary_radius"] * cos(deg2rad($point["lat"])) * sin(deg2rad($point["lon"]));
        $point["z"] = $point["planetary_radius"] * sin(deg2rad($point["lat"]));
        $point["point"] = "PointFromText('POINT({$point["lat"]} {$point["lon"]})')";
        
        if(substr($sql, -7) == "VALUES ")
        {
            #$sql .= "('{$line}', '{$sample}', '{$point["lat"]}', '{$point["lon"]}', '{$point["height"]}', '{$point["planetary_radius"]}', '{$point["x"]}', '{$point["y"]}', '{$point["z"]}', {$point["point"]})";
            $sql .= "('{$point["lat"]}', '{$point["lon"]}', '{$point["planetary_radius"]}', '{$point["x"]}', '{$point["y"]}', '{$point["z"]}', {$point["point"]})";
        }
        else
        {
            #$sql .= ", ('{$line}', '{$sample}', '{$point["lat"]}', '{$point["lon"]}', '{$point["height"]}', '{$point["planetary_radius"]}', '{$point["x"]}', '{$point["y"]}', '{$point["z"]}', {$point["point"]})";
            $sql .= ", ('{$point["lat"]}', '{$point["lon"]}', '{$point["planetary_radius"]}', '{$point["x"]}', '{$point["y"]}', '{$point["z"]}', {$point["point"]})";
        }
        
        $j++;
        
        // prevent extreme huge SQL queries
        // (cut at 24000 inserts, since one LDEM_64 line contains 23,040 values)
        if($j > 24000)
        {
            if(!$mysqli->query($sql))
            {
                $mysqli->close();
                die("Error: " . $mysqli->error);
            }
            $sql = "INSERT INTO LDEM_{$c_MAP_RESOLUTION} (lat, lon, planetary_radius, x, y, z, point) VALUES ";
            $j = 1;
        }
    }
    
    if(!$mysqli->query($sql))
    {
        $mysqli->close();
        die("Error: " . $mysqli->error);
    }
    
    $mtime = microtime();
    $mtime = explode(" ",$mtime);
    $mtime = $mtime[1] + $mtime[0];
    $lendtime = $mtime;
    $ltotaltime = ($lendtime - $lstarttime);
    
    cline("Line {$line} of {$c_LINE_LAST_PIXEL} processed (" . (memory_get_peak_usage()/(1024*1024)) . " MiB, total execution time: {$ltotaltime} sec.)...");
}

cline();
cline();
$mysqli->close();
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