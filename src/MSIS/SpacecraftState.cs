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
#  Filename:        /src/MSIS/SpacecraftState.cs
#  License:         GNU General Public License (GPL), version 2 or later
#
# *******************************************************************************
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MSIS
{
    class SpacecraftState: Spacecraft
    {
        [DllImport("../lib/SPICEhelper.dll")]
        static extern void calculateSunPosition(string utc, ref double x, ref double y, ref double z);

        protected double _time = 0;
        protected Vector3D _position = new Vector3D();
        protected Vector3D _sun_position = new Vector3D();
        protected Quaternion _orientation = new Quaternion(1,0,0,0);
        protected Vector3D _POV_right = new Vector3D(0, (Convert.ToDouble(Program.sim.getWidth()) / Convert.ToDouble(Program.sim.getHeight())), 0);
        protected Vector3D _POV_direction = new Vector3D(-1, 0, 0);
        protected Vector3D _POV_up = new Vector3D(0, 0, 1);
        public Dictionary<Vector2D, PixelInformation> PixelInfo = new Dictionary<Vector2D, PixelInformation>();

        public SpacecraftState(Spacecraft sc)
        {
            this._kepler_orbit = sc.getKeplerOrbit();
            this._initial_orientation = sc.getInitialOrientation();
            this._orientation_transition = sc.getOrientationTransition();
            this.orientation_given = sc.isOrientationGiven();
            this._fixed_position = sc._fixed_position;
            this.fixedPositionGiven = sc.fixedPositionGiven;
        }

        public void setTime(double time)
        {
            this._time = time;
            if (this.fixedPositionGiven)
            {
                this._position = this._fixed_position;
            }
            else
            {
                this._position = this._kepler_orbit.getPosition(time);
            }
            this._sun_position = this.calculateSunPosition();

            if (this.isOrientationGiven())
            {
                // take orientation transition into account
                this._orientation = new RotationQuaternion(new Vector3D(0, 0, 1), this._orientation_transition.z()) *
                                    new RotationQuaternion(new Vector3D(0, 1, 0), this._orientation_transition.y()) *
                                    new RotationQuaternion(new Vector3D(1, 0, 0), this._orientation_transition.x()) *
                                    this._initial_orientation;
                this._POV_right = this._POV_right.QuaternionRotate(this._orientation);
                this._POV_direction = ((new Vector3D(-1, 0, 0).QuaternionRotate(this._orientation) / (new Vector3D(-1, 0, 0).QuaternionRotate(this._orientation).norm())) * ((0.5 * this._POV_right.norm()) / Math.Tan(tools.deg2rad(Program.sim.getFOV()) / 2)));
            }
            else
            {
                // calculate necessary orientation quaternion to look nadir
                double phi, theta, r;
                r = this.getPosition().norm();
                phi = Math.Atan2(this.getPosition().y(), this.getPosition().x());
                theta = Math.Acos(this.getPosition().z() / r);

                this._orientation = new RotationQuaternion(new Vector3D(0, 1, 0), theta) * new RotationQuaternion(new Vector3D(0, 0, 1), -phi);
                this._POV_right = this._POV_right.QuaternionRotate(this._orientation);
                this._POV_direction = ((new Vector3D(-this.getPosition().x(), -this.getPosition().y(), -this.getPosition().z()) / (new Vector3D(-this.getPosition().x(), -this.getPosition().y(), -this.getPosition().z()).norm())) * ((0.5 * this._POV_right.norm()) / Math.Tan(tools.deg2rad(Program.sim.getFOV()) / 2))); 
            }

            this._POV_up = this._POV_right % this._POV_direction; // cross product
            this._POV_up = this._POV_up / this._POV_up.norm();
        }

        public Vector3D getPosition()
        {
            return this._position;
        }

        public double getTime()
        {
            return this._time;
        }

        private Vector3D calculateSunPosition()
        {
            double x = 0;
            double y = 0;
            double z = 0;
            string UTCString = tools.MJDtoUTC(this._time).ToString("s");

            calculateSunPosition(UTCString, ref x, ref y, ref z);

            return new Vector3D(x, y, z);
        }

        public Vector3D getSunPosition()
        {
            return this._sun_position;
        }

        public Quaternion getOrientation()
        {
            return this._orientation;
        }

        public Vector3D getPOVRight()
        {
            return this._POV_right;
        }

        public Vector3D getPOVDirection()
        {
            return this._POV_direction;
        }

        public Vector3D getPOVUp()
        {
            return this._POV_up;
        }
    }
}
