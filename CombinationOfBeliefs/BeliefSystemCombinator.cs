using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Web;

namespace CombinationOfBeliefs
{
    public static class BeliefSystemCombinator
    {
        public static BeliefSystem? CombineBeliefs(List<BeliefSystem> beliefSystems, int? amountToConsider = null) 
        {
            BeliefSystem? CombinedBeliefSystem = null;
            List<BeliefSystem> beliefSystemsToConsider;
            if (amountToConsider == null)
                beliefSystemsToConsider = beliefSystems;
            else
                beliefSystemsToConsider = beliefSystems.Take(amountToConsider.Value).ToList();

            foreach (var beliefSystem in beliefSystemsToConsider)
            {
                if (CombinedBeliefSystem == null)
                {
                    CombinedBeliefSystem = beliefSystem;
                    continue;
                }
                CombinedBeliefSystem = CombinedBeliefSystem.Combine(beliefSystem);
            }
            return CombinedBeliefSystem;
        }

        public static BeliefSystem? CombineLNSBeliefs(List<BeliefSystem> beliefSystems, int? amountToConsider = null) 
        {
            BeliefSystem? CombinedBeliefSystem = null;
            List<BeliefSystem> beliefSystemsToConsider;
            if (amountToConsider == null)
                beliefSystemsToConsider = beliefSystems;
            else
                beliefSystemsToConsider = beliefSystems.Take(amountToConsider.Value).ToList();

            CombinedBeliefSystem = beliefSystemsToConsider.FirstOrDefault();
            return CombinedBeliefSystem?.CombineLNS_CR(beliefSystems.Where(w => w != CombinedBeliefSystem).ToList());
        }

        public static BeliefSystem? CombineBeliefsInPairs(List<BeliefSystem> beliefSystems, int? amountToConsider = null) 
        {
            BeliefSystem? CombinedBeliefSystem = null;
            List<BeliefSystem> beliefSystemsToConsider;
            if (amountToConsider == null)
                beliefSystemsToConsider = beliefSystems;
            else
                beliefSystemsToConsider = beliefSystems.Take(amountToConsider.Value).ToList();

            List<BeliefSystem> iterationBeliefSystems = beliefSystemsToConsider;
            do
            {
                iterationBeliefSystems = PairAndCombineBeliefs(iterationBeliefSystems);

            } while (iterationBeliefSystems.Count() != 1);

            if (iterationBeliefSystems.Count() == 1)
            {
                CombinedBeliefSystem = iterationBeliefSystems.First();
            }
            return CombinedBeliefSystem;
        }

        private static List<BeliefSystem> PairAndCombineBeliefs(List<BeliefSystem> beliefSystems) 
        {
            BeliefSystem? firstBeliefSystem = null;
            List<BeliefSystem> pairedBeliefSystems = [];
            int counter = 0;
            foreach (var beliefSystem in beliefSystems)
            {
                counter++;
                if (firstBeliefSystem == null)
                {
                    firstBeliefSystem = beliefSystem;
                    if (counter == beliefSystems.Count())
                    {
                        var lastBeliefSystem = pairedBeliefSystems.Last();
                        lastBeliefSystem = pairedBeliefSystems.Last().Combine(beliefSystem);
                    }
                    continue;
                }
                else
                {
                    var CombinedBeliefSystem = firstBeliefSystem.Combine(beliefSystem);
                    firstBeliefSystem = null;
                    pairedBeliefSystems.Add(CombinedBeliefSystem);
                }

            }
            return pairedBeliefSystems;
        }


        public static ComparissonGraphData GetComparissonData(BeliefSystem belA, BeliefSystem belB)
        {
            ComparissonGraphData result = new ComparissonGraphData();
            var discrenmentFrame = belA.GetCombinedDiscernementFrame(belB);

            result.Labels = belA.ToLabelArgumentsString(discrenmentFrame);

            List<string> weightsA = [];
            List<string> weightsB = [];

            foreach (var argument in discrenmentFrame)
            {
                var weightA = belA.Matrix.FirstOrDefault(f => f.Argument == argument);
                if (weightA != null)
                    weightsA.Add(weightA.DataString);
                else 
                    weightsA.Add("0");

                var weightB = belB.Matrix.FirstOrDefault(f => f.Argument == argument);
                if (weightB != null)
                    weightsB.Add(weightB.DataString);
                else
                    weightsB.Add("0");
            }

            result.Data1 = belA.ToLabelWeightsString(weightsA.ToArray());
            result.Data2 = belA.ToLabelWeightsString(weightsB.ToArray());

            return result;
        }

        public static ComparissonGraphData GetComparissonData(BeliefSystem belA, BeliefSystem bel1, BeliefSystem bel2, BeliefSystem bel3, BeliefSystem bel4, BeliefSystem bel5)
        {
            ComparissonGraphData result = new ComparissonGraphData();
            var discrenmentFrame = belA.GetCombinedDiscernementFrame(bel1);

            result.Labels = belA.ToLabelArgumentsString(discrenmentFrame);

            List<string> weightsA = [];
            List<string> weights1 = [];
            List<string> weights2 = [];
            List<string> weights3 = [];
            List<string> weights4 = [];
            List<string> weights5 = [];

            foreach (var argument in discrenmentFrame)
            {
                var weightA = belA.Matrix.FirstOrDefault(f => f.Argument == argument);
                if (weightA != null)
                    weightsA.Add(weightA.DataString);
                else 
                    weightsA.Add("0");

                var weightB = bel1.Matrix.FirstOrDefault(f => f.Argument == argument);
                if (weightB != null)
                    weights1.Add(weightB.DataString);
                else
                    weights1.Add("0");

                var weightC = bel2.Matrix.FirstOrDefault(f => f.Argument == argument);
                if (weightC != null)
                    weights2.Add(weightC.DataString);
                else
                    weights2.Add("0");

                var weightD = bel3.Matrix.FirstOrDefault(f => f.Argument == argument);
                if (weightD != null)
                    weights3.Add(weightD.DataString);
                else
                    weights3.Add("0");

                var weightE = bel4.Matrix.FirstOrDefault(f => f.Argument == argument);
                if (weightE != null)
                    weights4.Add(weightE.DataString);
                else
                    weights4.Add("0");

                var weightF = bel5.Matrix.FirstOrDefault(f => f.Argument == argument);
                if (weightF != null)
                    weights5.Add(weightF.DataString);
                else
                    weights5.Add("0");
            }

            result.Data0 = belA.ToLabelWeightsString(weightsA.ToArray());
            result.Data1 = belA.ToLabelWeightsString(weights1.ToArray());
            result.Data2 = belA.ToLabelWeightsString(weights2.ToArray());
            result.Data3 = belA.ToLabelWeightsString(weights3.ToArray());
            result.Data4 = belA.ToLabelWeightsString(weights4.ToArray());
            result.Data5 = belA.ToLabelWeightsString(weights5.ToArray());

            return result;
        }
    }

    public class ComparissonGraphData
    {
        public string? Labels { get; set; }
        public string? Data0 { get; set; }
        public string? Data1 { get; set; }
        public string? Data2 { get; set; }
        public string? Data3 { get; set; }
        public string? Data4 { get; set; }
        public string? Data5 { get; set; }
    }
}