using System;
using System.Linq;
using System.Threading.Tasks;
using SagaPoc.Synchronous;

namespace SagaPoc
{
    class Program
    {
        static void Main(string[] args)
        {
            var p = new Program();
            p.RunDemo().GetAwaiter().GetResult();
            Console.ReadKey();
        }

        async Task RunDemo()
        {
            while (true)
            {
                Console.WriteLine("1. Synchronous Success\r\n2. Synchronous Failure\r\n");
                Console.Write("Select choice:");
                var choice = Console.ReadLine();

                var choices = new[] {1, 2};
                if (!int.TryParse(choice, out var choiceNum) || !choices.Contains(choiceNum))
                {
                    Console.WriteLine("Invalid choice!");
                    Console.WriteLine();
                    continue;
                }

                switch (choiceNum)
                {
                    case 1:
                        await new SynchronousExample(true).Run();
                        break;
                    case 2:
                        await new SynchronousExample(false).Run();
                        break;
                }

                Console.WriteLine();
            }
        }
    }

    public class TestSagaState : SagaState
    {

    }

    public abstract class SagaState
    {
        public string SagaName { get; set; }
    }

    public class SagaError
    {
        public SagaActivityException Exception { get; set; }
    }
}
