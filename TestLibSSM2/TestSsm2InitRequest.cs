// TestSsm2InitRequest.cs: Test SSM2 init request class.

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

namespace Subaru.SSM2
{

	[TestFixture()]
	public class TestSsm2InitRequest
	{
		/// <summary>
		/// Typical engine control unit init request.
		/// </summary>
		static readonly byte[] EcuInit1 = { 0x80, 0x10, 0xF0, 0x01, 0xBF, 0x40 };

		[Test()]
		public void CreateByConstructor ()
		{
			// Only 2 standard properties/arguments to specify for this type of packet.
			Ssm2InitRequest p = new Ssm2InitRequest (Ssm2Device.Engine10, Ssm2Device.DiagnosticToolF0);

			AssertProperties1 (p);
		}

		[Test()]
		public void CreateByProperties ()
		{
			// Ssm2InitRequest p = new Ssm2InitRequest ();
			// p.Destination = Ssm2Device.Engine10;
			// p.Source = Ssm2Device.DiagnosticToolF0;

			// same effect, using object initializer syntax:
			Ssm2InitRequest p = new Ssm2InitRequest { Destination = Ssm2Device.Engine10, Source = Ssm2Device.DiagnosticToolF0 };
			// Important: packet is still invalid at this point!

			// don't depend on Size yet even though it is correct for this type
			// Assert.AreEqual (packetData.Length, p.Size, "Size");
			// No checksum yet so check will fail.
			Assert.AreEqual (false, p.Check (), "Check()");

			// Important: as with default constructor
			// the object does not know when all properties have been set.
			// This call will calculate the checksum, making packet complete.
			p.Finish ();
			// packet is now complete

			AssertProperties1 (p);
		}

		[Test()]
		public void NewFromBytes ()
		{
			byte[] packetData = EcuInit1;
			// best for parsing as it does not allocate anything
			Ssm2InitRequest p = (Ssm2InitRequest)Ssm2Packet.NewFromBytes (packetData);
			// Same static method can also be accessed like this:
			// Ssm2InitRequest.NewFromBytes (...)

			AssertProperties1 (p);
		}

		[Test()]
		public void FromBytes ()
		{
			byte[] packetData = EcuInit1;
			// inefficient for parsing, since constructor allocates a buffer
			Ssm2InitRequest p = new Ssm2InitRequest ();
			// take given buffer, discarding previous implicit buffer
			p.FromBytes (packetData);

			AssertProperties1 (p);
		}

		[Test()]
		public void NewFromBytesBase ()
		{
			// same results as casting to specific type (Ssm2InitRequest)
			// sufficient to use base type here since this packet type has no specific data
			var p = Ssm2InitRequest.NewFromBytes (EcuInit1);
			// object reference gets type Ssm2Packet

			AssertProperties1 (p);
		}


		static void AssertProperties1 (Ssm2Packet p)
		{
			byte[] expectedPacketData = EcuInit1;

			Assert.IsInstanceOfType (typeof(Ssm2InitRequest), p, "type");
			Assert.IsInstanceOfType (typeof(Ssm2Packet), p, "base type");

			Assert.AreEqual (expectedPacketData.Length, p.Size, "Size");
			Assert.AreEqual (true, p.Check (), "Check()");

			Assert.AreEqual (Ssm2Device.Engine10, p.Destination, "Destination");
			Assert.AreEqual (Ssm2Device.DiagnosticToolF0, p.Source, "Source");
			Assert.AreEqual (Ssm2Command.InitRequestBF, p.Command, "Command");

			byte[] bytes = p.ToBytesCopy ();
			for (int i = 0; i < expectedPacketData.Length; i++) {
				Assert.AreEqual (expectedPacketData[i], bytes[i], "bytes[]");
			}
		}
	}
}
