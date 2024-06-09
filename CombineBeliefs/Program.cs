using CombinationOfBeliefs;
using System.IO;
using System.Reflection;

namespace CombineBeliefs
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var SolutionFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var path = $"{SolutionFolder}\\..\\..\\..\\Matrices\\";

            Console.WriteLine("reading a.txt!");
            var BelA = BeliefSystem.Load($"{path}a.txt");
            BelA.Print();

            Console.WriteLine();
            Console.WriteLine("reading b.txt!");
            var BelB = BeliefSystem.Load($"{path}b.txt");
            BelB.Print();

            Console.WriteLine();
            Console.WriteLine("Combining a & b!");
            var BelAB = BelA.Combine(BelB);
            BelAB.Print();
            BelAB.Output();

            Console.WriteLine();
            Console.WriteLine("Smets Combining a & b!");
            var BelSmetsAB = BelA.CombineSMETS(BelB);
            BelSmetsAB.Print();
            BelSmetsAB.Output();

            Console.WriteLine();
            Console.WriteLine("LNS Combining a & b!");
            var BelLNSAB = BelA.CombineLNS_CR(new List<BeliefSystem>() { new BeliefSystem(BelB) });
            BelLNSAB.Print();
            BelLNSAB.Output();

            Console.ReadLine();

        }
    }
}
