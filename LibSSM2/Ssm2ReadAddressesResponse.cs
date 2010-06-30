// Ssm2ReadAddressesResponse.cs: SSM2 packet class for address(es) read response.

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

namespace Subaru.SSM2
{

	/// <summary>
	/// This packet type is usually created by the car control unit and sent to the tester.
	/// Example: 0x80 0xF0 0x10 0x03 0xE8 0x7D 0xB1 0x99
	/// </summary>
	public sealed class Ssm2ReadAddressesResponse : Ssm2DataResponse, ISsm2Packet
	{
		#region constructors

		public Ssm2ReadAddressesResponse () : base()
		{
		}

		public Ssm2ReadAddressesResponse (byte[] buffer) : base(buffer)
		{
		}

		/// <summary>
		/// Constructs a complete packet.
		/// (Uses optimal buffer size)
		/// </summary>
		/// <param name='destination'>
		/// Destination ID.
		/// </param>
		/// <param name='source'>
		/// Source ID.
		/// </param>
		/// <param name='data'>
		/// Data bytes.
		/// </param>
		public Ssm2ReadAddressesResponse (Ssm2Device destination, Ssm2Device source, IList<byte> data) : base(destination, source, data)
		{
			this.Finish ();
		}

		#endregion constructors


		public override bool Check ()
		{
			return this.Command == Ssm2Command.ReadAddressesResponseE8 && base.Check ();
		}

		protected override void SetConstBytes ()
		{
			base.Command = Ssm2Command.ReadAddressesResponseE8;
			// no padding byte
		}
	}
}
