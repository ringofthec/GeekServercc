using Geek.Server.Core.Actors;
using Geek.Server.Core.Hotfix;
using Geek.Server.Core.Hotfix.Agent;

namespace Geek.Server.Core.Comps
{
    /*
        基础组件
            1. 管理 Camp 对应的 CampAgent
            2. 组件对应的 Actor 引用
            3. 组件的基本接口 Active Deactive SaveState
     */
    public abstract class BaseComp
    {
        private ICompAgent _cacheAgent = null;
        private readonly object _cacheAgentLock = new();

        // 从热更新模块中获取 CompAgent
        public ICompAgent GetAgent(Type refAssemblyType = null)
        {
            lock (_cacheAgentLock)
            {
                /*
                    _cacheAgent == null
                    情况1: 初始化的时候，默认就是null，就去创建一个
                    情况2: 热更模块加载成功后，会调用 ClearCacheAgent 函数把 _cacheAgent 置为null，原因是 Agnet被热更新了，要重新new出来
                */
                if (_cacheAgent != null && !HotfixMgr.DoingHotfix)
                    return _cacheAgent;
                
                // 从热更新模块中获取后，会缓存起来
                var agent = HotfixMgr.GetAgent<ICompAgent>(this, refAssemblyType);
                _cacheAgent = agent;
                return agent;
            }
        }

        // 热更新成功后，调用这里，把Agent清了，访问的时候重新从热更新中加载更新后的Agent
        public void ClearCacheAgent()
        {
            _cacheAgent = null;
        }

        // 这个 Comp 组件属于的 actor
        // 一个 actor 可以有多个 comp 组件
        internal Actor Actor { get; set; }

        // 获取 actor 的id
        internal long ActorId => Actor.Id;

        public bool IsActive { get; private set; } = false;

        // 激活
        public virtual Task Active()
        {
            IsActive = true;
            return Task.CompletedTask;
        }

        // 失效
        public virtual async Task Deactive()
        {
            var agent = GetAgent();
            if (agent != null)
                await agent.Deactive();
        }

        internal virtual Task SaveState() { return Task.CompletedTask; }

        internal virtual bool ReadyToDeactive => true;
    }
}
