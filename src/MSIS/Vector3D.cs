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
#  Filename:        /src/MSIS/Vector3D.cs
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
    class Vector3D
    {
        private double _x = 0;
        private double _y = 0;
        private double _z = 0;

        public Vector3D() { }

        public Vector3D(double x, double y, double z)
        {
            this._x = x;
            this._y = y;
            this._z = z;
        }

        public void setVector(double x, double y, double z)
        {
            this._x = x;
            this._y = y;
            this._z = z;
        }

        public static Vector3D operator +(Vector3D v1, Vector3D v2)
        {
            return new Vector3D(v1.x() + v2.x(), v1.y() + v2.y(), v1.z() + v2.z());
        }

        public static Vector3D operator *(double s, Vector3D vec)
        {
            return new Vector3D(s * vec.x(), s * vec.y(), s * vec.z());
        }

        public static Vector3D operator *(Vector3D vec, double s)
        {
            return new Vector3D(s * vec.x(), s * vec.y(), s * vec.z());
        }

        public static double operator *(Vector3D v1, Vector3D v2)
        {
            return v1.x() * v2.x() + v1.y() * v2.y() + v1.z() * v2.z();
        }

        // cross product of vector v1 and v2
        public static Vector3D operator %(Vector3D v1, Vector3D v2)
        {
            return new Vector3D(v1.y() * v2.z() - v1.z() * v2.y(),
                                v1.z() * v2.x() - v1.x() * v2.z(),
                                v1.x() * v2.y() - v1.y() * v2.x());
        }

        public static Vector3D operator /(Vector3D vec, double s)
        {
            return new Vector3D(vec.x() / s, vec.y() / s, vec.z() / s);
        }

        public static Vector3D operator -(Vector3D v1, Vector3D v2)
        {
            return new Vector3D(v1.x() - v2.x(), v1.y() - v2.y(), v1.z() - v2.z());
        }

        public static Vector3D operator -(Vector3D vec)
        {
            return new Vector3D(-vec.x(), -vec.y(), -vec.z());
        }

        public double x()
        {
            return this._x;
        }

        public double y()
        {
            return this._y;
        }

        public double z()
        {
            return this._z;
        }

        public double norm()
        {
            return Math.Sqrt(Math.Pow(this._x, 2) + Math.Pow(this._y, 2) + Math.Pow(this._z, 2));
        }

        public Vector3D unit()
        {
            return (this / this.norm());
        }

        public Vector3D QuaternionRotate(Vector3D axis, double angle)
        {
            Quaternion RotationQuaternion = new RotationQuaternion(axis, angle);
            Quaternion RotationQuaternionConjugate = RotationQuaternion.conjugate();
            Quaternion vec = new Quaternion(0, this._x, this._y, this._z);

            Quaternion result = RotationQuaternion * vec * RotationQuaternionConjugate;

            return result.getVector();
        }

        public Vector3D QuaternionRotate(Quaternion RotationQuaternion)
        {
            Quaternion RotationQuaternionConjugate = RotationQuaternion.conjugate();
            Quaternion vec = new Quaternion(0, this._x, this._y, this._z);

            Quaternion result = RotationQuaternion * vec * RotationQuaternionConjugate;

            return result.getVector();
        }
    }
}
