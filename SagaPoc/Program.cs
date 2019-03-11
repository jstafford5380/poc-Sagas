using System;
using System.Reflection.Metadata;
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
        }

        async Task RunDemo()
        {

        }

        Task SynchronousDemo()
        {
            var syncDemo = new Synchronous.Usage();
            return syncDemo.Run();
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
