namespace Geek.Server.Core.Actors.Impl
{
    public abstract class WorkWrapper
    {
        public WorkerActor Owner { get; set; }
        public int TimeOut { get; set; }
        public abstract Task DoTask();
        public abstract string GetTrace();
        public abstract void ForceSetResult();
        public long CallChainId { get; set; }
        protected void SetContext()
        {
            RuntimeContext.SetContext(CallChainId, Owner.Id);
            Owner.CurChainId = CallChainId;
        }
        public void ResetContext()
        {
            Owner.CurChainId = 0;
        }
    }

    // 无返回值 wrapper 
    public class ActionWrapper : WorkWrapper
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        public Action Work { private set; get; }
        public TaskCompletionSource<bool> Tcs { private set; get; }

        public ActionWrapper(Action work)
        {
            Work = work;
            
            // 如果要判断这个 work 是否真的执行完成，就是看这个 Tcs
            // 这个是Action 的，是没有返回值的，所以这里 Tcs 是一个bool，设置为true就表示这个action已经执行过了
            Tcs = new TaskCompletionSource<bool>();
        }

        public override Task DoTask()
        {
            try
            {
                SetContext();
                Work();
            }
            catch (Exception e)
            {
                LOGGER.Error(e.ToString());
            }
            finally
            {
                ResetContext(); 
                
                // 不管action是否报错，都是执行了的，所以这里设置为true
                Tcs.TrySetResult(true);
            }
            return Task.CompletedTask;
        }

        public override string GetTrace()
        {
            return Work.Target + "|" + Work.Method.Name;
        }

        public override void ForceSetResult()
        {
            ResetContext();
            Tcs.TrySetResult(false);
        }
    }

    // 返回值为T的 wrapper
    public class FuncWrapper<T> : WorkWrapper
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        public Func<T> Work { private set; get; }
        public TaskCompletionSource<T> Tcs { private set; get; }

        public FuncWrapper(Func<T> work)
        {
            Work = work;
            
            // 生成一个和返回值T类型一样的 Tcs
            Tcs = new TaskCompletionSource<T>();
        }

        public override Task DoTask()
        {
            T ret = default;
            try
            {
                SetContext();
                ret = Work();
            }
            catch (Exception e)
            {
                LOGGER.Error(e.ToString());
            }
            finally
            {
                ResetContext();
                
                // 执行完成，需要设置结果，类型是T
                Tcs.TrySetResult(ret);
            }
            return Task.CompletedTask;
        }

        public override string GetTrace()
        {
            return Work.Target + "|" + Work.Method.Name;
        }

        public override void ForceSetResult()
        {
            ResetContext();
            Tcs.TrySetResult(default);
        }
    }

    // 异步执行的没有返回值的 wrapper
    public class ActionAsyncWrapper : WorkWrapper
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        public Func<Task> Work { private set; get; }
        public TaskCompletionSource<bool> Tcs { private set; get; }

        public ActionAsyncWrapper(Func<Task> work)
        {
            Work = work;
            Tcs = new TaskCompletionSource<bool>();
        }

        public override async Task DoTask()
        {
            try
            {
                SetContext();
                
                // 这里是一个异步方法，所以await
                await Work();
            }
            catch (Exception e)
            {
                LOGGER.Error(e.ToString());
            }
            finally
            {
                ResetContext();
                Tcs.TrySetResult(true);
            }
        }

        public override string GetTrace()
        {
            return Work.Target + "|" + Work.Method.Name;
        }

        public override void ForceSetResult()
        {
            ResetContext();
            Tcs.TrySetResult(false);
        }
    }

    // 异步执行的返回值为T的 wrapper
    public class FuncAsyncWrapper<T> : WorkWrapper
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        public Func<Task<T>> Work { private set; get; }
        public TaskCompletionSource<T> Tcs { private set; get; }

        public FuncAsyncWrapper(Func<Task<T>> work)
        {
            Work = work;
            Tcs = new TaskCompletionSource<T>();
        }

        public override async Task DoTask()
        {
            T ret = default;
            try
            {
                SetContext();
                ret = await Work();
            }
            catch (Exception e)
            {
                LOGGER.Error(e.ToString());
            }
            finally
            {
                ResetContext();
                Tcs.TrySetResult(ret);
            }
        }

        public override string GetTrace()
        {
            return Work.Target + "|" + Work.Method.Name;
        }

        public override void ForceSetResult()
        {
            ResetContext();
            Tcs.TrySetResult(default);
        }
    }
}
