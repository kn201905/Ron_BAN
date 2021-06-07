using System;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

static class Program
{
	static async Task Main()
	{
		Console.WriteLine("--- before await");
		await (new LazyHello()).Run();
		Console.WriteLine("--- after await");
	}
    
	class LazyHello
	{
		public struct HelloAwaiter : INotifyCompletion
		{
			public LazyHello m_parent;
            
			public bool IsCompleted { get { return m_parent.mb_completed; } }
			public void OnCompleted(Action continuation) { ms_continuation = continuation; }
			public void GetResult() {}
		}

		HelloAwaiter m_awaiter;
		public HelloAwaiter GetAwaiter() => m_awaiter;

		bool mb_completed = false;
		static Action ms_continuation = null;

		public LazyHello()
		{
			m_awaiter.m_parent = this;
		}

		public LazyHello Run()
		{
			(new Thread(new ThreadStart(ThreadProc))).Start();
			return this;
		}

		static void ThreadProc()
		{
			Thread.Sleep(3000);
			Console.WriteLine("LazyHello!");

			ms_continuation();
		}
	}
}
