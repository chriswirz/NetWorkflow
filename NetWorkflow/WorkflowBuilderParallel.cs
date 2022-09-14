﻿
namespace NetWorkflow
{
    public abstract class WorkflowBuilderParallel : WorkflowBuilderNext { }

    public class WorkflowBuilderParallel<TContext, Tin, Tout> : WorkflowBuilderParallel, IWorkflowBuilderNext<TContext, Tin, Tout[]>
    {
        private readonly TContext _context;

        private readonly Func<IEnumerable<WorkflowStepAsync>> _func;

        public WorkflowBuilderParallel(Func<IEnumerable<WorkflowStepAsync>> func, TContext context)
        {
            _func = func;

            _context = context;
        }

        public IWorkflowBuilderNext<TContext, TNext[]> Parallel<TNext>(Func<IEnumerable<WorkflowStepAsync<Tout[], TNext>>> func)
        {
            Next = new WorkflowBuilderParallel<TContext, Tout, TNext>(func, _context);

            return (IWorkflowBuilderNext<TContext, TNext[]>)Next;
        }

        public IWorkflowBuilderNext<TContext, TNext[]> Parallel<TNext>(Func<TContext, IEnumerable<WorkflowStepAsync<Tout[], TNext>>> func)
        {
            Next = new WorkflowBuilderParallel<TContext, Tout, TNext>(() => func.Invoke(_context), _context);

            return (IWorkflowBuilderNext<TContext, TNext[]>)Next;
        }

        public Task<Tout>[] Run(Tin args, CancellationToken token = default)
        {
            return _func.Invoke().Select(x =>
            {
                return (Task<Tout>)x.GetType().GetMethod("RunAsync").Invoke(x, new object[] { args, token });
            }).ToArray();
        }

        public IWorkflowBuilderNext<TContext, Tout, TNext> Then<TNext>(Func<WorkflowStep<Tout, TNext>> func)
        {
            Next = new WorkflowBuilderNext<TContext, Tout, TNext>(func, _context);

            return (IWorkflowBuilderNext<TContext, Tout, TNext>)Next;
        }

        public IWorkflowBuilderNext<TContext, Tout, TNext> Then<TNext>(Func<TContext, WorkflowStep<Tout, TNext>> func)
        {
            Next = new WorkflowBuilderNext<TContext, Tout, TNext>(() => func.Invoke(_context), _context);

            return (IWorkflowBuilderNext<TContext, Tout, TNext>)Next;
        }

        public IWorkflowBuilderNext<TContext, Tout[], TNext> Then<TNext>(Func<WorkflowStep<Tout[], TNext>> func)
        {
            Next = new WorkflowBuilderNext<TContext, Tout[], TNext>(() => func.Invoke(), _context);

            return (IWorkflowBuilderNext<TContext, Tout[], TNext>)Next;
        }

        public IWorkflowBuilderNext<TContext, Tout[], TNext> Then<TNext>(Func<TContext, WorkflowStep<Tout[], TNext>> func)
        {
            Next = new WorkflowBuilderNext<TContext, Tout[], TNext>(() => func.Invoke(_context), _context);

            return (IWorkflowBuilderNext<TContext, Tout[], TNext>)Next;
        }
    }
}
