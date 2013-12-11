using Microsoft.AspNet.SignalR.Client;
using System.Threading;



namespace DynamicSignalR
{
	/// <summary>
	/// HubConnectionの拡張機能を提供します。
	/// </summary>
	public static class HubConnectionExtensions
	{
		/// <summary>
		/// Hubへの動的なアクセスを行うためのプロキシを生成します。
		/// </summary>
		/// <param name="connection">HubConnection</param>
		/// <param name="hubName">Hubの名称</param>
		/// <param name="capturesSynchronizationContext">現在の同期コンテキストを捕捉し、コールバックで利用するかどうか</param>
		/// <returns>動的なプロキシ</returns>
		public static dynamic CreateDynamicHubProxy(this HubConnection connection, string hubName, bool capturesSynchronizationContext = false)
		{
			var context = capturesSynchronizationContext ? SynchronizationContext.Current : null;
			return connection.CreateDynamicHubProxy(hubName, context);
		}


		/// <summary>
		/// Hubへの動的なアクセスを行うためのプロキシを生成します。
		/// </summary>
		/// <param name="connection">HubConnection</param>
		/// <param name="hubName">Hubの名称</param>
		/// <param name="context">コールバック時に利用する同期コンテキスト</param>
		/// <returns>動的なプロキシ</returns>
		public static dynamic CreateDynamicHubProxy(this HubConnection connection, string hubName, SynchronizationContext context = null)
		{
			var proxy = connection.CreateHubProxy(hubName);
			return new DynamicHubProxy(proxy, context);
		}
	}
}