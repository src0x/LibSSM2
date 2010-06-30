// TestSsm2ReadAddressesRequest.cs: Test SSM2 read addresses request class.

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


using System;
using System.Collections.Generic;
using NUnit.Framework;
using Subaru.SSM2;

namespace Subaru.SSM2
{

	[TestFixture()]
	public class TestSsm2ReadAddressesRequest
	{
		readonly byte[] TestPacket1 = { 0x80, 0x10, 0xf0, 0x5, 0xa8, 0x0, 0x12, 0x34, 0x56, 0xc9 };
		readonly byte[] TestPacket2 = { 0x80, 0x10, 0xf0, 0x8, 0xa8, 0x0, 0x12, 0x34, 0x56, 0x0,
		0x0, 0x1c, 0xe8 };

		[Test()]
		public void ConstructorComplete1 ()
		{
			// Create a ReadAddressesRequest, requesting one address.

			// This constructor takes all needed information,
			// allocates optimal buffer size and constructs a complete packet.
			Ssm2ReadAddressesRequest p = new Ssm2ReadAddressesRequest (
				Ssm2Device.Engine10,
				Ssm2Device.DiagnosticToolF0,
				new[] { 0x123456 }
			);

			// Predetermined packet bytes to compare with.
			byte[] expected = TestPacket1;

			// check standard properties
			Assert.AreEqual (expected.Length, p.Size, "Size");
			Assert.AreEqual (true, p.Check (), "Check()");
			Assert.AreEqual (Ssm2Device.Engine10, p.Destination, "Destination");
			Assert.AreEqual (Ssm2Device.DiagnosticToolF0, p.Source, "Source");
			Assert.AreEqual (Ssm2Command.ReadAddressesRequestA8, p.Command, "Command");

			// check packet type specifics
			IList<int> addrList = p.Addresses;
			Assert.AreEqual (1, addrList.Count, "addrList.Count");
			Assert.AreEqual (1, p.AddressesCount, "AddressesCount");
			Assert.AreEqual (0x123456, addrList[0], "adr[0]");

			// take raw packet bytes
			byte[] buffer = p.ToBytesCopy ();

			// compare raw packet bytes
			for (int i = 0; i < expected.Length; i++) {
				Assert.AreEqual (expected[i], buffer[i], "buffer[" + i.ToString () + "]");
			}
		}

		[Test()]
		public void Parse1 ()
		{
			byte[] expected = TestPacket1;

			// Static method NewFromBytes () analyzes packet type,
			// and returns a specific packet object.
			// Based on given data in this case we know upfront,
			// it'll be an read-addresses-request.
			Ssm2ReadAddressesRequest p;
			p = (Ssm2ReadAddressesRequest) Ssm2Packet.NewFromBytes (expected);

			// check standard properties
			Assert.AreEqual (expected.Length, p.Size, "Size");
			Assert.AreEqual (true, p.Check (), "Check()");
			Assert.AreEqual (Ssm2Device.Engine10, p.Destination, "Destination");
			Assert.AreEqual (Ssm2Device.DiagnosticToolF0, p.Source, "Source");
			Assert.AreEqual (Ssm2Command.ReadAddressesRequestA8, p.Command, "Command");

			// check packet type specifics
			IList<int> addresses = p.Addresses;
			Assert.AreEqual (1, addresses.Count, "addrList.Count");
			Assert.AreEqual (1, p.AddressesCount, "AddressesCount");
			Assert.AreEqual (0x123456, addresses[0], "adr[0]");
		}

		[Test()]
		public void Parse2 ()
		{
			byte[] packetData = TestPacket2;
			Ssm2ReadAddressesRequest p = new Ssm2ReadAddressesRequest ();
			p.FromBytes (packetData);

			Assert.AreEqual (packetData.Length, p.Size, "Size");
			Assert.AreEqual (true, p.Check (), "Check()");

			Assert.AreEqual (Ssm2Device.Engine10, p.Destination, "Destination");
			Assert.AreEqual (Ssm2Device.DiagnosticToolF0, p.Source, "Source");
			Assert.AreEqual (Ssm2Command.ReadAddressesRequestA8, p.Command, "Command");

			IList<int> addresses = p.Addresses;
			Assert.AreEqual (2, addresses.Count, "addrList.Count");
			Assert.AreEqual (2, p.AddressesCount, "AddressesCount");
			Assert.AreEqual (0x123456, addresses[0], "adr[0]");
			Assert.AreEqual (0x1c, addresses[1], "adr[1]");
		}

		// MaxAddressesPerPacket allowed range: 1 - 84

		[Test(), ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void StaticMaxAddressesPerPacket1 ()
		{
			Ssm2ReadAddressesRequest.MaxAddressesPerPacket = 0;
		}

		[Test(), ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void StaticMaxAddressesPerPacket2 ()
		{
			Ssm2ReadAddressesRequest.MaxAddressesPerPacket = 85;
		}

		[Test(), ExpectedException(typeof(InvalidOperationException))]
		public void NotAllPropertiesSet1 ()
		{
			Ssm2ReadAddressesRequest p = new Ssm2ReadAddressesRequest ();
			p.Source = Ssm2Device.DiagnosticToolF0;
			p.Addresses = new int[] { 0 };
			p.Finish ();
		}

		[Test(), ExpectedException(typeof(InvalidOperationException))]
		public void NotAllPropertiesSet2 ()
		{
			Ssm2ReadAddressesRequest p = new Ssm2ReadAddressesRequest ();
			p.Destination = Ssm2Device.Engine10;
			p.Addresses = new int[] { 0 };
			p.Finish ();
		}

		[Test(), ExpectedException(typeof(InvalidOperationException))]
		public void NotAllPropertiesSet3 ()
		{
			Ssm2ReadAddressesRequest p = new Ssm2ReadAddressesRequest ();
			p.Destination = Ssm2Device.Engine10;
			p.Source = Ssm2Device.DiagnosticToolF0;
			p.Finish ();
		}

		[Test()]
		public void ConstructByPropertiesAndModify1 ()
		{
			byte[] packetData = TestPacket1;

			// will allocate full buffer since number of addresses is unknown
			Ssm2ReadAddressesRequest p = new Ssm2ReadAddressesRequest ();

			p.Source = Ssm2Device.DiagnosticToolF0;
			p.Destination = Ssm2Device.Engine10;
			p.Addresses = new int[] { 0x123456 };

			// should be ok after setting Data
			Assert.AreEqual (packetData.Length, p.Size, "Size before Construct");

			// necessary to create checksum, making packet complete
			p.Finish ();


			byte[] actual = p.ToBytesCopy ();
			for (int i = 0; i < packetData.Length; i++) {
				Assert.AreEqual (packetData[i], actual[i], "actual[" + i.ToString () + "]");
			}

			Assert.AreEqual (packetData.Length, p.Size, "Size");
			Assert.AreEqual (true, p.Check (), "Check");

			Assert.AreEqual (Ssm2Device.Engine10, p.Destination, "Destination");
			Assert.AreEqual (Ssm2Device.DiagnosticToolF0, p.Source, "Source");
			Assert.AreEqual (Ssm2Command.ReadAddressesRequestA8, p.Command, "Command");

			IList<int> addresses = p.Addresses;
			Assert.AreEqual (1, addresses.Count, "addrList.Count");
			Assert.AreEqual (1, p.AddressesCount, "AddressesCount");
			Assert.AreEqual (0x123456, addresses[0], "adr[0]");

			// change properties that don't affect packet length
			p.Source = Ssm2Device.Engine10;
			p.Destination = Ssm2Device.DiagnosticToolF0;
			p.Addresses = new[] { 1, 2, 3 };
			p.Finish ();

			Assert.AreEqual (true, p.Check (), "Check2");

			Assert.AreEqual (Ssm2Device.DiagnosticToolF0, p.Destination, "Destination2");
			Assert.AreEqual (Ssm2Device.Engine10, p.Source, "Source2");
			Assert.AreEqual (Ssm2Command.ReadAddressesRequestA8, p.Command, "Command2");
			Assert.AreEqual (3, p.AddressesCount, "AddressesCount2");
			addresses = p.Addresses;
			Assert.AreEqual (3, addresses.Count, "addrList.Count2");
			Assert.AreEqual (1, addresses[0], "adr2[0]");
			Assert.AreEqual (2, addresses[1], "adr2[1]");
			Assert.AreEqual (3, addresses[2], "adr2[2]");
		}



		[Test(), ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void AddressesCountOutOfRange0 ()
		{
			// this constructor will allocate optimal buffer and return a complete packet
			new Ssm2ReadAddressesRequest (Ssm2Device.Engine10, Ssm2Device.DiagnosticToolF0, new int[] {  });
			// no need to call Construct() with above constructor
		}

		[Test(), ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void AddressesCountOutOfRange1 ()
		{
			Ssm2ReadAddressesRequest.MaxAddressesPerPacket = 5;
			new Ssm2ReadAddressesRequest (Ssm2Device.Engine10, Ssm2Device.DiagnosticToolF0, new int[] { 1, 2, 3, 4, 5, 6 });
		}

		[Test()]
		public void IncreaseAddressesCount1 ()
		{
			Ssm2ReadAddressesRequest.MaxAddressesPerPacket = 30;
			var p = new Ssm2ReadAddressesRequest (Ssm2Device.Engine10, Ssm2Device.DiagnosticToolF0, new int[] { 1, 2, 3, 4, 5, 6 });

			// modify
			p.Addresses = new int[] { 1, 2, 3, 4, 5, 6, 7 };
			p.Finish ();

//			Assert.AreEqual (packetData.Length, p.Size, "Size");
			Assert.AreEqual (true, p.Check (), "Check()");

			Assert.AreEqual (Ssm2Device.Engine10, p.Destination, "Destination");
			Assert.AreEqual (Ssm2Device.DiagnosticToolF0, p.Source, "Source");
			Assert.AreEqual (Ssm2Command.ReadAddressesRequestA8, p.Command, "Command");

			IList<int> addresses = p.Addresses;
			Assert.AreEqual (7, addresses.Count, "addrList.Count");
			Assert.AreEqual (7, p.AddressesCount, "AddressesCount");
//			Assert.AreEqual (0x123456, addresses[0], "adr[0]");

		}

		[Test(), ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void MaxAddressesPerPacket0 ()
		{
			// at least one address is required, max value cannot be < 1
			Ssm2ReadAddressesRequest.MaxAddressesPerPacket = 0;
		}

		[Test(), ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void MaxAddressesPerPacketMax ()
		{
			// theoretical packet maximum is 84
			// in practice it is usually lower, depends on car
			// Modern car ECUs support up to 82 addresses per packet.
			Ssm2ReadAddressesRequest.MaxAddressesPerPacket = 85;
		}

		[Test()]
		public void NewFromBytes1 ()
		{
			byte[] packetData = TestPacket1;
			// Static method NewFromBytes () analyzes packet type,
			// and returns a specific packet object.
			// Based on given data in this case we know upfront,
			// it'll be an read-addresses-request.
			var p = Ssm2ReadAddressesRequest.NewFromBytes (packetData);
			Assert.IsInstanceOfType (typeof(Ssm2ReadAddressesRequest), p, "type");
		}

		[Test()]
		public void NewFromBytes2 ()
		{
			byte[] packetData = TestPacket2;
			var p = Ssm2ReadAddressesRequest.NewFromBytes (packetData);
			Assert.IsInstanceOfType (typeof(Ssm2ReadAddressesRequest), p, "type");
		}
	}
}
