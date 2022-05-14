using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Celestial{
    ///<summary>
    ///Coordinates the gravitational actions of each celestial body to attract every body
    ///</summary>
    public class CelestialBodiesManager : BodiesManager
    {

        public override void FixedUpdate() 
        {
            //add gravity force to each object
            
            for(int i = 0; i <  CelestialBody.celestialBodies.Count; i++)
            {
                for(int k = 0; k <  Body.bodies.Count; k++)
                {
                    //if it's not the same body then attract
                    if(CelestialBody.celestialBodies[i] != Body.bodies[k])
                    {
                        CelestialBody.celestialBodies[i].AttractOther(Body.bodies[k]);
                    }
                }
            }

            //call the bodyPhysicsFixedUpdate of each body
            base.FixedUpdate();
        }

    }
}
