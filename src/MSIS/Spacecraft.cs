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
#  Filename:        /src/MSIS/Spacecraft.cs
#  License:         GNU General Public License (GPL), version 2 or later
#
# *******************************************************************************
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MSIS
{
    class Spacecraft
    {
        protected KeplerOrbit _kepler_orbit = new KeplerOrbit();
        public Vector3D _fixed_position = new Vector3D(0, 0, 0);
        protected Quaternion _initial_orientation = new Quaternion(1,0,0,0);
        protected Vector3D _orientation_transition = new Vector3D(0,0,0);
        private Dictionary<Int32, SpacecraftState> _spacecraft_state = new Dictionary<Int32, SpacecraftState>();
        protected bool orientation_given = false;
        public bool fixedPositionGiven = false;
        double _fixed_simulation_time = 0;

        public void setEpoch(double epoch)
        {
            this._kepler_orbit.setEpoch(epoch);
        }

        public double getEpoch()
        {
            return this._kepler_orbit.getEpoch();
        }

        public string getEpochAsUTCString()
        {
            return this._kepler_orbit.getEpochAsUTCString();
        }

        public void setKeplerOrbitParameters(double a, double e, double omega, double Omega, double i, double M0)
        {
            try
            {
                this._kepler_orbit.setKeplerElements(a, e, omega, Omega, i, M0);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public double getSemiMajorAxis()
        {
            return this._kepler_orbit.getSemiMajorAxis();
        }

        public double getEccentricity()
        {
            return this._kepler_orbit.getEccentricity();
        }

        public double getArgumentOfPeriapsis()
        {
            return this._kepler_orbit.getArgumentOfPeriapsis();
        }

        public double getLongOfAscNode()
        {
            return this._kepler_orbit.getLongOfAscNode();
        }

        public double getInclination()
        {
            return this._kepler_orbit.getInclination();
        }

        public double getMeanAnomalyAtEpoch()
        {
            return this._kepler_orbit.getMeanAnomalyAtEpoch();
        }

        public void setStateVectors(Vector3D r, Vector3D dr)
        {
            this._kepler_orbit = new KeplerOrbit(r, dr);
        }

        public void setPosition(Vector3D r)
        {
            this._fixed_position = r;
            this.fixedPositionGiven = true;
        }

        public void setOrientation(Quaternion orientation)
        {
            this._initial_orientation = orientation;
            this.orientation_given = true;
        }

        public Quaternion getInitialOrientation()
        {
            return this._initial_orientation;
        }

        public void setOrientationTransition(Vector3D trans)
        {
            this._orientation_transition = trans;
        }

        public Vector3D getOrientationTransition()
        {
            return this._orientation_transition;
        }

        public int addSpacecraftState(double time)
        {
            Int32 id = this._spacecraft_state.Count + 1;
            this._spacecraft_state.Add(id, new SpacecraftState(this));
            this._spacecraft_state[id].setTime(time);
            return id;
        }

        public KeplerOrbit getKeplerOrbit()
        {
            return this._kepler_orbit;
        }

        public SpacecraftState getSpacecraftState(int i)
        {
            return this._spacecraft_state[i];
        }

        public bool isOrientationGiven()
        {
            return this.orientation_given;
        }

        public void setFixedTime(double time)
        {
            this._fixed_simulation_time = time;
        }

        public double getFixedTime()
        {
            return this._fixed_simulation_time;
        }
    }
}
