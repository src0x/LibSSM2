// TestSsm2ReadBlockResponse.cs: Test SSM2 read block response class.

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
	public class TestSsm2ReadBlockResponse
	{
		readonly byte[] TestPacket1 = { 0x80, 0xF0, 0x10, 0x04, 0xE0, 0x01, 0x02, 0x03, 0x6A };
		readonly byte[] TestPacket1Data = { 0x01, 0x02, 0x03 };

		[Test()]
		public void Construct2 ()
		{
			byte[] packetData = TestPacket1;
			byte[] sourcePayloadData = TestPacket1Data;

			Ssm2ReadBlockResponse p = new Ssm2ReadBlockResponse ();
			p.Destination = Ssm2Device.DiagnosticToolF0;
			p.Source = Ssm2Device.Engine10;
			p.Data = TestPacket1Data;

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
			Assert.AreEqual (Ssm2Command.ReadBlockResponseE0, p.Command, "Command");

			IList<byte> data = p.Data;
			Assert.AreEqual (sourcePayloadData.Length, data.Count, "DataList.Count");
			Assert.AreEqual (sourcePayloadData.Length, p.DataCount, "DataCount");
			for (int i = 0; i < data.Count; i++) {
				Assert.AreEqual (TestPacket1Data[i], data[i], "data[" + i.ToString () + "]");
			}
		}

		[Test(), ExpectedException(typeof(InvalidOperationException))]
		public void NotAllPropertiesSet1 ()
		{
			Ssm2ReadBlockResponse p = new Ssm2ReadBlockResponse ();
			p.Source = Ssm2Device.DiagnosticToolF0;
			p.Data = new byte[] { 0 };
			p.Finish ();
		}

		[Test(), ExpectedException(typeof(InvalidOperationException))]
		public void NotAllPropertiesSet2 ()
		{
			Ssm2ReadBlockResponse p = new Ssm2ReadBlockResponse ();
			p.Destination = Ssm2Device.Engine10;
			p.Data = new byte[] { 0, 1, 2, 3, 4, 5 };
			p.Finish ();
		}

		[Test(), ExpectedException(typeof(InvalidOperationException))]
		public void NotAllPropertiesSet3 ()
		{
			Ssm2ReadBlockResponse p = new Ssm2ReadBlockResponse ();
			p.Destination = Ssm2Device.Engine10;
			p.Source = Ssm2Device.DiagnosticToolF0;
			p.Finish ();
		}

		[Test()]
		public void NewFromBytes2 ()
		{
			byte[] packetData = TestPacket1;
			var p = Ssm2ReadBlockResponse.NewFromBytes (packetData);
			Assert.IsInstanceOfType (typeof(Ssm2ReadBlockResponse), p, "type");
		}
	}
}
