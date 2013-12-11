using Microsoft.AspNet.SignalR.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Threading;



namespace DynamicSignalR
{
	/// <summary>
	/// Hubへの動的アクセスを行うための機能を提供します。
	/// </summary>
    internal class DynamicHubProxy : DynamicObject
	{
		#region フィールド
		/// <summary>
		/// Hubプロキシを保持します。
		/// </summary>
		private readonly IHubProxy proxy = null;


		/// <summary>
		/// コールバック時に利用する同期コンテキストを保持します。
		/// </summary>
		private readonly SynchronizationContext context = null;


		/// <summary>
		/// 関連付けたコールバックの解除機構を保持します。
		/// </summary>
		private readonly IDictionary<string, IDisposable> eventDisposers = null;
		#endregion


		#region コンストラクタ
		/// <summary>
		/// インスタンスを生成します。
		/// </summary>
		/// <param name="proxy">内部で利用するHubプロキシ</param>
		/// <param name="context">コールバック時に利用する同期コンテキスト</param>
		public DynamicHubProxy(IHubProxy proxy, SynchronizationContext context)
		{
			this.proxy		= proxy;
			this.context	= context;
			this.eventDisposers		= new Dictionary<string, IDisposable>();
		}
		#endregion


		#region オーバーライド
		/// <summary>
		/// メンバーを呼び出す演算の実装を提供します。派生クラスでこのメソッドをオーバーライドして、メソッドの呼び出しなどの演算の動的な動作を指定できます。
		/// </summary>
		/// <param name="binder">動的な演算に関する情報を提供します。</param>
		/// <param name="args">呼び出し演算でオブジェクト メンバーに渡される引数。</param>
		/// <param name="result">メンバー呼び出しの結果。</param>
		/// <returns>操作が正常に終了した場合は true。それ以外の場合は false。</returns>
		public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
		{
			result = this.proxy.Invoke(binder.Name, args);
			return true;
		}


		/// <summary>
		/// メンバー値を設定する演算の実装を提供します。派生クラスでこのメソッドをオーバーライドして、プロパティ値の設定などの演算の動的な動作を指定できます。
		/// </summary>
		/// <param name="binder">動的演算を呼び出したオブジェクトに関する情報を提供します。</param>
		/// <param name="value">メンバーに設定する値。</param>
		/// <returns>操作が正常に終了した場合は true。それ以外の場合は false。</returns>
		public override bool TrySetMember(SetMemberBinder binder, object value)
		{
			//--- 登録済みのハンドラを解除
			if (this.eventDisposers.ContainsKey(binder.Name))
			{
				this.eventDisposers[binder.Name].Dispose();
				this.eventDisposers.Remove(binder.Name);
			}

			//--- コールバックが指定されていなければ終了
			var callback = value as Delegate;
			if (callback == null)
				return true;
			
			//--- イベントハンドラ
			Action<IList<JToken>> handler = tokens =>
			{
				var args	= callback.GetMethodInfo().GetParameters()
							.Select((x, i) => new { Token = tokens[i], Type = x.ParameterType })
							.Select(x => x.Token == null ? null : x.Token.ToObject(x.Type))
							.ToArray();
				if (this.context == null)
				{
					callback.DynamicInvoke(args);
					return;
				}
				this.context.Post(_ =>
				{
					callback.DynamicInvoke(args);
				}, null);
			};

			//--- 購読 & 解除方法の記憶
			var subscription	= this.proxy.Subscribe(binder.Name);
			var disposer		= new AnonymousDisposable(() => subscription.Received -= handler);
			subscription.Received += handler;
			this.eventDisposers.Add(binder.Name, disposer);

			//--- ok
			return true;
		}
		#endregion
	}
}