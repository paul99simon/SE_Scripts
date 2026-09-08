using System;

// Space Engineers API references
using Sandbox.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.GUI.TextPanel;
using VRageMath;
using VRage.Collections;
using VRage.Network;
using VRage.Utils;
using VRage.Audio;
using System.Xml.Schema;
using Sandbox.ModAPI.Interfaces;

namespace SE_Scripts.ServoController
{
    public class Program : MyGridProgram
    {
        private IMyMotorStator motor;
        private float angleTolerance;

        private ISpeedFunction speedFunction; //calculates speed as a function of distance

        public interface ISpeedFunction
        {
            float calculateSpeed(float distance);
        }

        public class RASCFunction : ISpeedFunction
        {
            private float minSpeed = 0.1f;
            private float maxSpeed = 5f;
            private float k = 5f; //smoothing parameter
            private float w = -0.75f; //Width bias: w in (-1 to 1).

            private float amplitude = 0;
            private const float period = MathHelper.Pi / 180f;
            private float atan_k = 0;
            
            //https://www.geogebra.org/calculator/cwu88q2e
            public RASCFunction(float minSpeed = 0.1f, float maxSpeed = 5f, float k = 5f, float w = -0.75f)
            {
                if(minSpeed < 0 || 30 < minSpeed) throw new ArgumentException("minSpeed must be in [0, 30]");
                if(maxSpeed < 0 || 30 < maxSpeed) throw new ArgumentException("maxSpeed must be in [0, 30]");
                if(k <= 0) throw new ArgumentException("k must be in (0, infinity)");
                if(w <= -1 ||  1 <= w) throw new ArgumentException("w must be in (-1, 1)");

                this.minSpeed = minSpeed;
                this.maxSpeed = maxSpeed;
                this.k = k;
                this.w = w;

                amplitude = (this.maxSpeed - this.minSpeed) / 2f;
                atan_k = (float) Math.Atan(k);
            }

            public RASCFunction(MyIni ini) : this( 
                ini.Get("SpeedFunction", "min_speed").ToSingle(0.1f), 
                ini.Get("SpeedFunction", "max_speed").ToSingle(5f),
                ini.Get("SpeedFunction", "k").ToSingle(5f),
                ini.Get("SpeedFunction", "w").ToSingle(-0.75f)){}

            private float f(float x)
            {
                float rawCos = (float)Math.Cos(x * period);
                float numerator = (float)Math.Atan(k * ((rawCos + w) / (1f + w * rawCos)));
                float smoothness_width_term = -1f * (numerator / atan_k);
                return amplitude * smoothness_width_term + amplitude + minSpeed;
            } 

            public float calculateSpeed(float distance)
            {
                return distance >= 0 ? f(distance) : -1f * f(distance);
            }
        }

        public class LinearSpeedFunction : ISpeedFunction
        {
            private float minSpeed = 0;
            private float maxSpeed = float.MaxValue;

            private float slope = 0;

            public LinearSpeedFunction(float minSpeed, float maxSpeed)
            {
                if(minSpeed < 0 || 30 < minSpeed) throw new ArgumentException("minSpeed must be in [0, 30]");
                if(maxSpeed < 0 || 30 < maxSpeed) throw new ArgumentException("maxSpeed must be in [0, 30]");
                this.minSpeed = minSpeed;
                this.maxSpeed = maxSpeed;
                this.slope = (maxSpeed - minSpeed) / 360f;
            }

            public LinearSpeedFunction(MyIni ini) : this(
                ini.Get("SpeedFunction", "min_speed").ToSingle(0.1f), 
                ini.Get("SpeedFunction", "max_speed").ToSingle(0.1f))
            {}

            public float calculateSpeed(float distance)
            {
                return (distance >= 0 ? 1 : -1) * (slope * Math.Abs(distance) + minSpeed);
            }
        }


        public Program()
        {
            //Runtime.UpdateFrequency = UpdateFrequency.Update100;
        }

        private MyIni parseINI(string customData)
        {
            MyIni INI = new MyIni();
            MyIniParseResult result;
            if (!INI.TryParse(customData, out result))
            {
                Echo($"CustomData error:\nLine {result}");
                throw new System.Exception($"CustomData error:\nLine {result}");
            }
            return INI;
        }

        private float parseArgument(string argument)
        {
            return float.Parse(argument);
        }

        private void loadINI(MyIni INI)
        {
            motor = GridTerminalSystem.GetBlockWithName(INI.Get("Rotor", "name").ToString()) as IMyMotorStator;
            if (motor == null)
            {
                Echo($"Rotor not found: {INI.Get("Rotor", "name").ToString()}");
                throw new System.Exception($"Rotor not found: {INI.Get("Rotor", "name").ToString()}");
            }

            angleTolerance = INI.Get("Rotor", "angle_tolerance").ToSingle(0.1f);
            if(angleTolerance <= 0f)
            {
                Echo($"Invalid angle_tolerance: {angleTolerance}");
                throw new System.Exception($"Invalid angle_tolerance: {angleTolerance}");
            }

            switch (INI.Get("SpeedFunction", "name").ToString())
            {
                case "rasc": // raised asymetric smooth cosine
                    speedFunction = new RASCFunction(INI);
                    break;
                case "linear":
                    speedFunction = new LinearSpeedFunction(INI);
                    break;
                default:
                    throw new NotSupportedException("Speed function not supported");
            }
        }

        public float getDistance(float currentAngle, float targetAngle, float lowerLimit, float upperLimit)
        {
            float result = 0;
            if((lowerLimit < -360.5f ||  360f < lowerLimit) && lowerLimit != float.MinValue) throw new ArgumentException("lowerLimit must be in [-360.5, 0] or must be equal to float.MinValue");
            if((upperLimit >  360.5f || -360f > upperLimit) && upperLimit != float.MaxValue) throw new ArgumentException("upperLimit must be in [0, 360.5] or must be equal to float.MaxValue");
            if(upperLimit < lowerLimit) throw new ArgumentException("lowerlimit <= upperLimit must hold");
            if(lowerLimit != float.MinValue || upperLimit != float.MaxValue)
            {
                if(currentAngle < -360.5f || currentAngle > 360.5f) throw new ArgumentException("currentAngle must be in [-360.5, 360.5]");
                if(targetAngle  < -360.5f || targetAngle  > 360.5f) throw new ArgumentException("targetAngle must be in [-360.5, 360.5]");
                if(currentAngle < lowerLimit || upperLimit < currentAngle) throw new ArgumentException("currentAngle must be in [lowerLimt, upperLimit]");
                if(targetAngle < lowerLimit || upperLimit < targetAngle) throw new ArgumentException("targetAngle must be in [lowerLimt, upperLimit]");

                float cDiff  = (currentAngle < upperLimit && upperLimit < targetAngle) ? float.MaxValue : targetAngle - currentAngle;
                float ccDiff = (targetAngle < lowerLimit && lowerLimit < currentAngle) ? float.MaxValue : targetAngle - currentAngle;

                result = Math.Abs(cDiff) < Math.Abs(ccDiff) ? cDiff : ccDiff;
            }
            else
            {
                if(currentAngle < 0f || currentAngle >= 360f) throw new ArgumentException("currentAngle must be in [0, 360)");
                if(targetAngle  < 0f || targetAngle  >= 360f) throw new ArgumentException("targetAngle must be in [0, 360)");
                
                // 1. Calculate the raw distance as if there is no 0/360 boundary
                float diff =  currentAngle - targetAngle; 

                // 2. Calculate the distance if we cross the boundary
                float wrappedDiff = diff > 0 ? diff - 360f : diff + 360f; 

                // 3. Return whichever path requires less physical travel
                result = Math.Abs(diff) < Math.Abs(wrappedDiff) ? diff : wrappedDiff;
            }
            return result;
        }

        private bool isWithinTolerance(float difference,  float tolerance)
        {
            return Math.Abs(difference) <= tolerance;
        }
        
        public void Main(string argument, UpdateType updateSource)
        {
            try
            {
                MyIni INI = parseINI(Me.CustomData);
                loadINI(INI);
                float targetAngle = parseArgument(Me.TerminalRunArgument);
                float distance = getDistance(MathHelper.ToDegrees(motor.Angle), targetAngle, motor.LowerLimitDeg, motor.UpperLimitDeg);
                float speed = speedFunction.calculateSpeed(distance);
                Echo($"Current Angle: {MathHelper.ToDegrees(motor.Angle)}");
                Echo($"Target Angle: {targetAngle}");
                Echo($"Tolerance: {angleTolerance}");
                Echo($"Distance: {distance}");
                Echo($"Speed: {speed}");
                if(!isWithinTolerance(distance, angleTolerance))
                {
                    motor.TargetVelocityRPM = speed;
                }
                else
                {
                    motor.TargetVelocityRPM = 0f;
                    Echo("Target reached!");
                }
            }
            catch (System.Exception e)
            {
                Echo($"Error: {e.Message}");
            }
        }

        public void Save()
        {
            //Storage = "-" + currentTarget.Angle;
            return;
            // This method is called when the program needs to save its state. Use
            // this method to save your state to the Storage field or some other
            // means. 
        }
    }
}