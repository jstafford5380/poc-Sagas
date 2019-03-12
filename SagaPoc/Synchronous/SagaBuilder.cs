using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SagaPoc.Synchronous
{
    public interface ISagaBuilder<TStateType>
    {
        TStateType State { get; }

        ISagaActivity<TStateType> Activity(string name, Action<TStateType> activity);

        ISagaActivity<TStateType> Activity(string name, Func<Task> task);

        Task<TStateType> RunAsync();
    }

    public interface ISagaActivity<TStateType>
    {
        string Name { get; set; }
        
        ISagaActivity<TStateType> OnFail(Func<ISagaControl, TStateType, SagaError, Task> failureCallback);
        
        ISagaActivity<TStateType> OnSucceeded(Func<ISagaControl, TStateType, Task> successCallback);

        ISagaBuilder<TStateType> Build();
        
        Task RunAsync();
    }

    public class ActivityFailedEventArgs<TStateType> : EventArgs
    {
        public ISagaControl SagaControl { get; set; }

        public TStateType State { get; set; }

        public SagaError Error { get; set; }
    }

    public class ActivitySucceededArgs<TStateType> : EventArgs
    {
        public ISagaControl SagaControl { get; set; }

        public TStateType State { get; set; }
    }

    public class Activity<TStateType> : ISagaActivity<TStateType>
        where TStateType : new()
    {
        private readonly Func<Task> _task;
        private readonly ISagaBuilder<TStateType> _builder;
        private Func<ISagaControl, TStateType, Task> _whenSucceed;
        private Func<ISagaControl, TStateType, SagaError, Task> _whenFail;
        
        public string Name { get; set; }

        public Activity(string name, Func<Task> task, ISagaBuilder<TStateType> builder)
        {
            Name = name;

            _task = task;
            _builder = builder;
        }
        
        public ISagaActivity<TStateType> OnFail(Func<ISagaControl, TStateType, SagaError, Task> failureCallback)
        {
            _whenFail = failureCallback;
            return this;
        }

        public ISagaActivity<TStateType> OnSucceeded(Func<ISagaControl, TStateType, Task> successCallback)
        {
            _whenSucceed = successCallback;
            return this;
        }

        public ISagaBuilder<TStateType> Build() => _builder;

        public Func<ISagaControl, TStateType, SagaError, Task> GetFailureCallback() => _whenFail;

        public Func<ISagaControl, TStateType, Task> GetSuccessCallback() => _whenSucceed;

        public async Task RunAsync()
        {
            try
            {
                await _task();
            }
            catch (Exception e)
            {
                var ex = new SagaActivityException($"Saga activity '{Name}' failed :: {e.Message}", e);
                if (_whenFail != null)
                    await _whenFail(null, _builder.State, new SagaError { Exception = ex });

                throw ex;
            }
        }
    }

    public class SagaActivityException : Exception
    {
        public SagaActivityException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    public interface ISagaControl
    {
        void MarkAsComplete();
    }

    public class SagaBuilder<TStateType> : ISagaBuilder<TStateType>, ISagaControl
        where TStateType : new()
    {
        private string _name;

        public TStateType State { get; private set; }

        private readonly List<ISagaActivity<TStateType>> _steps = new List<ISagaActivity<TStateType>>();

        private SagaBuilder() { }

        public static ISagaBuilder<TStateType> Create(string name)
        {
            return new SagaBuilder<TStateType>
            {
                _name = name,
                State = new TStateType()
            };
        }

        public ISagaActivity<TStateType> Activity(string name, Action<TStateType> activity)
        {
            throw new NotImplementedException();
        }

        public ISagaActivity<TStateType> Activity(string name, Func<Task> task)
        {
            var activity = new Activity<TStateType>(name, task, this);
            _steps.Add(activity);
            return activity;
        }

        public async Task<TStateType> RunAsync()
        {
            ISagaActivity<TStateType> lastActivity = null;

            foreach (var activity in _steps)
            {
                // TODO: retries?
                lastActivity = activity;
                await activity.RunAsync().ConfigureAwait(false);
            }

            return State;
        }

        public void MarkAsComplete()
        {
            
        }
    }
}
