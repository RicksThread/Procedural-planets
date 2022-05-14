using System.Collections;
using System.Collections.Generic;
using UnityEngine; 
using UnityEditor;

namespace Celestial{
    //class that attracts every type of object
    public class CelestialBody : Body
    {
        public static List<CelestialBody> celestialBodies = new List<CelestialBody>();
        public Vector3 startVelocity = Vector3.zero; //the starting velocity of the celestial body (units per second)
        
        public const float G = 0.00001f;
        [SerializeField] float surfaceGravity = 10;
        [SerializeField] float radius = 100;


        #if UNITY_EDITOR
        private void OnValidate() 
        {
            if(FindObjectOfType<BodyOrbitProjection>() != null) FindObjectOfType<BodyOrbitProjection>().Refresh();
            mass = surfaceGravity * radius * radius / G;            
        }
        #endif

        protected override void OnEnable() 
        {
            base.OnEnable();
            celestialBodies.Add(this);
        }

        protected override void Start()
        {
            base.Start();
            BodyPhysicsVelocity = startVelocity;
        }

        ///<summary>
        ///Attracts the given body to this body
        ///</summary>
        ///<param name="body"> body to be attracted </param>
        public void AttractOther(Body body)
        {
            
            Vector3 force = GetGravityForce(body.transform.position, body.mass, this.transform.position, this.mass);
            body.AddForce(force,this);
        }

        public override void AddForce(Vector3 force, Body fromBody)
        {
            base.AddForce(force, fromBody);
        }

        //attract body from the formula: G * (m1*m2) / d^2
        public static Vector3 GetGravityForce(Vector3 posA, float massA, Vector3 posB, float massB)
        {
            //get direction from bodytarget to this
            Vector3 direction = posB - posA;
            float distance = direction.magnitude <0.05f ? 0.05f : direction.magnitude;
            float intensity = CelestialBody.G*
            (massB* massA)/
            (distance*distance) 
            ;
            return  intensity * direction.normalized;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            celestialBodies.Remove(this);
        }

    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(Celestial.CelestialBody))]
public class CelestialBodyEditor : Editor{
    Celestial.BodyOrbitProjection bodyOrbitManager;
    private void OnEnable() 
    {
        if (FindObjectOfType<Celestial.BodyOrbitProjection>() != null)
        {
            bodyOrbitManager = FindObjectOfType<Celestial.BodyOrbitProjection>();
            bodyOrbitManager.Refresh();
        }
    }
}
#endif