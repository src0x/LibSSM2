// TestSsm2InitResponse.cs: Test SSM2 init response class.

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
	public class TestSsm2InitResponse
	{
		/// <summary>
		/// Sample ECU init response from engine to diagnostic tool.
		/// Taken from documentation file SSM2_Protocol.txt.
		/// Contains 48 capability bytes.
		/// </summary>
		static readonly byte[] TestPacket1 = { 0x80, 0xf0, 0x10, 0x39, 0xff, 0xa2, 0x10, 0xf, 0x1b, 0x14,
		0x40, 0x5, 0x5, 0x73, 0xfa, 0xeb, 0x80, 0x2b, 0xc1, 0x2,
		0xaa, 0x0, 0x10, 0x0, 0x60, 0xce, 0x54, 0xf8, 0xb0, 0x60,
		0x0, 0x0, 0xe0, 0x0, 0x0, 0x0, 0x0, 0x0, 0xdc, 0x0,
		0x0, 0x55, 0x10, 0x0, 0x0, 0x2, 0x0, 0x0, 0x0, 0x0,
		0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0,
		0x0, 0x1f };

		// parts for verification
		static readonly byte[] SSMID1 = { 0xa2, 0x10, 0xf };
		static readonly byte[] ROMID1 = { 0x1b, 0x14, 0x40, 0x5, 0x5 };
		static readonly byte[] Capabilities1 = { 0x73, 0xfa, 0xeb, 0x80, 0x2b, 0xc1, 0x2, 0xaa, 0x0, 0x10,
		0x0, 0x60, 0xce, 0x54, 0xf8, 0xb0, 0x60, 0x0, 0x0, 0xe0,
		0x0, 0x0, 0x0, 0x0, 0x0, 0xdc, 0x0, 0x0, 0x55, 0x10,
		0x0, 0x0, 0x2, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0,
		0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0 };


		[Test()]
		public void NewFromBytes1 ()
		{
			byte[] packetData = TestPacket1;
			var p = (Ssm2InitResponse)Ssm2ReadAddressesRequest.NewFromBytes (packetData);

			Assert.IsInstanceOfType (typeof(Ssm2InitResponse), p, "type");
			Assert.IsInstanceOfType (typeof(Ssm2Packet), p, "base type");

			AssertKnownProperties1 (p);
			AssertContent (p, SSMID1, ROMID1, Capabilities1);
		}

		[Test()]
		public void Parse1 ()
		{
			byte[] packetData = TestPacket1;
			var p = new Ssm2InitResponse ();
			p.FromBytes (packetData);

			AssertKnownProperties1 (p);
			AssertContent (p, SSMID1, ROMID1, Capabilities1);
		}

		[Test()]
		public void Construct1 ()
		{
			var p = new Ssm2InitResponse (Ssm2Device.DiagnosticToolF0, Ssm2Device.Engine10, SSMID1, ROMID1, Capabilities1);

			AssertKnownProperties1 (p);
			AssertContent (p, SSMID1, ROMID1, Capabilities1);
		}


		#region SetSsmID

		[Test(), ExpectedException(typeof(ArgumentNullException))]
		public void SetSsmIDNull ()
		{
			var p = new Ssm2InitResponse ();
			p.SetSsmID (null);
		}

		[Test(), ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void SetSsmID2 ()
		{
			var p = new Ssm2InitResponse ();
			p.SetSsmID (new byte[2]);
		}

		[Test(), ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void SetSsmID4 ()
		{
			var p = new Ssm2InitResponse ();
			p.SetSsmID (new byte[4]);
		}

		[Test(), ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void SetSsmID3 ()
		{
			var p = new Ssm2InitResponse ();
			p.SetSsmID (new List<byte> (3).ToArray ());
		}

		#endregion SetSsmID

		#region SetRomID

		[Test(), ExpectedException(typeof(ArgumentNullException))]
		public void SetRomIDNull ()
		{
			var p = new Ssm2InitResponse ();
			p.SetRomID (null);
		}

		[Test(), ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void SetRomID2 ()
		{
			var p = new Ssm2InitResponse ();
			p.SetRomID (new byte[2]);
		}

		[Test(), ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void SetRomID6 ()
		{
			var p = new Ssm2InitResponse ();
			p.SetRomID (new byte[6]);
		}

		#endregion SetRomID

		#region SetCapabilities

		[Test(), ExpectedException(typeof(ArgumentNullException))]
		public void SetCapabilitiesNull ()
		{
			var p = new Ssm2InitResponse ();
			p.SetCapabilities (null);
		}

		// TODO Realistic capability bytes minimum, currently no exception
//		[Test(), ExpectedException(typeof(ArgumentOutOfRangeException))]
//		public void SetCapabilities0 ()
//		{
//			var p = new Ssm2InitResponse ();
//			p.SetCapabilities (new byte[0]);
//		}

		[Test(), ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void SetCapabilities247 ()
		{
			var p = new Ssm2InitResponse ();
			// MaxCapabilities length to fit in a packet is 246
			p.SetCapabilities (new byte[247]);
		}

		#endregion SetCapabilities


		static void AssertKnownProperties1 (Ssm2InitResponse p)
		{
			byte[] bytes = p.ToBytesCopy ();

			Assert.AreEqual (bytes.Length, p.Size, "Size");

			Assert.AreEqual (true, p.Check (), "Check()");
			Assert.AreEqual (0x1f, p.ChecksumCalculated, "ChecksumCalculated");

			Assert.AreEqual (Ssm2Device.DiagnosticToolF0, p.Destination, "Destination");
			Assert.AreEqual (Ssm2Device.Engine10, p.Source, "Source");
		}

		static void AssertContent (Ssm2InitResponse packet, byte[] ssmidExpected, byte[] romidExpected, byte[] capabilitesExpected)
		{
			Assert.AreEqual (true, packet.Check (), "Check()");
			Assert.AreEqual (Ssm2Command.InitResponseFF, packet.Command, "Command");

			byte[] ssmID = packet.GetSsmID ();
			Assert.AreEqual (3, ssmID.Length, "ssmID.Length");
			for (int i = 0; i < ssmID.Length; i++) {
				Assert.AreEqual (ssmidExpected[i], ssmID[i], "ssmID");
			}

			// array implements IList<T>
			IList<byte> ssmIDlist = packet.GetSsmID ();
			for (int i = 0; i < ssmIDlist.Count; i++) {
				Assert.AreEqual (ssmidExpected[i], ssmIDlist[i], "ssmID IList<byte>");
			}

			byte[] romID = packet.GetRomID ();
			Assert.AreEqual (5, romID.Length, "romID.Length");
			for (int i = 0; i < romID.Length; i++) {
				Assert.AreEqual (romidExpected[i], romID[i], "romID");
			}

			Assert.AreEqual (capabilitesExpected.Length, packet.CapabilitiesLength, "CapabilitiesLength");
			byte[] capabilities = packet.GetCapabilities ();
			Assert.AreEqual (capabilitesExpected.Length, capabilities.Length, "capabilities.Length");
			for (int i = 0; i < capabilities.Length; i++) {
				Assert.AreEqual (capabilitesExpected[i], capabilities[i], "capabilities");
			}
		}
	}
}
