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
#  Filename:        /src/MSIS/Quaternion.cs
#  License:         GNU General Public License (GPL), version 2 or later
#
# *******************************************************************************
*/


/** @file
 *    @brief
 *      Implements the Quaternion class.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MSIS
{
    /// <summary>
    ///     Provides the data structure, methods and operators to handle quaternions.
    /// </summary>
    /// <remarks>
    ///     Quaternions are a set of numbers extending the complex numbers. Quaternions not only have 
    ///     one imaginary part, but three of them and therefore they are often called hyper-complex numbers.
    ///     
    ///     The set of quaternions is often denoted as @f$\mathbb{H}@f$, honoring Sir William Rowan Hamilton, which first
    ///     described the quaternion algebra. In this documentation the following notation was used:
    ///     
    ///     A quaternion @f$q \in \mathbb{H}@f$ consists of a real part @f$r \in \mathbb{R}@f$ (also called scalar part) and
    ///     a three-dimensional vector part @f$\mathbf{v} = (v_x,v_y,v_z)^\mathrm{T} \in \mathbb{R}^{3 \times 1}@f$. A quaternion
    ///     is written as @f$q = [r, \mathbf{v}]@f$.
    /// </remarks>
    class Quaternion
    {
        /// <summary>
        ///     quaternion real part @f$r@f$
        /// </summary>
        protected double _r = 0;

        /// <summary>
        ///     quaternion vector part @f$\mathbf{v}@f$
        /// </summary>
        protected Vector3D _v = new Vector3D(0,0,0);

        /// <summary>
        ///     Default (empty) constructor for a quaternion.
        /// </summary>
        /// <remarks>
        ///     The empty constructor initializes the quaternion as @f$q = [0, (0,0,0)^\mathrm{T}]@f$.
        /// </remarks>
        public Quaternion() { }

        /// <summary>
        ///     Constructor for a quaternion defined by four single double values.
        /// </summary>
        /// <param name="real">real part</param>
        /// <param name="x">x-component of the vector part</param>
        /// <param name="y">y-component of the vector part</param>
        /// <param name="z">z-component of the vector part</param>
        public Quaternion(double real, double x, double y, double z)
        {
            this._r = real;
            this._v = new Vector3D(x, y, z);
        }

        /// <summary>
        ///     Constructor for a quaternion defined by one double value as real part and a Vector3D as vector part.
        /// </summary>
        /// <param name="real">real part</param>
        /// <param name="vec">vector part</param>
        public Quaternion(double real, Vector3D vec)
        {
            this._r = real;
            this._v = vec;
        }

        public static Quaternion operator +(Quaternion q1, Quaternion q2)
        {
            return new Quaternion(q1.r() + q2.r(), q1.v() + q2.v());
        }

        public static Quaternion operator *(Quaternion q1, Quaternion q2)
        {
            return new Quaternion(  q1.r() * q2.r() - q1.v().x() * q2.v().x() - q1.v().y() * q2.v().y() - q1.v().z() * q2.v().z(),
                                    q1.r() * q2.v().x() + q1.v().x() * q2.r() + q1.v().y() * q2.v().z() - q1.v().z() * q2.v().y(),
                                    q1.r() * q2.v().y() - q1.v().x() * q2.v().z() + q1.v().y() * q2.r() + q1.v().z() * q2.v().x(),
                                    q1.r() * q2.v().z() + q1.v().x() * q2.v().y() - q1.v().y() * q2.v().x() + q1.v().z() * q2.r());
        }

        public double r()
        {
            return this._r;
        }

        public Vector3D v()
        {
            return this._v;
        }

        public Quaternion conjugate()
        {
            return new Quaternion(this._r, -this._v);
        }

        public Vector3D getVector()
        {
            if (this.isPure())
            {
                return this._v;
            }
            else
            {
                throw new Exception("Error returning vector from Quaternion: Quaternion is not an pure Quaternion.");
            }
        }

        public bool isPure()
        {
            /*
            if (this._r == 0)
            {
                return true;
            }
            else
            {
                return false;
            }*/
            return true;
        }

        public double Norm()
        {
            return Math.Sqrt((this._r * this._r) + (this._v.x() * this._v.x()) + (this._v.y() * this._v.y()) + (this._v.z() * this._v.z()));
        }
    }
}
