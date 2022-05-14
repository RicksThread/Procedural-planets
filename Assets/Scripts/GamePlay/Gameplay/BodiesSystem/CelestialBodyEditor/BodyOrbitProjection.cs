using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Celestial
{
    public class BodyOrbitProjection : MonoBehaviour
    {
        List<Orbit> orbits = new List<Orbit>();
        public LineRenderer lineRendererPrefab;
        public int samples;
        public float timeStep;

        public CelestialBody relative;

        [SerializeField] bool active;

        public void Refresh(){
            if(!active) return;
            //get celestialBodies
            CelestialBody[] celestialBodies = FindObjectsOfType<CelestialBody>();
            if (orbits.Count > 0) ClearNullOrbits(); //clear bodies that do not contain a celestial body or linerenderer
            
            AddLineRenderers(celestialBodies); //add linerenderer to every celestial body's gameobject that doesn't have it
            if(celestialBodies.Length > 0)AddOrbit(celestialBodies); //add an orbit to every celestial body that hasn't got one

            //simulate then the orbits
            Simulate();
        }


        public void Clear(){
            ResetOrbits();
        }

        //it adds the linerenderers to each celestial object
        void AddLineRenderers(CelestialBody[] bodies){
            for(int i = 0; i < bodies.Length; i++){
                if (bodies[i].gameObject.GetComponent<LineRenderer>() == null){
                    LineRenderer line = bodies[i].gameObject.AddComponent<LineRenderer>();
                    line.sharedMaterial = lineRendererPrefab.sharedMaterial;
                    line.widthCurve = lineRendererPrefab.widthCurve;
                    line.startWidth = lineRendererPrefab.startWidth;
                    line.endWidth = lineRendererPrefab.endWidth;
                    line.widthMultiplier = lineRendererPrefab.widthMultiplier;
                }else{
                    LineRenderer line = bodies[i].gameObject.GetComponent<LineRenderer>();
                    line.sharedMaterial = lineRendererPrefab.sharedMaterial;
                    line.widthCurve = lineRendererPrefab.widthCurve;
                    line.startWidth = lineRendererPrefab.startWidth;
                    line.endWidth = lineRendererPrefab.endWidth;
                    line.widthMultiplier = lineRendererPrefab.widthMultiplier;
                }
            }
        }

        void ClearNullOrbits(){
            for(int i = 0; i < orbits.Count; i++){
                if(!orbits[i].integrity) orbits.Remove(orbits[i]);
            }
        }

        bool isInList(CelestialBody celestialBody){
            for(int i = 0; i < orbits.Count; i++){
                if(orbits[i].IsSameBody(celestialBody)) return true;
            }
            return false;
        }

        void AddOrbit(CelestialBody[] celestialBodies){
            foreach(CelestialBody body in celestialBodies){
                if (!isInList(body)){
                    LineRenderer renderer = body.gameObject.GetComponent<LineRenderer>();
                    Orbit orbit = new Orbit(body,renderer);
                    orbits.Add(orbit);
                }
            }
        }

        void ResetOrbits(){
            for(int i = 0; i < orbits.Count; i++){
                //prepare orbits
                orbits[i].ClearRefresh();
            } 
        }

        public void Simulate(){
            
            ResetOrbits();
            for (int i = 1; i < samples; i++)
            {
                for(int k = 0; k < orbits.Count; k++){
                    //simulate
                    SimulateGravity(orbits[k],i);
                }   
            }
            foreach(Orbit orbit in orbits){
                //set linerenderers
                orbit.SetLineRenderToSamples();
            }    
        }

        void SimulateGravity(Orbit orbit, int t){
            Vector3 force = Vector3.zero;
            for(int i = 0; i < orbits.Count; i++){
                if(orbits[i] != orbit) force += CelestialBody.GetGravityForce(orbit[t-1], orbit.BodyMass, orbits[i][t-1], orbits[i].BodyMass);
            }
            orbit.AddForceTrackedVelocity(force * timeStep);
            Orbit orbitRelative = FindOrbitFromBody(relative);
            orbit.AddSample(orbit[t-1]+orbit.trackedVelocity*timeStep - (orbitRelative!= null ? orbitRelative.trackedVelocity*timeStep : Vector3.zero));
        }

        Orbit FindOrbitFromBody(CelestialBody body){
            for(int i = 0; i < orbits.Count; i++){
                if(orbits[i].IsSameBody(body)) return orbits[i];
            }
            return null;
        }
    }

    public class Orbit{
        CelestialBody celestialBody;
        LineRenderer lineRenderer;
        List<Vector3> samples = new List<Vector3>();
        
        public Vector3 trackedVelocity{
            get;
            private set;
        }

        public bool integrity{
            get{
                return celestialBody != null && lineRenderer != null;
            }
        }

        public Vector3 this[int index]{
            get{
                return samples[index];
            }
        }

        public float BodyMass{
            get{
                return celestialBody.mass;
            }
        }

        public bool IsSameBody(CelestialBody body){
            return celestialBody == body;
        }

        public Orbit(CelestialBody celestialBody, LineRenderer lineRenderer){
            this.celestialBody = celestialBody;
            this.lineRenderer = lineRenderer;

            SetStartState();
        }

        void SetStartState(){
            samples.Clear();
            trackedVelocity = celestialBody.startVelocity;
            samples.Add(celestialBody.transform.position);

            lineRenderer.positionCount = 1;
            lineRenderer.SetPositions(new Vector3[] {Vector3.zero});
        }

        public void AddForceTrackedVelocity(Vector3 force){
            trackedVelocity += force/celestialBody.mass;
        }

        public void ClearRefresh(){
            SetStartState();
        }

        public void SetLineRenderToSamples(){
            lineRenderer.positionCount = samples.Count;
            lineRenderer.SetPositions(samples.ToArray());
        }

        public void AddSample(Vector3 pos){
            samples.Add(pos);
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(BodyOrbitProjection))]
    public class BodyOrbitEditor : Editor
    {

        BodyOrbitProjection projection;
    
        void OnEnable() {
            if(projection == null){
                projection = (BodyOrbitProjection)target;
                projection.Refresh();
            }
        }

        public override void OnInspectorGUI(){
            base.OnInspectorGUI();
            if(GUILayout.Button("Refresh")){
                projection.Refresh();
            }
            if(GUILayout.Button("Clear")){
                projection.Clear();
            }
        }
    }
#endif
}