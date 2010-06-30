// TestSsm2ReadBlockRequest.cs: Test SSM2 read block request class.

/* Copyright (C) 2010 src0x
 *
 * This file is part of LibSSM2.
 *
 * LibSSM2 is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * LibSSM2 is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with LibSSM2.  If not, see <http://www.gnu.org/licenses/>.
 */


using NUnit.Framework;
using Subaru.SSM2;

namespace Subaru.SSM2
{

	[TestFixture()]
	public class TestSsm2ReadBlockRequest
	{
		/// <summary>
		/// Read 128 (0x80) bytes from address 0x200000, to engine from diagnostic tool.
		/// </summary>
		static readonly byte[] TestPacket1 = { 0x80, 0x10, 0xf0, 0x6, 0xa0, 0x0, 0x20, 0x0, 0x0, 0x7f,
		0xc5 };



		[Test()]
		public void Parse1 ()
		{
			byte[] packetData = TestPacket1;
			Ssm2ReadBlockRequest p = new Ssm2ReadBlockRequest (packetData);
			p.FromBytes (packetData);

			AssertData1 (p);
		}

		[Test()]
		public void ConstructAndChangeProps1 ()
		{
			byte[] packetData = TestPacket1;

			Ssm2ReadBlockRequest p = new Ssm2ReadBlockRequest ();
			p.Source = Ssm2Device.DiagnosticToolF0;
			p.Destination = Ssm2Device.Engine10;
			p.Address = 0x200000;
			p.DataCount = 128;

			// should be ok after setting Data but don't depend on it!
			Assert.AreEqual (packetData.Length, p.Size, "Size before Construct");

			p.Finish ();

			AssertData1 (p);

			// changing some properties now

			p.DataCount = 129;
			// needed to calc new checksum
			p.Finish ();

			// check
			Assert.AreEqual (packetData.Length, p.Size, "Size2");
			Assert.AreEqual (true, p.Check (), "Check2()");
			Assert.AreEqual (Ssm2Device.Engine10, p.Destination, "Destination2");
			Assert.AreEqual (Ssm2Device.DiagnosticToolF0, p.Source, "Source2");
			Assert.AreEqual (Ssm2Command.ReadBlockRequestA0, p.Command, "Command2");
			Assert.AreEqual (0x200000, p.Address, "Address2");
			Assert.AreEqual (129, p.DataCount, "DataCount2");
		}

		[Test()]
		public void ConstructAndChangeProps2 ()
		{
			Ssm2ReadBlockRequest p = new Ssm2ReadBlockRequest ();
			p.Source = Ssm2Device.DiagnosticToolF0;
			p.Destination = Ssm2Device.Engine10;
			p.Address = 0x123456;
			p.DataCount = 1;

			p.Finish ();

			Assert.AreEqual (11, p.Size, "Size");
			Assert.AreEqual (true, p.Check (), "Check()");

			Assert.AreEqual (Ssm2Device.DiagnosticToolF0, p.Source, "Source");
			Assert.AreEqual (Ssm2Device.Engine10, p.Destination, "Destination");

			Assert.AreEqual (Ssm2Command.ReadBlockRequestA0, p.Command, "Command");

			Assert.AreEqual (0x123456, p.Address, "Address");
			Assert.AreEqual (1, p.DataCount, "DataCount");
		}

		[Test()]
		public void NewFromBytes ()
		{
			// best for parsing as it does not allocate anything
			var p = (Ssm2ReadBlockRequest)Ssm2Packet.NewFromBytes (TestPacket1);
			// Same static method can also be accessed like this:
			// Ssm2InitRequest.NewFromBytes (...)

			AssertData1 (p);
		}

		static void AssertData1 (Ssm2ReadBlockRequest p)
		{
			byte[] packetData = TestPacket1;

			Assert.AreEqual (packetData.Length, p.Size, "Size");
			Assert.AreEqual (true, p.Check (), "Check()");

			Assert.AreEqual (Ssm2Device.Engine10, p.Destination, "Destination");
			Assert.AreEqual (Ssm2Device.DiagnosticToolF0, p.Source, "Source");
			Assert.AreEqual (Ssm2Command.ReadBlockRequestA0, p.Command, "Command");

			Assert.AreEqual (0x200000, p.Address, "Address");
			Assert.AreEqual (128, p.DataCount, "Data");

			byte[] bytes = p.ToBytesCopy ();
			for (int i = 0; i < packetData.Length; i++) {
				Assert.AreEqual (packetData[i], bytes[i], "bytes[i]");
			}
		}
	}
}
