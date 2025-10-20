
using Geek.Server.Core.Net.BaseHandler;

namespace Server.Logic.Logic.Login
{
    // ReqLogin 入口，通过反射注入到协议的回调中
    // 在 HotfixModule 中的 ParseDll -> AddTcpHandler 中注册
    [MsgMapping(typeof(ReqLogin))]
    internal class ReqLoginHandler : GlobalCompHandler<LoginCompAgent>
    {
        public override async Task ActionAsync()
        {
            await Comp.OnLogin(Channel, Msg as ReqLogin);
        }
    }
}
