using System;
using Sandbox.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;

namespace SE_Scripts.FANUCController
{
    public class Program : MyGridProgram
    {
        private IMyProgrammableBlock r1_controller;   
        private IMyProgrammableBlock t1_controller;   
        private IMyProgrammableBlock t2_controller;   
        private IMyProgrammableBlock r2_controller;   
        private IMyProgrammableBlock t3_controller;   

        private Configuration configuration;

        public struct Configuration
        {
            public float[] positions;

            public Configuration(float[] positions)
            {
                if(positions.Length != 5) throw new ArgumentException("Must be exactly 5 elements");
                this.positions = positions;
            }
        }
    
        public Program() {}

        public void Save(){}

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

        private float[] parseArgument(string argument)
        {
            string[] arguments = argument.Split(',');
            if (arguments.Length != 5) throw new ArgumentException("Must be exactly 5 elements");
            return Array.ConvertAll(arguments, float.Parse);
        }

        private void loadINI(MyIni INI)
        {
            r1_controller = GridTerminalSystem.GetBlockWithName(INI.Get("R1_Controller", "name").ToString()) as IMyProgrammableBlock;
            t1_controller = GridTerminalSystem.GetBlockWithName(INI.Get("T1_Controller", "name").ToString()) as IMyProgrammableBlock;
            t2_controller = GridTerminalSystem.GetBlockWithName(INI.Get("T2_Controller", "name").ToString()) as IMyProgrammableBlock;
            r2_controller = GridTerminalSystem.GetBlockWithName(INI.Get("R2_Controller", "name").ToString()) as IMyProgrammableBlock;
            t3_controller = GridTerminalSystem.GetBlockWithName(INI.Get("T3_Controller", "name").ToString()) as IMyProgrammableBlock;
        }

        public void Main(string argument, UpdateType updateSource)
        {
            try
            {
                MyIni INI = parseINI(Me.CustomData);
                Echo("INI parsed...");
                loadINI(INI);
                Echo("INI loaded...");
                configuration = new Configuration(parseArgument(argument));
                Echo($"Config [{string.Join(", ", configuration.positions)}] loaded...");
                Echo($"{r1_controller.CustomName}");
                r1_controller.TryRun(configuration.positions[0].ToString());
                t1_controller.TryRun(configuration.positions[1].ToString());
                t2_controller.TryRun(configuration.positions[2].ToString());
                r2_controller.TryRun(configuration.positions[3].ToString());
                t3_controller.TryRun(configuration.positions[4].ToString());
            }
            catch (System.Exception e)
            {
                Echo($"Error: {e.Message}");
                Runtime.UpdateFrequency = UpdateFrequency.None;
            }
        }

    }
}