using System;
using System.Net;
using System.Reflection;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Proshot.CommandClient;
using Rhino.Mocks;
using System.Linq;

namespace CommandClientVisualStudioTest
{
	[TestClass]
	public class AdvancedMockTests
	{
		private MockRepository mocks;

		[TestMethod]
		public void VerySimpleTest()
		{
			CMDClient client = new CMDClient(null, "Bogus network name");
			Assert.AreEqual("Bogus network name", client.NetworkName);
		}

		[TestInitialize()]
		public void Initialize()
		{
			mocks = new MockRepository();
		}

		[TestMethod]
		public void TestUserExitCommand()
		{
			IPAddress ipaddress = IPAddress.Parse("127.0.0.1");
			Command command = new Command(CommandType.UserExit, ipaddress, null);
			System.IO.Stream fakeStream = mocks.DynamicMock<System.IO.Stream>();
			byte[] commandBytes = { 0, 0, 0, 0 };
			byte[] ipLength = { 9, 0, 0, 0 };
			byte[] ip = { 49, 50, 55, 46, 48, 46, 48, 46, 49 };
			byte[] metaDataLength = { 2, 0, 0, 0 };
			byte[] metaData = { 10, 0 };

			using (mocks.Ordered())
			{
				fakeStream.Write(commandBytes, 0, 4);
				fakeStream.Flush();
				fakeStream.Write(ipLength, 0, 4);
				fakeStream.Flush();
				fakeStream.Write(ip, 0, 9);
				fakeStream.Flush();
				fakeStream.Write(metaDataLength, 0, 4);
				fakeStream.Flush();
				fakeStream.Write(metaData, 0, 2);
				fakeStream.Flush();
			}
			mocks.ReplayAll();
			CMDClient client = new CMDClient(null, "Bogus network name");

			// we need to set the private variable here
			client.GetType().GetField("networkStream", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(client, fakeStream);

			client.SendCommandToServerUnthreaded(command);
			mocks.VerifyAll();

		}

		[TestMethod]
		public void TestUserExitCommandWithoutMocks()
		{
			IPAddress ipaddress = IPAddress.Parse("127.0.0.1");
			Command command = new Command(CommandType.UserExit, ipaddress, null);
			MemoryStream fakeStream = new MemoryStream();

			CMDClient client = new CMDClient(null, "Bogus network name");
			client.GetType().GetField("networkStream", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(client, fakeStream);

			client.SendCommandToServerUnthreaded(command);

			byte[] expected = { 0, 0, 0, 0, 9, 0, 0, 0, 49, 50, 55, 46, 48, 46, 48, 46, 49, 2, 0, 0, 0, 10, 0 };
			byte[] result = new byte[23];
			fakeStream.Seek(0, SeekOrigin.Begin);
			fakeStream.Read(result, 0, 23);

			CollectionAssert.AreEqual(expected, result);
		}

		[TestMethod]
		public void TestSemaphoreReleaseOnNormalOperation()
		{
			IPAddress ipaddress = IPAddress.Parse("127.0.0.1");
			Command command = new Command(CommandType.UserExit, ipaddress, null);
			System.IO.Stream fakeStream = mocks.DynamicMock<System.IO.Stream>();
			byte[] commandBytes = { 0, 0, 0, 0 };
			byte[] ipLength = { 9, 0, 0, 0 };
			byte[] ip = { 49, 50, 55, 46, 48, 46, 48, 46, 49 };
			byte[] metaDataLength = { 2, 0, 0, 0 };
			byte[] metaData = { 10, 0 };

			System.Threading.Semaphore fakeSemaphore = mocks.DynamicMock<System.Threading.Semaphore>();
			using (mocks.Ordered())
			{
				fakeStream.Write(commandBytes, 0, 4);
				fakeStream.Flush();
				fakeStream.Write(ipLength, 0, 4);
				fakeStream.Flush();
				fakeStream.Write(ip, 0, 9);
				fakeStream.Flush();
				fakeStream.Write(metaDataLength, 0, 4);
				fakeStream.Flush();
				fakeStream.Write(metaData, 0, 2);
				fakeStream.Flush();
			}
			mocks.ReplayAll();
			CMDClient client = new CMDClient(null, "Bogus network name");

			// we need to set the private variable here
			client.GetType().GetField("networkStream", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(client, fakeStream);
			client.GetType().GetField("semaphore", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(client, fakeSemaphore);

			client.SendCommandToServerUnthreaded(command);
			mocks.VerifyAll();
		}

		[TestMethod]
		public void TestSemaphoreReleaseOnExceptionalOperation()
		{
			IPAddress ipaddress = IPAddress.Parse("127.0.0.1");
			Command command = new Command(CommandType.UserExit, ipaddress, null);
			System.IO.Stream fakeStream = mocks.DynamicMock<System.IO.Stream>();
			byte[] commandBytes = { 0, 0, 0, 0 };
			byte[] ipLength = { 9, 0, 0, 0 };
			byte[] ip = { 49, 50, 55, 46, 48, 46, 48, 46, 49 };
			byte[] metaDataLength = { 2, 0, 0, 0 };
			byte[] metaData = { 10, 0 };

			System.Threading.Semaphore fakeSemaphore = mocks.DynamicMock<System.Threading.Semaphore>();
			using (mocks.Ordered())
			{
				Expect.Call(fakeSemaphore.WaitOne()).Return(true);
				fakeStream.Write(commandBytes, 0, 4);
				fakeStream.Flush();
				fakeStream.Write(ipLength, 0, 4);
				fakeStream.Flush();
				fakeStream.Write(ip, 0, 9);
				fakeStream.Flush();
				fakeStream.Write(metaDataLength, 0, 4);
				fakeStream.Flush();
				fakeStream.Write(metaData, 0, 2);
				fakeStream.Flush();
				LastCall.On(fakeStream).Throw(new IOException());
				Expect.Call(fakeSemaphore.Release()).Return(1);
			}
			mocks.ReplayAll();
			CMDClient client = new CMDClient(null, "Bogus network name");

			// we need to set the private variable here
			client.GetType().GetField("networkStream", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(client, fakeStream);
			client.GetType().GetField("semaphore", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(client, fakeSemaphore);

			try
			{
				client.SendCommandToServerUnthreaded(command);
			}
			catch (IOException e)
			{
			}
			mocks.VerifyAll();

		}
	}
}
