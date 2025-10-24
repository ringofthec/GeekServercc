using Geek.Server.Core.Actors;
using Geek.Server.Core.Hotfix;
using Geek.Server.Core.Hotfix.Agent;

namespace Geek.Server.Core.Comps
{
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
                    情况2: 热更模块
                */
                if (_cacheAgent != null && !HotfixMgr.DoingHotfix)
                    return _cacheAgent;
                
                // 从热更新模块中获取后，会缓存起来
                var agent = HotfixMgr.GetAgent<ICompAgent>(this, refAssemblyType);
                _cacheAgent = agent;
                return agent;
            }
        }

        public void ClearCacheAgent()
        {
            _cacheAgent = null;
        }

        // 这个 comp 组件属于的 actor
        // 一个 actor 可以有多个 comp 组件
        internal Actor Actor { get; set; }

        internal long ActorId => Actor.Id;

        public bool IsActive { get; private set; } = false;

        public virtual Task Active()
        {
            IsActive = true;
            return Task.CompletedTask;
        }

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
