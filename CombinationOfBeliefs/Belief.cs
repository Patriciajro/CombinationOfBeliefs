using System;
using System.Collections.Generic;
using System.Linq;

namespace CombinationOfBeliefs
{
    public class Belief
    {
        public int OrderGroup { get; set; } = 0;

        private string? argumentName;
        public string? ArgumentName 
        {
            get
            {
                if (string.IsNullOrEmpty(argumentName))
                    return Argument;
                else
                    return argumentName;
            } 
            set
            {
                argumentName = value;
            }
        }
        public string? Argument { get; set; }
        public double Weight { get; set; }

        public bool IsPheta { get; set; }
        public bool IsPhy { get; set; }

        public Belief() { }

        public Belief(string argument, double weight, BeliefType beliefType = BeliefType.Normal ) 
        { 
            Argument = argument;
            Weight = weight;
            switch (beliefType)
            {
                case BeliefType.Pheta:
                    IsPheta = true;
                    OrderGroup = 1;
                    break;
                case BeliefType.Phy:
                    IsPhy = true;
                    OrderGroup = 2;
                    break;
                case BeliefType.Normal:
                default:
                    IsPheta = false;
                    IsPhy = false;
                    break;
            }
        }

        public Belief(string argumentName, string argument, double weight, BeliefType beliefType = BeliefType.Normal ) 
        { 
            ArgumentName = argumentName;
            Argument = argument;
            Weight = weight;
            switch (beliefType)
            {
                case BeliefType.Pheta:
                    IsPheta = true;
                    OrderGroup = 1;
                    break;
                case BeliefType.Phy:
                    IsPhy = true;
                    OrderGroup = 2;
                    break;
                case BeliefType.Normal:
                default:
                    IsPheta = false;
                    IsPhy = false;
                    break;
            }
        }

        public Belief(string line)
        {
            if (line.Contains("\t"))
            {
                var tabLine = line.Split('\t');
                if (tabLine.Length >= 2)
                {
                    Argument = tabLine[0];
                    Weight = Convert.ToDouble(tabLine[1]);
                }
            }
            else if (line.Contains(";"))
            {
                var semicolonLine = line.Split(';');
                if (semicolonLine.Length >= 2)
                {
                    Argument = semicolonLine[0];
                    Weight = Convert.ToDouble(semicolonLine[1]);
                }
            }

            if (Argument == "Pheta")
            {
                IsPheta = true;
                OrderGroup = 1;
            }
            if (Argument == "Phy")
            {
                IsPhy = true;
                OrderGroup = 2;
            }
        }

        public void Print()
        {
            Console.WriteLine($"{Argument}\t{Weight}");

        }

        public override string ToString()
        {
            return $"{Argument}\t{Weight}";
        }
        public string ToMassString()
        {
            return $"m({Argument})={Weight}";
        }
        public string ToNameString()
        {
            return $"{Weight}\t{ArgumentName}";
        }

        public string LabelNameString
        {
            get
            {
                return $"\"{ArgumentName}\"";
            }
        }
        public string LabelString
        {
            get
            {
                return $"\"{Argument}\"";
            }
        }
        public string DataString
        {
            get
            {
                return $"{Weight}";
            }
        }
    }

    public enum BeliefType
    {
        Normal,
        Pheta,
        Phy
    }
}