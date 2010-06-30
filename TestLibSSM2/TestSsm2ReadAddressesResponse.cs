// TestSsm2ReadAddressesResponse.cs: Test SSM2 read addresses response class.

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
	public class TestSsm2ReadAddressesResponse
	{
		readonly byte[] TestPacket2 = { 0x80, 0xF0, 0x10, 0x03, 0xE8, 0x7D, 0xB1, 0x99 };
		readonly byte[] TestPacket2Data = { 0x7D, 0xB1 };

		[Test()]
		public void Construct2 ()
		{
			byte[] packetData = TestPacket2;

			Ssm2ReadAddressesResponse p = new Ssm2ReadAddressesResponse ();
			p.Destination = Ssm2Device.DiagnosticToolF0;
			p.Source = Ssm2Device.Engine10;
			p.Data = TestPacket2Data;

			// should be ok after setting Data
			Assert.AreEqual (packetData.Length, p.Size, "Size before Construct");

			p.Finish ();


			byte[] actual = p.ToBytesCopy ();
			for (int i = 0; i < packetData.Length; i++) {
				Assert.AreEqual (packetData[i], actual[i], "actual[" + i.ToString () + "]");
			}

			Assert.AreEqual (packetData.Length, p.Size, "Size");
			Assert.AreEqual (true, p.Check (), "Check");

			Assert.AreEqual (Ssm2Device.DiagnosticToolF0, p.Destination, "Destination");
			Assert.AreEqual (Ssm2Device.Engine10, p.Source, "Source");
			Assert.AreEqual (Ssm2Command.ReadAddressesResponseE8, p.Command, "Command");

			IList<byte> data = p.Data;
			Assert.AreEqual (2, data.Count, "DataList.Count");
			Assert.AreEqual (2, p.DataCount, "DataCount");
			for (int i = 0; i < data.Count; i++) {
				Assert.AreEqual (TestPacket2Data[i], data[i], "data[" + i.ToString () + "]");
			}
		}

		[Test(), ExpectedException(typeof(InvalidOperationException))]
		public void NotAllPropertiesSet1 ()
		{
			Ssm2ReadAddressesResponse p = new Ssm2ReadAddressesResponse ();
			p.Source = Ssm2Device.DiagnosticToolF0;
			p.Data = new byte[] { 0 };
			p.Finish ();
		}

		[Test(), ExpectedException(typeof(InvalidOperationException))]
		public void NotAllPropertiesSet2 ()
		{
			Ssm2ReadAddressesResponse p = new Ssm2ReadAddressesResponse ();
			p.Destination = Ssm2Device.Engine10;
			p.Data = new byte[] { 0, 1, 2, 3, 4, 5 };
			p.Finish ();
		}

		[Test(), ExpectedException(typeof(InvalidOperationException))]
		public void NotAllPropertiesSet3 ()
		{
			Ssm2ReadAddressesResponse p = new Ssm2ReadAddressesResponse ();
			p.Destination = Ssm2Device.Engine10;
			p.Source = Ssm2Device.DiagnosticToolF0;
			p.Finish ();
		}

		[Test()]
		public void NewFromBytes2 ()
		{
			byte[] packetData = TestPacket2;
			var p = Ssm2ReadAddressesRequest.NewFromBytes (packetData);
			Assert.IsInstanceOfType (typeof(Ssm2ReadAddressesResponse), p, "type");
		}
	}
}
