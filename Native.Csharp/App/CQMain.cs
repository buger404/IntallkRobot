//修改此处命名空间
using io.github.buger404.intallk.Code;
using Native.Csharp.Sdk.Cqp.EventArgs;
using Native.Csharp.Sdk.Cqp.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;

namespace Native.Csharp.App
{
	/// <summary>
	/// 酷Q应用主入口类
	/// </summary>
	public static class CQMain
	{
		/// <summary>
		/// 在应用被加载时将调用此方法进行事件注册, 请在此方法里向 <see cref="IUnityContainer"/> 容器中注册需要使用的事件
		/// </summary>
		/// <param name="container">用于注册的 IOC 容器 </param>
		public static void Register(IUnityContainer container)
		{
            container.RegisterType<ICQStartup, Event_Startup>("酷Q启动事件");
            container.RegisterType<IFriendAddRequest, Event_FriendAddRequest>("好友添加请求事件");
            container.RegisterType<IGroupMessage,Event_GroupMessage>("群消息处理");
		}
	}
}
