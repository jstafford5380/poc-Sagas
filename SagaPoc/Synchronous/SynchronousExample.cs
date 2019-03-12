using System;
using System.Threading.Tasks;

namespace SagaPoc.Synchronous
{
    public class SynchronousExample
    {
        private readonly bool _shouldSucceed;
        private readonly Random _rand = new Random();

        public SynchronousExample(bool shouldSucceed)
        {
            _shouldSucceed = shouldSucceed;
        }

        public async Task Run()
        {
            var builder = SagaBuilder<TestSagaState>.Create("Synchronous Saga Example")
                .Activity("Step 1", Step1)
                    .OnSucceeded((control, state) => Task.Run(() => Console.WriteLine("Step 1 succeeded!")))
                    .OnFail(RollbackStep1)
                    .Build()
                .Activity("Step 2", Step2)
                    .OnSucceeded((control, state) => Task.Run(() =>
                    {
                        Console.WriteLine("Step 2 succeeded!");
                        control.MarkAsComplete();
                    }))
                    .OnFail(RollbackStep2)
                    .Build();

            try
            {
                await builder.RunAsync(); 
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failure Caught: {e.Message}");
            }
        }

        private Task Step1()
        {
            var shouldFail = _rand.Next() % 2 == 0;
            if (!_shouldSucceed && shouldFail)
                throw new Exception("Simulated fail");

            Console.WriteLine("Step 1 succeeded!");
            return Task.CompletedTask;
        }

        private Task RollbackStep1(ISagaControl control, TestSagaState state, SagaError error)
        {
            Console.WriteLine($"Step 1 failed; Rolling back.");
            return Task.CompletedTask;
        }

        private Task Step2()
        {
            var shouldFail = _rand.Next() % 2 == 0;
            if (!_shouldSucceed && shouldFail)
                throw new Exception("Simulated fail");

            Console.WriteLine("Step 2 succeeded!");
            return Task.CompletedTask;
        }

        private async Task RollbackStep2(ISagaControl control, TestSagaState state, SagaError error)
        {
            Console.WriteLine("Step 2 failed; rolling back.");
            
        }
    }
}
