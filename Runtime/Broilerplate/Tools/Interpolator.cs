using System;
using System.Collections;
using UnityEngine;

//Courtesy of PhantomLab
namespace Broilerplate.Tools {
    public class Interpolator {
        public Interpolator(float duration = 2f) {
            this.duration = duration;
        }

        private float startTime = float.NegativeInfinity;
        private readonly float duration;
        private float stateA = 1f;
        private float stateB;
        private float currentValue;

        private float timeOfPause = 0f;

        public bool IsDone =>  Mathf.Abs(Sample() - stateB) <= 0.00001f;

        public bool DirectionIsZeroOne => stateA == 0;
        public float Duration => duration;

        public void Pause() {
            if(timeOfPause != 0f) {
                return;
            }
            Sample();
            timeOfPause = Time.time;
        }

        public void Resume() {
            if(timeOfPause == 0f) {
                return;
            }
            startTime = Time.time-(timeOfPause-startTime);
            timeOfPause = 0f;
        }

        public void StartFade01(bool reset = false) {
            StartFade(0f, 1f, reset);
        }

        public void StartFade10(bool reset = false) {
            StartFade(1f, 0f, reset);
        }

        public void Force1()
        {
            Force(1);
        }

        public void Force0() {
            Force(0);
        }

        private void Force(float t)
        {
            stateA = 1f - t;
            stateB = t;
            startTime = Time.time - duration;
        }

        void StartFade(float a, float b, bool reset) {
            stateA = a;
            stateB = b;
            if(reset)
            {
                //Just start at A and go to B
                startTime = Time.time;
            } else {
                //If the interpolator gets interrupted in the middle of a fading, continue at the same position
                startTime = Mathf.Lerp(Time.time, Time.time - duration, Mathf.InverseLerp(stateA, stateB, currentValue));
            }
        }

        public float Sample() {
            if(timeOfPause != 0f) {
                return currentValue;
            }
            currentValue = Mathf.Lerp(stateA, stateB, Mathf.Clamp01((Time.time - startTime) / duration));
            //Debug.Log(currentValue);
            return currentValue;
        }

        public float SampleTime() {
            return Mathf.Clamp((Time.time - startTime), 0, duration) ;
        }

        public static void StopAnim8(Coroutine routine) {
            CoroutineJobs.StopJob(routine);
        }

        public static Coroutine Anim8(Coroutine previous, float duration, bool directionIs01, Action<float> onSample, Action onFinish) {
            CoroutineJobs.StopJob(previous);
            return Anim8(duration, directionIs01, onSample, onFinish);
        }

        public static Coroutine Anim8(float duration,bool directionIs01,Action<float> onSample, Action onFinish) {
            return CoroutineJobs.StartJob(new Interpolator(duration)._Anim8(directionIs01,onSample,onFinish));
        }

        private IEnumerator _Anim8(bool directionIs01, Action<float> onSample, Action onFinish)
        {
            if(onSample == null) {
                yield return new WaitForSecondsRealtime(duration);
            } else {
                if(directionIs01) {
                    StartFade01(true);
                } else {
                    StartFade10(true);
                }

                while(!IsDone) {
                    onSample(Sample());
                    yield return null;
                }
                onSample(Sample());
            }

            onFinish?.Invoke();
        }
    }
    
    public class CoroutineJobs : MonoBehaviour {
        private static CoroutineJobs localInstance; // scene local jobs
        private static CoroutineJobs globalInstance; // jobs that run across multiple scenes

        private static CoroutineJobs GetJobs(bool global) {
            if (global) {
                if (!globalInstance) {
                    var go = new GameObject("InterpolatorHelper - Global");
                    globalInstance = go.AddComponent<CoroutineJobs>();
                    DontDestroyOnLoad(go);
                }

                return globalInstance;
            }

            if (!localInstance) {
                var go = new GameObject("InterpolatorHelper - Scene");
                localInstance = go.AddComponent<CoroutineJobs>();
            }

            return localInstance;
        }

        public static Coroutine StartJob(IEnumerator ie, bool global = false) {
            
            return GetJobs(global)._StartJob(ie);
        }
        
        public static void RestartJob(IEnumerator ie, ref Coroutine routine, bool global = false) {
            StopJob(routine);
            
            routine = StartJob(ie, global);
        }
        
        public static void StopJob(Coroutine previous, bool global = false) {
            if (previous != null) {
                GetJobs(global).StopCoroutine(previous);
            }
        }

        private Coroutine _StartJob(IEnumerator ie)
        {
            return StartCoroutine(ie);
        }
    }
}