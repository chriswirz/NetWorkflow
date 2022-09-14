﻿
namespace NetWorkflow
{
    public abstract class WorkflowBuilderNext
    {
        internal WorkflowBuilderNext? Next { get; set; }
    }

    #region With Params

    public class WorkflowBuilderNext<TContext, Tin, Tout> : WorkflowBuilderNext, IWorkflowBuilderNext<TContext, Tin, Tout>
    {
        private readonly TContext _context;

        private readonly Func<WorkflowStep<Tin, Tout>> _func;

        public WorkflowBuilderNext(Func<WorkflowStep<Tin, Tout>> func, TContext context)
        {
            _func = func ?? throw new ArgumentNullException(nameof(func));

            _context = context; 
        }

        public IWorkflowBuilderNext<TContext, TNext[]> Parallel<TNext>(Func<IEnumerable<WorkflowStepAsync<Tout, TNext>>> func)
        {
            Next = new WorkflowBuilderParallel<TContext, Tout, TNext[]>(func, _context);

            return (IWorkflowBuilderNext<TContext, TNext[]>)Next;
        }

        public IWorkflowBuilderNext<TContext, TNext[]> Parallel<TNext>(Func<TContext, IEnumerable<WorkflowStepAsync<Tout, TNext>>> func)
        {
            Next = new WorkflowBuilderParallel<TContext, Tout, TNext>(() => func.Invoke(_context), _context);

            return (IWorkflowBuilderNext<TContext, TNext[]>)Next;
        }

        public Tout Run(Tin args, CancellationToken token = default) => _func.Invoke().Run(args, token);

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
    }

    #endregion

    #region No Params

    public class WorkflowBuilderNext<TContext, Tout> : WorkflowBuilderNext, IWorkflowBuilderNext<TContext, Tout>
    {
        private readonly TContext _context;

        private readonly Func<WorkflowStep<Tout>> _func;

        public WorkflowBuilderNext(Func<WorkflowStep<Tout>> func, TContext context)
        {
            _func = func;

            _context = context;
        }

        public IWorkflowBuilderNext<TContext, TNext[]> Parallel<TNext>(Func<IEnumerable<WorkflowStepAsync<Tout, TNext>>> func)
        {
            Next = new WorkflowBuilderParallel<TContext, Tout, TNext[]>(func, _context);

            return (IWorkflowBuilderNext<TContext, TNext[]>)Next;
        }

        public IWorkflowBuilderNext<TContext, TNext[]> Parallel<TNext>(Func<TContext, IEnumerable<WorkflowStepAsync<Tout, TNext>>> func)
        {
            Next = new WorkflowBuilderParallel<TContext, Tout, TNext>(() => func.Invoke(_context), _context);

            return (IWorkflowBuilderNext<TContext, TNext[]>)Next;
        }

        public Tout Run(CancellationToken token = default) => _func.Invoke().Run(token);

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
    }

    #endregion
}
