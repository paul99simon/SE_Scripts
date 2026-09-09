using System;

// Space Engineers API references
using Sandbox.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRageMath;

namespace SE_Scripts.ServoHingeController
{
    public class Program : MyGridProgram
    {
        private IMyMotorStator hinge;
        private float angleTolerance;

        private float targetAngle = float.NaN;
        private float initialAngle = float.NaN;

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
            private const float period = MathHelper.Pi / 90f;
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
                if(minSpeed < 0 || 60 < minSpeed) throw new ArgumentException("minSpeed must be in [0, 30]");
                if(maxSpeed < 0 || 60 < maxSpeed) throw new ArgumentException("maxSpeed must be in [0, 30]");
                this.minSpeed = minSpeed;
                this.maxSpeed = maxSpeed;
                this.slope = (maxSpeed - minSpeed) / 180f;
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
            hinge = GridTerminalSystem.GetBlockWithName(INI.Get("Hinge", "name").ToString()) as IMyMotorStator;
            if (hinge == null)
            {
                Echo($"Hinge not found: {INI.Get("Hinge", "name").ToString()}");
                throw new System.Exception($"Hinge not found: {INI.Get("Hinge", "name").ToString()}");
            }

            angleTolerance = INI.Get("Hinge", "angle_tolerance").ToSingle(0.1f);
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
            if(lowerLimit < -90f ||  90f < lowerLimit) throw new ArgumentException("lowerLimit must be in [-90, 90]");
            if(upperLimit >  90f || -90f > upperLimit) throw new ArgumentException("upperLimit must be in [-90, 90]");
            if(upperLimit < lowerLimit) throw new ArgumentException("lowerlimit <= upperLimit must hold");
            if(currentAngle < -90f || currentAngle > 90f) throw new ArgumentException("currentAngle must be in [-90, 90]");
            if(targetAngle  < -90f || targetAngle  > 90f) throw new ArgumentException("targetAngle must be in [-90, 90]");
            if(currentAngle < lowerLimit || upperLimit < currentAngle) throw new ArgumentException("currentAngle must be in [lowerLimt, upperLimit]");
            if(targetAngle  < lowerLimit || upperLimit < targetAngle)  throw new ArgumentException("targetAngle must be in [lowerLimt, upperLimit]");

            float cDiff  = (currentAngle < upperLimit && upperLimit < targetAngle) ? float.MaxValue : targetAngle - currentAngle;
            float ccDiff = (targetAngle < lowerLimit && lowerLimit < currentAngle) ? float.MaxValue : targetAngle - currentAngle;
            result = Math.Abs(cDiff) < Math.Abs(ccDiff) ? cDiff : ccDiff;
            return result;
        }

        private bool isWithinTolerance(float difference,  float tolerance)
        {
            return Math.Abs(difference) <= tolerance;
        }

        public Program()
        {
            //Storage = "";
            Echo($"Storage : {Storage}");
            if(Storage != "")
            {
                float[] storage = Array.ConvertAll(Storage.Split(','), float.Parse);
                initialAngle = storage[0];
                targetAngle = storage[1];
                Storage = "";
            }
        }

        public void Save()
        {
            Storage = initialAngle.ToString() + "," + targetAngle.ToString();
        }
        
        public void Main(string argument, UpdateType updateSource)
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
            try
            {
                MyIni INI = parseINI(Me.CustomData);
                loadINI(INI);
                if(float.IsNaN(targetAngle))
                {
                    targetAngle = parseArgument(argument);
                }
                if(float.IsNaN(initialAngle))
                {
                    initialAngle = MathHelper.ToDegrees(hinge.Angle); 
                }
                float distanceTarget  = getDistance(MathHelper.ToDegrees(hinge.Angle), targetAngle, hinge.LowerLimitDeg, hinge.UpperLimitDeg);
                float distanceInitial = getDistance(MathHelper.ToDegrees(hinge.Angle), initialAngle, hinge.LowerLimitDeg, hinge.UpperLimitDeg);
                Echo($"Current Angle: {MathHelper.ToDegrees(hinge.Angle)}");
                Echo($"Target Angle: {targetAngle}");
                Echo($"Tolerance: {angleTolerance}");
                Echo($"Distance: {distanceTarget}");
                if(!isWithinTolerance(distanceTarget, angleTolerance))
                {
                    float speedTarget  = speedFunction.calculateSpeed(distanceTarget);
                    float speedInitial = speedFunction.calculateSpeed(distanceInitial);
                    hinge.TargetVelocityRPM = Math.Abs(speedInitial) < Math.Abs(speedTarget) ? Math.Sign(speedTarget) * Math.Abs(speedInitial) : speedTarget;
                    Echo($"Speed: {speedTarget}");
                }
                else
                {
                    hinge.TargetVelocityRPM = 0f;
                    Echo("Target reached!");
                    targetAngle = float.NaN;
                    initialAngle = float.NaN;
                    Runtime.UpdateFrequency = UpdateFrequency.None;
                }
            }
            catch (System.Exception e)
            {
                Echo($"Error: {e.Message}");
                Runtime.UpdateFrequency = UpdateFrequency.None;
            }
        }

    }
}