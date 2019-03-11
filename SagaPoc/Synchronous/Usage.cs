using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SagaPoc.Synchronous
{
    public class Usage
    {
        private readonly Random _rand = new Random();

        public Task Run()
        {
            var builder = SagaBuilder<TestSagaState>.Create("Synchronous Saga Example")
                .Activity("Step 1", Step1)
                    .OnSucceeded((control, state) => Console.WriteLine("Step 1 succeeded!"))
                    .OnFail(RollbackStep1)
                    .Build()
                .Activity("Step 2", Step2)
                    .OnSucceeded((control, state) =>
                    {
                        Console.WriteLine("Step 2 succeeded!");
                        control.MarkAsComplete();
                    })
                    .OnFail(RollbackStep2)
                    .Build();

            return builder.RunAsync();
        }

        private Task Step1()
        {
            var shouldFail = _rand.Next() % 2 == 0;
            if (shouldFail)
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
            if (shouldFail)
                throw new Exception("Simulated fail");

            Console.WriteLine("Step 2 succeeded!");
            return Task.CompletedTask;
        }

        private async Task RollbackStep2(ISagaControl control, TestSagaState state, SagaError error)
        {
            Console.WriteLine("Step 2 failed; rolling back.");
            await RollbackStep1(control, state, error);
        }
    }
}
