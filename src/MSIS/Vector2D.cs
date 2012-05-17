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
#  Filename:        /src/MSIS/Vector2D.cs
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
    class Vector2D
    {
        private double _x = 0;
        private double _y = 0;

        public Vector2D() { }

        public Vector2D(double x, double y)
        {
            this._x = x;
            this._y = y;
        }

        public void setVector(double x, double y)
        {
            this._x = x;
            this._y = y;
        }

        public static Vector2D operator +(Vector2D v1, Vector2D v2)
        {
            return new Vector2D(v1.x() + v2.x(), v1.y() + v2.y());
        }

        public static Vector2D operator *(double s, Vector2D vec)
        {
            return new Vector2D(s * vec.x(), s * vec.y());
        }

        public static Vector2D operator *(Vector2D vec, double s)
        {
            return new Vector2D(s * vec.x(), s * vec.y());
        }

        public static double operator *(Vector2D v1, Vector2D v2)
        {
            return v1.x() * v2.x() + v1.y() * v2.y();
        }

        public static Vector2D operator /(Vector2D vec, double s)
        {
            return new Vector2D(vec.x() / s, vec.y() / s);
        }

        public static Vector2D operator -(Vector2D v1, Vector2D v2)
        {
            return new Vector2D(v1.x() - v2.x(), v1.y() - v2.y());
        }

        public static Vector2D operator -(Vector2D vec)
        {
            return new Vector2D(-vec.x(), -vec.y());
        }

        public double x()
        {
            return this._x;
        }

        public double y()
        {
            return this._y;
        }

        public double norm()
        {
            return Math.Sqrt(Math.Pow(this._x, 2) + Math.Pow(this._y, 2));
        }

        public Vector2D rotate(double phi)
        {
            return new Vector2D(this._x * Math.Cos(-phi) - this._y * Math.Sin(-phi), this._x * Math.Sin(-phi) + this._y * Math.Cos(-phi));
        }
    }
}
