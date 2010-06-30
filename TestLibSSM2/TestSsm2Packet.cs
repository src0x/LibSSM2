// TestSsm2Packet.cs: Test SSM2 packet base class.

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
using System;

namespace Subaru.SSM2
{

	[TestFixture()]
	public class TestSsm2Packet
	{

		[Test()]
		public void EmptyPacketSpecifyingBuffer ()
		{
			byte[] buffer = new byte[Ssm2Packet.PacketSizeMax];

			// Constructor for initializing packet object by
			// providing own (empty) buffer

			// For parsing existing content use instance method .FromBytes (buffer) or
			// static method Ssm2Packet.NewFromBytes (buffer) instead!

			Ssm2Packet p = new Ssm2Packet (buffer);

			// packet buffer mostly contains zeroes
			// undefined, don't check:
			// Size, CheckSum, etc.

			//Assert.AreEqual (0, p.Size, "Size");

			Assert.AreEqual ((Ssm2Command)0, p.Command, "Command");

			// Assert.AreEqual (0, p.ChecksumCalculated, "ChecksumCalculated");
			Assert.AreEqual (false, p.Check (), "Check");

			// not useful, just demonstrating that packet is invalid
			// byte[] buf = p.ToBytesCopy ();
			//Assert.AreEqual (0, buf.Length, "buf.Length");
		}

		[Test()]
		public void DefaultPacket ()
		{
			Ssm2Packet p = new Ssm2Packet ();

			Assert.AreEqual (false, p.Check (), "Check");

			// Following tests are very implementation specific
			// and rather for debugging purposes!
			// Real code should not depend on most packet properties
			// except Check(), Capacity, Size.

			// packet is almost empty since no properties haven't been set yet
			Assert.AreEqual (Ssm2Packet.PacketSizeMin, p.Size, "Size");
			// only 1st byte is set to 128, all other bytes are 0,
			// therefore checksum is 128, too.
			Assert.AreEqual (128, p.ChecksumCalculated, "ChecksumCalculated");

			// not useful, just demonstrating that packet is invalid
			byte[] buf = p.ToBytesCopy ();

			// generic packet type has minimum size
			Assert.AreEqual (Ssm2Packet.PacketSizeMin, buf.Length, "buf.Length");
		}

		[Test()]
		public void EqualsDefaultPacket ()
		{
			Ssm2Packet p1, p2;
			p1 = new Ssm2Packet ();
			p2 = new Ssm2Packet ();

			Assert.AreEqual (true, p1.Equals (p2), "Equals1");
			Assert.AreEqual (true, p1.Equals ((object)p2), "Equals2");
		}

	}
}
