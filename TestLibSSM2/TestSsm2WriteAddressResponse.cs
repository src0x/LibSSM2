// TestSsm2WriteAddressResponse.cs: Test SSM2 write addresses response class.

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
	public class TestSsm2WriteAddressResponse
	{
		/// <summary>
		/// Confirm written data byte 0x02, to diagnostic tool from engine.
		/// </summary>
		readonly byte[] TestPacket1 = { 0x80, 0xF0, 0x10, 0x02, 0xF8, 0x02, 0x7C };


		[Test()]
		public void Parse1 ()
		{
			byte[] packetData = TestPacket1;
			var p = new Ssm2WriteAddressResponse ();
			p.FromBytes (packetData);

			Assert.AreEqual (packetData.Length, p.Size, "Size");
			Assert.AreEqual (true, p.Check (), "Check()");

			Assert.AreEqual (Ssm2Device.DiagnosticToolF0, p.Destination, "Destination");
			Assert.AreEqual (Ssm2Device.Engine10, p.Source, "Source");
			Assert.AreEqual (Ssm2Command.WriteAddressResponseF8, p.Command, "Command");

			Assert.AreEqual (0x2, p.Data, "Data");
		}

		[Test()]
		public void NewFromBytes1 ()
		{
			byte[] packetData = TestPacket1;
			var p = Ssm2ReadAddressesRequest.NewFromBytes (packetData);
			Assert.IsInstanceOfType (typeof(Ssm2WriteAddressResponse), p, "type");
		}
	}
}
