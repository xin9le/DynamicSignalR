using System;



namespace DynamicSignalR
{
	/// <summary>
	/// Dispose処理を外部から指定する機能を提供します。
	/// </summary>
	internal class AnonymousDisposable : IDisposable
	{
		/// <summary>
		/// Dispose処理を保持します。
		/// </summary>
		private readonly Action onDispose = null;


		/// <summary>
		/// Dispose処理を指定してインスタンスを生成します。
		/// </summary>
		/// <param name="onDispose">Dispose処理</param>
		public AnonymousDisposable(Action onDispose)
		{
			this.onDispose = onDispose;
		}


		/// <summary>
		/// 使用したリソースを開放します。
		/// </summary>
		public void Dispose()
		{
			if (this.onDispose != null)
				this.onDispose();
		}
	}
}