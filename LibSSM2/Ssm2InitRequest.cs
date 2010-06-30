// Ssm2InitRequest.cs: SSM2 packet class for init request.

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
	/// Init request packet. Does not have specific properties.
	/// This packet type is usually created by the tester and sent to the car control unit.
	/// (Packet always consists of 6 bytes: 5 byte header + 1 checksum.
	/// Typically for engine it is {0x80, 0x10, 0xF0, 0x01, 0xBF, 0x40} )
	/// </summary>
	public sealed class Ssm2InitRequest : Ssm2Packet
	{
		private const int PacketSizeSpecificFixed = HeaderLength + 1;

		#region constructors

		public Ssm2InitRequest () : base (PacketSizeSpecificFixed)
		{
			// base class constructor will be called anyway
			//  : base()
			// this type of packet has fixed length
			this.count = PacketSizeSpecificFixed;
			this.propsSet = SetProperties.AllButChecksum;
		}

		public Ssm2InitRequest (byte[] buffer) : base (buffer, PacketSizeSpecificFixed)
		{
			this.propsSet = SetProperties.AllButChecksum;
		}

		/// <summary>
		/// Creates a complete init request packet.
		/// (Uses optimal buffer size of 6 bytes.)
		/// </summary>
		/// <param name="destination">
		/// A <see cref="SsmAddress"/>
		/// </param>
		/// <param name="source">
		/// A <see cref="SsmAddress"/>
		/// </param>
		public Ssm2InitRequest (Ssm2Device destination,
		                        Ssm2Device source)
			: this ()
		{
			this.Destination = destination;
			this.Source = source;

			this.Finish ();
		}

		#endregion constructors

		public override bool Check ()
		{
			return this.Command == Ssm2Command.InitRequestBF
				&& this.count == PacketSizeSpecificFixed
				&& base.Check ();
		}

		protected override void SetConstBytes ()
		{
			base.Command = Ssm2Command.InitRequestBF;
		}
	}
}
