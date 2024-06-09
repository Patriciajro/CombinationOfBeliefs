using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CombinationOfBeliefs
{
    public class BeliefSystem
    {
        public static BeliefSystem Load(string path)
        {
            BeliefSystem beliefSystem = new BeliefSystem();

            // Read a text file line by line.  
            string[] lines = File.ReadAllLines(path);

            foreach (string line in lines)
                if (!string.IsNullOrEmpty(line)) beliefSystem.Matrix.Add(new Belief(line));

            beliefSystem.Name = Path.GetFileName(path).Replace(".txt", "");

            LocalPath = Path.GetDirectoryName(path);
            return beliefSystem;
        }

        public static string LocalPath { get; set; } = "";

        public string? Name { get; set; }
        public List<Belief> Matrix { get; set; } = [];

        public List<BeliefSystem> CombinedBeliefSystems = [];

        private double? deltaSquaredMax;

        private double? deltaSquaredMin;

        public double DeltaSquaredMax 
        { 
            get
            {
                if( deltaSquaredMax == null )
                {
                    FindInternalDeltas();
                }
                return deltaSquaredMax ?? 0;
            }
        }
        public double DeltaSquaredMin
        {
            get
            {
                if (deltaSquaredMin == null)
                {
                    FindInternalDeltas();
                }
                return deltaSquaredMin ?? 0;
            }
        }

        public BeliefSystem() 
        {
        }

        public BeliefSystem(BeliefSystem beliefSystem)
        {
            Matrix = beliefSystem.Matrix;
            PrepareBeliefSystem();
        }

        public BeliefSystem(List<Belief> matrix) 
        {
            Matrix = matrix;
            PrepareBeliefSystem();
        }

        public string[] GetDiscernementFrame()
        {
            var arguments = Matrix.Where(w => !string.IsNullOrEmpty(w.Argument) && !w.IsPheta && w.Argument != "Pheta")
                      .OrderBy(o => o.OrderGroup)
                      .OrderBy(o => o.Argument)
                      .Select(s => s.Argument)
                      .ToArray();

            return arguments;
        }
        
        public string[] GetCombinedDiscernementFrame(BeliefSystem beliefSystem)
        {
            var argumentsA = GetDiscernementFrame();
            var argumentsB = beliefSystem.GetDiscernementFrame();

            foreach (var argument in argumentsB)
            {
                if (!argumentsA.Any(a => a == argument))
                {
                    argumentsA.Append(argument);
                }
            }
            return argumentsA;
        }

        public double Compare(BeliefSystem beliefSystem)
        {
            double deltaSquared = 0;
            var discrenmentFrame = GetCombinedDiscernementFrame(beliefSystem);

            foreach (var argument in discrenmentFrame)
            {
                var weightA = this.Matrix.FirstOrDefault(f => f.Argument == argument)?.Weight ?? 0;
                var weightB = beliefSystem.Matrix.FirstOrDefault(f => f.Argument == argument)?.Weight ?? 0;

                deltaSquared += ((weightA - weightB) * (weightA - weightB));
            }

            return deltaSquared;
        }

        public double CompareFuzzyMenbership(BeliefSystem beliefSystem)
        {
            double fuzzyMenbership = 0;
            double deltaSquared = 0;
            var discrenmentFrame = GetCombinedDiscernementFrame(beliefSystem);

            foreach (var argument in discrenmentFrame)
            {
                var weightA = this.Matrix.FirstOrDefault(f => f.Argument == argument)?.Weight ?? 0;
                var weightB = beliefSystem.Matrix.FirstOrDefault(f => f.Argument == argument)?.Weight ?? 0;

                deltaSquared += ((weightA - weightB) * (weightA - weightB));
            }

            if (deltaSquared < beliefSystem.DeltaSquaredMin)
                fuzzyMenbership = 0;
            else if (deltaSquared > beliefSystem.DeltaSquaredMax)
                fuzzyMenbership = 1;
            else
                fuzzyMenbership = deltaSquared / (beliefSystem.DeltaSquaredMax - beliefSystem.DeltaSquaredMin);

            return fuzzyMenbership;
        }

        public void FindInternalDeltas()
        {
            foreach (var beliefSystem in CombinedBeliefSystems)
            {
                var deltaSquared = Compare(beliefSystem);
                if (deltaSquared < (deltaSquaredMin ?? 0) || deltaSquaredMin == null)
                    deltaSquaredMin = deltaSquared;
                if (deltaSquared > (deltaSquaredMax ?? 0) || deltaSquaredMax == null)
                    deltaSquaredMax = deltaSquared;
            }
    }

        public ComparissonGraphData GetComparissonData(BeliefSystem beliefSystem)
        {
            ComparissonGraphData result = new ComparissonGraphData();
            var discrenmentFrame = GetCombinedDiscernementFrame(beliefSystem);

            result.Labels = ToLabelArgumentsString(discrenmentFrame);

            List<string> weightsA = [];
            List<string> weightsB = [];

            foreach (var argument in discrenmentFrame)
            {
                var weightA = this.Matrix.FirstOrDefault(f => f.Argument == argument);
                if (weightA != null)
                    weightsA.Add(weightA.DataString);
                else
                    weightsA.Add("0");

                var weightB = beliefSystem.Matrix.FirstOrDefault(f => f.Argument == argument);
                if (weightB != null)
                    weightsB.Add(weightB.DataString);
                else
                    weightsB.Add("0");
            }

            result.Data1 = ToLabelWeightsString(weightsA.ToArray());
            result.Data2 = ToLabelWeightsString(weightsB.ToArray());

            return result;
        }

        public BeliefSystem Combine(BeliefSystem beliefSystem)
        {
            CombinedBeliefSystems.Add(this.Clone());
            CombinedBeliefSystems.AddRange(beliefSystem.CombinedBeliefSystems.Select(s => s.Clone()));

            BeliefSystem combinedBeliefSystem = new BeliefSystem();
            PrepareBeliefSystem();
            beliefSystem.PrepareBeliefSystem();

            foreach (var belief1 in Matrix)
            {
                foreach (var belief2 in beliefSystem.Matrix)
                {
                    Belief combinedBelief3 = new Belief();
                    if (!belief1.IsPhy && !belief2.IsPhy)
                    {
                        combinedBelief3.Weight = belief1.Weight * belief2.Weight;
                        if (belief1.IsPheta && belief2.IsPheta)
                        {
                            combinedBelief3.ArgumentName = "Pheta";
                            combinedBelief3.Argument = "Pheta";
                            combinedBelief3.IsPheta = true;
                            combinedBelief3.OrderGroup = 1;
                        }
                        else if (belief1.Argument == belief2.Argument)
                        {
                            combinedBelief3.ArgumentName = belief2.ArgumentName;
                            combinedBelief3.Argument = belief2.Argument;
                        }
                        else if (belief1.IsPheta && !belief2.IsPheta)
                        {
                            combinedBelief3.ArgumentName = belief2.ArgumentName;
                            combinedBelief3.Argument = belief2.Argument;
                        }
                        else if (!belief1.IsPheta && belief2.IsPheta)
                        {
                            combinedBelief3.ArgumentName = belief1.ArgumentName;
                            combinedBelief3.Argument = belief1.Argument;
                        }

                        else
                        {
                            combinedBelief3.ArgumentName = "Phy";
                            combinedBelief3.Argument = "Phy";
                            combinedBelief3.IsPhy = true;
                            combinedBelief3.OrderGroup = 2;
                        }
                        combinedBeliefSystem.Matrix.Add(combinedBelief3);
                    }
                }
            }

            BeliefSystem metaBeliefSystem = new BeliefSystem();

            if (combinedBeliefSystem.Matrix.Count() > 0)
            {
                metaBeliefSystem.Name = Name + beliefSystem.Name;

                var k = combinedBeliefSystem.Matrix.Where(w => w.IsPhy).Sum(s => s.Weight);

                foreach (var combinedBelief in combinedBeliefSystem.Matrix.Where(w => !w.IsPhy))
                {
                    Belief metaBelief = new Belief();
                    metaBelief.ArgumentName = combinedBelief.ArgumentName;
                    if (!metaBeliefSystem.Matrix.Any(a => a.Argument == combinedBelief.Argument))
                    {
                        metaBelief.ArgumentName = combinedBelief.ArgumentName;
                        metaBelief.Argument = combinedBelief.Argument;
                        metaBelief.Weight = combinedBeliefSystem.Matrix.Where(w => w.Argument == combinedBelief.Argument).Sum(s => s.Weight);
                        if (k > 0)
                        {
                            metaBelief.Weight = metaBelief.Weight / (1 - k);
                        }
                        metaBelief.IsPheta = combinedBelief.IsPheta;

                        metaBeliefSystem.Matrix.Add(metaBelief);
                    }
                }
            }
            
            return metaBeliefSystem;
        }

        public BeliefSystem CombineSMETS(BeliefSystem beliefSystem)
        {
            CombinedBeliefSystems.Add(this.Clone());
            CombinedBeliefSystems.AddRange(beliefSystem.CombinedBeliefSystems.Select(s => s.Clone()));

            BeliefSystem combinedBeliefSystem = new BeliefSystem();
            PrepareBeliefSystem();
            beliefSystem.PrepareBeliefSystem();

            foreach (var belief1 in Matrix)
            {
                foreach (var belief2 in beliefSystem.Matrix)
                {
                    Belief combinedBelief3 = new Belief();
                    if (!belief1.IsPhy && !belief2.IsPhy)
                    {
                        combinedBelief3.Weight = belief1.Weight * belief2.Weight;
                        if (belief1.IsPheta && belief2.IsPheta)
                        {
                            combinedBelief3.Argument = "Pheta";
                            combinedBelief3.IsPheta = true;
                            combinedBelief3.OrderGroup = 1;
                        }
                        else if (belief1.Argument == belief2.Argument)
                        {
                            combinedBelief3.Argument = belief2.Argument;
                        }
                        else if (belief1.IsPheta && !belief2.IsPheta)
                        {
                            combinedBelief3.Argument = belief2.Argument;
                        }
                        else if (!belief1.IsPheta && belief2.IsPheta)
                        {
                            combinedBelief3.Argument = belief1.Argument;
                        }

                        else
                        {
                            combinedBelief3.Argument = "Phy";
                            combinedBelief3.IsPhy = true;
                            combinedBelief3.OrderGroup = 2;
                        }
                        combinedBeliefSystem.Matrix.Add(combinedBelief3);
                    }
                }
            }

            BeliefSystem metaBeliefSystem = new BeliefSystem();

            if (combinedBeliefSystem.Matrix.Count() > 0)
            {
                metaBeliefSystem.Name = Name + beliefSystem.Name;
                var k = combinedBeliefSystem.Matrix.Where(w => w.IsPhy).Sum(s => s.Weight);
                foreach (var combinedBelief in combinedBeliefSystem.Matrix.Where(w => !w.IsPhy))
                {
                    Belief metaBelief = new Belief();
                    if (!metaBeliefSystem.Matrix.Any(a => a.Argument == combinedBelief.Argument))
                    {
                        metaBelief.Argument = combinedBelief.Argument;
                        metaBelief.Weight = combinedBeliefSystem.Matrix.Where(w => w.Argument == combinedBelief.Argument).Sum(s => s.Weight);
                        metaBelief.Weight = metaBelief.Weight;
                        metaBelief.IsPheta = combinedBelief.IsPheta;

                        metaBeliefSystem.Matrix.Add(metaBelief);
                    }
                }
                Belief phetaBelief = new Belief() { Argument = "Pheta", IsPheta = true };
                phetaBelief.Weight = k;
                metaBeliefSystem.Matrix.Add(phetaBelief);
            }

            metaBeliefSystem.Normalise();

            return metaBeliefSystem;
        }

        public BeliefSystem CombineLNS_CR(List<BeliefSystem> beliefSystems)
        {
            int ammountOfBeliefSystems = beliefSystems.Count + 1;

            PrepareBeliefSystem();
            foreach (var beliefSystem in beliefSystems)
            {
                beliefSystem.PrepareBeliefSystem();
            }

            // Step 1: Cluster belief functions into groups based on focal elements (Ai)
            Dictionary<string, List<Belief>> groupedBeliefs = [];
            foreach (var belief in Matrix)
            {
                if (!groupedBeliefs.ContainsKey(belief.Argument))
                {
                    if (belief.Weight != 0)
                        groupedBeliefs[belief.Argument] = [];
                }
                if (belief.Weight != 0)
                    groupedBeliefs[belief.Argument].Add(belief);
            }

            foreach (var beliefSystem in beliefSystems)
            {
                foreach (var belief in beliefSystem.Matrix)
                {
                    if (!groupedBeliefs.ContainsKey(belief.Argument))
                    {
                        if (belief.Weight != 0)
                            groupedBeliefs[belief.Argument] = [];
                    }
                    if (belief.Weight != 0)
                        groupedBeliefs[belief.Argument].Add(belief);
                }

                CombinedBeliefSystems.Add(beliefSystem.Clone());
            }
            CombinedBeliefSystems.Add(this.Clone());

            // Step 2 & 3: Combine and discount belief functions within each group
            BeliefSystem combinedBeliefSystem = new BeliefSystem();
            foreach (var group in groupedBeliefs)
            {
                string focalElement = group.Key;
                List<Belief> reliabilityOfSources = [];
                List<Belief> productOfSources = [];

                foreach (var source in group.Value)
                {

                    Belief belief1 = new Belief(source.Argument, source.Weight);
                    belief1.ArgumentName = source.ArgumentName;
                    reliabilityOfSources.Add(belief1);

                    Belief belief2 = new Belief(source.Argument, source.Weight);
                    belief2.ArgumentName = source.ArgumentName;
                    productOfSources.Add(belief2);
                }

                double commonality = reliabilityOfSources.Min(m => m.Weight);
                int qtyOfSingleBelief = reliabilityOfSources.Count;

                foreach (var source in reliabilityOfSources)
                {
                    source.Weight *= commonality;
                }

                double reliability = reliabilityOfSources.Sum(m => m.Weight) / qtyOfSingleBelief;

                foreach (var source in productOfSources)
                {
                    source.Weight *= reliability;
                }

                double combinedWeight = 0;
                if (reliability != 0)
                {
                    combinedWeight = productOfSources.Sum(m => m.Weight) / reliability;
                }

                Belief combinedBelief = new Belief(focalElement, combinedWeight);
                combinedBelief.ArgumentName = productOfSources.FirstOrDefault().ArgumentName;

                combinedBeliefSystem.Matrix.Add(combinedBelief);
            }

            combinedBeliefSystem.Normalise();
            BeliefSystem metaBeliefSystem = combinedBeliefSystem;
            metaBeliefSystem.PrepareBeliefSystem();
            metaBeliefSystem.CombinedBeliefSystems = CombinedBeliefSystems;

            return metaBeliefSystem;
        }


        public BeliefSystem CombineWithStepsToFile(BeliefSystem beliefSystem)
        {
            CombinedBeliefSystems.Add(this.Clone());
            CombinedBeliefSystems.AddRange(beliefSystem.CombinedBeliefSystems.Select(s => s.Clone()));

            BeliefSystem combinedBeliefSystem = new BeliefSystem();
            PrepareBeliefSystem();
            beliefSystem.PrepareBeliefSystem();

            using (StreamWriter writer = new StreamWriter($"{LocalPath}\\{Name}x{beliefSystem.Name}.txt"))
            {
                writer.WriteLine($"\tm({beliefSystem.Name})");
                string header = $"m({Name})\t";
                foreach (var belief in beliefSystem.Matrix.Where(w => !string.IsNullOrEmpty(w.Argument)).OrderBy(o => o.OrderGroup).OrderBy(o => o.Argument))
                    header += $"{belief.ToMassString()}\t";
                writer.WriteLine(header);

                foreach (var belief1 in Matrix)
                {
                    string line = $"{belief1.ToMassString()}\t";
                    foreach (var belief2 in beliefSystem.Matrix)
                    {
                        Belief combinedBelief3 = new Belief();
                        if (!belief1.IsPhy && !belief2.IsPhy)
                        {
                            combinedBelief3.Weight = belief1.Weight * belief2.Weight;
                            if (belief1.IsPheta && belief2.IsPheta)
                            {
                                combinedBelief3.Argument = "Pheta";
                                combinedBelief3.IsPheta = true;
                                combinedBelief3.OrderGroup = 1;
                            }
                            else if (belief1.Argument == belief2.Argument)
                            {
                                combinedBelief3.Argument = belief2.Argument;
                            }
                            else if (belief1.IsPheta && !belief2.IsPheta)
                            {
                                combinedBelief3.Argument = belief2.Argument;
                            }
                            else if (!belief1.IsPheta && belief2.IsPheta)
                            {
                                combinedBelief3.Argument = belief1.Argument;
                            }

                            else
                            {
                                combinedBelief3.Argument = "Phy";
                                combinedBelief3.IsPhy = true;
                                combinedBelief3.OrderGroup = 2;
                            }
                            line += $"{combinedBelief3.ToMassString()}\t";
                            combinedBeliefSystem.Matrix.Add(combinedBelief3);
                        }
                    }
                    writer.WriteLine(line);
                }
            }

            BeliefSystem metaBeliefSystem = new BeliefSystem();

            if (combinedBeliefSystem.Matrix.Count() > 0)
            {
                metaBeliefSystem.Name = Name + beliefSystem.Name;

                var k = combinedBeliefSystem.Matrix.Where(w => w.IsPhy).Sum(s => s.Weight);

                foreach (var combinedBelief in combinedBeliefSystem.Matrix.Where(w => !w.IsPhy))
                {
                    Belief metaBelief = new Belief();
                    if (!metaBeliefSystem.Matrix.Any(a => a.Argument == combinedBelief.Argument))
                    {
                        metaBelief.Argument = combinedBelief.Argument;
                        metaBelief.Weight = combinedBeliefSystem.Matrix.Where(w => w.Argument == combinedBelief.Argument).Sum(s => s.Weight);
                        if (k > 0)
                        {
                            metaBelief.Weight = metaBelief.Weight / (1 - k);
                        }
                        metaBelief.IsPheta = combinedBelief.IsPheta;

                        metaBeliefSystem.Matrix.Add(metaBelief);
                    }

                }

            }

            return metaBeliefSystem;
        }


        public void Print()
        {
            foreach (var belief in Matrix.Where(w => !string.IsNullOrEmpty(w.Argument)).OrderBy(o => o.OrderGroup).OrderBy(o => o.Argument))
                belief.Print();
        }

        public void Output()
        {
            using (StreamWriter writer = new StreamWriter($"{LocalPath}\\{Name}.txt"))
            {
                foreach (var belief in Matrix.Where(w => !string.IsNullOrEmpty(w.Argument)).OrderBy(o => o.OrderGroup).OrderBy(o => o.Argument))
                    writer.WriteLine(belief.ToString());
            }
        }


        public BeliefSystem Clone()
        {
            return new BeliefSystem()
            {
                Name = Name,
                Matrix = Matrix,
            };
        }


        public override string ToString()
        {
            string result = "";
            foreach (var belief in Matrix.Where(w => !string.IsNullOrEmpty(w.Argument)).OrderBy(o => o.OrderGroup).OrderBy(o => o.Argument))
                result += belief.ToString() + "\n";

            return result;
        }

        public string ToNameString()
        {
            string result = "";
            foreach (var belief in Matrix.Where(w => !string.IsNullOrEmpty(w.Argument)).OrderBy(o => o.OrderGroup).OrderBy(o => o.Argument))
                result += belief.ToNameString() + "\n";

            return result;
        }

        public string ToLabelString()
        {
            string result = "labels : [";
            var arguments = Matrix.Where(w => !string.IsNullOrEmpty(w.Argument) && !w.IsPheta && w.Argument != "Pheta")
                                  .OrderBy(o => o.OrderGroup)
                                  .OrderBy(o => o.Argument)
                                  .Select(s => s.LabelString)
                                  .ToArray();
            result += string.Join(",", arguments);

            return result + "]";
        }

        public string ToLabelArgumentsString(string[] arguments)
        {
            string result = "labels : [\"";
            result += string.Join("\",\"", arguments);

            return result + "\"]";
        }

        public string ToLabelWeightsString(string[] weights)
        {
            string result = "data : [";
            result += string.Join(",", weights);

            return result + "]";
        }

        public string ToLabelNameString()
        {
            string result = "labels : [";
            var arguments = Matrix.Where(w => !string.IsNullOrEmpty(w.Argument) && !w.IsPheta && w.Argument != "Pheta")
                                  .OrderBy(o => o.OrderGroup)
                                  .OrderBy(o => o.Argument)
                                  .Select(s => s.LabelNameString)
                                  .ToArray();
            result += string.Join(",", arguments);

            return result + "]";
        }
        
        public string ToDataString()
        {
            string result = "data : [";
            var arguments = Matrix.Where(w => !string.IsNullOrEmpty(w.Argument) && !w.IsPheta && w.Argument != "Pheta")
                                  .OrderBy(o => o.OrderGroup)
                                  .OrderBy(o => o.Argument)
                                  .Select(s => s.DataString)
                                  .ToArray();
            result += string.Join(",", arguments);

            return result + "]";
        }


        #region public helper methods

        public void PrepareBeliefSystem()
        {
            if (!Matrix.Any(a => a.IsPheta))
            {
                Belief pheta = new Belief() { Argument = "Pheta", IsPheta = true };
                var sumWeights = Matrix.Sum(s => s.Weight);
                if (sumWeights < 0.999999999999999999999999999)
                {
                    pheta.Weight = 1 - sumWeights;
                    Matrix.Add(pheta);
                }
            }
        }

        public void Normalise()
        {
            double SumOfWeights = Matrix.Sum(s => s.Weight);
            double countOfBeliefs = Matrix.Sum(s => s.Weight);

            if (SumOfWeights > 1)
            {
                foreach (var item in Matrix)
                {
                    item.Weight = (item.Weight / SumOfWeights);
                }
            }
        }

        #endregion

    }
}