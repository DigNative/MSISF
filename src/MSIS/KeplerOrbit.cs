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
#                   mail: <mail@rene-schwarz.com>
#                   web:  http://www.rene-schwarz.com
#                   on behalf of the German Aerospace Center (DLR)
#  Date:            2012/05/17
#  Filename:        /src/MSIS/KeplerOrbit.cs
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
    class KeplerOrbit
    {
        private const double _moon_radius = 1737150;
        private const double _GM = 4.90277790e+012;

        double _epoch = 51544.5;
        double _a = 2000000 + _moon_radius;
        double _e = 1;
        double _omega = 0;
        double _Omega = 0;
        double _i = 0;
        double _M0 = 0;

        public KeplerOrbit() { }

        public void setKeplerElements(double a, double e, double omega, double Omega, double i, double M0)
        {
            try
            {
                this._set_a(a);
                this._set_e(e);
                this._set_omega(omega);
                this._set_Omega(Omega);
                this._set_i(i);
                this._set_M0(M0);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void setKeplerElements(double epoch, double a, double e, double omega, double Omega, double i, double M0)
        {
            try
            {
                this.setEpoch(epoch);
                this._set_a(a);
                this._set_e(e);
                this._set_omega(omega);
                this._set_Omega(Omega);
                this._set_i(i);
                this._set_M0(M0);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public KeplerOrbit(Vector3D r, Vector3D dr)
        {
            Vector3D h = r % dr;
            Vector3D ev = ((dr % h) / _GM) - (r / r.norm());
            Vector3D n = new Vector3D(0, 0, 1) % h;
            double nu = 0;

            if (r * dr >= 0)
            {
                nu = Math.Acos((ev * r) / (ev.norm() * r.norm()));
            }
            else
            {
                nu = (2*Math.PI - Math.Acos((ev * r) / (ev.norm() * r.norm())));
            }

            double E = Math.Acos((ev.norm() + Math.Cos(nu)) / (1 + (ev.norm() * Math.Cos(nu))));
            double i = Math.Acos(h.z()/h.norm());
            double e = ev.norm();
            double Omega = 0;

            if (n.y() >= 0)
            {
                Omega = Math.Acos(n.x() / n.norm());
            }
            else
            {
                Omega = 2*Math.PI - Math.Acos(n.x() / n.norm());
            }

            double omega = 0;

            if (ev.z() >= 0)
            {
                omega = Math.Acos((n*ev) / (n.norm()*ev.norm()));
            }
            else
            {
                omega = 2*Math.PI - Math.Acos((n * ev) / (n.norm() * ev.norm()));
            }

            double M = E - (e * Math.Sin(E));
            double a = 1 / ((2/r.norm()) - (Math.Pow(dr.norm(),2)/_GM));

            this.setKeplerElements(a, e, omega, Omega, i, M);
        }

        public Vector3D getPosition(double t)
        {
            double M = 0;
            double dt = 0;
            double E = 0;
            double nu = 0;
            double r_c = 0;
            Vector3D o = new Vector3D();
            //Vector3D d_o = new Vector3D();
            Vector3D r = new Vector3D();

            if (t == this._epoch)
            {
                M = this._M0;
            }
            else
            {
                dt = 86400 * (t - this._epoch);
                M = this._M0 + dt * Math.Sqrt((_GM) / (Math.Pow(this._a, 3)));
                M = tools.normalizeAngle(M);
            }

            E = tools.solveKeplerForE(M, this._e);
            nu = 2 * Math.Atan2(Math.Sqrt(1 + this._e) * Math.Sin(E / 2), Math.Sqrt(1 - this._e) * Math.Cos(E / 2));
            r_c = this._a * (1 - this._e * Math.Cos(E));
            o = r_c * new Vector3D(Math.Cos(nu), Math.Sin(nu), 0);
            //d_o = (Math.Sqrt(_GM * this._a) / r_c) * new Vector3D(-Math.Sin(E), Math.Sqrt(1 - Math.Pow(this._e, 2)) * Math.Cos(E), 0);

            r = new Vector3D(
                    o.x() * (Math.Cos(this._omega) * Math.Cos(this._Omega) - Math.Sin(this._omega) * Math.Cos(this._i) * Math.Sin(this._Omega)) - o.y() * (Math.Sin(this._omega) * Math.Cos(this._Omega) + Math.Cos(this._omega) * Math.Cos(this._i) * Math.Sin(this._Omega)),
                    o.x() * (Math.Cos(this._omega) * Math.Sin(this._Omega) - Math.Sin(this._omega) * Math.Cos(this._i) * Math.Cos(this._Omega)) + o.y() * (Math.Cos(this._omega) * Math.Cos(this._i) * Math.Cos(this._Omega) - Math.Sin(this._omega) * Math.Sin(this._Omega)),
                    o.x() * (Math.Sin(this._omega) * Math.Sin(this._i)) + o.y() * (Math.Cos(this._omega) * Math.Sin(this._i)));

            //Console.WriteLine(o.x().ToString("e"));
            //Console.WriteLine(o.y().ToString("e"));
            //Console.WriteLine(o.z().ToString("e"));

            return r;
        }

        protected void _set_a(double a)
        {
            if (a > _moon_radius)
            {
                this._a = a;
            }
            else
            {
                throw new Exception("S/C orbit shape error: Semi-major axis a is smaller than the Moon's radius.");
            }
        }

        protected void _set_e(double e)
        {
            if (e > 0 && e <= 1)
            {
                this._e = e;
            }
            else
            {
                throw new Exception("S/C orbit shape error: This software is not made to calculate non-circular/non-elliptic orbits.\nOrbit eccentricity e is not in ]0,1].");
            }
        }

        protected void _set_omega(double omega)
        {
            if (omega >= 0 && omega < 2*Math.PI)
            {
                this._omega = omega;
            }
            else
            {
                throw new Exception("S/C orbit shape error: Argument of periapsis omega is not in [0,2\\pi[.");
            }
        }

        protected void _set_Omega(double Omega)
        {
            if (Omega >= 0 && Omega < 2 * Math.PI)
            {
                this._Omega = Omega;
            }
            else
            {
                throw new Exception("S/C orbit shape error: Longitude of ascending node Omega is not in [0,2\\pi[.");
            }
        }

        protected void _set_i(double i)
        {
            if (i >= 0 && i < 2 * Math.PI)
            {
                this._i = i;
            }
            else
            {
                throw new Exception("S/C orbit shape error: Inclination i is not in [0,2\\pi[.");
            }
        }

        protected void _set_M0(double M0)
        {
            if (M0 >= 0 && M0 < 2 * Math.PI)
            {
                this._M0 = M0;
            }
            else
            {
                throw new Exception("S/C orbit shape error: Mean anomaly M_0 at epoch t_0 is not in [0,2\\pi[.");
            }
        }

        public void setEpoch(double epoch)
        {
            if (epoch >= 0)
            {
                this._epoch = epoch;
            }
            else
            {
                throw new Exception("Epoch is less than 0.");
            }
        }

        public double getEpoch()
        {
            return this._epoch;
        }

        public string getEpochAsUTCString()
        {
            return tools.MJDtoUTC(this._epoch).ToString("s");
        }

        public double getSemiMajorAxis()
        {
            return this._a;
        }

        public double getEccentricity()
        {
            return this._e;
        }

        public double getArgumentOfPeriapsis()
        {
            return this._omega;
        }

        public double getLongOfAscNode()
        {
            return this._Omega;
        }

        public double getInclination()
        {
            return this._i;
        }

        public double getMeanAnomalyAtEpoch()
        {
            return this._M0;
        }
    }
}
