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
#  Filename:        /src/MSIS/RotationQuaternion.cs
#  License:         GNU General Public License (GPL), version 2 or later
#
# *******************************************************************************
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MSIS
{
    class RotationQuaternion: Quaternion
    {
        public RotationQuaternion(Vector3D axis, double angle)
        {
            double sinCoeff = Math.Sin(angle / 2);

            this._r = Math.Cos(angle / 2);
            this._v = new Vector3D( axis.x() * sinCoeff,
                                    axis.y() * sinCoeff,
                                    axis.z() * sinCoeff);
        }
    }
}
