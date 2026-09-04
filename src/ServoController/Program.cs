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

namespace SE_Scripts.ServoController
{
    public class Program : MyGridProgram
    {
        private IMyMotorStator motor;
        private float angleTolerance;
        
        private float targetAngle = 0;

        private Func<float, float> speedFunction; //calculates speed as a function of distance

        public enum ProportionFunctions{}

 
        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
            //Storage.
        }

        private MyIni parseINI(string customData)
        {
            MyIni INI = new MyIni();
            MyIniParseResult result;
            if (!INI.TryParse(Me.CustomData, out result))
            {
                Echo($"CustomData error:\nLine {result}");
                throw new System.Exception($"CustomData error:\nLine {result}");
            }
            return INI;
        }

        private float parseArgument(string argument)
        {
            return 0;
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
            /*
            speedModeRPM = INI.Get("Rotor", "speed_mode_rpm").ToSingle(1f);
            if (speedModeRPM <= 0f)
            {
                Echo($"Invalid speed_mode_rpm: {speedModeRPM}");
                throw new System.Exception($"Invalid speed_mode_rpm: {speedModeRPM}");
            }

            precisionModeRPM = INI.Get("Rotor", "precision_mode_rpm").ToSingle(0.1f);
            if (precisionModeRPM <= 0f)
            {
                Echo($"Invalid precision_mode_rpm: {precisionModeRPM}");
                throw new System.Exception($"Invalid precision_mode_rpm: {precisionModeRPM}");      
            }

            precisionModeAngle = INI.Get("Rotor", "precision_mode_angle").ToSingle(1f);
            if (precisionModeAngle <= 0f)
            {
                Echo($"Invalid precision_mode_angle: {precisionModeAngle}");
                throw new System.Exception($"Invalid precision_mode_angle: {precisionModeAngle}");      
            }
            */
        }


        public static float getDistance(float currentAngle, float targetAngle, float lowerLimit, float upperLimit)
        {
            return 0;
        }

        private bool isWithinTolerance(float difference,  float tolerance)
        {
            return difference <= tolerance;
        }
        


        public void Main(string argument, UpdateType updateSource)
        {
            try
            {
                MyIni INI = parseINI(Me.CustomData);
                loadINI(INI);
                float targetAngle = parseArgument(argument);
                float distance = getDistance(MathHelper.ToDegrees(motor.Angle), targetAngle, motor.LowerLimitDeg, motor.UpperLimitDeg);
                float speed = speedFunction.Invoke(distance);
                if(!isWithinTolerance(distance, angleTolerance))
                {
                    motor.TargetVelocityRPM = speed;
                }
                else
                {
                    motor.TargetVelocityRPM = 0f;
                    Echo($"Target reached: {targetAngle}");
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