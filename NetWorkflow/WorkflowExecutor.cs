﻿using System.Collections;
using System.Linq.Expressions;
using System.Reflection;

namespace NetWorkflow
{
    public abstract class WorkflowExecutor<TContext>
    {
        public abstract object? Run(object? args, CancellationToken token = default);
    }

    public class WorkflowExecutor<TContext, TResult> : WorkflowExecutor<TContext>
    {
        private readonly Expression _expression;

        private readonly TContext _context;

        public WorkflowExecutor(Expression expression, TContext context)
        {
            _expression = expression;

            _context = context;
        }

        public override object? Run(object? args, CancellationToken token = default)
        {
            MethodInfo? executor = null;

            object? body = null;

            if (_expression is LambdaExpression exp)
            {
                body = exp.Parameters.Count == 1 ? exp.Compile().DynamicInvoke(_context) : exp.Compile().DynamicInvoke();

                if (body == null) throw new InvalidOperationException("WorkflowStep cannot be null");

                if (body is WorkflowStep step)
                {
                    executor = step.GetType().GetMethod("Run");

                    if (executor == null) throw new InvalidOperationException("Method not found");

                    int count = executor.GetParameters().Length;

                    if (count == 1)
                    {
                        return (TResult?)executor.Invoke(body, new object[] { token });
                    }
                    else
                    {
                        return (TResult?)executor.Invoke(body, new object?[] { args, token });
                    }
                }
                else if (body is IEnumerable<WorkflowStepAsync> asyncSteps)
                {
                    Task<TResult>?[] tasks = asyncSteps.Select(x =>
                    {
                        executor = x.GetType().GetMethod("RunAsync");

                        if (executor == null) throw new InvalidOperationException("Internal error");

                        if (executor.GetParameters().Length > 1)
                        {
                            return ((Task<TResult>)executor.Invoke(x, new object?[] { args, token }));
                        }
                        else
                        {
                            return ((Task<TResult>)executor.Invoke(x, new object[] { token }));
                        }
                    }).ToArray();

                    Task.WaitAll(tasks);

                    return tasks.Where(x => x != null).Select(x => x.Result).ToArray();
                }
                else
                {
                    throw new InvalidOperationException("Internal error");
                }
            }

            throw new InvalidOperationException("Internal error");
        }
    }
}
